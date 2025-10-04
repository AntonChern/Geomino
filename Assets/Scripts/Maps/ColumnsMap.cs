using Unity.Netcode;
using UnityEngine;

public class ColumnsMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject placesPrefab;
    private GameObject places;

    private Vector2 backgroundOffset;
    private float cameraOffset;

    private void Awake()
    {
        if (NetworkManager.Singleton == null && PlayerPrefs.GetString("map") != "columns" ||
            NetworkManager.Singleton != null && RoomManager.Instance.ActiveSession.Properties[RoomManager.mapProperty].Value != "columns")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        places = Instantiate(placesPrefab, gameObject.transform);

        backgroundOffset = places.GetComponent<Renderer>().material.mainTextureOffset;
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
        backgroundOffset -= new Vector2(offset.x / places.transform.localScale.x, offset.y / places.transform.localScale.y);
        places.GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        PositionUpdate();
    }
}
