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

    public float carveOutDistance = 0.5f;

    public LayerMask walllayer;

    List<Collider> colliders;

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
    }

    // Update is called once per frame
    public void FetchUpdate()
    {
        if (splineGeneratorsWallParent == null) { return; }
        if (splineGeneratorsPathwayParent == null) { return; }

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

    public void CarveOut()
    {
        if (splineGeneratorsWallParent.transform.childCount == 0) { return; }
        Debug.Log($"1");

        if(colliders.Count != 0)
        {
            foreach (Collider hit in colliders)
            {
                hit.gameObject.SetActive(true);
            }
        }

        colliders.Clear();

        foreach (SplineGenerator spline in splineGeneratorsWall)
        {
            if (spline == null) { continue; }
            if (spline.line.linePoints == null) { continue; }
            if (spline.line.linePoints.Count == 0) { continue; }
            foreach (SplineGenerator path_spline in splineGeneratorsPathway)
            {
                if (path_spline == null) { continue; }
                if (path_spline.line.linePoints == null) { continue; }
                if (path_spline.line.linePoints.Count == 0) { continue; }

                if (path_spline.CheckSplinesIntersection(path_spline.line, spline.line, out List<Vector3> splineIntersectPoints))
                {
                    foreach (Vector3 point in splineIntersectPoints)
                    {
                        //get each renderer that intersects with the point and carve out the wall at distance carveOutDistance in an sphere
                        colliders.AddRange(Physics.OverlapSphere(point, carveOutDistance, walllayer));
                        Debug.Log($"{colliders.Count}");
                    }
                }
            }
        }

        foreach (Collider hit in colliders)
        {
            hit.gameObject.SetActive(false);
            Debug.Log($"Carving out wall");
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
            foreach (SplineGenerator path_spline in splineGeneratorsPathway)
            {
                if (path_spline.CheckSplinesIntersection(path_spline.line, spline.line, out List<Vector3> splineIntersectPoints))
                {
                    foreach (Vector3 point in splineIntersectPoints)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(point, 0.1f);
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(point, carveOutDistance);
                    }
                }
            }
        }
    }
}
