using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dragger : MonoBehaviour
{
    [SerializeField] private Button home;

    private Vector3 dragOrigin;
    private Vector3 initialPosition;
    private bool dragStarted;

    private void Start()
    {
        initialPosition = transform.position;

        home.onClick.AddListener(() =>
        {
            ResetPosition();
        });
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
            if (!dragStarted) return;

            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offset = currentMousePos - dragOrigin;

            transform.position -= offset;

            MapManager.Instance.DragBackground(offset);
        }

        if (Input.GetMouseButtonUp(1))
        {
            dragStarted = false;
        }
    }
}
