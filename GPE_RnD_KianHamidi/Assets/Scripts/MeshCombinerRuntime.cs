using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombinerRuntime : MonoBehaviour
{
    [SerializeField] Material material;

    private void Start()
    {
        if (material == null)
        {
            Debug.LogError("Material is not set");
        }
    }
    //function that combines the child meshes into one mesh
    public void CombineMeshes()
    {
        for (int j = 0; j < transform.childCount; j++)
        {
            GameObject currentChild = transform.GetChild(j).gameObject;


            // Ensure the current child has MeshFilter and MeshRenderer components

            if (!currentChild.TryGetComponent(out MeshFilter meshFilter2))
            {
                meshFilter2 = currentChild.AddComponent<MeshFilter>();
            }

            if (!currentChild.TryGetComponent(out MeshRenderer meshRenderer2))
            {
                meshRenderer2 = currentChild.AddComponent<MeshRenderer>();
            }

            // Skip this child if it's not active in the hierarchy
            if (!currentChild.activeInHierarchy)
            {
                continue;
            }

            //save the position and rotation of the child
            Vector3 position = currentChild.transform.position;
            Quaternion rotation = currentChild.transform.rotation;
            //reset the position and rotation of the child
            currentChild.transform.position = Vector3.zero;
            currentChild.transform.rotation = Quaternion.identity;

            // Create a new CombineInstance array
            CombineInstance[] combine = new CombineInstance[currentChild.transform.childCount];
            // Loop through each child
            for (int i = 0; i < currentChild.transform.childCount; i++)
            {
                GameObject subChild = currentChild.transform.GetChild(i).gameObject;

                // Check if the sub-child is active before proceeding
                if (!subChild.activeInHierarchy)
                {
                    continue; // Skip this sub-child if it's not active
                }

                // Get the child's mesh filter
                MeshFilter meshFilter = subChild.GetComponent<MeshFilter>();
                // Check if the mesh filter is not null
                if (meshFilter != null)
                {
                    // Set the combine instance's mesh to the child's mesh
                    combine[i].mesh = meshFilter.sharedMesh;
                    // Set the combine instance's transform to the child's transform
                    combine[i].transform = meshFilter.transform.localToWorldMatrix;
                    // Optionally, you could deactivate the child's game object here
                    //subChild.SetActive(false); // Consider your requirements before using
                }
            }

            if (currentChild.transform.childCount > 0)
            {
                meshRenderer2.material = material;
            }
            // Set the mesh filter's mesh to a new mesh
            meshFilter2.mesh = new Mesh();
            // Combine the meshes
            meshFilter2.mesh.CombineMeshes(combine, true, true);


            //reset the position and rotation of the child
            currentChild.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
