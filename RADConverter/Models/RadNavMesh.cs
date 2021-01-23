using System;
using System.ComponentModel;

public class RadNavMesh
{
    [Description("NavMesh Header")]
    public RadNavMeshSetHeader NavMeshSetHeader;

    [Description("NavMesh Tiles")]
    public RadMeshTile[] MeshTile;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        NavMeshSetHeader = new RadNavMeshSetHeader();
        NavMeshSetHeader.PopulateWithData(data, ref offset);

        MeshTile = new RadMeshTile[NavMeshSetHeader.NumTiles];
        for (var i = 0; i < NavMeshSetHeader.NumTiles; i++)
        {
            MeshTile[i] = new RadMeshTile();
            MeshTile[i].PopulateWithData(data, ref offset);
        }
    }
}

public class RadNavMeshSetHeader
{
    [Description("Magic")]
    public int Magic;

    [Description("Version")]
    public int Version;

    [Description("Tiles count")]
    public int NumTiles;

    [Description("Parameters")]
    public RadNavMeshParams Params;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        Magic = BitConverter.ToInt32(data, offset);
        offset += 4;
        Version = BitConverter.ToInt32(data, offset);
        offset += 4;
        NumTiles = BitConverter.ToInt32(data, offset);
        offset += 4;

        Params = new RadNavMeshParams();
        Params.PopulateWithData(data, ref offset);
    }
}

public class RadNavMeshParams
{
    [Description("The world space origin of the navigation mesh's tile space. [(x, y, z)]")]
    public Vector3 Orig;

    [Description("The width of each tile. (Along the x-axis.)")]
    public float TileWidth;

    [Description("The height of each tile. (Along the z-axis.)")]
    public float TileHeight;

    [Description("The maximum number of tiles the navigation mesh can contain.This and maxPolys are used to calculate how many bits are needed to identify tiles and polygons uniquely.")]
    public int MaxTiles;

    [Description("The maximum number of polygons each tile can contain. This and maxTiles are used to calculate how many bits are needed to identify tiles and polygons uniquely.")]
    public int MaxPolys;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        Orig = new Vector3();
        Orig.PopulateWithData(data, ref offset);
        TileWidth = BitConverter.ToSingle(data, offset);
        offset += 4;
        TileHeight = BitConverter.ToSingle(data, offset);
        offset += 4;
        MaxTiles = BitConverter.ToInt32(data, offset);
        offset += 4;
        MaxPolys = BitConverter.ToInt32(data, offset);
        offset += 4;
    }
}

public class RadMeshTile
{
    [Description("NavMesh TileHeader")]
    public RadNavMeshTileHeader NavMeshTileHeader;

    [Description("The tile header.")]
    public RadMeshHeader MeshHeader;

    [Description("The tile vertices. [Size: dtMeshHeader::vertCount]")]
    public Vector3[] Verts;

    [Description("The tile polygons. [Size: dtMeshHeader::polyCount]")]
    public RadPoly[] Polys;

    [Description("The tile links. [Size: dtMeshHeader::maxLinkCount]")]
    public RadLink[] Links;

    [Description("Defines the location of detail sub-mesh data within a dtMeshTile.")]
    public RadPolyDetail[] DetailMeshes;

    [Description("The detail mesh's unique vertices. [(x, y, z) * dtMeshHeader::detailVertCount]")]
    public Vector3[] DetailVerts;

    [Description("The detail mesh's triangles. [(vertA, vertB, vertC, triFlags) * dtMeshHeader::detailTriCount].")]
    public byte[] DetailTris;

    [Description("The tile bounding volume nodes. [Size: dtMeshHeader::bvNodeCount]")]
    public RadBVNode[] BVTree;
    
    [Description("The tile off-mesh connections. [Size: dtMeshHeader::offMeshConCount]")]
    public RadOffMeshConnection[] OffMeshConnections;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        NavMeshTileHeader = new RadNavMeshTileHeader();
        NavMeshTileHeader.PopulateWithData(data, ref offset);

        MeshHeader = new RadMeshHeader();
        MeshHeader.PopulateWithData(data, ref offset);
        
        Verts = new Vector3[MeshHeader.VertCount];
        for (var i = 0; i < MeshHeader.VertCount; i++)
        {
            Verts[i] = new Vector3();
            Verts[i].PopulateWithData(data, ref offset);
        }
        
        Polys = new RadPoly[MeshHeader.PolyCount];
        for (var i = 0; i < MeshHeader.PolyCount; i++)
        {
            Polys[i] = new RadPoly();
            Polys[i].PopulateWithData(data, ref offset);
        }

        Links = new RadLink[MeshHeader.MaxLinkCount];
        for (var i = 0; i < MeshHeader.MaxLinkCount; i++)
        {
            Links[i] = new RadLink();
            Links[i].PopulateWithData(data, ref offset);
        }

        DetailMeshes = new RadPolyDetail[MeshHeader.DetailMeshCount];
        for (var i = 0; i < MeshHeader.DetailMeshCount; i++)
        {
            DetailMeshes[i] = new RadPolyDetail();
            DetailMeshes[i].PopulateWithData(data, ref offset);
        }

        DetailVerts = new Vector3[MeshHeader.DetailVertCount];
        for (var i = 0; i < MeshHeader.DetailVertCount; i++)
        {
            DetailVerts[i] = new Vector3();
            DetailVerts[i].PopulateWithData(data, ref offset);
        }

        DetailTris = new byte[MeshHeader.DetailTriCount * 4];
        for (var i = 0; i < MeshHeader.DetailTriCount * 4; i++)
        {
            DetailTris[i] = data[offset];
            offset += 1;
        }

        BVTree = new RadBVNode[MeshHeader.NodeCount];
        for (var i = 0; i < MeshHeader.NodeCount; i++)
        {
            BVTree[i] = new RadBVNode();
            BVTree[i].PopulateWithData(data, ref offset);
        }
        
        OffMeshConnections = new RadOffMeshConnection[MeshHeader.OffMeshConCount];
        for (var i = 0; i < MeshHeader.OffMeshConCount; i++)
        {
            OffMeshConnections[i] = new RadOffMeshConnection();
            OffMeshConnections[i].PopulateWithData(data, ref offset);
        }
    }
}

public class RadNavMeshTileHeader
{
    public uint TileRef;

    public int DataSize;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        TileRef = BitConverter.ToUInt32(data, offset);
        offset += 4;
        DataSize = BitConverter.ToInt32(data, offset);
        offset += 4;
    }
}

public class RadMeshHeader
{
    [Description("Tile magic number. (Used to identify the data format.)")]
    public int Magic;
    [Description("Tile data format version number.")]
    public int Version;
    [Description("The position of the tile within the dtNavMesh tile grid. (x, y, layer)")]
    public Vector2i Origin;
    [Description("The layer of the tile within the dtNavMesh tile grid. (x, y, layer)")]
    public int Layer;
    [Description("The user defined id of the tile.")]
    public uint UserId;
    [Description("The number of polygons in the tile.")]
    public int PolyCount;
    [Description("The number of vertices in the tile.")]
    public int VertCount;
    [Description("The number of allocated links.")]
    public int MaxLinkCount;
    [Description("The number of sub-meshes in the detail mesh.")]
    public int DetailMeshCount;
    [Description("The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)")]
    public int DetailVertCount;
    [Description("The number of triangles in the detail mesh.")]
    public int DetailTriCount;
    [Description("The number of bounding volume nodes. (Zero if bounding volumes are disabled.)")]
    public int NodeCount;
    [Description("The number of off-mesh connections.")]
    public int OffMeshConCount;
    [Description("The index of the first polygon which is an off-mesh connection.")]
    public int OffMeshBase;
    [Description("The height of the agents using the tile.")]
    public float WalkableHeight;
    [Description("The radius of the agents using the tile.")]
    public float WalkableRadius;
    [Description("The maximum climb height of the agents using the tile.")]
    public float WalkableClimb;
    [Description("The minimum bounds of the tile's AABB. [(x, y, z)]")]
    public Vector3 BoundsMin;
    [Description("The maximum bounds of the tile's AABB. [(x, y, z)]")]
    public Vector3 BoundsMax;
    [Description("The bounding volume quantization factor. ")]
    public float QuantFactor;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        Magic = BitConverter.ToInt32(data, offset);
        offset += 4;
        Version = BitConverter.ToInt32(data, offset);
        offset += 4;
        Origin = new Vector2i();
        Origin.PopulateWithData(data, ref offset);
        Layer = BitConverter.ToInt32(data, offset);
        offset += 4;
        UserId = BitConverter.ToUInt32(data, offset);
        offset += 4;
        PolyCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        VertCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        MaxLinkCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        DetailMeshCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        DetailVertCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        DetailTriCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        NodeCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        OffMeshConCount = BitConverter.ToInt32(data, offset);
        offset += 4;
        OffMeshBase = BitConverter.ToInt32(data, offset);
        offset += 4;
        WalkableHeight = BitConverter.ToSingle(data, offset);
        offset += 4;
        WalkableRadius = BitConverter.ToSingle(data, offset);
        offset += 4;
        WalkableClimb = BitConverter.ToSingle(data, offset);
        offset += 4;
        BoundsMin = new Vector3();
        BoundsMin.PopulateWithData(data, ref offset);
        BoundsMax = new Vector3();
        BoundsMax.PopulateWithData(data, ref offset);
        QuantFactor = BitConverter.ToSingle(data, offset);
        offset += 4;
    }
}

public class RadPoly
{
    [Description("Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)")]
    public uint FirstLink;

    [Description("The indices of the polygon's vertices. The actual vertices are located in dtMeshTile::verts.")]
    public ushort[] Verts = new ushort[6];

    [Description("Packed data representing neighbor polygons references and flags for each edge.")]
    public ushort[] Neis = new ushort[6];

    [Description("The user defined polygon flags.")]
    public ushort Flags;

    [Description("The number of vertices in the polygon.")]
    public byte VertCount;

    [Description("Polygon type")]
    public int Type;

    [Description("Polygon area")]
    public byte Area;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        FirstLink = BitConverter.ToUInt32(data, offset);
        offset += 4;

        for(var i = 0; i < Verts.Length; i++)
        {
            Verts[i] = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }
        for (var i = 0; i < Neis.Length; i++)
        {
            Neis[i] = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }

        Flags = BitConverter.ToUInt16(data, offset);
        offset += 2;
        VertCount = data[offset];
        offset += 1;
        var areaAndType = data[offset];
        offset += 1;

        Type = areaAndType >> 6;
        Area = Convert.ToByte(areaAndType & 0x3f);
    }
};

public class RadLink
{
    [Description("Neighbour reference. (The neighbor that is linked to.)")]
    public uint Ref;
    [Description("Index of the next link.")]
    public uint Next;
    [Description("Index of the polygon edge that owns this link.")]
    public byte Edge;
    [Description("If a boundary link, defines on which side the link is.")]
    public byte Side;
    [Description("If a boundary link, defines the minimum sub-edge area.")]
    public byte BMin;
    [Description("If a boundary link, defines the maximum sub-edge area.")]
    public byte BMax;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        Ref = BitConverter.ToUInt32(data, offset);
        offset += 4;
        Next = BitConverter.ToUInt32(data, offset);
        offset += 4;
        Edge = data[offset];
        offset += 1;
        Side = data[offset];
        offset += 1;
        BMin = data[offset];
        offset += 1;
        BMax = data[offset];
        offset += 1;
    }
};

public class RadPolyDetail
{
    [Description("The offset of the vertices in the dtMeshTile::detailVerts array.")]
    public uint VertBase;
    [Description("The offset of the triangles in the dtMeshTile::detailTris array.")]
    public uint TriBase;
    [Description("The number of vertices in the sub-mesh.")]
    public byte VertCount;
    [Description("The number of triangles in the sub-mesh.")]
    public byte TriCount;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        VertBase = BitConverter.ToUInt32(data, offset);
        offset += 4;
        TriBase = BitConverter.ToUInt32(data, offset);
        offset += 4;
        VertCount = data[offset];
        offset += 1;
        TriCount = data[offset];
        offset += 1;

        //Byte alignment
        offset += 2;
    }
};

public class RadBVNode
{
    [Description("Minimum bounds of the node's AABB. [(x, y, z)]")]
    public ushort[] BMin;
    [Description("Maximum bounds of the node's AABB. [(x, y, z)]")]
    public ushort[] BMax;
    [Description("The node's index. (Negative for escape sequence.)")]
    public int i;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        BMin = new ushort[3];
        for (var j = 0; j < 3; j++)
        {
            BMin[j] = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }
        BMax = new ushort[3];
        for (var j = 0; j < 3; j++)
        {
            BMax[j] = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }
        i = BitConverter.ToInt32(data, offset);
        offset += 4;
    }
}

public class RadOffMeshConnection
{
    [Description("The endpoints of the connection. [(ax, ay, az, bx, by, bz)]")]
    public float[] Pos;

    [Description("The radius of the endpoints. [Limit: >= 0]")]
    public float Rad;

    [Description("The polygon reference of the connection within the tile.")]
    public ushort Poly;

    [Description("Link flags.")]
    public byte Flags;

    [Description("End point side.")]
    public byte Side;

    [Description("The id of the offmesh connection. (User assigned when the navigation mesh is built.)")]
    public int UserId;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        Pos = new float[6];
        for (var i = 0; i < 6; i++)
        {
            Pos[i] = BitConverter.ToSingle(data, offset);
            offset += 4;
        }
        Rad = BitConverter.ToSingle(data, offset);
        offset += 4;
        Poly = BitConverter.ToUInt16(data, offset);
        offset += 2;
        Flags = data[offset];
        offset += 1;
        Side = data[offset];
        offset += 1;
        UserId = BitConverter.ToInt32(data, offset);
        offset += 4;
    }
}
