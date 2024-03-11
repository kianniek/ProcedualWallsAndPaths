using System.Collections;
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
        for (int i = 0; i < line.linePoints.Count -1; i++)
        {
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
        Debug.DrawLine(bottomLeft, bottomLeft + wallRight, Color.red, 3);
        Debug.DrawLine(bottomLeft, bottomLeft + wallUp, Color.green, 3);
        Debug.DrawLine(bottomLeft, bottomLeft + wallForward, Color.blue, 3);

        int numberOfBricksAcross = Mathf.FloorToInt(wallWidth / minBrickSize.x * resolution);
        int numberOfBricksHigh = Mathf.FloorToInt(wallHeight / minBrickSize.y * resolution);
        isPositionOccupied = new bool[numberOfBricksAcross, numberOfBricksHigh];

        //make a wallsegemnt gameobject
        GameObject wallSegment = new("WallSegment");
        wallSegment.transform.parent = transform;
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
                            print(xLerp);
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

        //check size of wallsegment and set it to the correct size which is the length the line
        float wallSegementLength = GetWallSegmentSize(wallSegment).magnitude;
        wallSegment.transform.localScale = Vector3.one + wallSegment.transform.right * wallSegementLength;
    }

    //function that gets the physical size of a wall segment (gameobject with bricks as children)
    public Vector3 GetWallSegmentSize(GameObject wallSegment)
    {
        Vector3 size = Vector3.zero;
        foreach (Transform child in wallSegment.transform)
        {
            size = Vector3.Max(size, child.GetComponent<MeshRenderer>().bounds.size);
        }
        return size;
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
        //if (isPositionOccupied != null)
        //{
        //    for (int y = 0; y < isPositionOccupied.GetLength(1); y++)
        //    {
        //        for (int x = 0; x < isPositionOccupied.GetLength(0); x++)
        //        {
        //            if (isPositionOccupied[x, y])
        //            {
        //                Gizmos.color = Color.red;
        //                Gizmos.DrawSphere(new Vector3(x, y, 0), 0.1f);
        //            }
        //            else
        //            {
        //                Gizmos.color = Color.green;
        //                Gizmos.DrawSphere(new Vector3(x, y, 0), 0.1f);
        //            }
        //        }
        //    }
        //}
    }
}