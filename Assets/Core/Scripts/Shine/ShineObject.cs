using UnityEngine;

public class ShineObject : MonoBehaviour
{
    public bool Catched = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Camera cam;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        cam = Camera.main;
    }

    void Update()
    {
        // If caught, check if we are still looking at it (inside camera viewport)
        if (Catched && cam != null)
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
            bool isInFrontOfCamera = viewportPos.z > 0;
            bool isOnScreen = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

            // If it goes off-screen or behind the camera, release it
            if (!isInFrontOfCamera || !isOnScreen)
            {
                ReleaseObject();
            }
        }

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.enabled = Catched;
        }
    }

    public void CatchObject()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        Catched = true;
    }

    public void ReleaseObject()
    {
        Catched = false;

        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}