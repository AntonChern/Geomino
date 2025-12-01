using Unity.Netcode;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class ColumnsMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;
    private GameObject places;
    private MapScaler scaler;

    private float cameraOffset;

    private void Awake()
    {
        if (NetworkManager.Singleton == null && PlayerPrefs.GetInt("map") != 3 ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "3")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        places = Instantiate(placesPrefab, gameObject.transform);
        scaler = places.GetComponent<MapScaler>();
        cameraOffset = places.transform.position.y - Camera.main.transform.position.y;
    }

    private void PositionUpdate()
    {
        places.transform.position = new Vector3(Camera.main.transform.position.x, cameraOffset + Camera.main.transform.position.y, places.transform.position.z);
    }

    public bool CanBePlaced(Vector2 position)
    {
        if ((int)position.x == 0 && (int)position.y == 0 || (int)Mathf.Round(position.y / (Mathf.Sqrt(3) / 6)) % 3 != 0) return true;
        return !((int)Mathf.Round(position.y / (Mathf.Sqrt(3) / 2)) % 2 == 0 && (int)Mathf.Round(position.x) % 3 == 0 ||
                 (int)Mathf.Round(position.y / (Mathf.Sqrt(3) / 2)) % 2 != 0 && ((int)Mathf.Round(position.x - 1.5f)) % 3 == 0);
    }

    public void DragMap(Vector2 offset)
    {
        scaler.SetDragTextureOffset(offset);
        PositionUpdate();
    }
}
