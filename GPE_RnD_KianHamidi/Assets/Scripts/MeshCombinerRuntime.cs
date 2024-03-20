using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshCombinerRuntime : MonoBehaviour
{
    //function that cobines the cild meshes into one mesh
    public void CombineMeshes()
    {
        for (int j = 0; j < transform.childCount; j++)
        {
            //save the position and rotation of the child
            Vector3 position = transform.GetChild(j).position;
            Quaternion rotation = transform.GetChild(j).rotation;
            //reset the position and rotation of the child
            transform.GetChild(j).position = Vector3.zero;
            transform.GetChild(j).rotation = Quaternion.identity;

            // Create a new CombineInstance array
            CombineInstance[] combine = new CombineInstance[transform.GetChild(j).childCount];
            // Loop through each child
            for (int i = 0; i < transform.GetChild(j).childCount; i++)
            {
                // Get the child's mesh filter
                MeshFilter meshFilter = transform.GetChild(j).GetChild(i).GetComponent<MeshFilter>();
                // Check if the mesh filter is not null
                if (meshFilter != null)
                {
                    // Set the combine instance's mesh to the child's mesh
                    combine[i].mesh = meshFilter.sharedMesh;
                    // Set the combine instance's transform to the child's transform
                    combine[i].transform = meshFilter.transform.localToWorldMatrix;
                    // Destroy the child's game object
                    meshFilter.gameObject.SetActive(false);
                }
            }

            if(transform.GetChild(j).GetComponent<MeshFilter>() == null)
            {
                transform.GetChild(j).gameObject.AddComponent<MeshFilter>();
            }
            if (transform.GetChild(j).GetComponent<MeshRenderer>() == null)
            {
                transform.GetChild(j).gameObject.AddComponent<MeshRenderer>();
            }
            // Create a new mesh filter
            MeshFilter meshFilter2 = transform.GetChild(j).GetComponent<MeshFilter>();
            MeshRenderer meshRenderer2 = transform.GetChild(j).GetComponent<MeshRenderer>();

            meshRenderer2.material = transform.GetChild(j).GetChild(0).GetComponent<MeshRenderer>().material;
            // Set the mesh filter's mesh to a new mesh
            meshFilter2.mesh = new Mesh();
            // Combine the meshes
            meshFilter2.mesh.CombineMeshes(combine);
            

            //reset the position and rotation of the child
            transform.GetChild(j).SetPositionAndRotation(position, rotation);
        }

    }
}
