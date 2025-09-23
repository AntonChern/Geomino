using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class ColumnsMap : MonoBehaviour, IMap
{
    private Vector2 backgroundOffset;
    private float cameraOffset;

    private void Awake()
    {
        if (PlayerPrefs.GetString("map") != "columns")
        {
            gameObject.SetActive(false);
        }

        backgroundOffset = GetComponent<Renderer>().material.mainTextureOffset;
        cameraOffset = transform.position.y - Camera.main.transform.position.y; // =13/18
    }

    private void PositionUpdate()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, cameraOffset + Camera.main.transform.position.y, transform.position.z);
    }

    //private bool InsideCircle(Vector2 center, float radius, Vector2 point)
    //{
    //    return Vector2.Distance(center, point) < radius;
    //}

    public bool CanBePlaced(Vector2 position)
    {
        if ((int)position.x == 0 && (int)position.y == 0 || (int)Mathf.Round(position.y / (Mathf.Sqrt(3) / 6)) % 3 != 0) return true;
        //Debug.Log($"{-3 % 3}");
        Debug.Log($"{position} = {!((int)Mathf.Round(2f * position.y / Mathf.Sqrt(3)) % 2 == 0 && (int)Mathf.Round(position.x) % 3 == 0 || (int)Mathf.Round(2f * position.y / Mathf.Sqrt(3)) % 2 == 1 && ((int)Mathf.Round(position.x - 1.5f)) % 3 == 0)}, {Mathf.Round(2f * position.y / Mathf.Sqrt(3))} {Mathf.Round(position.x)} {Mathf.Round(position.x - 1.5f)}");
        return !((int)Mathf.Round(position.y / (Mathf.Sqrt(3) / 2)) % 2 == 0 && (int)Mathf.Round(position.x) % 3 == 0 ||
                 (int)Mathf.Round(position.y / (Mathf.Sqrt(3) / 2)) % 2 != 0 && ((int)Mathf.Round(position.x - 1.5f)) % 3 == 0);
        //return !((int)Mathf.Round(position.y / Mathf.Sqrt(3)) % 2 == 0 && (int)Mathf.Round(position.x) % 3 == 0 ||
        //       (int)Mathf.Round((position.y - 2f * Mathf.Sqrt(3) / 3) / Mathf.Sqrt(3)) % 2 == 0 && ((int)Mathf.Round(position.x) - 1.5f) % 3 == 0);
    }

    public void DragMap(Vector2 offset)
    {
        backgroundOffset -= new Vector2(offset.x / transform.localScale.x, offset.y / transform.localScale.y);
        GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        PositionUpdate();
    }
}
