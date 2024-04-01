using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataTextureCreator))]
public class DataTextureCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector

        DataTextureCreator script = (DataTextureCreator)target;

        if (GUILayout.Button("Run Start Function"))
        {
            script.DataToTexture(); // Call the Start method manually
        }
    }
}
