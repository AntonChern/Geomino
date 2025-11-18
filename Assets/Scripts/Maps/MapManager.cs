using UnityEngine;
using UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    private IMap map;

    [SerializeField] private GameObject background;
    private MapScaler backgroundScaler;
    private Vector2 backgroundOffset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        backgroundScaler = background.GetComponent<MapScaler>();
        backgroundOffset = background.GetComponent<Renderer>().material.mainTextureOffset;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                map = transform.GetChild(i).GetComponent<IMap>();
                break;
            }
        }
    }

    public bool CanBePlaced(Vector2 position)
    {
        return map.CanBePlaced(position);
    }

    public void DragBackground(Vector3 offset)
    {
        backgroundScaler.SetDragTextureOffset(offset);
        //backgroundOffset -= new Vector2(offset.x, offset.y * 15f / 26f);
        //backgroundRenderer.material.mainTextureOffset = backgroundOffset;
        background.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, background.transform.position.z);

        map.DragMap(offset);
    }
}

public static class MapHandler
{
    public static string TranslateToEnglish(string map)
    {
        string result = string.Empty;
        switch (map)
        {
            case "Бесконечная":
                result = "infinity";
                break;
            case "Коридор":
                result = "hallway";
                break;
            case "Гексагон":
                result = "hexagon";
                break;
            case "Колонны":
                result = "columns";
                break;
        }
        return result;
    }

    public static string TranslateToRussian(string map)
    {
        string result = string.Empty;
        switch (map)
        {
            case "infinity":
                result = "Бесконечная";
                break;
            case "hallway":
                result = "Коридор";
                break;
            case "hexagon":
                result = "Гексагон";
                break;
            case "columns":
                result = "Колонны";
                break;
        }
        return result;
    }
}