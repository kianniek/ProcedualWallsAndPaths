using UnityEditor;
using UnityEngine;

public class TextureModifier : MonoBehaviour
{
    // Assuming the correct class name is SplineGenerator.
    public SplineGenerator splineGenerator;
    public DataTextureCreator dataTextureCreator;
    public int textureSize;
    private Texture2D dataTexture;
    public Renderer ground;

    public Vector2 debugDraw;

    void Start()
    {
        textureSize = dataTextureCreator.textureSize;
        if (ground == null)
        {
            Debug.LogError("Ground renderer is not assigned.");
            return;
        }

        // Create a new texture
        dataTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        ClearTexture();

        // Initialize texture as _MaskingTexture
        ground.material.SetTexture("_MaskingTexture", dataTexture);

        // Call the method to draw the square once at the start
        //DrawSquareAtWorldCoordinates(debugDraw, 10, 10, Color.red);
        //dataTexture.Apply();
    }

    void Update()
    {
        if (transform.childCount > 0 && splineGenerator == null)
        {
            // Update the splineGenerator reference only if it hasn't been set yet.
            splineGenerator = transform.GetChild(0).GetComponent<SplineGenerator>();
            if (splineGenerator == null)
            {
                Debug.LogError("SplineGenerator component not found on the first child.");
                return;
            }
        }

        if (splineGenerator != null)
        {
            // Optionally clear texture each frame to update points.
            // ClearTexture();

            DrawSplinePointsOnTexture();
            dataTexture.Apply();
        }
    }
    void DrawSplinePointsOnTexture()
    {
        if (splineGenerator != null && splineGenerator.line.linePoints != null)
        {
            foreach (var point in splineGenerator.line.linePoints)
            {
                DrawSquareAtWorldCoordinates(new Vector2(point.position.x, point.position.z), 5, 5, Color.red);
            }
            dataTexture.Apply(); // Apply changes after all squares are drawn.
        }
    }

    Vector2 WorldToTextureCoord(Vector3 worldCoord)
    {
        if (ground == null)
        {
            Debug.LogError("Ground object is not set.");
            return Vector2.zero;
        }

        // Convert the world coordinate to the ground object's local space.
        Vector3 localCoord = ground.transform.InverseTransformPoint(worldCoord);
        Debug.Log($"Local Coord: {localCoord}");
        // Now localCoord is relative to the ground object's position and rotation.
        // Since the ground is considered flat and aligned with the XZ plane, we ignore the local y-coordinate.

        Bounds bounds = new Bounds(Vector3.zero, ground.transform.localScale);

        // Normalize the coordinates to a [0, 1] range based on the ground's size.
        // The bounds are centered at zero, so we adjust by adding 0.5 to the normalized value to shift the range from [-0.5, 0.5] to [0, 1].
        float normalizedX = (localCoord.x / bounds.size.x) + 0.5f;
        float normalizedZ = (localCoord.z / bounds.size.z) + 0.5f;

        // Use the normalized coordinates to get the texture coordinates, clamping to ensure they stay within bounds.
        int textureX = Mathf.Clamp((int)(normalizedX * textureSize), 0, textureSize - 1);
        int textureY = Mathf.Clamp((int)(normalizedZ * textureSize), 0, textureSize - 1);
        Debug.Log($"Texture X: {textureX}, Texture Y: {textureY}");
        return new Vector2(textureX, textureY);
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

    void DrawSquareAtWorldCoordinates(Vector2 center, float width, float height, Color color)
    {
        // Convert center world coordinates to texture coordinates
        Vector2 textureCenter = WorldToTextureCoord(new Vector3(center.x, 0, center.y));
        Debug.Log($"Texture Center: {textureCenter}");

        // Calculate start and end points in texture coordinates
        int startX = Mathf.Clamp((int)(textureCenter.x - (width / 2)), 0, textureSize - 1);
        int startY = Mathf.Clamp((int)(textureCenter.y - (height / 2)), 0, textureSize - 1);
        int endX = Mathf.Clamp((int)(textureCenter.x + (width / 2)), 0, textureSize - 1);
        int endY = Mathf.Clamp((int)(textureCenter.y + (height / 2)), 0, textureSize - 1);

        // Fill in the square on the texture
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startY; j <= endY; j++)
            {
                dataTexture.SetPixel(i, j, color);
            }
        }
        // Moved texture.Apply() to be called after all drawing operations in the calling method.
    }
}
