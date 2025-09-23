using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    private IMap map;

    [SerializeField] private GameObject background;
    //[SerializeField] private GameObject hallwayWalls;
    private Vector2 backgroundOffset;

    private void Awake()
    {
        Instance = this;

        //switch (PlayerPrefs.GetString("map"))
        //{
        //    case "infinity":
        //        //Debug.Log("infinity");
        //        map = new InfinityMap();
        //        break;
        //    case "hallway":
        //        //Debug.Log("коридор");
        //        map = new HallwayMap();
        //        break;

        //}
    }

    private void Start()
    {
        backgroundOffset = background.GetComponent<Renderer>().material.mainTextureOffset;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                map = transform.GetChild(i).GetComponent<IMap>();
                break;
            }
        }
        //map = Camera.main.transform
    }

    public bool CanBePlaced(Vector2 position)
    {
        return map.CanBePlaced(position);
    }

    public void DragBackground(Vector3 offset)
    {
        backgroundOffset -= new Vector2(offset.x / background.transform.localScale.x, offset.y / background.transform.localScale.y);
        background.GetComponent<Renderer>().material.mainTextureOffset = backgroundOffset;
        
        map.DragMap(offset);
    }
}
