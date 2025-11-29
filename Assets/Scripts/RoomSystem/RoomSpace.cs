using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomSpace : MonoBehaviour
{
    public static RoomSpace Instance;

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveGameButton;
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private TMP_InputField[] nameFields;
    [SerializeField] private TextMeshProUGUI[] starCounters;

    [SerializeField] private TMP_InputField oldNameField;
    [SerializeField] private GameObject oldStars;

    [SerializeField] private TMP_Dropdown map;
    [SerializeField] private GameObject mapName;

    [SerializeField] private TextMeshProUGUI roomName;
    private bool mapShowed = false;

    private int currentIndex = -1;

    private void Start()
    {
        Instance = this;

        NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        RoomManager.Instance.OnUpdateRoom += OnUpdateRoom;

        startGameButton.onClick.AddListener(() =>
        {
            RoomManager.Instance.LockSession();
            NetworkManager.Singleton.SceneManager.LoadScene(RoomManager.Instance.GameScene, LoadSceneMode.Single);
        });

        leaveGameButton.onClick.AddListener(() =>
        {
            Leave();
        });

        map.onValueChanged.AddListener((int value) =>
        {
            RoomManager.Instance.UpdateMap(MapHandler.TranslateToEnglish(map.options[map.value].text));
        });

        UpdatePlayerList();
        Hide();

        if (RoomManager.Instance.ActiveSession != null)
        {
            Show();
        }
    }

    public void Leave()
    {
        RoomManager.Instance.LeaveSession();
        Hide();
        RoomList.Instance.Show();
    }

    private void OnDestroy()
    {
        RoomManager.Instance.OnUpdateRoom -= OnUpdateRoom;
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
    }

    private void OnPreShutdown()
    {
        Hide();
        RoomList.Instance.Show();
    }

    private void OnUpdateRoom(object sender, System.EventArgs e)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        var session = RoomManager.Instance.ActiveSession;

        foreach (GameObject player in tiles)
        {
            player.SetActive(false);
        }

        if (session == null) return;

        int tileIndex = 0;
        int newCurrentIndex = 0;
        foreach (IReadOnlyPlayer player in session.Players)
        {
            if (player.Id == session.CurrentPlayer.Id)
            {
                newCurrentIndex = tileIndex;
            };
            var playerTile = tiles[tileIndex++];
            playerTile.SetActive(true);

            RoomPlayer playerUI = playerTile.GetComponent<RoomPlayer>();
            playerUI.SetKickPlayerButtonVisible(
                RoomManager.Instance.IsHost() &&
                player.Id != AuthenticationService.Instance.PlayerId
            );
            playerUI.UpdatePlayer(player);
        }

        if (currentIndex != newCurrentIndex)
        {
            currentIndex = newCurrentIndex;
            HideNameField();
            ShowNameField();
        }

        if (RoomManager.Instance.IsHost())
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
            {
                EnableStartButton();
            }
            else
            {
                DisableStartButtonHost();
            }
        }
        else
        {
            DisableStartButtonClient();
        }

        if (RoomManager.Instance.IsHost())
        {
            map.gameObject.SetActive(true);
            if (!mapShowed)
            {
                for (int i = 0; i < map.options.Count; i++)
                {
                    if (map.options[i].text == MapHandler.TranslateToRussian(session.Properties[RoomManager.mapProperty].Value))
                    {
                        map.value = i;
                        break;
                    }
                }
                mapShowed = true;
            }
        }
        else
        {
            mapName.SetActive(true);
            mapName.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = MapHandler.TranslateToRussian(session.Properties[RoomManager.mapProperty].Value);
        }

        roomName.text = session.Name;
    }

    private void ColorDisabled(Button button)
    {
        button.GetComponent<Image>().color = Color.gray;
    }

    private void ColorEnabled(Button button)
    {
        button.GetComponent<Image>().color = Color.white;
    }

    private void DisableStartButtonHost()
    {
        startGameButton.enabled = false;
        foreach (Transform child in startGameButton.transform)
        {
            if (child.GetComponent<TextMeshProUGUI>() == null) continue;
            child.GetComponent<TextMeshProUGUI>().text = "Ожидание игроков...";
            break;
        }
        ColorDisabled(startGameButton);
    }

    private void DisableStartButtonClient()
    {
        startGameButton.enabled = false;
        foreach (Transform child in startGameButton.transform)
        {
            if (child.GetComponent<TextMeshProUGUI>() == null) continue;
            child.GetComponent<TextMeshProUGUI>().text = "Ожидание, пока хост начнёт игру...";
            break;
        }
        ColorDisabled(startGameButton);
    }

    private void EnableStartButton()
    {
        startGameButton.enabled = true;
        foreach (Transform child in startGameButton.transform)
        {
            if (child.GetComponent<TextMeshProUGUI>() == null) continue;
            child.GetComponent<TextMeshProUGUI>().text = "Начать";
            break;
        }
        ColorEnabled(startGameButton);
    }

    private void ShowNameField()
    {
        nameFields[currentIndex].gameObject.SetActive(true);
    }

    private void HideNameField()
    {
        foreach (var nameField in nameFields)
        {
            nameField.gameObject.SetActive(false);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        oldNameField.gameObject.SetActive(false);
        oldStars.SetActive(false);
    }

    private void HideTiles()
    {
        foreach (var tile in tiles)
        {
            tile.SetActive(false);
        }
    }

    public void Hide()
    {
        startGameButton.enabled = false;
        ColorDisabled(startGameButton);
        HideNameField();
        mapShowed = false;
        map.gameObject.SetActive(false);
        mapName.SetActive(false);
        oldNameField.text = RoomManager.Instance.PlayerName;
        oldNameField.gameObject.SetActive(true);
        oldStars.SetActive(true);

        roomName.text = string.Empty;
        currentIndex = -1;
        HideTiles();

        gameObject.SetActive(false);
    }
}
