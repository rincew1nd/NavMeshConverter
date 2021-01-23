using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RADConverter.Models;
using SharpNav;
using SharpNav.IO.Json;
using SharpNav.Pathfinding;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SharpNav.Collections.BVTree;
using Path = System.IO.Path;

namespace RADConverter
{
    public class RadToSnConverter
    {
        private readonly JsonSerializer _serializer;

        public RadToSnConverter()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                Converters = new List<JsonConverter>() { new Vector3Converter(), new AreaConverter(), new PolyIdConverter() }
            });
        }

        public void Convert(string path)
        {
            var file = File.ReadAllBytes(path);

            var offset = 0;
            var navMesh = new RadNavMesh();
            navMesh.PopulateWithData(file, ref offset);

            var snNavMesh = MapRadToSharpNav(navMesh);
            var json = SerializeSn(snNavMesh);

            File.WriteAllText(
                Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}.snj"),
                json
            );
        }

        private SharpNavMesh MapRadToSharpNav(RadNavMesh navMesh)
        {
            var snNavMesh = new SharpNavMesh();

            //Header
            snNavMesh.Origin = navMesh.NavMeshSetHeader.Params.Orig;
            snNavMesh.TileWidth = navMesh.NavMeshSetHeader.Params.TileWidth;
            snNavMesh.TileHeight = navMesh.NavMeshSetHeader.Params.TileHeight;
            snNavMesh.MaxTiles = navMesh.NavMeshSetHeader.Params.MaxTiles;
            snNavMesh.MaxPolys = navMesh.NavMeshSetHeader.Params.MaxPolys;

            //Tiles
            snNavMesh.Tiles = new List<SnNavTile>();
            for (var i = 0; i < navMesh.MeshTile.Length; i++)
            {
                var meshTile = navMesh.MeshTile[i];

                var snMeshTile = new SnNavTile();

                snMeshTile.Location = meshTile.MeshHeader.Origin;
                snMeshTile.Layer = meshTile.MeshHeader.Layer;
                snMeshTile.Bounds = new SharpNav.Geometry.BBox3(meshTile.MeshHeader.BoundsMin.ToSn(), meshTile.MeshHeader.BoundsMax.ToSn());
                snMeshTile.BvNodeCount = meshTile.MeshHeader.NodeCount;
                snMeshTile.BvQuantFactor = meshTile.MeshHeader.QuantFactor;
                snMeshTile.WalkableClimb = meshTile.MeshHeader.WalkableClimb;

                snMeshTile.Polys =
                    meshTile.Polys.Select(p => new NavPoly()
                    {
                        Verts = p.Verts.Select(v => (int)v).ToArray(),
                        Neis = p.Neis.Select(v => (int)v).ToArray(),
                        PolyType = p.Type == 0 ? NavPolyType.Ground : NavPolyType.OffMeshConnection,
                        Area = new Area(p.Area),
                        VertCount = p.VertCount,
                        //TODO meshTile.Links should be there, but i have not found how to do it right now
                    }).ToArray();

                snMeshTile.Verts = meshTile.Verts;

                snMeshTile.DetailMeshes =
                    meshTile.DetailMeshes.Select(d => new PolyMeshDetail.MeshData()
                    {
                        TriangleCount = d.TriCount,
                        TriangleIndex = (int)d.TriBase,
                        VertexCount = d.VertCount,
                        VertexIndex = (int)d.VertBase
                    }).ToArray();

                snMeshTile.DetailVerts = meshTile.DetailVerts;

                snMeshTile.DetailTris = new List<PolyMeshDetail.TriangleData>();
                for (var j = 0; j < meshTile.DetailTris.Length; j += 4)
                {
                    snMeshTile.DetailTris.Add(new PolyMeshDetail.TriangleData()
                    {
                        VertexHash0 = System.Convert.ToInt32(meshTile.DetailTris[j]),
                        VertexHash1 = System.Convert.ToInt32(meshTile.DetailTris[j + 1]),
                        VertexHash2 = System.Convert.ToInt32(meshTile.DetailTris[j + 2]),
                        Flags = System.Convert.ToInt32(meshTile.DetailTris[j + 3]),
                    });
                }

                snMeshTile.OffMeshConnections =
                    meshTile.OffMeshConnections.Select(o => new OffMeshConnection()
                    {
                        Pos0 = new SharpNav.Geometry.Vector3(o.Pos[0], o.Pos[1], o.Pos[2]),
                        Pos1 = new SharpNav.Geometry.Vector3(o.Pos[3], o.Pos[4], o.Pos[5]),
                        Radius = o.Rad,
                        Poly = o.Poly,
                        Flags = (o.Flags & 0xff) == 1 ? OffMeshConnectionFlags.Bidirectional : OffMeshConnectionFlags.None,
                        Side = (BoundarySide) o.Side
                    }).ToArray();

                snMeshTile.BVTree = meshTile.BVTree.Select(n =>
                    new Node()
                    {
                        Bounds = new PolyBounds(
                            new PolyVertex(n.BMin[0], n.BMin[1], n.BMin[2]),
                            new PolyVertex(n.BMax[0], n.BMax[1], n.BMax[2])
                        ),
                        Index = n.i
                    }).ToArray();

                snMeshTile.PolyId = (int)meshTile.NavMeshTileHeader.TileRef;
                snMeshTile.Salt = 1; //TODO Temporary hack. Maybe it will work fine?

                snNavMesh.Tiles.Add(snMeshTile);
            }

            return snNavMesh;
        }

        private string SerializeSn(SharpNavMesh snNavMesh)
        {
            JObject root = new JObject();

            root.Add("meta", JToken.FromObject(snNavMesh.Meta));

            root.Add("origin", JToken.FromObject(snNavMesh.Origin, _serializer));
            root.Add("tileWidth", JToken.FromObject(snNavMesh.TileWidth, _serializer));
            root.Add("tileHeight", JToken.FromObject(snNavMesh.TileHeight, _serializer));
            root.Add("maxTiles", JToken.FromObject(snNavMesh.MaxTiles, _serializer));
            root.Add("maxPolys", JToken.FromObject(snNavMesh.MaxPolys, _serializer));

            var tilesArray = new JArray();
            foreach (SnNavTile tile in snNavMesh.Tiles)
            {
                tilesArray.Add(SerializeSnMeshTile(tile));
            }

            root.Add("tiles", tilesArray);

            return root.ToString();
        }

        private JObject SerializeSnMeshTile(SnNavTile tile)
        {
            var result = new JObject();
            result.Add("polyId", JToken.FromObject(tile.PolyId, _serializer));
            result.Add("location", JToken.FromObject(tile.Location, _serializer));
            result.Add("layer", JToken.FromObject(tile.Layer, _serializer));
            result.Add("salt", JToken.FromObject(tile.Salt, _serializer));
            result.Add("bounds", JToken.FromObject(tile.Bounds, _serializer));
            result.Add("polys", JToken.FromObject(tile.Polys, _serializer));
            result.Add("verts", JToken.FromObject(tile.Verts, _serializer));
            result.Add("detailMeshes", JToken.FromObject(tile.DetailMeshes, _serializer));
            result.Add("detailVerts", JToken.FromObject(tile.DetailVerts, _serializer));
            result.Add("detailTris", JToken.FromObject(tile.DetailTris, _serializer));
            result.Add("offMeshConnections", JToken.FromObject(tile.OffMeshConnections, _serializer));

            JObject treeObject = new JObject();
            JArray treeNodes = new JArray();
            for (int i = 0; i < tile.BVTree.Length; i++)
                treeNodes.Add(JToken.FromObject(tile.BVTree[i], _serializer));
            treeObject.Add("nodes", treeNodes);

            result.Add("bvTree", treeObject);
            result.Add("bvQuantFactor", JToken.FromObject(tile.BvQuantFactor, _serializer));
            result.Add("bvNodeCount", JToken.FromObject(tile.BvNodeCount, _serializer));
            result.Add("walkableClimb", JToken.FromObject(tile.WalkableClimb, _serializer));

            return result;
        }
    }
}
