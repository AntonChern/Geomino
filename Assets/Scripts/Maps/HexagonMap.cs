using UnityEngine;

public class HexagonMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject places;

    private float radius = 4f * Mathf.Sqrt(3) / 2;
    private Vector2 offset = new Vector2(0f, Mathf.Sqrt(3) / 3);
    private float k = Mathf.Sqrt(3);
    private float b = 4f * Mathf.Sqrt(3);

    private void Awake()
    {
        if (PlayerPrefs.GetString("map") == "hexagon")
        {
            places.gameObject.SetActive(true);
        }
    }

    public bool CanBePlaced(Vector2 position)
    {
        Vector2 target = position - offset;
        return target.y < radius && target.y > -radius &&
               target.y < k * target.x + b && target.y > k * target.x - b &&
               target.y < -k * target.x + b && target.y > -k * target.x - b;
    }

    public void DragMap(Vector2 backgroundOffset)
    {
        return;
    }
}
