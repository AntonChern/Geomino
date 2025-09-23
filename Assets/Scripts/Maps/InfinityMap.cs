using UnityEngine;
using UnityEngine.UIElements;

public class InfinityMap : MonoBehaviour, IMap
{
    //[SerializeField] private GameObject places;
    private Vector2 backgroundOffset;

    private void Awake()
    {
        if (PlayerPrefs.GetString("map") != "infinity")
        {
            gameObject.SetActive(false);
        }
        backgroundOffset = GetComponent<Renderer>().material.mainTextureOffset;
    }

    private void PositionUpdate()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
    }

    public bool CanBePlaced(Vector2 position)
    {
        return true;
    }

    public void DragMap(Vector2 offset)
    {
        backgroundOffset -= new Vector2(offset.x / transform.localScale.x, offset.y / transform.localScale.y);
        GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        PositionUpdate();
    }
}
