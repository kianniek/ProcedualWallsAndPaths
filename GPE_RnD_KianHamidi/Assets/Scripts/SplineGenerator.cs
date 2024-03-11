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
    WallGenerationFloodfill wallGenerationFloodfill;
    private void Start()
    {
        wallGenerationFloodfill = GetComponent<WallGenerationFloodfill>();
        //this.line.linePoints = new List<Point>();

    }

    private void Update()
    {
        DrawLine();

        if (Input.GetKey(KeyCode.Return))
        {
            wallGenerationFloodfill.GenerateOnLine(line);
        }
    }

    private void FixedUpdate()
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

            CalculateCatmullRomSpline(i);
        }
    }
    protected void DrawLine()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isDrawingLine)
            {
                // Start a new line
                print("Drawing Line");
                line = new()
                {
                    controlPoints = new List<Point>()
                };
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
            wallGenerationFloodfill.GenerateOnLine(line);
            print("Stopped Drawing Line");

        }
    }

    protected Vector3 GetMouseWorldPosition()
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
    void CalculateCatmullRomSpline(int pos)
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

            Vector3 right = Vector3.Cross(Vector3.up, (newPos - lastPos).normalized);

            //Add this point to the line
            Point point = new()
            {
                position = newPos,
                //get the right direction for the line using 
                direction = right
            };
            this.line.linePoints.Add(point);



            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    public Vector3 GetPointAlongSpline(float t)
    {
        // Ensure t is within bounds.
        t = Mathf.Clamp01(t);

        // Calculate the appropriate segment of the spline t falls into.
        int totalPoints = line.controlPoints.Count;
        float perSegmentT = 1f / (totalPoints - (isLooping ? 0 : 3));
        int segmentIndex = Mathf.Min(Mathf.FloorToInt(t / perSegmentT), totalPoints - (isLooping ? 1 : 4));

        // Calculate the local t for the specific segment.
        float localT = (t % perSegmentT) / perSegmentT;

        // Adjust index for looping if necessary.
        if (isLooping && segmentIndex >= totalPoints - 3)
        {
            segmentIndex = totalPoints - 3; // Wrap to the start for looping.
        }

        // Use the Catmull-Rom formula to find the exact position along the spline.
        Vector3 p0 = line.controlPoints[ClampListPos(segmentIndex - 1)].position;
        Vector3 p1 = line.controlPoints[ClampListPos(segmentIndex)].position;
        Vector3 p2 = line.controlPoints[ClampListPos(segmentIndex + 1)].position;
        Vector3 p3 = line.controlPoints[ClampListPos(segmentIndex + 2)].position;

        return GetCatmullRomPosition(localT, p0, p1, p2, p3);
    }

    // Make sure ClampListPos and GetCatmullRomPosition methods are accessible (public or internal if needed).


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
    //https://iquilezles.org/articles/minispline/
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
        FixedUpdate();
        //draw line between the line points
        for (int i = 0; i < this.line.linePoints.Count; i++)
        {
            if (i < this.line.linePoints.Count - 1)
            {
                //Draw the line
                Gizmos.color = Color.white;
                Gizmos.DrawLine(this.line.linePoints[i].position, this.line.linePoints[i + 1].position);
            }
            //Draw this line segment
            Gizmos.color = Color.cyan;
            //Calculate the right diretionusing the cross product of the up and forward vectors
            Gizmos.DrawRay(this.line.linePoints[i].position, this.line.linePoints[i].direction);
        }

        //Draw the control points
        for (int i = 0; i < this.line.controlPoints.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(this.line.controlPoints[i].position, 0.1f);
        }

    }
}
