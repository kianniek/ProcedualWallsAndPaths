using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorHandler : MonoBehaviour
{
    public Button placingWallButton; // Assign in editor or find dynamically
    public Button placingPathButton; // Assign in editor or find dynamically
    public static CursorHandler Instance;
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

    [SerializeField] CursorType currentCursor;

    void Awake() // Using Awake for initialization
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //DontDestroyOnLoad(gameObject);

        InitializeCursorHolder();
        InitializeCursors();

        placingWallButton.onClick.AddListener(() => SetCurrentCursorType(CursorType.PlacingWall));
        placingPathButton.onClick.AddListener(() => SetCurrentCursorType(CursorType.PlacingPath));
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
        for (int i = 0; i < cursors.Length; i++)
        {
            if (cursors[i].Object != null)
            {
                GameObject go = Instantiate(cursors[i].Object, cursorHolder.transform);
                go.SetActive(false);
                go.name = cursors[i].name;
                go.transform.localPosition = cursors[i].hotspot;
                cursors[i].Object = go;
                cursorDictionary.Add((CursorType)Array.IndexOf(cursors, cursors[i]), cursors[i]);
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
        foreach (var item in cursorDictionary)
        {
            item.Value.Object.SetActive(false);
        }

        // Then enable only the selected cursor
        Cursor value = new Cursor();
        if (cursorDictionary.TryGetValue(cursorType, out value))
        {
            Console.WriteLine($"For key = {cursorType}, value = {value}.", value);
            value.Object.SetActive(true);
        }
        else
        {
            Console.WriteLine($"Key = {cursorType} is not found.");
        }
        
    }
    public void SetCurrentCursorType(CursorType cursorType)
    {
        Debug.Log($"Cursor type changed to: {cursorType}");
        currentCursor = cursorType;
    } 
    public CursorType GetCurrentCursorType()
    {
        return currentCursor;
    }

    [Serializable]
    public struct Cursor
    {
        public string name;
        public GameObject Object;
        public Vector3 hotspot;
    }
}
