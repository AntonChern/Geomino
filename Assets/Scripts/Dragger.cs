using UnityEngine;
using UnityEngine.InputSystem;

public class Dragger : MonoBehaviour
{
    private Vector3 dragOrigin;
    private Vector2 backgroundOffset;

    //private float maxHeight;
    //private float maxWidth;

    private void Start()
    {
        backgroundOffset = transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offset = currentMousePos - dragOrigin;

            //transform.position = new Vector3(
            //    Mathf.Clamp(transform.position.x - offset.x, -maxWidth, maxWidth),
            //    Mathf.Clamp(transform.position.y - offset.y, -maxHeight, maxHeight),
            //    transform.position.z
            //);

            transform.position -= offset;
            //transform.position = transform.position - offset;
            //Debug.Log(maxWidth + ", " + maxHeight);

            backgroundOffset -= new Vector2(offset.x / transform.GetChild(0).transform.localScale.x, offset.y / transform.GetChild(0).transform.localScale.y);
            transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
            //transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset += new Vector2(Time.time, 0f);
        }
    }
}
