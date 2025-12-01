using Unity.Netcode;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class HallwayMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;
    private GameObject places;

    private MapScaler scaler;

    private float radius = 2.5f * Mathf.Sqrt(3) / 2;

    private void Awake()
    {
        if (NetworkManager.Singleton == null && PlayerPrefs.GetInt("map") != 1 ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "1")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        places = Instantiate(placesPrefab, gameObject.transform);
        scaler = places.GetComponent<MapScaler>();
    }

    private void PositionUpdate()
    {
        places.transform.position = new Vector3(Camera.main.transform.position.x, places.transform.position.y, places.transform.position.z);
    }

    public bool CanBePlaced(Vector2 position)
    {
        return position.y < radius && position.y > -radius;
    }

    public void DragMap(Vector2 offset)
    {
        scaler.SetDragTextureOffset(new Vector2(offset.x, 0f));
        PositionUpdate();
    }
}
