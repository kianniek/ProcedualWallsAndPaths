using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class DataTextureCreator : MonoBehaviour
{
    public int width = 256; // Number of data points
    public Vector3[] positions; // Your positions array
    public Material material; // Material to assign the texture to
    public Texture2D dataTexture;
    void Start()
    {
        DataToTexture();
    }

    public void DataToTexture()
    {
        dataTexture = new Texture2D(width, 1, TextureFormat.RGBAFloat, false, true);
        dataTexture.filterMode = FilterMode.Point;

        // Loop through each position and set pixels
        for (int i = 0; i < positions.Length && i < width; i++)
        {
            // Normalize your position data if necessary
            Vector3 normalizedPosition = NormalizePosition(positions[i]);

            // Pack the position into the RGBA channels, using RGB for the position and A for an optional value or weight
            Color dataColor = new Color(normalizedPosition.x, normalizedPosition.y, normalizedPosition.z, 1.0f); // 1.0f or any additional data

            dataTexture.SetPixel(i, 0, dataColor);
        }

        dataTexture.Apply();

        SaveTextureToResources();

        // Use the texture in your material, assign it to a shader, or save it
        material.SetTexture("_DataTex", dataTexture);
    }

    Vector3 NormalizePosition(Vector3 position)
    {
        // Normalize your position based on your specific data range
        // Example: Assuming position ranges from -10 to 10 in each axis
        return (position + new Vector3(10, 10, 10)) / 20.0f;
    }

    public void SaveTextureToResources()
    {
        // Ensure the Resources folder exists
        string resourcesPath = "Assets/Resources";
        if (!System.IO.Directory.Exists(resourcesPath))
        {
            System.IO.Directory.CreateDirectory(resourcesPath);
            AssetDatabase.Refresh();
        }

        // Save the texture to the Resources folder
        string path = System.IO.Path.Combine(resourcesPath, "DataTexture.asset");
        AssetDatabase.CreateAsset(dataTexture, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Texture saved to " + path);
    }
}
