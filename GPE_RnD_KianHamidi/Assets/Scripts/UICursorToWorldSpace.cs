using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICursorToWorldSpace : MonoBehaviour
{
    public LayerMask layerMask;
    public float visualSmoothing = 20;

    public float cursorScale = 1;
    //store the cursor position in world space
    Vector3 cursorPosition;
    Quaternion cursorRotation;

    //store the previous cursor position in world space
    Vector3 previousCursorPosition;
    Quaternion previousCursorRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // cursor to world space only hitting geometry on a specigied layer using raycast in the fixed update
    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            cursorPosition = hit.point;
            cursorRotation = Quaternion.LookRotation(hit.normal);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * cursorScale, Time.deltaTime * visualSmoothing);
        }
        else
        {
            //smoothly scale the cursor down to 0 if it is not hitting anything
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * visualSmoothing);
        }
        //calculate the speed of the cursor movement
        float speed = Vector3.Distance(previousCursorPosition, cursorPosition) / Time.deltaTime;
        //multiply the speed by the visual smoothing to get a smooth movement
        float smoothing = Mathf.Lerp(visualSmoothing, speed, Time.deltaTime * 10);
        //lerp the cursor position and rotation to smooth out the movement
        cursorPosition = Vector3.Lerp(previousCursorPosition, cursorPosition, smoothing);
        cursorRotation = Quaternion.Slerp(previousCursorRotation, cursorRotation, smoothing);

        //apply the cursor position and rotation to the transform
        transform.position = cursorPosition;
        transform.rotation = cursorRotation;

        previousCursorPosition = cursorPosition;
        previousCursorRotation = cursorRotation;
    }


}
