using Unity.Netcode;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class HexagonMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;

    private float radius = 4f * Mathf.Sqrt(3) / 2;
    private Vector2 offset = new Vector2(0f, Mathf.Sqrt(3) / 3);
    private float k = Mathf.Sqrt(3);
    private float b = 4f * Mathf.Sqrt(3);

    private void Awake()
    {
        if (NetworkManager.Singleton == null && PlayerPrefs.GetInt("map") != 2 ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "2")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
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
