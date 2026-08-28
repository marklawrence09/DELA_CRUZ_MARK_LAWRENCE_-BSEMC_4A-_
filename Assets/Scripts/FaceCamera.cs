using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // Rotates the canvas to face the camera active on screen
            transform.rotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
        }
    }
}