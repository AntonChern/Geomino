using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Dragger : MonoBehaviour
{
    [SerializeField] private Button home;

    private Vector3 dragOrigin;
    //private Vector2 backgroundOffset;

    //private float maxHeight;
    //private float maxWidth;
    private Vector3 initialPosition;
    private bool dragStarted;

    private void Start()
    {
        initialPosition = transform.position;

        home.onClick.AddListener(() =>
        {
            ResetPosition();
        });
        //backgroundOffset = transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset;
    }

    private void ResetPosition()
    {
        Vector3 offset = initialPosition - transform.position;
        transform.position = initialPosition;
        MapManager.Instance.DragBackground(-offset);
    }

    private bool Pointable()
    {
        PointerEventData eventData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("UIElement"))
                return false;
        }
        return true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!Pointable()) return;
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragStarted = true;
        }

        if (Input.GetMouseButton(1))
        {
            //if (!Pointable()) return;
            if (!dragStarted) return;

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

            MapManager.Instance.DragBackground(offset);
            //backgroundOffset -= new Vector2(offset.x / transform.GetChild(0).transform.localScale.x, offset.y / transform.GetChild(0).transform.localScale.y);
            //transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
            //transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset += new Vector2(Time.time, 0f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            dragStarted = false;
        }
    }
}
