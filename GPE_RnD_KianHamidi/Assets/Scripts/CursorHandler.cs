using System;
using System.Collections.Generic;
using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    public GameObject cursorHolder;

    [SerializeField] private Cursor[] cursors;
    private Dictionary<CursorType, Cursor> cursorDictionary = new Dictionary<CursorType, Cursor>();

    public enum CursorType
    {
        PlacingWall,
        RemovingWall,
        EditingWall,
        PlacingPath,
        RemovingPath,
        RemovingAll,
    }

    [SerializeField] private CursorType currentCursor;

    void Awake() // Using Awake for initialization
    {
        InitializeCursorHolder();
        InitializeCursors();
    }

    private void InitializeCursorHolder()
    {
        if (cursorHolder == null)
        {
            cursorHolder = new GameObject("CursorHolder");
            cursorHolder.transform.parent = transform; // Optionally set the parent
        }

        // Clear all children of the cursor holder
        foreach (Transform child in cursorHolder.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void InitializeCursors()
    {
        foreach (var cursor in cursors)
        {
            if (cursor.Object != null)
            {
                GameObject go = Instantiate(cursor.Object, cursorHolder.transform);
                //go.SetActive(false);
                go.name = cursor.name;
                go.transform.localPosition = cursor.hotspot;
                cursorDictionary.Add((CursorType)Array.IndexOf(cursors, cursor), cursor);
            }
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < cursors.Length; i++)
        {
            if (cursors[i].name != null)
            {
                if (i >= Enum.GetNames(typeof(CursorType)).Length) break;
                cursors[i].name = ((CursorType)i).ToString();
            }
        }
    }

    void Update() // Switched to Update for cursor management
    {
        if (cursorDictionary.ContainsKey(currentCursor))
        {
            SetCursor(currentCursor);
        }
    }

    public void SetCursor(CursorType cursorType)
    {
        foreach (var pair in cursorDictionary)
        {
            // Disable all cursors
            //pair.Value.Object.SetActive(false);
        }

        // Then enable only the selected cursor
        if (cursorDictionary.TryGetValue(cursorType, out Cursor selectedCursor))
        {
            selectedCursor.Object.SetActive(true);
        }
    }


    [Serializable]
    public struct Cursor
    {
        public string name;
        public GameObject Object;
        public Vector3 hotspot;
    }
}
