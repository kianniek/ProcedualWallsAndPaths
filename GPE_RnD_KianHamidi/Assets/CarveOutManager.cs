using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarveOutManager : MonoBehaviour
{
    // Static singleton property
    public static CarveOutManager Instance { get; private set; }

    WallManager wallManager;
    PathwayManager pathwayManager;
    public CarveOutWall carveOutWall;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Check if instance already exists and if it's not this instance, destroy it
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (wallManager == null)
        {
            wallManager = GetComponentInChildren<WallManager>();
        }

        if (pathwayManager == null)
        {
            pathwayManager = GetComponentInChildren<PathwayManager>();
        }

        if (carveOutWall == null)
        {
            carveOutWall = GetComponent<CarveOutWall>();
        }
    }

    private void FixedUpdate()
    {
        //check if wallManager and pathwayManager have children
        if (
            wallManager == null ||
            pathwayManager == null ||
            wallManager.transform.childCount == 0 ||
            pathwayManager.transform.childCount == 0
           )
        {
            return;
        }
    }

    // Additional methods specific to CarveOutManager can be added here
}
