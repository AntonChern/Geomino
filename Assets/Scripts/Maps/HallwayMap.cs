using Unity.Netcode;
using UnityEngine;

public class HallwayMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;
    private GameObject places;

    //private Vector2 backgroundOffset;
    private MapScaler scaler;

    private float radius = 2.5f * Mathf.Sqrt(3) / 2;

    private void Awake()
    {
        if (NetworkManager.Singleton == null && PlayerPrefs.GetString("map") != "hallway" ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "hallway")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        places = Instantiate(placesPrefab, gameObject.transform);
        scaler = places.GetComponent<MapScaler>();
        //backgroundOffset = places.GetComponent<Renderer>().material.mainTextureOffset;
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
        //backgroundOffset -= new Vector2(offset.x / places.transform.localScale.x, 0f);
        //places.GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        scaler.SetDragTextureOffset(new Vector2(offset.x, 0f));
        PositionUpdate();
    }
}
