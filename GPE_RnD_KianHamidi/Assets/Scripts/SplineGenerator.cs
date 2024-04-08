using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SplineGenerator : MonoBehaviour
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
    private List<Point> previousLinePoints;
    public float lineSampleResolution = 0.01f;
    [Tooltip("The resolution of the spline. The lower the value, the more points the spline will have. /n Make sure it adds up to 1")]
    public float splineResolution = 0.2f;
    Vector3 mouseWorldPos;
    [SerializeField] protected LayerMask groundLayer;

    [SerializeField] private float intersectionBuffer = 1f;

    //distance we need to be to an existing point to start editing the line
    public float editDistance = 0.5f;

    //Are we making a line or a loop?
    public bool isLooping = true;
    bool wasLooping = true;

    //Are we currently drawing a line?
    private bool isDrawingLine;

    //Are we currently editing a line?
    [SerializeField] private bool isDrawn;
    [SerializeField] private bool isEditingLine;

    //The WallGenerationFloodfill script
    WallGenerationFloodfill wallGenerationFloodfill;


    private void Start()
    {
        TryGetComponent(out wallGenerationFloodfill);
        previousLinePoints = new();
        wasLooping = isLooping;
        CarveOutManager.Instance.carveOutWall.FetchUpdate();
        CarveOutManager.Instance.carveOutWall.CarveOut();

    }

    private void Update()
    {
        DrawLine();

        if (wallGenerationFloodfill != null)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                wallGenerationFloodfill.GenerateOnLine(line);
            }

            if (isLooping != wasLooping)
            {
                wasLooping = isLooping;
                wallGenerationFloodfill.GenerateOnLine(line);
            }
        }

    }

    private void FixedUpdate()
    {
        line.linePoints = new List<Point>();


        //Draw the Catmull-Rom spline between the points
        for (int i = 1; i < line.controlPoints.Count - 1; i++)
        {
            //...if we are not making a looping line
            if ((i == line.controlPoints.Count - 1) && !isLooping)
            {
                continue;
            }

            CalculateCatmullRomSpline(i);
        }

        // Example condition to check changes
        if (HaveLinePointsChanged())
        {
            Debug.Log("Line points have changed.");
            CarveOutManager.Instance.carveOutWall.CarveOut();
            UpdatePreviousLinePoints();
        }
    }

    public virtual void DrawLine()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseWorldPos = GetMouseWorldPosition();

            CheckIfEditLine(mouseWorldPos);
        }

        if (Input.GetMouseButton(0))
        {
            mouseWorldPos = GetMouseWorldPosition();

            if (!isDrawn)
            {
                if (!isDrawingLine) // Start a new line
                {
                    print("Drawing Line");
                    line = new()
                    {
                        controlPoints = new List<Point>()
                    };
                    Point point = new()
                    {
                        position = mouseWorldPos

                    };
                    line.controlPoints.Add(point);
                    isDrawingLine = true;
                }
                else
                {
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
            else if (isEditingLine)
            {
                if (!isDrawingLine) // Start a new line
                {
                    // Continue the current line from the point closest to the mouse
                    // Remove all points after the point closest to the mouse
                    int closestPointIndex = 0;
                    float closestDistance = float.MaxValue;
                    for (int j = 0; j < line.controlPoints.Count; j++)
                    {
                        float distance = Vector3.Distance(line.controlPoints[j].position, mouseWorldPos);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestPointIndex = j;
                        }
                    }
                    Debug.Log(closestPointIndex);
                    line.controlPoints.RemoveRange(closestPointIndex + 1, line.controlPoints.Count - closestPointIndex - 1);

                    isDrawingLine = true;
                }
                else
                {
                    // Continue the current line
                    if (Vector3.Distance(line.controlPoints[^1].position, mouseWorldPos) > lineSampleResolution)
                    {
                        Point point = new()
                        {
                            position = mouseWorldPos
                        };
                        line.controlPoints.Add(point);
                        CarveOutManager.Instance.carveOutWall.CarveOut();
                    }
                }
            }
        }
        else if (isDrawingLine)
        {
            // Mouse button released, end current line drawing
            isDrawingLine = false;
            isEditingLine = false;
            isDrawn = true;

            if (wallGenerationFloodfill != null)
            {
                wallGenerationFloodfill.GenerateOnLine(line);
            }

            //update the carve out wall manager
            CarveOutManager.Instance.carveOutWall.FetchUpdate();
            CarveOutManager.Instance.carveOutWall.CarveOut();
        }
    }

    //check if we are starting to draw a line very close to one of th points on this existing line
    public virtual void CheckIfEditLine(Vector3 mousePos)
    {
        if (line.controlPoints.Count == 0) { return; }
        if (isLooping) { isEditingLine = false; return; }

        //only check if the first and last point are close enough to be edited
        if (Vector3.Distance(line.controlPoints[0].position, mousePos) < editDistance)
        {
            isEditingLine = true;
            return;
        }
        if (Vector3.Distance(line.controlPoints[^1].position, mousePos) < editDistance)
        {
            isEditingLine = true;
            return;
        }

    }

    public virtual Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (hit.collider != null)
            {
                mouseWorldPos = hit.point;
            }
        }
        return mouseWorldPos;
    }

    //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    public virtual void CalculateCatmullRomSpline(int controlPointIndex)
    {
        if (controlPointIndex - 1 < 0)
        {
            Point point = new()
            {
                position = line.controlPoints[controlPointIndex].position,
                //get the right direction for the line using 
                direction = Vector3.Cross(Vector3.up, (line.controlPoints[controlPointIndex + 1].position - line.controlPoints[controlPointIndex].position).normalized)
            };
            line.linePoints.Add(point);
        }


        //The 4 points we need to form a spline between p1 and p2 
        Vector3 p0 = line.controlPoints[ClampListPos(controlPointIndex - 1)].position;
        Vector3 p1 = line.controlPoints[ClampListPos(controlPointIndex)].position;
        Vector3 p2 = line.controlPoints[ClampListPos(controlPointIndex + 1)].position;
        Vector3 p3 = line.controlPoints[ClampListPos(controlPointIndex + 2)].position;

        //The start position of the line
        Vector3 lastPos = p1;

        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = splineResolution;

        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        //because we start at i=1, we add the first point at p1
        Point firstPoint = new()
        {
            position = lastPos,
            direction = Vector3.zero
        };
        line.linePoints.Add(firstPoint);
        //Loop through each segment of the line
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
            line.linePoints.Add(point);

            Debug.DrawLine(lastPos, newPos);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    public virtual Vector3 GetPointAlongSpline(float t)
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
    public virtual int ClampListPos(int controlPointIndex)
    {
        if (controlPointIndex < 0)
        {
            controlPointIndex = line.controlPoints.Count - 1;
        }

        if (controlPointIndex > line.controlPoints.Count)
        {
            controlPointIndex = 1;
        }
        else if (controlPointIndex > line.controlPoints.Count - 1)
        {
            controlPointIndex = 0;
        }

        return controlPointIndex;
    }

    //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
    //https://iquilezles.org/articles/minispline/
    public virtual Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
    void OnDrawGizmosSelected()
    {
        if (line.controlPoints == null || line.linePoints == null) { return; }
        if (line.controlPoints.Count == 0 || line.linePoints.Count == 0) { return; }

        line.linePoints = new List<Point>();

        //Draw the Catmull-Rom spline between the points
        for (int i = 1; i < line.controlPoints.Count - 1; i++)
        {
            //...if we are not making a looping line
            if ((i == line.controlPoints.Count - 1) && !isLooping)
            {
                continue;
            }

            CalculateCatmullRomSpline(i);
        }

        //draw line between the line points
        for (int i = 0; i < line.linePoints.Count; i++)
        {
            //Draw this line segment
            Gizmos.color = Color.cyan;
            //Calculate the right diretionusing the cross product of the up and forward vectors
            Gizmos.DrawRay(line.linePoints[i].position, line.linePoints[i].direction);
        }
        //Calculate the right diretionusing the cross product of the up and forward vectors
        Gizmos.DrawRay(line.linePoints[^1].position, line.linePoints[^1].direction);

        //Draw the control points
        for (int i = 0; i < line.controlPoints.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(line.controlPoints[i].position, 0.1f);
        }

    }

    private bool LineSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection, float buffer = 0f)
    {
        intersection = Vector3.zero;

        float d = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);
        if (Math.Abs(d) <= buffer) // Adjusting for buffer in parallel check
        {
            // Considering a simple approach to check for "nearness" rather than exact intersection
            float distance12_34 = Math.Min(Vector3.Distance(p1, p3), Vector3.Distance(p1, p4));
            distance12_34 = Math.Min(distance12_34, Vector3.Distance(p2, p3));
            distance12_34 = Math.Min(distance12_34, Vector3.Distance(p2, p4));

            if (distance12_34 <= buffer)
            {
                // Find a point that is somewhat central to all points involved for simplicity
                intersection = (p1 + p2 + p3 + p4) / 4;
                return true;
            }
            return false; // Parallel and not within buffer
        }

        float u = ((p3.x - p1.x) * (p4.z - p3.z) - (p3.z - p1.z) * (p4.x - p3.x)) / d;
        float v = ((p3.x - p1.x) * (p2.z - p1.z) - (p3.z - p1.z) * (p2.x - p1.x)) / d;

        // Adjusting the intersection check to consider the buffer
        if (u >= -buffer && u <= 1.0f + buffer && v >= -buffer && v <= 1.0f + buffer)
        {
            intersection = p1 + u * (p2 - p1);
            return true;
        }

        return false;
    }


    public bool CheckSplinesIntersection(Line spline1, Line spline2, out List<Vector3> intersections)
    {
        intersections = new List<Vector3>();
        bool foundIntersection = false;

        for (int i = 0; i < spline1.linePoints.Count - 1; i++)
        {
            for (int j = 0; j < spline2.linePoints.Count - 1; j++)
            {
                if (LineSegmentsIntersect(spline1.linePoints[i].position, spline1.linePoints[i + 1].position,
                                          spline2.linePoints[j].position, spline2.linePoints[j + 1].position,
                                          out Vector3 intersection, intersectionBuffer))
                {
                    intersections.Add(intersection);
                    foundIntersection = true;
                }
            }
        }
        return foundIntersection;
    }

    // Call this method to update previousLinePoints to the current state
    public void UpdatePreviousLinePoints()
    {
        previousLinePoints = new List<Point>(line.linePoints);
    }

    // This function checks if the line points have changed
    public bool HaveLinePointsChanged()
    {
        if (previousLinePoints.Count != line.linePoints.Count)
        {
            return true; // Different number of points
        }

        for (int i = 0; i < line.linePoints.Count; i++)
        {
            // Here, you can compare the positions (and directions if necessary) of each point
            // This is a simple position comparison; extend it as needed for your use case
            if (previousLinePoints[i].position != line.linePoints[i].position)
            {
                return true; // Found a difference in points
            }
        }

        return false; // No changes found
    }


    public void SplitBezierCurveAtT(float t, out List<Point> leftCurve, out List<Point> rightCurve)
    {
        // Initialize the output lists
        leftCurve = new List<Point>();
        rightCurve = new List<Point>();

        // Assuming `line.controlPoints` contains the control points of the Bezier curve to be split
        // Check to ensure there are enough control points for a cubic Bezier curve
        if (line.controlPoints.Count != 4)
        {
            Debug.LogError("This function currently supports splitting cubic Bezier curves only.");
            return;
        }

        // The first point of the left curve is the first control point of the original curve
        leftCurve.Add(line.controlPoints[0]);

        // The last point of the right curve is the last control point of the original curve
        rightCurve.Add(line.controlPoints[line.controlPoints.Count - 1]);

        // Intermediate lists to hold the points at each iteration
        List<Vector3> currentPoints = line.controlPoints.Select(p => p.position).ToList();
        List<Vector3> nextPoints = new List<Vector3>();

        // Iteratively apply De Casteljau's algorithm
        for (int i = 0; i < line.controlPoints.Count; i++)
        {
            nextPoints.Clear();

            // Calculate intermediate points
            for (int j = 0; j < currentPoints.Count - 1; j++)
            {
                Vector3 intermediatePoint = Vector3.Lerp(currentPoints[j], currentPoints[j + 1], t);
                nextPoints.Add(intermediatePoint);

                // The first iteration defines the second control point of the left curve
                // and the second-to-last control point of the right curve
                if (i == 0)
                {
                    leftCurve.Add(new Point { position = intermediatePoint });
                }
                else if (i == line.controlPoints.Count - 2)
                {
                    rightCurve.Insert(0, new Point { position = intermediatePoint });
                }
            }

            // Prepare for the next iteration
            currentPoints = new List<Vector3>(nextPoints);

            // The last point of iteration is the split point, added to both curves
            if (i == line.controlPoints.Count - 2)
            {
                leftCurve.Add(new Point { position = currentPoints[0] });
                rightCurve.Insert(0, new Point { position = currentPoints[0] });
            }
        }

        // The right curve control points are added in reverse order; reverse them to maintain order
        rightCurve.Reverse();
    }


    public bool IsEditing()
    {
        CheckIfEditLine(GetMouseWorldPosition());
        return isEditingLine;
    }
}
