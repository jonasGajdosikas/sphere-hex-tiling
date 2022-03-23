using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(0, 100)]
    public int LoD = 0;

    [SerializeField, HideInInspector]
    ClusterTriangle[] terrainFaces;
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    [HideInInspector]
    public float tileEdgeRadius;
    public Dictionary<Vector3, Vertex> Vertices;
    [SerializeField, HideInInspector]
    Dictionary<Vector3, Tile> Tiles;

    public void GeneratePlanet()
    {
        Initalize();
        GenerateMesh();
    }
    void Initalize()
    {
        Vertices = new Dictionary<Vector3, Vertex>();
        if (meshFilters == null || meshFilters.Length == 0)
            meshFilters = new MeshFilter[20];
        terrainFaces = new ClusterTriangle[20];
        for (int i = 0; i < 20; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObject = new GameObject("Mesh");
                meshObject.transform.parent = transform;

                meshObject.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            terrainFaces[i] = new ClusterTriangle(Metrics.IcoTriangles[i], i, LoD, meshFilters[i].sharedMesh);
        }
        tileEdgeRadius = 1 / Mathf.Cos(2 * Mathf.Atan2(2, Mathf.Sqrt(5) + 1) / (3 * (LoD + 1)));
    }
    void GenerateMesh()
    {
        foreach (ClusterTriangle face in terrainFaces) face.GenerateVertices(ref Vertices);
        Tiles = new Dictionary<Vector3, Tile>();
        List<Tile>[] sectorTiles = new List<Tile>[20];
        for (int i = 0; i < 20; i++) sectorTiles[i] = new List<Tile>();
        foreach(Vertex vert in Vertices.Values)
        {
            if (vert.isTileCenter)
            {
                Tile tile = new Tile(vert);
                sectorTiles[vert.ParentClusterID].Add(tile);
                Tiles.Add(vert.Position, tile);
            }
        }
        /**
        #region Adjacencies
        for (int i = 0; i < 20; i++)
        {
            foreach (Tile tile in sectorTiles[i])
            {
                for (int j = 0; j < tile.centerVertex.SortedNeighbors.Length; j++)
                {
                    Vertex v1 = tile.centerVertex.SortedNeighbors[j],
                           v2 = tile.centerVertex.SortedNeighbors[(j + 1) % tile.centerVertex.SortedNeighbors.Length];
                    foreach (Vertex third in v1.neigbors)
                    {
                        if (v2.neigbors.Contains(third) && tile.centerVertex != third)
                        {
                            tile.AddNeighbor(Tiles[third.Position]);
                            break;
                        }
                    }
                }
            }
        } 
        #endregion
        **/
        for(int i = 0; i < 20; i++)
        {
            terrainFaces[i].ConstructMesh(sectorTiles[i], tileEdgeRadius);
        }
    }
}

public class Vertex : System.IEquatable<Vertex>
{
    const float epsilon = 0.000001f;
    public Vector3 Position { get; private set; }
    public List<Vertex> neigbors { get; set; }
    Vertex[] sortedNeighbors;
    public Vertex[] SortedNeighbors
    {
        get
        {
            if (sortedNeighbors != null && sortedNeighbors.Length == neigbors.Count)
                return sortedNeighbors;
            Vertex[] arr = neigbors.ToArray();
            Vector3 p1 = Position;
            for(int i = 1; i < arr.Length; i++)
            {
                Vector3 p2 = arr[i - 1].Position;
                foreach (Vertex next in arr[i - 1].neigbors)
                {
                    if (!neigbors.Contains(next))
                        continue;
                    Vector3 p3 = next.Position;
                    if (Vector3.Dot(TriangleCenter(p1, p2, p3), TriangleNormal(p1,p2,p3)) < 0)
                        continue;
                    arr[i] = next;
                }
            }
            sortedNeighbors = arr;
            return sortedNeighbors;
        }
    }
    public int ParentClusterID;
    public Vertex(Vector3 position, int ParentClusterID, bool isTileCenter)
    {
        this.Position = position;
        this.ParentClusterID = ParentClusterID;
        this.isTileCenter = isTileCenter;
        neigbors = new List<Vertex>();
    }
    public void AddNeighbor(Vertex other)
    {
        if (!neigbors.Contains(other))
        {
            neigbors.Add(other);
            other.neigbors.Add(this);
        }
    }
    public bool isTileCenter;
    public override bool Equals(object obj)
    {
        return Equals(obj as Vertex);
    }
    public bool Equals(Vertex other)
    {
        return (this.Position - other.Position).sqrMagnitude <= epsilon * epsilon;
    }
    public override int GetHashCode()
    {
        int mult = 1024;
        int x = (int)(Position.x * mult),
            y = (int)(Position.y * mult),
            z = (int)(Position.z * mult);
        return System.Tuple.Create(x,y,z).GetHashCode();
    }
    /// <summary>
    /// Calculates the normal vector to a triangle given it's vertices
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    public static Vector3 TriangleNormal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Cross(p2 - p1, p3 - p1);
    }
    public static Vector3 TriangleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1 + p2 + p3) / 3f;
    }
}
public class Tile
{
    public Vertex centerVertex;
    public float height;
    public Tile[] NeighborTiles { get { return neighbors.ToArray(); } }
    public List<Tile> neighbors;
    public void AddNeighbor(Tile other)
    {
        neighbors.Add(other);
    }
    public Tile(Vertex centerVertex)
    {
        this.centerVertex = centerVertex;
    }
}
