using Unity.Netcode;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class InfinityMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;

    private GameObject places;
    private MapScaler scaler;

    private void Awake()
    {

        if (NetworkManager.Singleton == null && PlayerPrefs.GetString("map") != "infinity" ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "infinity")
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
        places.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, places.transform.position.z);
    }

    public bool CanBePlaced(Vector2 position)
    {
        return true;
    }

    public void DragMap(Vector2 offset)
    {
        scaler.SetDragTextureOffset(offset);
        PositionUpdate();
    }
}
