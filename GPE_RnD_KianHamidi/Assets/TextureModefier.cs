using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class TextureModifier : MonoBehaviour
{
    public PathwayManager pathwayManager;
    public DataTextureCreator dataTextureCreator;
    public int textureSize;
    public float textureSizeOffset;
    public Texture2D dataTexture;
    public Renderer ground;
    public Vector3 textureRotation;

    [Range(1, 10)]
    public int brushSize = 1;

    void Start()
    {
        pathwayManager = GetComponent<PathwayManager>();
        textureSize = dataTextureCreator.textureSize;
        if (ground == null)
        {
            Debug.LogError("Ground renderer is not assigned.");
            return;
        }

        //// Create a new texture
        //dataTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        //ClearTexture();

        // Initialize texture as _MaskingTexture
        ground.material.SetTexture("_MaskingTexture", dataTexture);

        // Call the method to draw the square once at the start
        //dataTexture.Apply();
    }

    void Update()
    {
        if (pathwayManager.GetSplines() == null)
        {
            Debug.LogError("SplineGenerator component not found on the first child.");
            return;
        }
        if (transform.childCount > 0 && pathwayManager.GetSplines() == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<SplineGenerator>() != null && !pathwayManager.GetSplines().Contains(transform.GetChild(i).GetComponent<SplineGenerator>()))
                {
                    pathwayManager.GetSplines().Add(transform.GetChild(i).GetComponent<SplineGenerator>());
                }
            }

        }
        // Optionally clear texture each frame to update points.
        // ClearTexture();

        DrawSplinePointsOnTexture();
        dataTexture.Apply();
    }

    void DrawSplinePointsOnTexture()
    {
        ClearTexture(); // Clear the texture before drawing new points.
        foreach (var spline in pathwayManager.GetSplines())
        {
            if (spline != null && spline.line.linePoints != null)
            {
                foreach (var point in spline.line.linePoints)
                {
                    DrawSquareAtWorldCoordinates(new Vector2(point.position.x, point.position.z), Color.red);
                }
            }
        }
        BlurTexture(dataTexture, 1); // Blur the texture after all points are drawn.
        dataTexture.Apply(); // Apply changes after all squares are drawn.
    }

    Vector2 CalculateTextureCoordinate(Vector3 worldPosition, float tiling, Vector2 offset)
    {
        // Normalize world position to [0,1] for a 1x1 plane
        Vector2 normalizedPosition = new Vector2(worldPosition.x, worldPosition.y);

        // Apply tiling and offset
        Vector2 tiledPosition = normalizedPosition * tiling + offset;

        return tiledPosition;
    }

    void DrawSquareAtWorldCoordinates(Vector2 worldPosition, Color color)
    {
        // Convert center world coordinates to texture coordinates
        Vector2 uv = CalculateTextureCoordinate(worldPosition, 0.1f, new Vector2(0.5f, 0.5f));

        uv = Quaternion.Euler(textureRotation) * uv;
        // Convert UV to pixel coordinates
        int x = Mathf.RoundToInt(uv.x * dataTexture.width);
        int y = Mathf.RoundToInt(uv.y * dataTexture.height);

        DrawCircle(dataTexture, x, y, brushSize, color);
        // Moved dataTexture.Apply() to be called after all drawing operations in the calling method.
    }

    public void ClearTexture()
    {
        if (textureSize != dataTexture.width)
        {

            Debug.Log($"Change in texture size detected form {dataTexture.width} to {textureSize}, Created new texture. Please reapply the texture to places it was used");
        }
        Color clearColor = Color.black; // Or any color representing the "empty" state
        Color[] clearColors = new Color[textureSize * textureSize];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = clearColor;
        }
        dataTexture.SetPixels(clearColors);
        dataTexture.Apply(); // Ensure changes are applied.
    }

    //fuctions that takes in a texture and sets pixels in a circle x and y are the center of the circle and takes in a color
    public void DrawCircle(Texture2D tex, int x, int y, int radius, Color color)
    {
        int diameter = radius * 2;

        for (int i = -diameter; i < diameter; i++)
        {
            for (int j = -diameter; j < diameter; j++)
            {

                float distance = Mathf.Sqrt((i - radius) * (i - radius) + (j - radius) * (j - radius));

                if (distance < radius)
                {
                    tex.SetPixel(x + i - radius, y + j - radius, color);
                }
                else
                {
                    //tex.SetPixel(i * x + j, i * y + j, color / distance);
                }
            }
        }
    }

    //blur the texture
    public void BlurTexture(Texture2D tex, int radius)
    {
        Color[] original = tex.GetPixels();
        Color[] blurred = new Color[original.Length];

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                Color sum = Color.black;
                int count = 0;

                for (int k = -radius; k <= radius; k++)
                {
                    for (int l = -radius; l <= radius; l++)
                    {
                        int x = i + k;
                        int y = j + l;

                        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                        {
                            sum += original[y * tex.width + x];
                            count++;
                        }
                    }
                }

                blurred[j * tex.width + i] = sum / count;
            }
        }

        tex.SetPixels(blurred);
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
