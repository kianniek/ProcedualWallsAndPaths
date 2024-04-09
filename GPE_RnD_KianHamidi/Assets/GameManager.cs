using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Slider heightSlider; // Assign this in the inspector

    // Define UnityEvents for each key action
    public UnityEvent actionForKeyB;
    public UnityEvent actionForKeyLeftBracket;
    public UnityEvent actionForKeyRightBracket;
    public UnityEvent actionForKeyW;
    public UnityEvent actionForKeyP;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reset the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(KeyCode.B) && actionForKeyB != null)
        {
            // Invoke the action for B key
            actionForKeyB.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket) && actionForKeyLeftBracket != null)
        {
            // Invoke the action for [ key
            actionForKeyLeftBracket.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.RightBracket) && actionForKeyRightBracket != null)
        {
            // Invoke the action for ] key
            actionForKeyRightBracket.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.W) && actionForKeyW != null)
        {
            // Invoke the action for W key
            actionForKeyW.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.P) && actionForKeyP != null)
        {
            // Invoke the action for P key
            actionForKeyP.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Increase slider value
            if (heightSlider != null) heightSlider.value += 1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Decrease slider value
            if (heightSlider != null) heightSlider.value -= 1;
        }
    }
}
