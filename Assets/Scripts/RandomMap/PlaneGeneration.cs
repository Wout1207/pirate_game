using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class PlaneGeneration : MonoBehaviour
{
    Mesh myMesh;
    MeshFilter meshFilter;

    [SerializeField]
    public Vector2 planeSize = new Vector2();
    [SerializeField]
    int planeResolution = 1;

    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;
    // Start is called before the first frame update
    public void CreatePlane()
    {
        myMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = myMesh;

        GeneratePlane(planeSize, planeResolution);
        AssignMesh();
    }

    public void GeneratePlane(Vector2 size, int resolution)
    {
        vertices = new List<Vector3>();
        float xPerStep = size.x / resolution;
        float yPerStep = size.y / resolution;
        for (int y = 0; y<resolution+1;y++)
        {
            for (int x = 0; x<resolution+1;x++)
            {
                vertices.Add(new Vector3(x * xPerStep, 0, y * yPerStep));
            }
        }

        triangles = new List<int>();
        for (int row = 0; row<resolution; row++)
        {
            for (int column = 0; column<resolution; column++)
            {
                int i = (row * resolution) + row + column;
                triangles.Add(i);
                triangles.Add(i + resolution + 1);
                triangles.Add(i + resolution + 2);

                triangles.Add(i);
                triangles.Add(i + resolution + 2);
                triangles.Add(i + 1);
            }
        }

        uvs = new List<Vector2>();
        for (int y = 0; y < resolution + 1; y++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                uvs.Add(new Vector2((float)x / resolution, (float)y / resolution));
            }
        }
    }

    public void AssignMesh()
    {
        myMesh.Clear();
        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangles.ToArray();
        myMesh.uv = uvs.ToArray();
    }
}
