using UnityEngine;

public class Scaler : MonoBehaviour
{
    public float minOrthoSize = 5f;
    public float maxOrthoSize = 15f;
    public float zoomSpeed = 1f;

    private Camera cam;
    private bool isMobile = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        isMobile = Application.isMobilePlatform;
    }

    void Update()
    {
        if (!isMobile)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            float newOrthoSize = cam.orthographicSize * (1 - scroll * zoomSpeed);

            newOrthoSize = Mathf.Clamp(newOrthoSize, minOrthoSize, maxOrthoSize);

            cam.orthographicSize = newOrthoSize;
            if (scroll != 0f) Debug.Log(Input.mousePosition);
        }
        else
        {
            // Check if there are two touches on the device
            if (Input.touchCount == 2)
            {
                // Get the touch positions from the previous frame
                Touch tZero = Input.GetTouch(0);
                Touch tOne = Input.GetTouch(1);

                Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
                Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

                // Calculate the previous and current distances between the touches
                float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
                float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

                // Adjust the camera's zoom
                Zoom(oldTouchDistance, currentTouchDistance);
            }
        }
    }

    void Zoom(float oldTouchDistance, float currentTouchDistance)
    {
        //oldTouchDistance / currentTouchDistance <=> newOrthoSize / cam.orthographicSize;
        float newOrthoSize = cam.orthographicSize * oldTouchDistance / currentTouchDistance;

        newOrthoSize = Mathf.Clamp(newOrthoSize, minOrthoSize, maxOrthoSize);
        cam.orthographicSize = newOrthoSize;
    }
}
