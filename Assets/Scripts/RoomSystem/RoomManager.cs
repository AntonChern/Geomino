using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;

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

    private string playerName = "Name";
    public string PlayerName
    {
        get => playerName;
        set {  playerName = value; }
    }

    public string GameScene = "MultiplayerGame";

    string sessionName = "MyRoom";

    public const string playerNamePropertyKey = "playerName";
    public const string starCounterPropertyKey = "starCounter";
    public const string mapProperty = "map";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        string existingPlayerName = PlayerPrefs.GetString(playerNamePropertyKey);
        //if (YG2.player.auth)
        //{
        //    existingPlayerName = YG2.player.name;
        //}
        if (existingPlayerName != "")
            playerName = existingPlayerName;
    }

    async void Start()
    {
        try
        {
            SceneManager.sceneLoaded += SceneLoaded;

            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Signed in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        switch (arg0.name)
        {
            case "MainMenu":
                AuthenticationService.Instance.SignOut();
                break;
            //case "RoomSystem":

            //    break;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
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

    public void UpdateMap(string value)
    {
        SessionProperty sessionProperty = new SessionProperty(value, VisibilityPropertyOptions.Public);
        ActiveSession.AsHost().SetProperty(mapProperty, sessionProperty);
        ActiveSession.AsHost().SavePropertiesAsync();
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
    }

    public void UpdateStarCounter(string newStarCounter)
    {
        if (ActiveSession == null) return;
        var starCounterProperty = new PlayerProperty(newStarCounter, VisibilityPropertyOptions.Member);
        ActiveSession.CurrentPlayer.SetProperty(starCounterPropertyKey, starCounterProperty);
        ActiveSession.SaveCurrentPlayerDataAsync();
    }

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
    }

    public bool IsHost()
    {
        return ActiveSession != null && ActiveSession.IsHost;
    }

    Dictionary<string, PlayerProperty> GetPlayerProperties()
    {
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        var starCounterProperty = new PlayerProperty(PlayerPrefs.GetInt("stars").ToString(), VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty>
        {
            { playerNamePropertyKey, playerNameProperty },
            { starCounterPropertyKey, starCounterProperty },
        };
    }

    public void LockSession()
    {
        ActiveSession.AsHost().IsLocked = true;
        ActiveSession.AsHost().SavePropertiesAsync();
    }

    public void UnlockSession()
    {
        ActiveSession.AsHost().IsLocked = false;
        ActiveSession.AsHost().SavePropertiesAsync();
    }

    public async void CreateSessionAsHost(string name, int maxPlayers, bool isPrivate, string map)
    {
        var playerProperties = GetPlayerProperties();
        var sessionMapProperty = new SessionProperty(map, VisibilityPropertyOptions.Public);

        var options = new SessionOptions
        {
            Name = name,
            //MaxPlayers = players,
            MaxPlayers = maxPlayers,
            IsLocked = false,
            IsPrivate = isPrivate,
            PlayerProperties = playerProperties,
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                { mapProperty, sessionMapProperty }
            },
        }.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
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
    }

    public async void JoinSessionByCode(string sessionCode)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Session {ActiveSession.Id} joined!");
    }

    public async void KickPlayer(string playerId)
    {
        if (!ActiveSession.IsHost) return;
        await ActiveSession.AsHost().RemovePlayerAsync(playerId);
    }

    public async Task<IList<ISessionInfo>> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions();
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }

    public async void LeaveSession()
    {
        if (ActiveSession == null) return;
        try
        {
            if (IsHost())
            {
                await ActiveSession.AsHost().DeleteAsync();
            }
            else
            {
                await ActiveSession.LeaveAsync();
            }
        }
        catch
        {

        }
        finally
        {
            ActiveSession = null;
        }
    }
}
