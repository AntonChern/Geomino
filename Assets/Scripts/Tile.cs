using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    private bool pointerOver = false;
    private bool pointerDown = false;

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

    private void OnMouseDown()
    {
        if (!gameObject.CompareTag("Place"))
            return;

        if (!Pointable()) return;

        pointerDown = true;
    }

    private void OnMouseUp()
    {
        if (!gameObject.CompareTag("Place"))
            return;

        if (!pointerOver || !pointerDown) return;
        pointerDown = false;
        gameObject.GetComponent<ITile>().Dice();
    }

    private void OnMouseOver()
    {
        pointerOver = true;

        if (!Pointable()) return;

        if (gameObject.CompareTag("Place"))
            ChangeColor(gameObject, Color.yellow);
    }

    public void ChangeColor(GameObject obj, Color color)
    {
        obj.GetComponent<SpriteRenderer>().color = color;
        foreach (Transform child in obj.transform)
        {
            ChangeColor(child.gameObject, color);
        }
    }

    private void OnMouseExit()
    {
        pointerOver = false;

        if (gameObject.CompareTag("Place"))
            ChangeColor(gameObject, Color.gray);
    }
}
