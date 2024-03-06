using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
public class MeshCombinerRuntime : MonoBehaviour
{
    //function that cobines the cild meshes into one mesh
    public void CombineMeshes()
    {
        // Create a new CombineInstance array
        CombineInstance[] combine = new CombineInstance[transform.childCount];
        // Loop through each child
        for (int i = 0; i < transform.childCount; i++)
        {
            // Get the child's mesh filter
            MeshFilter meshFilter = transform.GetChild(i).GetComponent<MeshFilter>();
            // Check if the mesh filter is not null
            if (meshFilter != null)
            {
                // Set the combine instance's mesh to the child's mesh
                combine[i].mesh = meshFilter.sharedMesh;
                // Set the combine instance's transform to the child's transform
                combine[i].transform = meshFilter.transform.localToWorldMatrix;
                // Destroy the child's game object
                Destroy(meshFilter.gameObject);
            }
        }
        // Create a new mesh filter
        MeshFilter meshFilter2 = gameObject.GetComponent<MeshFilter>();
        // Set the mesh filter's mesh to a new mesh
        meshFilter2.mesh = new Mesh();
        // Combine the meshes
        meshFilter2.mesh.CombineMeshes(combine);
    }
}
