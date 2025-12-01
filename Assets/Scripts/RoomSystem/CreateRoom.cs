using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;
using YG.LanguageLegacy;

public class CreateRoom : MonoBehaviour
{
    public static CreateRoom Instance;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_Dropdown players;
    [SerializeField] private Toggle isPrivate;
    [SerializeField] private TMP_Dropdown map;
    private string[] roomNameWarning = new string[]
    {
        "Название комнаты не может быть пустым!",
        "Room name cannot be empty!"
    };
    private string[] roomNameTemplate = new string[]
    {
        "Комната игрока ",
        " player's room"
    };
    private string[] createText = new string[]
    {
        "Создать",
        "Create"
    };
    private Func<string, string, string>[] constructRoomName = new Func<string, string, string>[]
    {
        (string template, string playerName) => { return template + playerName; },
        (string template, string playerName) => { return playerName + template; }
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        createRoomButton.onClick.AddListener(() =>
        {
            //RoomManager.Instance.CreateSessionAsHost(roomName.text, int.Parse(players.options[players.value].text), isPrivate.isOn, MapHandler.TranslateToEnglish(map.options[map.value].text));
            RoomManager.Instance.CreateSessionAsHost(roomName.text, int.Parse(players.options[players.value].text), isPrivate.isOn, map.value.ToString());
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
                    child.GetComponent<TextMeshProUGUI>().text = roomNameWarning[CorrectLang.langIndices[YG2.lang]];
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
                    child.GetComponent<TextMeshProUGUI>().text = createText[CorrectLang.langIndices[YG2.lang]];
                    break;
                }
            }
        });

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
        roomName.text = constructRoomName[CorrectLang.langIndices[YG2.lang]](
            roomNameTemplate[CorrectLang.langIndices[YG2.lang]],
            RoomManager.Instance.PlayerName
        );
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
