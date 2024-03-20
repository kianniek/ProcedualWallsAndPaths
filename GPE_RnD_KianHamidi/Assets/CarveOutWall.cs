using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SplineGenerator))]
public class CarveOutWall : MonoBehaviour
{
    //use the SplineGenerator to get every point on the spline
    public GameObject splineGeneratorsWallParent;
    public List<SplineGenerator> splineGeneratorsWall;

    public float carveOutDistance = 0.5f;
    SplineGenerator this_splineGenerator;
    // Start is called before the first frame update
    void Start()
    {
        if (splineGeneratorsWallParent == null)
        {
            Debug.LogError("SplineGenerator is not set");
        }

        this_splineGenerator = GetComponent<SplineGenerator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(splineGeneratorsWallParent == null) { return; }

        Debug.Log($"1");

        //get the splines from the parent
        splineGeneratorsWall = new List<SplineGenerator>();
        foreach (Transform child in splineGeneratorsWallParent.transform)
        {
            if (child.TryGetComponent<SplineGenerator>(out var spline))
            {
                splineGeneratorsWall.Add(spline);
            }
        }

        if (splineGeneratorsWallParent.transform.childCount == 0) { return; }

        Debug.Log($"2");

        foreach (SplineGenerator spline in splineGeneratorsWall)
        {
            if (spline == null) { continue; }
            if (spline.line.linePoints == null) { continue; }
            if (spline.line.linePoints.Count == 0) { continue; }

            if (spline.CheckSplinesIntersection(this_splineGenerator.line, spline.line, out List<Vector3> splineIntersectPoints))
            {
                Debug.Log($"3");

                CarveOut(splineIntersectPoints);
            }
        }
        
    }

    void CarveOut(List<Vector3> intersectionPoints)
    {
        foreach (Vector3 point in intersectionPoints)
        {
            //get each renderer that intersects with the point and carve out the wall at distance carveOutDistance in an sphere
            Collider[] colliders = Physics.OverlapSphere(point, carveOutDistance);
            foreach (Collider hit in colliders)
            {
                Debug.Log($"4");

                if (hit.TryGetComponent<Renderer>(out var rend))
                {
                    if(rend.gameObject.CompareTag("Wall") == false) { continue; }
                    Debug.Log($"Carving out wall at {rend.gameObject.name}");
                    //rend.gameObject.SetActive(false);

                    rend.enabled = false;
                }
            }
        }

    }

    //draw the intersection points
    private void OnDrawGizmos()
    {
        if (splineGeneratorsWall == null) { return; }
        if (splineGeneratorsWall.Count == 0) { return; }
        foreach (SplineGenerator spline in splineGeneratorsWall)
        {
            if (spline == null) { continue; }
            if (spline.line.linePoints == null) { continue; }
            if (spline.line.linePoints.Count == 0) { continue; }

            if (spline.CheckSplinesIntersection(this_splineGenerator.line, spline.line, out List<Vector3> splineIntersectPoints))
            {
                foreach (Vector3 point in splineIntersectPoints)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(point, 0.1f);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(point, carveOutDistance * 2);
                }
            }
        }
    }
}
