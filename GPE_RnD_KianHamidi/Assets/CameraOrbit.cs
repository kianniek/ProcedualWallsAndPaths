using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public float orbitSpeed = 10.0f; // Target speed of rotation
    public float smoothFactor = 0.5f; // Smoothing factor for start/stop transitions
    public float smoothFactorSave = 0.5f; // Smoothing factor for start/stop transitions
    public float smoothFactorNoKeys = 0.9f; // Smoothing factor for start/stop transitions
    private float currentSpeed = 0.0f; // Current speed, will be updated to smoothly transition towards orbitSpeed

    private void Start()
    {
        smoothFactorSave = smoothFactor;
    }
    void Update()
    {
        float targetSpeed = 0;

        if (Input.GetKey(KeyCode.A))
        {
            // Set target speed for rotation around the Y axis at the origin point
            targetSpeed = orbitSpeed;
            smoothFactor = smoothFactorSave;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Set target speed for rotation in the opposite direction
            targetSpeed = -orbitSpeed;
            smoothFactor = smoothFactorSave;
        }
        else
        {
            smoothFactor = smoothFactorNoKeys;
        }

        // Smoothly interpolate current speed towards the target speed based on the smoothFactor
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, smoothFactor * Time.deltaTime);

        // Rotate the camera around the Y axis at the origin point using the smoothly adjusted current speed
        transform.RotateAround(Vector3.zero, Vector3.up, currentSpeed * Time.deltaTime);
    }
}
