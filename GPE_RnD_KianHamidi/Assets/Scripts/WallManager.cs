using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public GameObject WallPrefab;

    //enum input.getbuttondown strings
    public enum ButtonDownStrings
    {
        button0,
        button1,
        button2,
    }

    public ButtonDownStrings buttonTypeDown;

    [SerializeField] List<SplineGenerator> splines;
    // Start is called before the first frame update
    void Start()
    {
        splines = new List<SplineGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(CursorHandler.Instance == null) { return; }
        if(CursorHandler.Instance.GetCurrentCursorType() != CursorHandler.CursorType.PlacingWall)
        {
            return;
        }
        if (CheckIfEditing()) { return; }

        InstatiateWall();
    }

    void InstatiateWall()
    {
        int button = 0;

        switch (buttonTypeDown)
        {
            case ButtonDownStrings.button0:
                button = 0;
                break;
            case ButtonDownStrings.button1:
                button = 1;
                break;
            case ButtonDownStrings.button2:
                button = 2;
                break;
        }

        if (Input.GetMouseButtonDown(button))
        {
            print("Drawing");
            GameObject go = Instantiate(WallPrefab, transform.position, transform.rotation, transform);
            splines.Add(go.GetComponent<SplineGenerator>());
        }
    }

    bool CheckIfEditing()
    {
        //check all SplineGenerator scripts in the children\
        if(splines == null) { return false; }
        if(splines.Count == 0) { return false; }
        foreach (SplineGenerator s in splines)
        {
            if (s.IsEditing())
            {
                return true;
            }
        }
        return false;
    }
}
