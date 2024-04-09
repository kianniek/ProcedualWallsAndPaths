using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class CarveOutWall : MonoBehaviour
{
    //use the SplineGenerator to get every point on the spline
    public GameObject splineGeneratorsWallParent;
    public List<SplineGenerator> splineGeneratorsWall;

    public GameObject splineGeneratorsPathwayParent;
    public List<SplineGenerator> splineGeneratorsPathway;

    public DataTextureCreator dataTextureCreator;
    public float carveOutDistance = 0.5f;

    public LayerMask walllayer;

    [SerializeField] List<Collider> colliders;

    List<SplineGenerator.Intersection> splineIntersectPoints;

    public List<GameObject> archways;

    public bool hideWallBricks = false;
    public bool addArchways = false;

    // Start is called before the first frame update
    void Start()
    {
        if (splineGeneratorsWallParent == null)
        {
            Debug.LogError("SplineGenerator is not set");
        }

        if (splineGeneratorsPathwayParent == null)
        {
            Debug.LogError("SplineGenerator is not set");
        }
        colliders = new();

        splineIntersectPoints = new();
    }

    // Update is called once per frame
    public void FetchUpdate()
    {
        if (splineGeneratorsWallParent == null || splineGeneratorsPathwayParent == null) { return; }

        //get the splines from the parent
        splineGeneratorsWall = new List<SplineGenerator>();
        foreach (Transform child in splineGeneratorsWallParent.transform)
        {
            if (child.TryGetComponent<SplineGenerator>(out var spline))
            {
                splineGeneratorsWall.Add(spline);
            }
        }

        //get the splines from the parent
        splineGeneratorsPathway = new List<SplineGenerator>();
        foreach (Transform child in splineGeneratorsPathwayParent.transform)
        {
            if (child.TryGetComponent<SplineGenerator>(out var spline))
            {
                splineGeneratorsPathway.Add(spline);
            }
        }
    }

    public void ClearCollisionList()
    {
        if (colliders.Count != 0)
        {
            foreach (Collider hit in colliders)
            {
                print(hit);
                if (hit.gameObject != null)
                {
                    hit.gameObject.SetActive(true);
                }
                else
                {
                    colliders.Remove(hit);
                }
            }
        }
        colliders.Clear();
    }

    public void ForceClearCollisionList()
    {
        colliders.Clear();
    }

    public void CarveOut()
    {
        if (splineGeneratorsWallParent.transform.childCount == 0) { return; }

        if (colliders.Count != 0)
        {
            foreach (Collider hit in colliders)
            {
                if (hit == null)
                {
                    colliders.Remove(hit);
                }
                else
                {
                    hit.gameObject.SetActive(true);
                }
            }
        }

        colliders.Clear();

        foreach (SplineGenerator spline in splineGeneratorsWall)
        {
            if (spline == null || spline.line.linePoints == null || spline.line.linePoints.Count == 0) { continue; }

            foreach (SplineGenerator path_spline in splineGeneratorsPathway)
            {
                if (path_spline.CheckSplinesIntersection(path_spline.line, spline.line, out splineIntersectPoints))
                {
                    foreach (SplineGenerator.Intersection pointData in splineIntersectPoints)
                    {
                        //get each renderer that intersects with the point and carve out the wall at distance carveOutDistance in an sphere
                        colliders.AddRange(Physics.OverlapSphere(pointData.center, carveOutDistance, walllayer));
                        if (addArchways)
                        {
                            AddArchwayColumns(pointData);
                        }
                    }
                }
            }
        }

        if (hideWallBricks)
        {
            foreach (Collider hit in colliders)
            {

                hit.gameObject.SetActive(false);
            }
        }
    }

    private void AddArchwayColumns(SplineGenerator.Intersection point)
    {
        //add columns to the archway
        GameObject columnLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject columnRight = GameObject.CreatePrimitive(PrimitiveType.Cube);

        if (archways == null) { archways = new(); }

        columnLeft.transform.position = point.point1 + (point.center - point.point1).normalized * carveOutDistance + Vector3.up * dataTextureCreator.height / 2 + (point.center - point.point1).normalized * 0.1f;
        columnLeft.transform.localScale = new Vector3(0.1f, dataTextureCreator.height, 0.1f);
        columnLeft.transform.rotation = Quaternion.identity;
        columnLeft.transform.LookAt(columnLeft.transform.position + point.direction1);
        columnLeft.GetComponent<Renderer>().material.color = Color.white;

        columnRight.transform.position = point.point2 + (point.center - point.point2).normalized * carveOutDistance + Vector3.up * dataTextureCreator.height / 2;
        columnRight.transform.localScale = new Vector3(0.1f, dataTextureCreator.height, 0.1f);
        columnRight.transform.rotation = Quaternion.identity;
        columnRight.transform.LookAt(columnRight.transform.position + point.direction2);
        columnRight.GetComponent<Renderer>().material.color = Color.white;

        //debug draw line between the points
        Debug.DrawLine(point.center, point.center + Vector3.up, Color.magenta, 10);
        Debug.DrawLine(point.point1, point.point1 + Vector3.up, Color.magenta, 10);
        Debug.DrawLine(point.point2, point.point2 + Vector3.up, Color.magenta, 10);
        Debug.DrawLine(point.point1, point.point2, Color.magenta, 10);
        Debug.DrawLine(point.center, point.point2, Color.red, 10);
        Debug.DrawLine(point.center, point.point1, Color.red, 10);

        if (archways.Count != 0)
        {
            //remove archways that share the same point
            foreach (var item in archways)
            {
                if (columnLeft.transform.position != item.transform.position)
                {
                    archways.Add(columnLeft);
                }
                else
                {
                    Destroy(columnLeft);
                }

                if (columnRight.transform.position != item.transform.position)
                {
                    archways.Add(columnRight);
                }
                else
                {
                    Destroy(columnRight);
                }

            }
        }

    }

    //draw the intersection points
    private void OnDrawGizmosSelected()
    {
        if (splineGeneratorsWall == null) { return; }
        if (splineGeneratorsWall.Count == 0) { return; }
        foreach (SplineGenerator spline in splineGeneratorsWall)
        {
            if (spline == null) { continue; }
            if (spline.line.linePoints == null) { continue; }
            if (spline.line.linePoints.Count == 0) { continue; }

            if (splineIntersectPoints == null) { return; }
            foreach (SplineGenerator.Intersection pointData in splineIntersectPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(pointData.center, 0.1f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(pointData.center, carveOutDistance);
            }
        }
    }
}
