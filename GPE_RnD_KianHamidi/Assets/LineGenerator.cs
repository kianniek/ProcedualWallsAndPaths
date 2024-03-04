using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[Serializable]
public struct Line
{
    [SerializeField] public List<Vector3> LinePoints;
}
public class LineGenerator : MonoBehaviour
{
    [SerializeField] private float _lineResolution = 1;
    [SerializeField] bool lineSmoothing = true;
    [SerializeField] int lineSmoothingSegments = 3;
    private Vector3 mouseWorldPos;
    // Changed to a list of a list of Vector3 to support multiple disconnected lines.
    [SerializeField] private List<Line> lines;
    private bool isDrawingLine = false; // Track if we're currently drawing a line.

    Vector3 lastMousePos;
    public Vector3 mouseDelta
    {
        get
        {
            return Input.mousePosition - lastMousePos;
        }
    }
    void Start()
    {
        lines = new();
        // Initialize the value to avoid an anomalous first-frame value
        lastMousePos = Input.mousePosition;
    }

    void FixedUpdate()
    {
        DrawLine();
    }

    private void DrawLine()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    mouseWorldPos = hit.point;
                    if (!isDrawingLine)
                    {
                        // Start a new line
                        Line line = new();
                        line.LinePoints = new List<Vector3>
                        {
                            mouseWorldPos
                        };
                        lines.Add(line);
                        isDrawingLine = true;
                    }
                    else
                    {
                        // Continue the current line
                        var currentLine = lines[^1];
                        if (Vector3.Distance(currentLine.LinePoints[^1], mouseWorldPos) > _lineResolution)
                        {
                            currentLine.LinePoints.Add(mouseWorldPos);
                        }
                    }
                }
            }
        }
        else if (isDrawingLine)
        {
            // Mouse button released, end current line drawing
            isDrawingLine = false;
            var currentLine = lines[^1];
            List<Vector3> smoothendCurve = new List<Vector3>();
            smoothendCurve.AddRange(FitCurve(currentLine.LinePoints, lineSmoothingSegments));
            for (int i = 0; i < currentLine.LinePoints.Count; i++)
            {
                currentLine.LinePoints[i] = smoothendCurve[i];
            }
        }
    }
    /// <summary>
    /// apply curve smooting
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    List<Vector3> FitCurve(List<Vector3> points, int numberOfSegments)
    {
        List<Vector3> curvePoints = new List<Vector3>();
        if (points.Count < 4)
        {
            Debug.LogError("Need at least 4 points to create a Catmull-Rom spline.");
            return curvePoints;
        }

        // The first and last two points are used as control points only, and are not included in the final curve,
        // so we start with the second point and end at the second-to-last point.
        for (int i = 1; i < points.Count - 2; i++)
        {
            Vector3 p0 = points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = points[i + 2];

            Vector3 position = p1;
            // The Catmull-Rom spline formula for t in the range [0, 1].
            for (int j = 0; j <= numberOfSegments; j++)
            {
                float t = j / (float)numberOfSegments;
                float t2 = t * t;
                float t3 = t2 * t;

                position = 0.5f * (2f * p1 +
                                           (-p0 + p2) * t +
                                           (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                                           (-p0 + 3f * p1 - 3f * p2 + p3) * t3);

            }
            curvePoints.Add(position);
        }
        Debug.Log("curvePoints"+curvePoints.Count);
        Debug.Log("points"+points.Count);
        return curvePoints;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(mouseWorldPos, 0.1f);
        Gizmos.color = Color.yellow;
        if (lines == null) { return; }
        foreach (var line in lines)
        {
            for (int i = 1; i < line.LinePoints.Count; i++)
            {
                if (i - 1 < 0) { continue; }
                Gizmos.DrawLine(line.LinePoints[i], line.LinePoints[i - 1]);
            }
        }
    }
}
