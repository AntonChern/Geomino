using UnityEngine;

public class HallwayMap : MonoBehaviour, IMap
{
    //[SerializeField] private GameObject places;
    private Vector2 backgroundOffset;

    private float radius = 2.5f * Mathf.Sqrt(3) / 2;

    private void Awake()
    {
        if (PlayerPrefs.GetString("map") != "hallway")
        {
            gameObject.SetActive(false);
        }
        backgroundOffset = GetComponent<Renderer>().material.mainTextureOffset;
    }

    private void PositionUpdate()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y, transform.position.z);
    }

    public bool CanBePlaced(Vector2 position)
    {
        return position.y < radius && position.y > -radius;
    }

    public void DragMap(Vector2 offset)
    {
        backgroundOffset -= new Vector2(offset.x / transform.localScale.x, 0f);
        GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        PositionUpdate();
    }
}
