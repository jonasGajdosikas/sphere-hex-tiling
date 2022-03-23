using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterTriangle
{
    public Vector3 VertexA, VertexB, VertexC,
                   Dir1, Dir2;
    public int sectorIndex;
    #region NeighborClusterIDs
    public int neighborAB
    {
        get
        {
            return (sectorIndex % 4) switch
            {
                0 => ((sectorIndex + 16) % 20),
                1 => ((sectorIndex + 1) % 20),
                2 => ((sectorIndex + 17) % 20),
                3 => ((sectorIndex + 4) % 20),
                _ => throw new System.ArgumentOutOfRangeException("Modulo yielded results more than N or less than 0")
            };

        }
    }
    public int neighborAC
    {
        get
        {
            return (sectorIndex % 4) switch
            {
                0 => ((sectorIndex + 4) % 20),
                1 => ((sectorIndex + 17) % 20),
                2 => ((sectorIndex + 19) % 20),
                3 => ((sectorIndex + 16) % 20),
                _ => throw new System.ArgumentOutOfRangeException("Modulo yielded results more than N or less than 0")
            };

        }
    }
    public int neighborBC
    {
        get
        {
            return (sectorIndex % 2) switch
            {
                0 => ((sectorIndex + 1) % 20),
                1 => ((sectorIndex + 19) % 20),
                _ => throw new System.ArgumentOutOfRangeException("Modulo yielded results more than N or less than 0")
            };

        }
    } 
    #endregion
    readonly int LoD;
    int resolution => 3 + 3 * LoD;
    Mesh mesh;
    public bool Flipped => (sectorIndex & 1) == 1;

    public ClusterTriangle(int[] Vertices, int sectorIndex, int loD, Mesh mesh)
    {
        VertexA = Metrics.IcoVertices[Vertices[0]];
        VertexB = Metrics.IcoVertices[Vertices[1]];
        VertexC = Metrics.IcoVertices[Vertices[2]];
        this.sectorIndex = sectorIndex;
        LoD = loD;
        Dir1 = (VertexB - VertexA) / resolution;
        Dir2 = (VertexC - VertexB) / resolution;
        this.mesh = mesh;
        
    }
    Vector3 GetVertex(int x, int y)
    {
        return VertexA + Dir1 * x + Dir2 * y;
    }
    public void GenerateVertices(ref Dictionary<Vector3, Vertex> Vertices)
    {
        for (int i = 0; i <= resolution; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                Vector3 pos = GetVertex(i, j);
                if (!Vertices.ContainsKey(pos))
                {
                    Vertex vert = new Vertex(pos, sectorIndex, (i + j) % 3 == 0);
                    Vertices.Add(pos, vert);
                }
                try
                {
                    if (i > 0)
                    {
                        if (j < i)
                        {
                            Vertices[pos].AddNeighbor(Vertices[GetVertex(i - 1, j)]);
                        }
                        if (j > 0)
                        {
                            Vertices[pos].AddNeighbor(Vertices[GetVertex(i - 1, j - 1)]);
                        }
                    }
                    if (j > 0)
                    {
                        Vertices[pos].AddNeighbor(Vertices[GetVertex(i, j - 1)]);
                    }
                }
                catch (KeyNotFoundException e)
                {
                    Debug.LogError("i:" + i + ",j:" + j);
                    //Debug.LogException(e);
                }
            }
        }
    }
    public void ConstructMesh(List<Tile> tiles, float tileCornerMult)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach(Tile tile in tiles)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(tile.centerVertex.Position.normalized);
            foreach (Vertex vert in tile.centerVertex.SortedNeighbors) vertices.Add(vert.Position.normalized * tileCornerMult);
            int l = tile.centerVertex.SortedNeighbors.Length;
            for (int i = 0; i < l; i++)
            {
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + i);
                triangles.Add(vertexIndex + (i + 1) % l);
            }
        }
        

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
