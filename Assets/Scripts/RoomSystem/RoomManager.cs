using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    ISession activeSession;

    public ISession ActiveSession {
        get => activeSession;
        set {
            activeSession = value;
            Debug.Log($"Active session: {activeSession}");
        }
    }

    private float sessionPollTimer = 0f;
    private float sessionPollTimerMax = 1.1f;

    public event EventHandler OnUpdateRoom;

    //private bool gameStarted = false;
    private string playerName = "Name";
    public string PlayerName
    {
        get => playerName;
        set {  playerName = value; }
    }

    public string GameScene = "MultiplayerGame";

    //NetworkManager networkManager;

    string sessionName = "MyRoom";

    const string playerNamePropertyKey = "playerName";

    private void Awake()
    {
        //Debug.Log($"NetworkManager.Singleton.LocalClientId {NetworkManager.Singleton.LocalClientId}");
        if (Instance != null)
        {
            //this.ActiveSession = Instance.ActiveSession;
            //Destroy(Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        string existingPlayerName = PlayerPrefs.GetString(playerNamePropertyKey);
        if (existingPlayerName != "")
            playerName = existingPlayerName;
    }

    //private void SceneManager_OnLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    //{
    //    if (sceneName == "GameScene")
    //    {
    //        StartGame();
    //    }
    //    else
    //    {
    //        EndGame();
    //    }
    //}

    async void Start()
    {
        try
        {
            //networkManager = gameObject.GetComponent<NetworkManager>();
            //networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            //networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
            //networkManager.SceneManager.OnLoad += SceneManager_OnLoad;
            //Debug.Log(networkManager.SceneManager == null);

            //Debug.Log($"Initialized? {UnityServices.State == ServicesInitializationState.Initialized}");
            //if (UnityServices.State != ServicesInitializationState.Initialized)
            //{
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //}
            Debug.Log($"Signed in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void OnDestroy()
    {
        //UnityServices.
        //Debug.Log($"Sign Out");
        AuthenticationService.Instance.SignOut();
    }

    private void Update()
    {
        if (ActiveSession == null || SceneManager.GetActiveScene().name == GameScene) return;
        sessionPollTimer += Time.deltaTime;
        if (sessionPollTimer >= sessionPollTimerMax)
        {
            sessionPollTimer = 0f;
            OnUpdateRoom?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdatePlayerName(string newName)
    {
        playerName = newName;
        PlayerPrefs.SetString(playerNamePropertyKey, playerName);
        PlayerPrefs.Save();
        if (ActiveSession == null) return;
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        ActiveSession.CurrentPlayer.SetProperty(playerNamePropertyKey, playerNameProperty);
        ActiveSession.SaveCurrentPlayerDataAsync();
        //ActiveSession.AsHost().SetProperties()
        //foreach (var player in ActiveSession.Players)
        //{
        //    player.Properties[playerNamePropertyKey] = playerName;
        //}
    }

    //public void StartGame()
    //{
    //    gameStarted = true;
    //}

    //public void EndGame()
    //{
    //    gameStarted = false;
    //}

    //private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    //{
    //    if (networkManager.LocalClient.IsSessionOwner)
    //    {
    //        Debug.Log($"Client-{networkManager.LocalClientId} is the session owner!");
    //    }
    //}

    //private void OnClientConnectedCallback(ulong clientId)
    //{
    //    if (networkManager.LocalClientId == clientId)
    //    {
    //        Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
    //    }
    //}

    public async void CreateSession()
    {
        var options = new SessionOptions()
        {
            Name = sessionName,
            MaxPlayers = 4,
            IsLocked = false,
            IsPrivate = false,
        }.WithDistributedAuthorityNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");

        //OnUpdateRoom?.Invoke(this, EventArgs.Empty);
    }

    public bool IsHost()
    {
        return ActiveSession != null && ActiveSession.IsHost;
    }

    //void RegisterSessionEvents()
    //{
    //    ActiveSession.Changed += OnSessionChanged;
    //}

    //void UnregisterSessionEvents()
    //{
    //    ActiveSession.Changed -= OnSessionChanged;
    //}

    //private void OnSessionChanged()
    //{

    //}

    Dictionary<string, PlayerProperty> GetPlayerProperties()
    {
        //var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty } };
    }

    //async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    //{
    //    var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
    //    var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
    //    return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty } };
    //}

    public async void CreateSessionAsHost(string name, int players, bool isPrivate)
    {
        var playerProperties = GetPlayerProperties();

        var options = new SessionOptions
        {
            Name = name,
            MaxPlayers = players,
            IsLocked = false,
            IsPrivate = isPrivate,
            PlayerProperties = playerProperties
        }.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");

        //networkManager.SceneManager.OnLoad += SceneManager_OnLoad;
        //OnUpdateRoom?.Invoke(this, EventArgs.Empty);
    }

    public async void JoinSessionById(string sessionId)
    {
        var playerProperties = GetPlayerProperties();

        var options = new JoinSessionOptions
        {
            PlayerProperties = playerProperties
        };

        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, options);
        Debug.Log($"Session {ActiveSession.Id} joined!");

        //networkManager.SceneManager.OnLoad += SceneManager_OnLoad;
        //OnUpdateRoom?.Invoke(this, EventArgs.Empty);
    }

    public async void JoinSessionByCode(string sessionCode)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Session {ActiveSession.Id} joined!");

        //OnUpdateRoom?.Invoke(this, EventArgs.Empty);
    }

    public async void KickPlayer(string playerId)
    {
        if (!ActiveSession.IsHost) return;
        await ActiveSession.AsHost().RemovePlayerAsync(playerId);

        //OnUpdateRoom?.Invoke(this, EventArgs.Empty);
    }

    public async Task<IList<ISessionInfo>> QuerySessions()
    //public async IList<ISessionInfo> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions();
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }

    public async void LeaveSession()
    {
        if (ActiveSession != null)
        {
            //UnregisterSessionEvents();
            try
            {
                if (IsHost())
                {
                    //Debug.Log("Deleting");
                    await ActiveSession.AsHost().DeleteAsync();
                }
                else
                {
                    await ActiveSession.LeaveAsync();
                }

                //await ActiveSession.AsHost().RemovePlayerAsync(AuthenticationService.Instance.PlayerId);
                //NetworkManager.Singleton.Shutdown();
                //if (IsHost())
                //{
                //    ActiveSession;
                //}
                //await ActiveSession.SaveCurrentPlayerDataAsync();
                //ActiveSession.
            }
            catch
            {

            }
            finally
            {
                ActiveSession = null;

                //OnUpdateRoom?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
