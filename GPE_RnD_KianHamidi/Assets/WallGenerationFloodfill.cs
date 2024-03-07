using UnityEngine;

public class WallGenerationFloodfill : MonoBehaviour
{
    [Header("Optimalisation")]
    [Space(10)]
    [SerializeField] bool combineMeshes = false;
    [Space(10)]
    [Header("Wall Generation")]
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallDepth = 0.1f;
    [SerializeField] [Range(0,1)] private float brickDepthDiviation = 0.1f;
    [SerializeField] private Vector2Int minBrickSize, maxBrickSize;
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private float resolution = 8;
    [SerializeField] private Vector2 rotationDiviation = new(1, 1);
    private bool[,] isPositionOccupied;

    MeshCombinerRuntime meshCombinerRuntime;

    LineGenerator lineGenerator;

    private void Start()
    {
        meshCombinerRuntime = GetComponent<MeshCombinerRuntime>();
        lineGenerator = GetComponent<LineGenerator>();
        ReGenerate();
    }

    private void Update()
    {
        //when enter is pressed, generate a new wall and destroy the old one
        if (Input.GetKey(KeyCode.Return))
        {
            ReGenerate();
        }
    }

    public void ReGenerate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        //if the line generator is not null, generate the wall from the lines
        if (lineGenerator != null)
        {
            foreach (var line in lineGenerator.Lines)
            {
                for (int i = 1; i < line.LinePoints.Count; i++)
                {
                    if (i - 1 < 0) { continue; }
                    GenerateWall(line, i, wallHeight, minBrickSize, maxBrickSize);
                }
            }
        }
        else
        {
            Debug.LogError("LineGenerator is null");
            //GenerateWall(wallLeft, wallRight, wallHeight, minBrickSize, maxBrickSize);
        }
    }

    public void GenerateWall(Line line, int index, float wallHeight, Vector2 minBrickSize, Vector2 maxBrickSize)
    {
        Vector3 bottomLeft = line.LinePoints[index - 1];
        Vector3 bottomRight = line.LinePoints[index];
        float wallWidth = Vector3.Distance(bottomLeft, bottomRight);
        //get the up, right and forward vectors of the wall
        Vector3 wallUp = Vector3.up * wallHeight;
        Vector3 wallRight = (bottomRight - bottomLeft).normalized * wallWidth;
        Vector3 wallForward = Vector3.Cross(wallUp, wallRight).normalized * wallDepth;

        //draw the wall vectors in the scene view
        Debug.DrawLine(bottomLeft, bottomLeft + wallRight, Color.red, 10);
        Debug.DrawLine(bottomLeft, bottomLeft + wallUp, Color.green, 10);
        Debug.DrawLine(bottomLeft, bottomLeft + wallForward, Color.blue, 10);

        int numberOfBricksAcross = Mathf.FloorToInt(wallWidth / minBrickSize.x * resolution);
        int numberOfBricksHigh = Mathf.FloorToInt(wallHeight / minBrickSize.y * resolution);
        isPositionOccupied = new bool[numberOfBricksAcross, numberOfBricksHigh];

        FillWallArea(line, index, bottomLeft, wallUp,wallRight, wallForward, numberOfBricksAcross, numberOfBricksHigh, minBrickSize, maxBrickSize, ref isPositionOccupied);

        if (!combineMeshes) { return; }
        meshCombinerRuntime.CombineMeshes();
    }

    private void FillWallArea(Line line, int index ,Vector3 bottomLeft, Vector3 wallUp, Vector3 wallRight, Vector3 wallForward, int numberOfBricksAcross, int numberOfBricksHigh, Vector2 minBrickSize, Vector2 maxBrickSize, ref bool[,] isPositionOccupied)
    {
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
                            // Calculate the position more accurately
                            Vector3 position = bottomLeft +
                                wallRight.normalized * ((x + brickWidth / 2f) / resolution) +
                                wallUp.normalized * ((y + brickHeight / 2f) / resolution);

                            //create a 0 to 1 value for the x position of the brick
                            float xNormalized = (float)x / numberOfBricksAcross;

                            //lerp the up rotation of the brick from the bottomLeft to the bottomRight point direction of the line
                            Vector3 rotation = Vector3.Lerp(line.LineDirection[index-1], line.LineDirection[index], xNormalized);

                            InstantiateBrickAt(position, rotation, currentBrickSize, wallRight, wallUp, wallForward);
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
            FillWallArea(line, index, bottomLeft, wallUp, wallRight, wallForward, numberOfBricksAcross, numberOfBricksHigh, minBrickSize, maxBrickSize, ref isPositionOccupied);
        }
    }
    private Vector2 ChooseBrickSize(Vector2 minBrickSize, Vector2 maxBrickSize)
    {
        float width = Random.Range((int)minBrickSize.x, (int)maxBrickSize.x + 1) / resolution;
        float height = Random.Range((int)minBrickSize.y, (int)maxBrickSize.y + 1) / resolution;
        return new Vector2(width, height);
    }

    private void InstantiateBrickAt(Vector3 position, Vector3 rotation,  Vector2 size, Vector3 wallRight, Vector3 wallUp, Vector3 wallForward)
    {
        GameObject brick = Instantiate(brickPrefab, position, Quaternion.identity, transform);
        brick.transform.localScale = new Vector3(size.x, size.y, wallDepth);
        brick.transform.Rotate(rotation);
        brick.transform.Rotate(Vector3.up, Random.Range(-rotationDiviation.x, rotationDiviation.x));
        //oriant the brick to the wall by using the wall vectors
        brick.transform.right = wallRight;
        brick.transform.up = wallUp;
        brick.transform.forward = wallForward;
        //randlomize rotation of the brick on the y axis by rotationDiviation degrees
        if (rotationDiviation != Vector2.zero)
        {
            brick.transform.Rotate(Vector3.up, Random.Range(-rotationDiviation.x, rotationDiviation.x));
            brick.transform.Rotate(Vector3.right, Random.Range(-rotationDiviation.y, rotationDiviation.y));
        }
        if (brickDepthDiviation != 0)
        {
            brick.transform.position += new Vector3(0,0,Random.Range(0, wallDepth * brickDepthDiviation));
        }
    }
    //draw gizmo of relevant information
    private void OnDrawGizmos()
    {
        //draw the wall using the lineGenerator points
        if (lineGenerator != null)
        {
            foreach (var line in lineGenerator.Lines)
            {
                for (int i = 1; i < line.LinePoints.Count; i++)
                {
                    if (i - 1 < 0) { continue; }
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(line.LinePoints[i - 1], line.LinePoints[i]);
                    //draw the wall height from the two points and add a line connecing the two points
                    Vector3 wallHeightVector = new Vector3(0, wallHeight, 0);
                    Gizmos.DrawLine(line.LinePoints[i - 1], line.LinePoints[i] + wallHeightVector);
                }
            }
        }

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