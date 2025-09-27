using Unity.Netcode;
using UnityEngine;

public class HexagonMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;
    //private GameObject places;

    private float radius = 4f * Mathf.Sqrt(3) / 2;
    private Vector2 offset = new Vector2(0f, Mathf.Sqrt(3) / 3);
    private float k = Mathf.Sqrt(3);
    private float b = 4f * Mathf.Sqrt(3);

    private void Awake()
    {
        if (NetworkManager.Singleton == null && PlayerPrefs.GetString("map") != "hexagon" ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "hexagon")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        //places.gameObject.SetActive(true);
        Instantiate(placesPrefab, gameObject.transform);
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
