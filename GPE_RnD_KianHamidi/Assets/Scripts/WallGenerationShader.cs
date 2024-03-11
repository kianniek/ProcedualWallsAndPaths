using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerationShader : MonoBehaviour
{
    //use the line generator to generate the wall
    LineGenerator lineGenerator;
    MeshFilter meshFilter;
    //set the depth of the wall
    public float wallDepth = 0.1f;
    //set the height of the wall
    public float wallHeight = 2f;

    //get the line generator and mesh filter
    private void Awake()
    {
        lineGenerator = GetComponent<LineGenerator>();
        meshFilter = GetComponent<MeshFilter>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //when enter is pressed, generate a new wall and destroy the old one
        if (Input.GetKey(KeyCode.Return))
        {
            GenerateWall();
        }
    }

    //generate the wall mesh
    public void GenerateWall()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        if (lineGenerator != null)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            int verticesStartIndex = 0;

            Mesh mesh = new Mesh();
            foreach (var line in lineGenerator.Lines)
            {
                for (int i = 1; i < line.LinePoints.Count; i++)
                {
                    if (i - 1 < 0) { continue; }

                    // Generate data for each segment
                    GenerateMeshData(vertices, triangles, normals, uvs, line.LinePoints[i - 1], line.LinePoints[i], wallDepth, wallHeight, verticesStartIndex);
                    verticesStartIndex += 4; // Each segment adds 4 vertices
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;
        }
    }

    private void GenerateMeshData(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs, Vector3 start, Vector3 end, float depth, float height, int startIndex)
    {
        // Add vertices for the current segment
        vertices.Add(start);
        vertices.Add(end);
        vertices.Add(start + new Vector3(0, height, 0));
        vertices.Add(end + new Vector3(0, height, 0));

        // Add triangle indices
        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 3);
        triangles.Add(startIndex + 2);

        // Add normals
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        // Add UVs
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
    }

}
