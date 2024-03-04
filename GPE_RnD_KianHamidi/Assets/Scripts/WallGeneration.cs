using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
struct Partition
{
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    public Vector3 middlePoint;

    public Partition(Vector3 _topLeft, Vector3 _topRight, Vector3 _bottomLeft, Vector3 _bottomRight)
    {
        topLeft = _topLeft;
        topRight = _topRight;
        bottomLeft = _bottomLeft;
        bottomRight = _bottomRight;
        middlePoint = (topLeft + topRight + bottomLeft + bottomRight) / 4;
    }
}
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
    Vector2 minBrickSize, maxBrickSize;

    [SerializeField]
    int wallPartitionResulution = 1;

    [SerializeField]
    List<Partition> partitions = new List<Partition>();
    List<Vector3> partitionCenter = new List<Vector3>();

    public GameObject cubePrefab;

    private void Awake()
    {
        lineGenerator = GetComponent<LineGenerator>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public void GenerateWall()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //Use Flood fill algoritm to fill in gaps
        //Make a grid out off the wall segments
        partitions = new List<Partition>();
        foreach (var line in lineGenerator.Lines)
        {
            for (int i = 1; i < line.LinePoints.Count; i++)
            {
                if (i - 1 < 0) { continue; }
                SegmentPartition(line.LinePoints[i], line.LinePoints[i - 1], wallPartitionResulution, wallHeight);
            }
        }

        GenerateMesh();
    }

    void GenerateMesh()
    {
        foreach (var gridPos in partitions)
        {
            GameObject brick = Instantiate(cubePrefab, transform);
            
            brick.transform.position = gridPos.middlePoint;
        }
    }
    void SegmentPartition(Vector3 p0, Vector3 p1, int resolution, float wallHeight)
    {
        Vector3 baseVector = p1 - p0;
        float wallLength = baseVector.magnitude;
        Vector3 wallDirection = baseVector.normalized;
        Vector3 wallUp = Vector3.up * wallHeight;

        int divisions_x = Mathf.FloorToInt(wallLength * resolution);
        int divisions_y = Mathf.FloorToInt(wallHeight * resolution);
        for (int y = 0; y < divisions_y; y++)
        {
            for (int x = 0; x < divisions_x; x++)
            {
                float factor_x1 = (float)x / divisions_x;
                float factor_x2 = (float)(x + 1) / divisions_x;
                float factor_y1 = (float)y / divisions_y;
                float factor_y2 = (float)(y + 1) / divisions_y;

                Vector3 bl = Vector3.Lerp(p0, p1, factor_x1) + (wallUp * factor_y1);
                Vector3 br = Vector3.Lerp(p0, p1, factor_x2) + (wallUp * factor_y1);
                Vector3 tl = Vector3.Lerp(p0, p1, factor_x1) + (wallUp * factor_y2);
                Vector3 tr = Vector3.Lerp(p0, p1, factor_x2) + (wallUp * factor_y2);

                partitions.Add(new Partition(tl, tr, bl, br));
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

        if (partitions == null) return;

        foreach (var partition in partitions)
        {
            // Draw corner points in blue
            Gizmos.color = Color.blue;
            float size = 0.1f / wallPartitionResulution;
            Gizmos.DrawSphere(partition.topLeft, size);
            Gizmos.DrawSphere(partition.topRight, size);
            Gizmos.DrawSphere(partition.bottomLeft, size);
            Gizmos.DrawSphere(partition.bottomRight, size);

            // Draw middle point in black
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(partition.middlePoint, size);
        }
    }
}
