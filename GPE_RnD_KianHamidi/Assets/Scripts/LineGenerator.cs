using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[Serializable]
public struct Line
{
    [SerializeField] public List<Vector3> LinePoints;
    [SerializeField] public List<Vector3> LineDirection;
}
public class LineGenerator : MonoBehaviour
{
    [SerializeField] bool mouseStabelizing = true;
    [SerializeField] private int smoothingLevel = 5; // Number of positions to average
    private Queue<Vector3> positionsQueue = new Queue<Vector3>();
    private Vector3 mouseWorldPos;

    [SerializeField] float lineResolution = 1;
    // Changed to a list of a list of Vector3 to support multiple disconnected lines.
    [SerializeField] private List<Line> lines;

    WallGeneration wallGeneration;
    public List<Line> Lines
    {
        get { return lines; }
        set { lines = value; }
    }
    private bool isDrawingLine = false; // Track if we're currently drawing a line.


    Vector3 lastMousePos;
    public Vector3 WorldMouseDelta
    {
        get
        {
            return GetMouseWorldPosition() - lastMousePos;
        }
    }
    void Start()
    {
        lines = new();
        // Initialize the value to avoid an anomalous first-frame value

        wallGeneration = GetComponent<WallGeneration>();
    }

    void FixedUpdate()
    {
        DrawLine();
    }

    private void DrawLine()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isDrawingLine)
            {
                // Start a new line
                Line line = new();
                line.LinePoints = new List<Vector3>
                        {
                            GetMouseWorldPosition()
                        };
                line.LineDirection = new List<Vector3>
                {
                    WorldMouseDelta
                };
                lines.Add(line);
                EnqueueMousePosition(mouseWorldPos);
                isDrawingLine = true;
            }
            else
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                EnqueueMousePosition(mousePosition);
                Vector3 smoothedPosition = CalculateSmoothedPosition();
                // Continue the current line
                var currentLine = lines[^1];
                if (Vector3.Distance(currentLine.LinePoints[^1], mouseWorldPos) > lineResolution)
                {
                    currentLine.LinePoints.Add(smoothedPosition);
                    currentLine.LineDirection.Add(WorldMouseDelta.normalized);
                    wallGeneration.GenerateWall();
                }
            }
            lastMousePos = GetMouseWorldPosition();
        }
        else if (isDrawingLine)
        {
            // Mouse button released, end current line drawing
            isDrawingLine = false;
            ClearMousePositionQueue();

        }
    }
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null)
            {
                mouseWorldPos = hit.point;
            }
        }
        return mouseWorldPos;
    }

    void EnqueueMousePosition(Vector3 position)
    {
        if (positionsQueue.Count >= smoothingLevel)
        {
            positionsQueue.Dequeue(); // Remove the oldest position
        }
        positionsQueue.Enqueue(position); // Add the new position
    }

    void ClearMousePositionQueue()
    {
        if (positionsQueue.Count == 0) { return; }
        positionsQueue.Clear();
    }

    Vector3 CalculateSmoothedPosition()
    {
        if (mouseStabelizing) { return GetMouseWorldPosition(); }
        Vector3 sum = Vector3.zero;
        foreach (Vector3 pos in positionsQueue)
        {
            sum += pos;
        }
        return sum / positionsQueue.Count; // Return the average position
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(mouseWorldPos, 0.01f);
        Gizmos.color = Color.yellow;
        if (lines == null) { return; }
        foreach (var line in lines)
        {
            for (int i = 1; i < line.LinePoints.Count; i++)
            {
                if (i - 1 < 0) { continue; }
                Gizmos.DrawLine(line.LinePoints[i], line.LinePoints[i - 1]);
                Gizmos.DrawSphere(line.LinePoints[i], 0.1f);
            }
        }


    }
}
