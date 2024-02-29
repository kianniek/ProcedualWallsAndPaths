using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LineGenerator : MonoBehaviour
{
    [SerializeField] private float _lineResolution = 1;
    private Vector3 mouseWorldPos;
    // Changed to a list of a list of Vector3 to support multiple disconnected lines.
    [SerializeField] private List<List<Vector3>> lines;
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
        lines = new List<List<Vector3>>();
        // Initialize the value to avoid an anomalous first-frame value
        lastMousePos = Input.mousePosition;
    }

    void FixedUpdate()
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
                        lines.Add(new List<Vector3>() { mouseWorldPos });
                        isDrawingLine = true;
                    }
                    else
                    {
                        // Continue the current line
                        var currentLine = lines[^1];
                        if (Vector3.Distance(currentLine[^1], mouseWorldPos) > _lineResolution)
                        {
                            currentLine.Add(mouseWorldPos);
                        }
                    }
                }
            }
        }
        else if (isDrawingLine)
        {
            // Mouse button released, end current line drawing
            isDrawingLine = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(mouseWorldPos, 0.1f);
        Gizmos.color = Color.yellow;
        if(lines == null) { return; }
        foreach (var line in lines)
        {
            for (int i = 1; i < line.Count; i++)
            {
                if(i - 1 < 0) { continue; }
                Gizmos.DrawLine(line[i], line[i - 1]);
            }
        }
    }
}
