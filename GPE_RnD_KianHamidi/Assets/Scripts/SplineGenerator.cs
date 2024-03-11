using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SplineGenrator : MonoBehaviour
{
    [Serializable]
    public struct Point
    {
        [SerializeField] public Vector3 position;
        [SerializeField] public Vector3 direction;
    }
    [Serializable]
    public struct Line
    {
        [SerializeField] public List<Point> controlPoints;
        [SerializeField] public List<Point> linePoints;
    }
    //Has to be at least 4 points
    public Line line;
    public float lineSampleResolution = 0.01f;
    [Tooltip("The resolution of the spline. The lower the value, the more points the spline will have. /n Make sure it adds up to 1")]
    public float splineResolution = 0.2f;

    //Are we making a line or a loop?
    public bool isLooping = true;
    private bool isDrawingLine;
    Vector3 mouseWorldPos;

    protected virtual void DrawLine()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isDrawingLine)
            {
                // Start a new line
                Line line = new();
                line.controlPoints = new List<Point>();
                Point point = new()
                {
                    position = GetMouseWorldPosition()
                };
                line.controlPoints.Add(point);
                isDrawingLine = true;
            }
            else
            {
                mouseWorldPos = GetMouseWorldPosition();
                // Continue the current line
                if (Vector3.Distance(line.controlPoints[^1].position, mouseWorldPos) > lineSampleResolution)
                {
                    Point point = new()
                    {
                        position = mouseWorldPos
                    };
                    line.controlPoints.Add(point);
                }
            }
        }
        else if (isDrawingLine)
        {
            // Mouse button released, end current line drawing
            isDrawingLine = false;
        }
    }
    protected virtual Vector3 GetMouseWorldPosition()
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
    //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    void DisplayCatmullRomSpline(int pos)
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = this.line.controlPoints[ClampListPos(pos - 1)].position;
        Vector3 p1 = this.line.controlPoints[ClampListPos(pos)].position;
        Vector3 p2 = this.line.controlPoints[ClampListPos(pos + 1)].position;
        Vector3 p3 = this.line.controlPoints[ClampListPos(pos + 2)].position;

        //The start position of the line
        Vector3 lastPos = p1;

        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = splineResolution;

        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            //Add this point to the line
            Point point = new()
            {
                position = newPos,
                direction = (lastPos - newPos).normalized
            };
            this.line.linePoints.Add(point);

            //Draw this line segment
            Gizmos.color = Color.white;
            Gizmos.DrawLine(lastPos, newPos);
            Gizmos.DrawRay(newPos, (point.direction / 2));

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    //Clamp the list positions to allow looping
    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = this.line.controlPoints.Count - 1;
        }

        if (pos > this.line.controlPoints.Count)
        {
            pos = 1;
        }
        else if (pos > this.line.controlPoints.Count - 1)
        {
            pos = 0;
        }

        return pos;
    }

    //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
    //http://www.iquilezles.org/www/articles/minispline/minispline.htm
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }

    //Display without having to press play
    void OnDrawGizmos()
    {
        this.line.linePoints = new List<Point>();
        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < this.line.controlPoints.Count; i++)
        {
            //...if we are not making a looping line
            if ((i == this.line.controlPoints.Count - 1) && !isLooping)
            {
                continue;
            }

            DisplayCatmullRomSpline(i);
        }

    }
}
