using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using static SharpNav.Collections.BVTree;

namespace RADConverter.Models
{
	public class SharpNavMesh
	{
		public object Meta { get; set; }
		public Vector3 Origin { get; set; }
		public float TileWidth { get; set; }
		public float TileHeight { get; set; }
		public float MaxTiles { get; set; }
		public float MaxPolys { get; set; }
		public List<SnNavTile> Tiles { get; set; }

		public SharpNavMesh()
        {
			Meta = new
			{
				version = new
				{
					snj = 3,
					sharpnav = "1.0.0-alpha.2"
				}
			};
		}
	}

	public class SnNavTile
	{
		public int PolyId { get; set; }
		public Vector2i Location { get; set; }
		public int Layer { get; set; }
		public int Salt { get; set; } //TODO Find out how to generate it
		public int PolyCount { get; set; }
		public NavPoly[] Polys { get; set; }
		public Vector3[] Verts { get; set; }
		public PolyMeshDetail.MeshData[] DetailMeshes { get; set; }
		public Vector3[] DetailVerts { get; set; }
		public List<PolyMeshDetail.TriangleData> DetailTris { get; set; }
		public OffMeshConnection[] OffMeshConnections { get; set; }
		public int OffMeshConnectionCount { get; set; }
		public Node[] BVTree { get; set; }
		public float BvQuantFactor { get; set; }
		public int BvNodeCount { get; set; }
		public BBox3 Bounds { get; set; }
		public float WalkableClimb { get; set; }
	}
}
