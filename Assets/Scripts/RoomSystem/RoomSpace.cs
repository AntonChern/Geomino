using System.Collections.Generic;
using System.Linq;
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
    //[SerializeField] private TMP_InputField roomName;
    //[SerializeField] private Transform container;
    //[SerializeField] private Transform playerSample;
    [SerializeField] private GameObject[] tiles;
    //[SerializeField] private GameObject hostTile;
    [SerializeField] private TMP_InputField[] nameFields;

    [SerializeField] private TMP_InputField oldNameField;

    [SerializeField] private TMP_Dropdown map;
    [SerializeField] private GameObject mapName;

    [SerializeField] private TextMeshProUGUI roomName;

    //private List<string> playerIds = new List<string>();
    //private int[] tilePositions;
    //private bool nameFieldShowed = false;
    private bool mapShowed = false;

    private int currentIndex = -1;

    private void Start()
    {
        Instance = this;

        //tilePositions = new int[3] { -1, -1, -1 };

        //NetworkManager.Singleton.OnServerStopped += OnRoomUpdated;
        //NetworkManager.Singleton.OnServerStarted += OnRoomUpdated;
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback; ;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback; ;
        //NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        RoomManager.Instance.OnUpdateRoom += OnUpdateRoom;

        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;

        startGameButton.onClick.AddListener(() =>
        {
            //RoomManager.Instance.StartGame();
            RoomManager.Instance.LockSession();
            NetworkManager.Singleton.SceneManager.LoadScene(RoomManager.Instance.GameScene, LoadSceneMode.Single);
            //RoomManager.Instance.ActiveSession;
        });
        //startGameButton.gameObject.SetActive(false);

        leaveGameButton.onClick.AddListener(() =>
        {
            Leave();
        });
        //leaveGameButton.gameObject.SetActive(false);

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

    //private async void OnClientConnectedCallback(ulong obj)
    //{
    //    Debug.Log($"OnClientConnectedCallback");
    //    var session = RoomManager.Instance.ActiveSession;

    //    if (session == null || !RoomManager.Instance.IsHost()) return;

    //    //int index = 0;
    //    foreach (IPlayer player in RoomManager.Instance.ActiveSession.AsHost().Players)
    //    {
    //        //player.Properties
    //        if (player.Properties[RoomManager.tilePosition].Value == null)
    //        {
    //            for (int i = 0; i < tilePositions.Length; i++)
    //            {
    //                if (tilePositions[i] == -1)
    //                {
    //                    tilePositions[i] = i;
    //                    //player.Properties[RoomManager.tilePosition] = new SessionProperty(i.ToString(), VisibilityPropertyOptions.Public);
    //                    player.SetProperty(RoomManager.tilePosition, new PlayerProperty(i.ToString(), VisibilityPropertyOptions.Member));
    //                    await RoomManager.Instance.ActiveSession.AsHost().SavePlayerDataAsync(player.Id);
    //                    Debug.Log($"Property changed {i}");
    //                    //RoomManager.Instance.ActiveSession.AsHost().SavePropertiesAsync();
    //                    break;
    //                }
    //            }
    //        }

    //        //var playerTile = tiles[int.Parse(player.Properties[RoomManager.tilePosition].Value)];
    //        //playerTile.SetActive(true);

    //        ////Transform playerTransform = Instantiate(playerSample, container);
    //        ////playerTransform.gameObject.SetActive(true);
    //        //RoomPlayer playerUI = playerTile.GetComponent<RoomPlayer>();
    //        //playerUI.SetKickPlayerButtonVisible(
    //        //    RoomManager.Instance.IsHost() &&
    //        //    player.Id != AuthenticationService.Instance.PlayerId
    //        //);
    //        //playerUI.UpdatePlayer(player);
    //    }

    //    foreach (var p in RoomManager.Instance.ActiveSession.Players)
    //    {
    //        Debug.Log($"{p.Properties["playerName"].Value} {p.Properties[RoomManager.tilePosition].Value}");
    //    }
    //}

    public void Leave()
    {
        RoomManager.Instance.LeaveSession();
        Hide();
        RoomList.Instance.Show();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
        RoomManager.Instance.OnUpdateRoom -= OnUpdateRoom;

        //NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    }

    //private void OnClientDisconnectCallback(ulong obj)
    //{
    //    Debug.Log("OnClientDisconnectCallback");
    //    UpdatePlayerList();
    //}

    //private void OnClientConnectedCallback(ulong obj)
    //{
    //    Debug.Log("OnClientConnectedCallback");
    //    UpdatePlayerList();
    //}

    private void OnPreShutdown()
    {
        //Debug.Log("OnPreShutdown");
        Hide();
        RoomList.Instance.Show();
    }

    private void OnUpdateRoom(object sender, System.EventArgs e)
    {
        //Debug.Log("RoomManager.Instance.OnUpdateRoom");
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        //Debug.Log($"Update Player List");
        var session = RoomManager.Instance.ActiveSession;
        //if (session != null) Debug.Log($"CLIENTS: {session.Players.Count}");
        //var existingSessions = await RoomManager.Instance.QuerySessions();
        //foreach (var existingSession in existingSessions)
        //{
        //    if (existingSession.Id == session.Id)
        //    {
        //        session = existingSession;
        //        break;
        //    }
        //}

        foreach (GameObject player in tiles)
        {
            player.SetActive(false);
        }
        //foreach (Transform child in container)
        //{
        //    if (child == playerSample) continue;

        //    Destroy(child.gameObject);
        //}

        if (session == null) return;



        //playerIds.Clear();
        //List<string> newPlayerIds = new List<string>();
        //Debug.Log($"session.Players.Count = {session.Players.Count}");
        int tileIndex = 0;
        int newCurrentIndex = 0;
        foreach (IReadOnlyPlayer player in session.Players)
        {
            //newPlayerIds.Add(player.Id);
            //if (player.Id == AuthenticationService.Instance.PlayerId && RoomManager.Instance.ActiveSession.) continue;
            //player.Properties
            //if (player.Properties[RoomManager.tilePosition].Value == null)
            //{
            //    foreach (int position in tilePositions)
            //    {
            //        if (position == -1)
            //        {
            //            RoomManager.Instance.ActiveSession.AsHost()
            //            break;
            //        }
            //    }
            //}
            //var tilePosition = player.Properties[RoomManager.tilePosition].Value;
            //Debug.Log($"tilePosition {tilePosition} " + (tilePosition == null));
            //if (tilePosition == null) continue;
            //if (tilePosition == "host")
            //{
            //    hostTile.GetComponent<RoomPlayer>().UpdatePlayer(player);
            //    continue;
            //}

            if (player.Id == session.CurrentPlayer.Id)
            {
                newCurrentIndex = tileIndex;
            };
            //var playerTile = tiles[int.Parse(tilePosition)];
            var playerTile = tiles[tileIndex++];
            playerTile.SetActive(true);

            //Transform playerTransform = Instantiate(playerSample, container);
            //playerTransform.gameObject.SetActive(true);
            RoomPlayer playerUI = playerTile.GetComponent<RoomPlayer>();
            //playerUI.SetNameFieldVisible(player.Id == AuthenticationService.Instance.PlayerId);
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
        //currentIndex = newCurrentIndex;
        //if (RoomManager.Instance.IsHost())
        //{
        //    foreach (string id in playerIds.Except(newPlayerIds))
        //    {
        //        Debug.Log($"Removed {id}");
        //        session.AsHost().RemovePlayerAsync(id);
        //    }
        //}
        //playerIds = newPlayerIds;

        //if (RoomManager.Instance.IsHost() && NetworkManager.Singleton.ConnectedClientsIds.Count == session.MaxPlayers)
        if (RoomManager.Instance.IsHost())
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
            {
                //startGameButton.gameObject.SetActive(true);
                EnableStartButton();
            }
            else
            {
                DisableStartButtonHost();
                // дождитесь хотя бы одного игрока
            }
        }
        else
        {
            DisableStartButtonClient();
            // дождитесь, пока хотс начнёт игру
            //startGameButton.gameObject.SetActive(false);
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

        //Debug.Log($"{RoomManager.Instance.ActiveSession != null}");
        //NetworkManager.Singleton.isActiveAndEnabled)
        //if (RoomManager.Instance.ActiveSession != null)
        //{
        //    leaveGameButton.gameObject.SetActive(true);
        //}
        //else
        //{
        //    leaveGameButton.gameObject.SetActive(false);
        //}
        //Debug.Log($"AvailableSlots = {RoomManager.Instance.ActiveSession.AvailableSlots}");

        roomName.text = session.Name;
    }

    private void ColorDisabled(Button button)
    {
        button.GetComponent<Image>().color = Color.gray;
        //for (int i = 0; i < 3; i++)
        //{
        //    button.transform.GetChild(i).GetComponent<Image>().color = Color.gray;
        //}
    }

    private void ColorEnabled(Button button)
    {
        button.GetComponent<Image>().color = Color.white;
        //for (int i = 0; i < 3; i++)
        //{
        //    button.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        //}
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
        //startGameButton.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Ожидание игроков...";
        //startGameButton.transform.GetChild(3).GetComponent<TextMeshProUGUI>().fontSize = 37;
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
        //startGameButton.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Ожидание, пока хост начнёт игру...";
        //startGameButton.transform.GetChild(3).GetComponent<TextMeshProUGUI>().fontSize = 37;
        ColorDisabled(startGameButton);
    }

    private void EnableStartButton()
    {
        //startGameButton.gameObject.SetActive(true);
        startGameButton.enabled = true;
        foreach (Transform child in startGameButton.transform)
        {
            if (child.GetComponent<TextMeshProUGUI>() == null) continue;
            child.GetComponent<TextMeshProUGUI>().text = "Начать";
            break;
        }
        //startGameButton.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Начать";
        //startGameButton.transform.GetChild(3).GetComponent<TextMeshProUGUI>().fontSize = 80;
        ColorEnabled(startGameButton);
    }

    private void ShowNameField()
    {
        //int index = nameFields.Length - 1;
        //string tilePosition = RoomManager.Instance.ActiveSession.CurrentPlayer.Properties[RoomManager.tilePosition].Value;
        //if (tilePosition != "host")
        //{
        //    index = int.Parse(tilePosition);
        //}
        nameFields[currentIndex].gameObject.SetActive(true);
        //nameFieldShowed = true;
    }

    private void HideNameField()
    {
        foreach (var nameField in nameFields)
        {
            nameField.gameObject.SetActive(false);
        }
        //nameFieldShowed = false;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        //roomName.text = RoomManager.Instance.ActiveSession.Name;

        oldNameField.gameObject.SetActive(false);
        //ShowNameField();
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
        //startGameButton.gameObject.SetActive(false);
        startGameButton.enabled = false;
        ColorDisabled(startGameButton);
        //leaveGameButton.gameObject.SetActive(false);
        HideNameField();
        mapShowed = false;
        map.gameObject.SetActive(false);
        mapName.SetActive(false);
        oldNameField.text = RoomManager.Instance.PlayerName;
        oldNameField.gameObject.SetActive(true);

        roomName.text = string.Empty;
        currentIndex = -1;
        HideTiles();

        gameObject.SetActive(false);
    }
}
