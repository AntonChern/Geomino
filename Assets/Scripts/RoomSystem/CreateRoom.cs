using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    public static CreateRoom Instance;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_Dropdown players;
    [SerializeField] private Toggle isPrivate;
    [SerializeField] private TMP_Dropdown map;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        createRoomButton.onClick.AddListener(() =>
        {
            RoomManager.Instance.CreateSessionAsHost(roomName.text, int.Parse(players.options[players.value].text), isPrivate.isOn);
            Hide();
            RoomSpace.Instance.Show();
        });
        cancelButton.onClick.AddListener(() =>
        {
            Cancel();
        });
        map.onValueChanged.AddListener((int value) =>
        {
            switch (map.options[value].text)
            {
                case "Бесконечная":
                    PlayerPrefs.SetString("map", "infinity");
                    break;
                case "Коридор":
                    PlayerPrefs.SetString("map", "hallway");
                    break;
                case "Гексагон":
                    PlayerPrefs.SetString("map", "hexagon");
                    break;
                case "Колонны":
                    PlayerPrefs.SetString("map", "columns");
                    break;
            }
            PlayerPrefs.Save();
        });
        PlayerPrefs.SetString("map", "infinity");
        PlayerPrefs.Save();

        Hide();

        isPrivate.gameObject.SetActive(false);

    }

    public void Cancel()
    {
        Hide();
        RoomList.Instance.Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        roomName.text = "Комната игрока " + RoomManager.Instance.PlayerName;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
