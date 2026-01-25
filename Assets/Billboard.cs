using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool flip180 = false;
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            Vector3 targetDirection = mainCameraTransform.rotation * Vector3.forward;
            if (flip180) targetDirection = -targetDirection;

            // Make the object face the camera (or away if flipped)
            transform.LookAt(transform.position + targetDirection,
                             mainCameraTransform.rotation * Vector3.up);
        }
    }
}
