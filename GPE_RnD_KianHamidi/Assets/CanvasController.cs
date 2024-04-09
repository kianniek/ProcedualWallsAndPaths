using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private WallGenerationFloodfill wallGenerationFloodfill;
    // Public fields to assign UI elements from the Inspector
    public Slider heightSlider;
    public Slider depthSlider;
    public Slider BDDSlider;
    public TMP_InputField XInputField;
    public TMP_InputField YInputField;

    // Use this for initialization
    void Start()
    {
        // Register slider event listeners
        heightSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        depthSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        BDDSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        // Register input field event listeners
        XInputField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        YInputField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    // This function is called when any slider's value is changed
    public void ValueChangeCheck()
    {
        Debug.Log("Height Slider Value: " + heightSlider.value);
        Debug.Log("Depth Slider Value: " + depthSlider.value);
        Debug.Log("BDD Slider Value: " + BDDSlider.value);
        Debug.Log("X Input Field Value: " + XInputField.text);
        Debug.Log("Y Input Field Value: " + YInputField.text);

        wallGenerationFloodfill.SetWallHeight(heightSlider.value);
        wallGenerationFloodfill.SetWallWidth(depthSlider.value);
        wallGenerationFloodfill.SetBrickDepthDiviation(BDDSlider.value);
        wallGenerationFloodfill.SetRotationDiviation(new Vector2(int.Parse(XInputField.text), int.Parse(YInputField.text)));
    }
}