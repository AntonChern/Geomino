//using System.Collections.Generic;
//using Unity.Netcode;
//using Unity.Services.Authentication;
//using Unity.Services.Lobbies;
//using Unity.Services.Lobbies.Models;
//using UnityEngine;
//using static LobbyManager;

//public class PlayerData : MonoBehaviour
//{
//    public static PlayerData Instance;

//    [SerializeField] private GameObject lobbyUI;

//    private class LobbyState
//    {
//        public string lobbyName;
//        public int maxPlayers;
//        public bool isPrivate;
//        public LobbyManager.GameMode gameMode;
//    }

//    public string playerName;
//    private bool isHost = false;
//    private LobbyState lobbyState = new LobbyState();
//    public Lobby lobby;

//    [Rpc(SendTo.ClientsAndHost)]
//    private void ConnectRpc(ulong index, string lobbyCode)
//    {
//        if (index != NetworkManager.Singleton.LocalClientId)
//        {
//            return;
//        }

//        LobbyManager.Instance.JoinLobbyByCode(lobbyCode);
//        //NetworkManager.Singleton.Shutdown();
//    }

//    //[Rpc(SendTo.Server)]
//    //private void DestroyInstanceRpc()
//    //{
//    //    Destroy(gameObject);
//    //}

//    private async void Start()
//    {
//        if (Instance != null)
//        {
//            //Debug.Log("Authentication " + AuthenticationService.Instance.PlayerName + " " + AuthenticationService.Instance.PlayerId);
//            //Debug.Log("Lobby " + Instance.lobby + " " + (Instance.lobby == null));
//            //Debug.Log(Instance.lobby.AvailableSlots);
//            Debug.Log("NetworkManager.Singleton.ConnectedClientsIds.Count " + NetworkManager.Singleton.ConnectedClientsIds.Count);



//            if (Instance.isHost)
//            {
//                Instance.lobby = await Lobbies.Instance.UpdateLobbyAsync(Instance.lobby.Id, new UpdateLobbyOptions
//                {
//                    Data = new Dictionary<string, DataObject> {
//                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Public, "0") }
//                }
//                });
//                //Destroy(NetworkManager.Singleton.gameObject);
//                //LobbyManager.Instance.SetRelayJoinCode("");
//            }
//            LobbyManager.Instance.SetJoinedLobby(Instance.lobby);
//            //NetworkManager.Singleton.StopClient();
//            NetworkManager.Singleton.Shutdown();
//            //Destroy(NetworkManager.Singleton.gameObject);

//            ////DestroyInstanceRpc();

//            ////LobbyUI.Instance.gameObject.SetActive(true);
//            //AuthenticateUI.Instance.gameObject.SetActive(false);
//            //LobbyListUI.Instance.gameObject.SetActive(false);

//            //EditPlayerName.Instance.SetName(Instance.playerName);
//            //Debug.Log("PlayerData.Start");
//            ////this.lobbyUI.SetActive(true);
//            ////LobbyManager.Instance.Authenticate(Instance.playerName);
//            ////if (Instance.isHost)
//            ////{
//            ////    LobbyState lobbyState = Instance.lobbyState;
//            ////    LobbyManager.Instance.CreateLobby(lobbyState.lobbyName, lobbyState.maxPlayers, lobbyState.isPrivate, lobbyState.gameMode);
//            ////    foreach (ulong index in NetworkManager.Singleton.ConnectedClientsIds)
//            ////    {
//            ////        if (index == NetworkManager.Singleton.LocalClientId)
//            ////        {
//            ////            continue;
//            ////        }
//            ////        //ConnectRpc(index, LobbyManager.Instance.GetJoinedLobby().LobbyCode);
//            ////    }
//            ////    //NetworkManager.Singleton.Shutdown();
//            ////}
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    public void SetPlayerName(string playerName)
//    {
//        this.playerName = playerName;
//    }

//    public void SetLobbyState(string lobbyName, int maxPlayers, bool isPrivate, LobbyManager.GameMode gameMode)
//    {
//        lobbyState.lobbyName = lobbyName;
//        lobbyState.maxPlayers = maxPlayers;
//        lobbyState.isPrivate = isPrivate;
//        lobbyState.gameMode = gameMode;
//    }

//    public void SetHost(bool isHost)
//    {
//        this.isHost = isHost;
//    }
//}
