using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WallGenerationFloodfill : MonoBehaviour
{
    [Header("Optimalisation")]
    [Space(10)]

    [SerializeField] bool combineMeshes = false;
    [Space(10)]

    [Header("Wall Generation")]
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallDepth = 0.1f;
    [SerializeField][Range(0, 1)] private float brickDepthDiviation = 0.1f;
    [SerializeField] private Vector2Int minBrickSize, maxBrickSize;
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private float resolution = 8;
    [SerializeField] private Vector2 rotationDiviation = new(1, 1);
    private bool[,] isPositionOccupied;

    MeshCombinerRuntime meshCombinerRuntime;
    SplineGenrator splineGenrator;

    [Space(10)]
    [Header("Debugging")]
    [SerializeField] private bool debug = false;
    private void Start()
    {
        meshCombinerRuntime = GetComponent<MeshCombinerRuntime>();
        splineGenrator = GetComponent<SplineGenrator>();
        ReGenerate();
    }

    private void Update()
    {

    }

    public void ReGenerate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void GenerateOnLine(SplineGenrator.Line line)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (debug)
        {
            //use a coroutine to generate the wall
            StartCoroutine(GenerateWallCoroutine(line, wallHeight));
        }
        else
        {
            for (int i = 0; i < line.linePoints.Count - 1; i++)
            {
                GenerateWall(line, i, wallHeight);
            }
        }
    }

    IEnumerator GenerateWallCoroutine(SplineGenrator.Line line, float wallHeight)
    {
        for (int i = 0; i < line.linePoints.Count - 1; i++)
        {
            yield return new WaitForSeconds(0.1f);
            GenerateWall(line, i, wallHeight);
        }
    }

    public void GenerateWall(SplineGenrator.Line line, int index, float wallHeight)
    {
        Vector3 bottomLeft = line.linePoints[index].position;
        Vector3 bottomRight = line.linePoints[index + 1].position;
        float wallWidth = Vector3.Distance(bottomLeft, bottomRight);

        //get the up, right and forward vectors of the wall
        Vector3 wallUp = Vector3.up;
        Vector3 wallRight = line.linePoints[index + 1].direction;
        Vector3 wallForward = (bottomRight - bottomLeft).normalized;

        //draw the wall vectors in the scene view
        if (debug)
        {
            Debug.DrawLine(bottomLeft, bottomLeft + wallRight, Color.red, 3);
            Debug.DrawLine(bottomLeft, bottomLeft + wallUp, Color.green, 3);
            Debug.DrawLine(bottomLeft, bottomLeft + wallForward, Color.blue, 3);
        }

        int numberOfBricksAcross = Mathf.FloorToInt(wallWidth / minBrickSize.x * resolution);
        int numberOfBricksHigh = Mathf.FloorToInt(wallHeight / minBrickSize.y * resolution);
        isPositionOccupied = new bool[numberOfBricksAcross, numberOfBricksHigh];

        //make a wallsegemnt gameobject
        GameObject wallSegment = new("WallSegment");
        wallSegment.transform.parent = transform;

        //fill the wall area with bricks using a fitting algorithm
        FillWallArea(wallSegment, line, index, wallWidth, bottomLeft, wallUp, wallRight, wallForward, numberOfBricksAcross, numberOfBricksHigh, ref isPositionOccupied);

        if (!combineMeshes) { return; }
        meshCombinerRuntime.CombineMeshes();
    }

    private void FillWallArea(GameObject wallSegment, SplineGenrator.Line line, int index, float wallWidth, Vector3 bottomLeft, Vector3 wallUp, Vector3 wallRight, Vector3 wallForward, int numberOfBricksAcross, int numberOfBricksHigh, ref bool[,] isPositionOccupied)
    {
        wallSegment.transform.position = bottomLeft;
        Quaternion targetRotation = Quaternion.LookRotation(wallRight, Vector3.up);
        wallSegment.transform.rotation = targetRotation;

        for (int y = 0; y < numberOfBricksHigh; y++)
        {
            for (int x = 0; x < numberOfBricksAcross; x++)
            {
                if (!isPositionOccupied[x, y]) // Check if the current position is not occupied
                {
                    Vector2 currentBrickSize = ChooseBrickSize(minBrickSize, maxBrickSize);
                    int brickWidth = Mathf.FloorToInt(currentBrickSize.x * resolution);
                    int brickHeight = Mathf.FloorToInt(currentBrickSize.y * resolution);

                    // Check if the brick fits in the remaining space
                    if (x + brickWidth <= numberOfBricksAcross && y + brickHeight <= numberOfBricksHigh)
                    {
                        bool spaceAvailable = true;
                        for (int i = 0; i < brickWidth && spaceAvailable; i++)
                        {
                            for (int j = 0; j < brickHeight && spaceAvailable; j++)
                            {
                                if (isPositionOccupied[x + i, y + j])
                                {
                                    spaceAvailable = false;
                                }
                            }
                        }

                        if (spaceAvailable)
                        {
                            for (int i = 0; i < brickWidth; i++)
                            {
                                for (int j = 0; j < brickHeight; j++)
                                {
                                    isPositionOccupied[x + i, y + j] = true;
                                }
                            }

                            // Calculate the local position offset for the brick based on its size and the resolution.
                            Vector3 localPositionOffset = wallForward.normalized * ((x + brickWidth / 2f) / resolution) +
                                                          wallUp.normalized * ((y + brickHeight / 2f) / resolution);

                            // Adjust the position by combining the spline point and the local offset.
                            Vector3 position = bottomLeft + localPositionOffset;

                            //xLerp that is an float from 0 to 1 that is an lerp between the start and end of the line
                            float xLerp = (float)x / (float)numberOfBricksAcross;
                            //lerp the up rotation of the brick from the bottomLeft to the bottomRight point direction of the line
                            Vector3 rotation = wallRight;

                            InstantiateBrickAt(position, rotation, currentBrickSize, wallSegment);
                        }
                    }
                }
            }
        }

        //ckeck if all the positions are occupied
        bool allPositionsOccupied = true;
        for (int y = 0; y < numberOfBricksHigh; y++)
        {
            for (int x = 0; x < numberOfBricksAcross; x++)
            {
                if (!isPositionOccupied[x, y])
                {
                    allPositionsOccupied = false;
                }
            }
        }

        //if not all positions are occupied, call the function again
        if (!allPositionsOccupied)
        {
            FillWallArea(wallSegment, line, index, wallWidth, bottomLeft, wallUp, wallRight, wallForward, numberOfBricksAcross, numberOfBricksHigh, ref isPositionOccupied);
        }
        else
        {
            FitWallSegementToWall(wallSegment, wallWidth);
        }
    }

    Transform FitWallSegementToWall(GameObject wallSegment, float wallWidth)
    {
        //make a Tranform to return
        Transform newTransform;

        // Adjust segment size and scale after filling is complete
        Vector3 positionBackup = wallSegment.transform.position;
        Quaternion rotationBackup = wallSegment.transform.rotation;

        // Reset position and rotation to avoid Bound Calculation issues
        wallSegment.transform.position = Vector3.zero;
        wallSegment.transform.rotation = Quaternion.identity;

        // Get the actual size of the wall segment
        float actualWallSegmentLength = GetPhysicalWallSegmentSize(wallSegment).x; // Adjust based on orientation

        // Calculate the scaling factor to fit the wall segment to the wall
        wallWidth = wallWidth == 0 ? 0.01f : wallWidth; // Avoid division by zero
        actualWallSegmentLength = actualWallSegmentLength == 0 ? 0.01f : actualWallSegmentLength; // Avoid division by zero
        float segmentGapScalingFactor = wallWidth / actualWallSegmentLength;
        wallSegment.transform.localScale = (Vector3.one - wallSegment.transform.right) + wallSegment.transform.right * segmentGapScalingFactor;

        // Restore position and rotation
        wallSegment.transform.position = positionBackup;
        wallSegment.transform.rotation = rotationBackup;

        newTransform = wallSegment.transform;
        return newTransform;
    }
    //function that gets the physical size of a wall segment (gameobject with bricks as children)
    public Vector3 GetPhysicalWallSegmentSize(GameObject wallSegment)
    {
        Bounds combinedBounds = CalculateAxisAlignedBounds(wallSegment);
        Vector3 localSize = wallSegment.transform.InverseTransformVector(combinedBounds.size);
        if (debug)
        {
            DrawBounds(wallSegment);

            // Transform the size from world space to local space
            Debug.DrawLine(wallSegment.transform.position, wallSegment.transform.right * localSize.x, Color.red, 3f);
            Debug.DrawLine(wallSegment.transform.position, wallSegment.transform.up * localSize.y, Color.blue, 3f);
            Debug.DrawLine(wallSegment.transform.position, wallSegment.transform.forward * localSize.z, Color.green, 3f);
        }
        return localSize; // Return the local size which is the size relative to the parent object
    }

    public Bounds CalculateAxisAlignedBounds(GameObject wallSegment)
    {
        // Initialize an empty bounds object
        Bounds axisAlignedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;

        // Iterate over each child (brick) of the wallSegment
        foreach (Renderer renderer in wallSegment.GetComponentsInChildren<Renderer>())
        {
            // For each renderer, get its bounds
            Bounds rendererBounds = renderer.bounds;

            // Convert the renderer's bounds to the local space of the wallSegment, ignoring rotation
            Bounds localBounds = new Bounds(
                wallSegment.transform.InverseTransformPoint(rendererBounds.center),
                Vector3.Scale(rendererBounds.size, wallSegment.transform.InverseTransformDirection(wallSegment.transform.lossyScale))
            );

            // If this is the first renderer, initialize the bounds with its local bounds
            if (!boundsInitialized)
            {
                axisAlignedBounds = localBounds;
                boundsInitialized = true;
            }
            else
            {
                // Otherwise, encapsulate the existing bounds with the new renderer's bounds
                axisAlignedBounds.Encapsulate(localBounds);
            }
        }

        // Return the combined, axis-aligned bounds
        return axisAlignedBounds;
    }

    private Vector2 ChooseBrickSize(Vector2 minBrickSize, Vector2 maxBrickSize)
    {
        float width = Random.Range((int)minBrickSize.x, (int)maxBrickSize.x + 1) / resolution;
        float height = Random.Range((int)minBrickSize.y, (int)maxBrickSize.y + 1) / resolution;
        return new Vector2(width, height);
    }

    private GameObject InstantiateBrickAt(Vector3 position, Vector3 forwardDirection, Vector2 size, GameObject parent)
    {
        GameObject brick = Instantiate(brickPrefab, position, Quaternion.identity, parent.transform);
        // Scale the brick according to its size. No change needed here.
        brick.transform.localScale = new Vector3(size.x, size.y, wallDepth);

        // Align the brick's forward direction with the spline's tangent at this point.
        // This replaces the previous rotation assignment.
        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, Vector3.up);

        // Apply a small random rotation around the brick's up axis to add variation.
        // This keeps the deviation but ensures bricks generally follow the spline's direction.
        float yRotationVariance = Random.Range(-rotationDiviation.x, rotationDiviation.x);
        float xRotationVariance = Random.Range(-rotationDiviation.y, rotationDiviation.y);

        // Create a rotation that combines the spline's direction with the random variance.
        Quaternion varianceRotation = Quaternion.Euler(xRotationVariance, yRotationVariance, 0);
        brick.transform.rotation = targetRotation * varianceRotation;

        // Adjust the brick's depth position randomly within a specified range.
        // This adds depth variation to the bricks.
        if (brickDepthDiviation != 0)
        {
            brick.transform.position += brick.transform.forward * Random.Range(0, wallDepth * brickDepthDiviation);
        }
        return brick;
    }

    //draw gizmo of relevant information
    private void OnDrawGizmos()
    {

    }

    void DrawBounds(GameObject wallSegment)
    {
        // Assuming 'combinedBounds' is the Bounds of your wallSegment
        Bounds combinedBounds = CalculateAxisAlignedBounds(wallSegment); // Ensure this function gives you the world space bounds
        // Extract the min and max points of the bounds
        Vector3 min = combinedBounds.min;
        Vector3 max = combinedBounds.max;

        // Calculate the corners of the bounds
        Vector3 corner1 = new Vector3(min.x, min.y, min.z);
        Vector3 corner2 = new Vector3(max.x, min.y, min.z);
        Vector3 corner3 = new Vector3(max.x, min.y, max.z);
        Vector3 corner4 = new Vector3(min.x, min.y, max.z);
        Vector3 corner5 = new Vector3(min.x, max.y, min.z);
        Vector3 corner6 = new Vector3(max.x, max.y, min.z);
        Vector3 corner7 = new Vector3(max.x, max.y, max.z);
        Vector3 corner8 = new Vector3(min.x, max.y, max.z);

        // Draw lines between the corners to form the wire box
        Debug.DrawLine(corner1, corner2, Color.red, duration: 3f);
        Debug.DrawLine(corner2, corner3, Color.red, duration: 3f);
        Debug.DrawLine(corner3, corner4, Color.red, duration: 3f);
        Debug.DrawLine(corner4, corner1, Color.red, duration: 3f);

        Debug.DrawLine(corner5, corner6, Color.red, duration: 3f);
        Debug.DrawLine(corner6, corner7, Color.red, duration: 3f);
        Debug.DrawLine(corner7, corner8, Color.red, duration: 3f);
        Debug.DrawLine(corner8, corner5, Color.red, duration: 3f);

        Debug.DrawLine(corner1, corner5, Color.red, duration: 3f);
        Debug.DrawLine(corner2, corner6, Color.red, duration: 3f);
        Debug.DrawLine(corner3, corner7, Color.red, duration: 3f);
        Debug.DrawLine(corner4, corner8, Color.red, duration: 3f);
    }
}