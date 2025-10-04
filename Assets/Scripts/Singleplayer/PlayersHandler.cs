using UnityEngine;
using UnityEngine.UI;

public class PlayersHandler : MonoBehaviour
{
    [SerializeField] private Button addBot2Button;
    [SerializeField] private Button addBot3Button;

    [SerializeField] private GameObject bot2Tile;
    [SerializeField] private GameObject bot3Tile;


    private void Start()
    {
        addBot2Button.onClick.AddListener(() =>
        {
            bot2Tile.SetActive(true);
        });
        addBot3Button.onClick.AddListener(() =>
        {
            bot3Tile.SetActive(true);
        });
    }
}
