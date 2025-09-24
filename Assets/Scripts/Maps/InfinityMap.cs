using UnityEngine;
using UnityEngine.UIElements;

public class InfinityMap : MonoBehaviour, IMap
{
    [SerializeField] private GameObject places;
    private Vector2 backgroundOffset;

    private void Awake()
    {
        if (PlayerPrefs.GetString("map") == "infinity")
        {
            places.SetActive(true);
        }
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
