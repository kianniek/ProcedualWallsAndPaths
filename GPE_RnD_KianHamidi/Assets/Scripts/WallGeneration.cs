using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(LineGenerator))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class WallGeneration : MonoBehaviour
{
    LineGenerator lineGenerator;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    [SerializeField]
    float wallHeight = 2f;
    [SerializeField]
    float wallDepth = 0.5f;

    [SerializeField]
    float wallPartitionResulution = 1;

    [SerializeField]
    List<Vector3> gridPoints;

    private void Awake()
    {
        lineGenerator = GetComponent<LineGenerator>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        gridPoints = new List<Vector3>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GenerateWall();
    }

    void GenerateWall()
    {
        //Use Flood fill algoritm to fill in gaps
        //Make a grid out off the wall segments
        gridPoints = new List<Vector3>();
        foreach (var line in lineGenerator.Lines)
        {
            for (int i = 1; i < line.LinePoints.Count; i++)
            {
                if (i - 1 < 0) { continue; }
                SegmentPartition(line.LinePoints[i], line.LinePoints[i - 1], wallPartitionResulution, wallHeight);
            }
        }
        
        //A segment is a plane extruted from 2 line points
    }

    void SegmentPartition(Vector3 p0, Vector3 p1, float resolution, float wallHeight)
    {
        // Calculate the wall base vector and height
        Vector3 baseVector = p1 - p0;
        float wallLength = baseVector.magnitude;
        Vector3 wallDirection = baseVector.normalized;
        Vector3 wallUp = Vector3.up * wallHeight;

        // Determine the number of divisions based on resolution
        int divisions_x = Mathf.FloorToInt(wallLength * resolution);
        int divisions_y = Mathf.FloorToInt(wallHeight * resolution);

        Debug.Log($"Divisions: x= {divisions_x} y= {divisions_y}");

        for (int y = 0; y <= divisions_y; y++)
        {
            for (int x = 0; x <= divisions_x; x++)
            {
                // Calculate interpolation factors for the current indices
                float factor_x = (float)x / divisions_x;
                float factor_y = (float)y / divisions_y;

                // Interpolate base positions along the wall length
                Vector3 basePoint = Vector3.Lerp(p0, p1, factor_x);

                // Interpolate vertically based on wall height
                Vector3 finalPoint = basePoint + (wallUp * factor_y);

                gridPoints.Add(finalPoint);

                // For debugging: visualize the grid point
                //Debug.DrawLine(finalPoint, finalPoint + Vector3.up * 0.1f, Color.red, 2f);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (lineGenerator == null) { return; }
        if (lineGenerator.Lines == null) { return; }
        foreach (var line in lineGenerator.Lines)
        {
            Gizmos.DrawLine(line.LinePoints[0], line.LinePoints[0] + Vector3.up * wallHeight);
            for (int i = 1; i < line.LinePoints.Count; i++)
            {
                if (i - 1 < 0) { continue; }
                Gizmos.DrawLine(line.LinePoints[i] + Vector3.up * wallHeight, line.LinePoints[i - 1] + Vector3.up * wallHeight);
                Gizmos.DrawSphere(line.LinePoints[i], 0.1f);
            }
            Gizmos.DrawLine(line.LinePoints[^1], line.LinePoints[^1] + Vector3.up * wallHeight);
        }

        foreach (var item in gridPoints)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(item, 0.1f);
        }
    }
}
