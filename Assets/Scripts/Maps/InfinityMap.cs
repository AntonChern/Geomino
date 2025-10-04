using Unity.Netcode;
using UnityEngine;

public class InfinityMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;

    private GameObject places;
    private Vector2 backgroundOffset;

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
        backgroundOffset = places.GetComponent<Renderer>().material.mainTextureOffset;
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
        backgroundOffset -= new Vector2(offset.x / places.transform.localScale.x, offset.y / places.transform.localScale.y);
        places.GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        PositionUpdate();
    }
}
