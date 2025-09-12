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
    [SerializeField] private Transform container;
    [SerializeField] private Transform playerSample;

    private void Start()
    {
        Instance = this;

        //NetworkManager.Singleton.OnServerStopped += OnRoomUpdated;
        //NetworkManager.Singleton.OnServerStarted += OnRoomUpdated;
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback; ;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback; ;
        //NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        RoomManager.Instance.OnUpdateRoom += OnUpdateRoom;

        startGameButton.onClick.AddListener(() =>
        {
            //RoomManager.Instance.StartGame();
            NetworkManager.Singleton.SceneManager.LoadScene(RoomManager.Instance.GameScene, LoadSceneMode.Single);
            //RoomManager.Instance.ActiveSession;
        });
        //startGameButton.gameObject.SetActive(false);

        leaveGameButton.onClick.AddListener(() =>
        {
            Leave();
        });
        //leaveGameButton.gameObject.SetActive(false);

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
        NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
        RoomManager.Instance.OnUpdateRoom -= OnUpdateRoom;
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
        //var existingSessions = await RoomManager.Instance.QuerySessions();
        //foreach (var existingSession in existingSessions)
        //{
        //    if (existingSession.Id == session.Id)
        //    {
        //        session = existingSession;
        //        break;
        //    }
        //}

        foreach (Transform child in container)
        {
            if (child == playerSample) continue;

            Destroy(child.gameObject);
        }

        if (session == null) return;

        //Debug.Log($"session.Players.Count = {session.Players.Count}");
        foreach (IReadOnlyPlayer player in session.Players)
        {
            //player.Properties

            Transform playerTransform = Instantiate(playerSample, container);
            playerTransform.gameObject.SetActive(true);
            RoomPlayer playerUI = playerTransform.GetComponent<RoomPlayer>();
            playerUI.SetKickPlayerButtonVisible(
                RoomManager.Instance.IsHost() &&
                player.Id != AuthenticationService.Instance.PlayerId
            );
            playerUI.UpdatePlayer(player);
        }

        if (RoomManager.Instance.IsHost() && NetworkManager.Singleton.ConnectedClientsIds.Count == session.MaxPlayers)
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }

        //Debug.Log($"{RoomManager.Instance.ActiveSession != null}");
        //NetworkManager.Singleton.isActiveAndEnabled)
        if (RoomManager.Instance.ActiveSession != null)
        {
            leaveGameButton.gameObject.SetActive(true);
        }
        else
        {
            leaveGameButton.gameObject.SetActive(false);
        }
        //Debug.Log($"AvailableSlots = {RoomManager.Instance.ActiveSession.AvailableSlots}");

    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        startGameButton.gameObject.SetActive(false);
        leaveGameButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
