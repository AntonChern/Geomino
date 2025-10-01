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
            RoomManager.Instance.CreateSessionAsHost(roomName.text, int.Parse(players.options[players.value].text), isPrivate.isOn, MapHandler.TranslateToEnglish(map.options[map.value].text));
            //RoomManager.Instance.CreateSessionAsHost(roomName.text, isPrivate.isOn, map.options[map.value].text);
            Hide();
            RoomSpace.Instance.Show();
        });
        cancelButton.onClick.AddListener(() =>
        {
            Cancel();
        });
        roomName.onValueChanged.AddListener((string value) =>
        {
            if (value == string.Empty)
            {
                createRoomButton.enabled = false;
                createRoomButton.GetComponent<Image>().color = Color.gray;
                foreach (Transform child in createRoomButton.transform)
                {
                    if (child.GetComponent<TextMeshProUGUI>() == null) continue;
                    child.GetComponent<TextMeshProUGUI>().text = "Название комнаты не может быть пустым!";
                    break;
                }
            }
            else
            {
                createRoomButton.enabled = true;
                createRoomButton.GetComponent<Image>().color = Color.white;
                foreach (Transform child in createRoomButton.transform)
                {
                    if (child.GetComponent<TextMeshProUGUI>() == null) continue;
                    child.GetComponent<TextMeshProUGUI>().text = "Создать";
                    break;
                }
            }
        });
        //map.onValueChanged.AddListener((int value) =>
        //{
        //switch (map.options[value].text)
        //{
        //    case "Бесконечная":
        //        PlayerPrefs.SetString("map", "infinity");
        //        break;
        //    case "Коридор":
        //        PlayerPrefs.SetString("map", "hallway");
        //        break;
        //    case "Гексагон":
        //        PlayerPrefs.SetString("map", "hexagon");
        //        break;
        //    case "Колонны":
        //        PlayerPrefs.SetString("map", "columns");
        //        break;
        //}
        //PlayerPrefs.Save();
        //});
        //PlayerPrefs.SetString("map", "infinity");
        //PlayerPrefs.Save();

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
