using UnityEngine;

public class Scaler : MonoBehaviour
{
    public float minOrthoSize = 5f;
    public float maxOrthoSize = 15f;
    public float zoomSpeed = 1f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newOrthoSize = cam.orthographicSize * (1 - scroll * zoomSpeed);

        newOrthoSize = Mathf.Clamp(newOrthoSize, minOrthoSize, maxOrthoSize);

        cam.orthographicSize = newOrthoSize;
    }
}
