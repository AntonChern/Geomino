using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class RoomSpace : MonoBehaviour
{
    public static RoomSpace Instance;

    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI hintText;
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

    private bool readiness = false;

    private int currentIndex = -1;

    private string[] waitingClientText = new string[]
    {
        "Ожидание, пока хост начнёт игру...",
        "Waiting for the host to start the game..."
    };
    private string[] waitingHostText = new string[]
    {
        "Ожидание игроков...",
        "Waiting for the players..."
    };
    private string[] startText = new string[]
    {
        "Начать",
        "Start"
    };
    private string[] readyText = new string[]
    {
        "Готов",
        "Ready"
    };
    private string[] notReadyText = new string[]
    {
        "Не готов",
        "Not ready"
    };

    private void Start()
    {
        Instance = this;

        NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        RoomManager.Instance.OnUpdateRoom += OnUpdateRoom;

        startGameButton.onClick.AddListener(() =>
        {
            if (RoomManager.Instance.IsHost())
            {
                RoomManager.Instance.LockSession();
                NetworkManager.Singleton.SceneManager.LoadScene(RoomManager.Instance.GameScene, LoadSceneMode.Single);
            }
            else
            {
                readiness = !readiness;
                RoomManager.Instance.UpdateReadiness(readiness);
            }
        });

        leaveGameButton.onClick.AddListener(() =>
        {
            Leave();
        });

        map.onValueChanged.AddListener((int value) =>
        {
            //RoomManager.Instance.UpdateMap(MapHandler.TranslateToEnglish(map.options[map.value].text));
            RoomManager.Instance.UpdateMap(map.value.ToString());
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
            if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1 && AllReady())
            {
                EnableStartButton(startText[CorrectLang.langIndices[YG2.lang]]);
                hintText.text = string.Empty;
            }
            else
            {
                DisableStartButton();
                hintText.text = waitingHostText[CorrectLang.langIndices[YG2.lang]];
            }
        }
        else
        {
            if (readiness)
            {
                EnableStartButton(notReadyText[CorrectLang.langIndices[YG2.lang]]);
                if (AllReady())
                {
                    hintText.text = waitingClientText[CorrectLang.langIndices[YG2.lang]];
                }
                else
                {
                    hintText.text = waitingHostText[CorrectLang.langIndices[YG2.lang]];
                }
            }
            else
            {
                EnableStartButton(readyText[CorrectLang.langIndices[YG2.lang]]);
                hintText.text = string.Empty;
            }
        }

        if (RoomManager.Instance.IsHost())
        {
            map.gameObject.SetActive(true);
            if (!mapShowed)
            {
                for (int i = 0; i < map.options.Count; i++)
                {
                    if (map.options[i].text == MapHandler.GetMapByIndex(session.Properties[RoomManager.mapProperty].Value))
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
            mapName.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = MapHandler.GetMapByIndex(session.Properties[RoomManager.mapProperty].Value);
        }

        roomName.text = session.Name;
    }

    private bool AllReady()
    {
        var session = RoomManager.Instance.ActiveSession;
        if (session == null) return false;
        bool result = true;
        foreach (IReadOnlyPlayer player in session.Players)
        {
            result = result && bool.Parse(player.Properties[RoomManager.readinessPropertyKey].Value);
        }
        return result;
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
            child.GetComponent<TextMeshProUGUI>().text = waitingHostText[CorrectLang.langIndices[YG2.lang]];
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
            child.GetComponent<TextMeshProUGUI>().text = waitingClientText[CorrectLang.langIndices[YG2.lang]];
            break;
        }
        ColorDisabled(startGameButton);
    }

    private void DisableStartButton()
    {
        startGameButton.enabled = false;
        ColorDisabled(startGameButton);
    }

    private void EnableStartButton(string text)
    {
        startGameButton.enabled = true;
        foreach (Transform child in startGameButton.transform)
        {
            if (child.GetComponent<TextMeshProUGUI>() == null) continue;
            child.GetComponent<TextMeshProUGUI>().text = text;
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
