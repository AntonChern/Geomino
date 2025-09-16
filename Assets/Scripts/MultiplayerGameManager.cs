//using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;

//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEngine.UIElements;
//using Mirror;

public class MultiplayerGameManager : NetworkBehaviour
{
    private Dictionary<ulong, bool> sceneLoaded = new Dictionary<ulong, bool>();

    public NetworkList<int> winners = new NetworkList<int>();
    //public int width = 2 * Camera.main.pixelWidth;
    public List<int[]> history = new List<int[]>();
    //public int height = 2 * Camera.main.pixelHeight;

    //public float scale = 1f;
    //[SerializeField]
    //private Transform tilePrefab; 
    //private int move = 0;
    //private int faces = 3;
    //[SyncList]
    private int skippedPlayers = 0;

    public int[] chosenCode;
    private List<int[]> bag;
    //public NetworkList<int> bag = new NetworkList<int>();
    //public NetworkVariable<int[][]> bag = new NetworkVariable<int[][]>();
    public NetworkVariable<ulong> currentIdTurn = new NetworkVariable<ulong>(0);

    public NetworkList<int> scores = new NetworkList<int>();
    //public NetworkList<int> scores;

    public static MultiplayerGameManager Instance { get; private set; }

    public event EventHandler<OnDoMoveArgs> OnDoMove;
    public class OnDoMoveArgs : EventArgs
    {
        public Vector2 position;
        public bool state;
        public int[] code;
    }

    public event EventHandler<OnAddDiceArgs> OnAddDice;
    public class OnAddDiceArgs : EventArgs
    {
        public int[] code;
    }

    public event EventHandler<OnUpdateScoreArgs> OnUpdateScore;
    public class OnUpdateScoreArgs : EventArgs
    {
        public int index;
        public int value;
    }

    public event EventHandler<OnRemoveDiceFromBagArgs> OnRemoveDiceFromBag;
    public class OnRemoveDiceFromBagArgs : EventArgs
    {
        public int size;
    }

    //public event EventHandler<OnGameEndedArgs> OnGameEnded;
    public event EventHandler OnGameEnded;
    //public class OnGameEndedArgs : EventArgs
    //{
    //    public string winnerName;
    //}
    public event EventHandler OnTurnChanged;

    public event EventHandler OnGameStarted;

    public event EventHandler OnBeginMove;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one MultiplayerGameManager instance!");
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        //var f = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<>;
        //base.OnNetworkSpawn();
        Debug.Log("On Network Spawn");
        if (IsServer)
        {
            //NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            foreach (ulong index in NetworkManager.Singleton.ConnectedClientsIds)
            {
                sceneLoaded[index] = false;
            }
        }
        //NetworkManager.Singleton.SceneManager.OnLoad += SceneManager_OnLoad;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

        currentIdTurn.OnValueChanged += (ulong oldValue, ulong newValue) =>
        {
            DisplayPlaces();
            OnTurnChanged?.Invoke(this, EventArgs.Empty);
            //OnGameStarted?.Invoke(this, EventArgs.Empty);
        };

        scores.OnListChanged += Scores_OnListChanged;


        //bag.OnListChanged += HandleListChanged;
    }

    [Rpc(SendTo.Server)]
    private void SceneLoadedRpc(ulong clientId)
    {
        sceneLoaded[clientId] = true;
        Debug.Log($"Client {clientId} is ready");
        if (sceneLoaded.Values.All(value => value))
        {
            GenerateBag();
            InitiateScores();
            OnGameStarted?.Invoke(this, EventArgs.Empty);
            //Debug.Log("Server is servering");
            DisplayPlaces();
        }
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= SceneManager_OnLoadComplete;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        if (sceneName == RoomManager.Instance.GameScene)
            SceneLoadedRpc(NetworkManager.Singleton.LocalClientId);
    }

    //private void SceneManager_OnLoad(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    //{
    //    Debug.Log("Test scene load");

    //}

    private void UpdateHistory()
    {
        //Debug.Log("scores.Count = " + scores.Count);
        int[] point = new int[scores.Count];
        for (int i = 0; i < scores.Count; i++)
        {
            point[i] = scores[i];
        }
        history.Add(point);
    }

    private void Scores_OnListChanged(NetworkListEvent<int> changeEvent)
    {
        //Debug.Log(changeEvent.Index + ") " + changeEvent.Value);
        CalculateWinnersRpc();
        if (NetworkManager.Singleton.ConnectedClientsList.Count != scores.Count)
        {
            return;
        }
        UpdateHistory();
        OnUpdateScore?.Invoke(this, new OnUpdateScoreArgs
        {
            index = changeEvent.Index,
            value = changeEvent.Value
        });
    }

    //private void HandleListChanged(NetworkListEvent<int> changeEvent)
    //{
    //    Debug.Log("List changed: " + string.Join(", ", changeEvent));
    //    //switch (changeEvent.Type)
    //    //{
    //    //    case NetworkListEvent.Add:
    //    //        Debug.Log($"Item added at index {changeEvent.Index} with value {changeEvent.Value}");
    //    //        break;
    //    //    case NetworkListEvent.Remove:
    //    //        Debug.Log($"Item removed from index {changeEvent.Index} with value {changeEvent.Value}");
    //    //        break;
    //    //    case NetworkListEvent.Value:
    //    //        Debug.Log($"Item at index {changeEvent.Index} changed from {changeEvent.PreviousValue} to {changeEvent.Value}");
    //    //        break;
    //    //    case NetworkListEvent.Clear:
    //    //        Debug.Log("List cleared");
    //    //        break;
    //    //        // Handle other event types as needed
    //    //}
    //}

    //public override void OnDestroy()
    //{
    //    winners.Dispose();
    //    scores.Dispose();
    //    currentIdTurn.Dispose();
    //}

    private void InitiateScores()
    {
        //scores = new NetworkList<int>();
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            scores.Add(0);
            //Debug.Log("Score added " + i);
        }
        //Debug.Log("Scores initiated");
    }

    //private void NetworkManager_OnClientConnectedCallback(ulong obj)
    //{
    //    //Debug.Log("Connected " + obj + ", " + NetworkManager.Singleton.ConnectedClientsList.Count);
    //    //Debug.Log(LobbyManager.IsHost);
    //    //if (NetworkManager.Singleton.ConnectedClientsList.Count == LobbyManager.Instance.maxPlayers)
    //    //{
    //    //    GenerateBag();
    //    //    InitiateScores();
    //    //    OnGameStarted?.Invoke(this, EventArgs.Empty);
    //    //    DisplayPlaces();

    //    //    //Debug.Log("OnClientConnectedCallback");
    //    //    //scores = new NetworkList<int>(NetworkManager.Singleton.ConnectedClientsList.Count);
    //    //    //for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
    //    //    //{
    //    //    //    scores.Add(0);
    //    //    //    Debug.Log("Score added " + i);
    //    //    //}
    //    //}
    //    //Debug.Log("Client added");
    //    if (NetworkManager.Singleton.ConnectedClientsList.Count == RoomManager.Instance.ActiveSession.MaxPlayers)
    //    {
    //        GenerateBag();
    //        InitiateScores();
    //        OnGameStarted?.Invoke(this, EventArgs.Empty);
    //        DisplayPlaces();

    //        //Debug.Log("OnClientConnectedCallback");
    //        //scores = new NetworkList<int>(NetworkManager.Singleton.ConnectedClientsList.Count);
    //        //for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
    //        //{
    //        //    scores.Add(0);
    //        //    Debug.Log("Score added " + i);
    //        //}
    //    }
    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void ReturnDiceRpc(ulong clientId, int[] result)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            //if (result != null)
            //    Debug.Log("Recieved " + result[0] + ", " + result[1] + ", " + result[2]);
            OnAddDice?.Invoke(this, new OnAddDiceArgs
            {
                code = result,
            });
        }
    }

    //[ServerRpc]
    //public int[] GetDiceRpc(ulong clientId, int index)
    [Rpc(SendTo.Server)]
    public void GetDiceRpc(ulong clientId, int index)
    {
        if (bag.Count == 0)
        {
            ReturnDiceRpc(clientId, null);
            return;
        }
        int[] result = bag[index];
        bag.RemoveAt(index);
        // изменить переменную, ответственную за количество оставшихся фишек
        OnRemoveDiceFromBag?.Invoke(this, new OnRemoveDiceFromBagArgs
        {
            size = bag.Count
        });
        //return result;
        //Debug.Log("Bag count = " + bag.Count);
        ReturnDiceRpc(clientId, result);
        //int[] result = new int[3] { bag[index] / 100, bag[index] / 10 % 10, bag[index] % 10 };
        //bag.RemoveAt(index);
        //return result;
    }

    //[ServerRpc]
    //public int[] GetDiceRpc()
    [Rpc(SendTo.Server)]
    public void GetDiceRpc(ulong clientId)
    {
        //return GetDiceRpc(UnityEngine.Random.Range(0, bag.Count - 1));
        //int[] result = GetDiceRpc(clientId, UnityEngine.Random.Range(0, bag.Count - 1));
        GetDiceRpc(clientId, UnityEngine.Random.Range(0, bag.Count - 1));
    }

    //[Rpc(SendTo.Server)]
    private void GenerateBag()
    {
        int colors = 7;
        //bag.Value = new int[70][];
        //int index = 0;
        //bag = new List<int[]>() {
        //    new int[3] { 0, 1, 2 },
        //    new int[3] { 0, 2, 1 },
        //    new int[3] { 3, 4, 5 },
        //    new int[3] { 3, 5, 4 },
        //    new int[3] { 3, 4, 6 },
        //    new int[3] { 3, 5, 6 },
        //    new int[3] { 3, 6, 5 },
        //    new int[3] { 3, 6, 4 }
        //};
        //int maxNumber = 2;
        bag = new List<int[]>();
        for (int i = 0; i < colors; i++)
        {
            for (int j = i + 1; j < colors; j++)
            {
                for (int k = j + 1; k < colors; k++)
                {
                    bag.Add(new int[3] { i, j, k });
                    bag.Add(new int[3] { i, k, j });
                    //if (bag.Count >= maxNumber)
                    //{
                    //    return;
                    //}
                    //bag.Value[index++] = new int[3] { i, j, k };
                    //bag.Value[index++] = new int[3] { i, k, j };
                    //bag.Value.Add(new int[3] { i, j, k });
                    //bag.Value.Add(new int[3] { i, k, j });
                }
            }
        }
        //Debug.Log("Bag generated");
    }

    private void Start()
    {
        //winners = new NetworkList<int>();
        //currentIdTurn = new NetworkVariable<ulong>();
        Debug.Log("Started in new scene");
        //if (IsServer)
        //{
        //    //Debug.Log("Server is servering");
        //    GenerateBag();
        //    InitiateScores();
        //    OnGameStarted?.Invoke(this, EventArgs.Empty);
        //    DisplayPlaces();
        //}
    }

    private void HidePlaces()
    {
        int layerIndex = LayerMask.NameToLayer("CurrentPlaces");
        Camera.main.cullingMask &= ~(1 << layerIndex);
    }

    private void ShowPlaces()
    {
        int layerIndex = LayerMask.NameToLayer("CurrentPlaces");
        Camera.main.cullingMask |= (1 << layerIndex);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    private void DisplayPlaces()
    {
        //Debug.Log("DisplayPlaces");
        if (currentIdTurn.Value == NetworkManager.Singleton.LocalClientId)
        {
            ShowPlaces();
            OnBeginMove?.Invoke(this, EventArgs.Empty);

            //Transform board = GameObject.FindGameObjectWithTag("Board").transform;
            //for (int i = 0; i < board.childCount; i++)
            //{
            //if (!board.GetChild(i).CompareTag("Place"))
            //    continue;
            //board.GetChild(i).gameObject.SetActive(true);
            //board.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Places");
            //board.GetChild(i).transform.position = new Vector3(board.GetChild(i).transform.position.x, board.GetChild(i).transform.position.y, 0f);
            //}
        }
        //else
        //{
        //    // Remove the layer from the camera's culling mask
        //    int layerIndex = LayerMask.NameToLayer("Places");
        //    Camera.main.cullingMask &= ~(1 << layerIndex);
        //    //foreach (GameObject place in GameObject.FindGameObjectsWithTag("Place"))
        //    //{
        //    //    //place.SetActive(false);
        //    //    place.transform.position = new Vector3(place.transform.position.x, place.transform.position.y, Camera.main.transform.position.z - 1f);
        //    //}
        //}
        //Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);
        //Debug.Log("Places displayed");
    }

    [Rpc(SendTo.Server)]
    private void NullSkippedPlayersRpc()
    {
        skippedPlayers = 0;
    }

    [Rpc(SendTo.Server)]
    public void SkipMoveRpc()
    {
        skippedPlayers++;
        //Debug.Log("skippedPlayers = " + skippedPlayers);
        if (NetworkManager.Singleton.ConnectedClientsList.Count == skippedPlayers)
        {
            //Debug.Log("END GAME");
            //ulong winnerId = CalculateWinner();
            //Debug.Log("winnerId = " + winnerId);
            EndGameRpc();
            OnGameEnded?.Invoke(this, EventArgs.Empty);
            //OnGameEnded?.Invoke(this, new OnGameEndedArgs
            //{
            //    winners = winnerId
            //});
            return;
        }
        ChangeTurnRpc();
    }

    //private ulong CalculateWinner()
    //{
    //    ulong index = 0;
    //    int maxValue = 0;
    //    for (int i = 0; i < scores.Count; i++)
    //    {
    //        if (scores[i] > maxValue)
    //        {
    //            maxValue = scores[i];
    //            index = (ulong)i;
    //        }
    //    }
    //    return index;
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //private void ShowWinner(string winnerName)
    //{
    //    OnGameEnded?.Invoke(this, new OnGameEndedArgs
    //    {
    //        winnerName = winnerName
    //    });
    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameRpc()
    {
        HidePlaces();
        //if (NetworkManager.Singleton.LocalClientId == winnerId)
        //{
        //    ShowWinner(EditPlayerName.Instance.GetPlayerName());
        //}
    }

    //private Vector2 PolarToCartesian(float radius, float angle)
    //{
    //    return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
    //}
    [Rpc(SendTo.Server)]
    private void ChangeTurnRpc()
    {
        currentIdTurn.Value = (currentIdTurn.Value + 1) % (ulong)NetworkManager.Singleton.ConnectedClientsList.Count;
        //currentIdTurn.Value = currentIdTurn.Value + 1;

        //currentIdTurn.Reset(currentIdTurn.Value + 1);
        //DisplayPlacesRpc();
        //Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void UpdateHistoryRpc()
    //{
    //    int[] point = new int[scores.Count];
    //    for (int i = 0; i < scores.Count; i++)
    //    {
    //        point[i] = scores[i];
    //    }
    //    history.Add(point);
    //}

    public void UpdateScores(ulong clientId, int value)
    {
        scores[(int)clientId] += value;

        //UpdateHistoryRpc();
        //List<int> indices = new List<int>();
        //int maxValue = 0;
        //for (int i = 0; i < scores.Count; i++)
        //{
        //    if (scores[i] < maxValue)
        //        continue;
        //    if (scores[i] > maxValue)
        //    {
        //        winners.Clear();
        //        //index = (ulong)i;
        //    }
        //    maxValue = scores[i];
        //    winners.Add(i);
        //    //if (scores[i] == maxValue)
        //    //{
        //    //    indices.Add(i);
        //    //}
        //}
        //return index;
        //CalculateWinners();
    }

    [Rpc(SendTo.Server)]
    private void CalculateWinnersRpc()
    {
        int maxValue = 0;
        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i] < maxValue)
                continue;
            if (scores[i] > maxValue)
            {
                winners.Clear();
            }
            maxValue = scores[i];
            winners.Add(i);
        }
    }

    //public void Boob()
    //{
    //    Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);
    //}

    //[Rpc(SendTo.Server)]
    public void DoMove(Vector2 position, bool state, int[] code)
    {
        HidePlaces();

        OnDoMove?.Invoke(this, new OnDoMoveArgs {
            position = position,
            state = state,
            code = code
        });
        //Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);

        // на сервере обновить skippedPlayers = 0
        NullSkippedPlayersRpc();
        ChangeTurnRpc();
        //Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);
        //DisplayPlacesRpc();

        //float originAngle = (state ? Mathf.PI : 0) - Mathf.PI / 2;
        ////move++;
        //for (int i = 0; i < faces; i++)
        //{
        //    float angle = originAngle + 2 * Mathf.PI * i / faces;
        //    Vector2 targetCoordinates = position + PolarToCartesian(scale / Mathf.Sqrt(3), angle);
        //    Collider2D hitCollider = Physics2D.OverlapPoint(targetCoordinates);

        //    if (hitCollider != null)
        //    {
        //        //Debug.Log("Found object at " + targetCoordinates + ": " + hitCollider.gameObject.name);
        //    }
        //    else
        //    {
        //        //Debug.Log("No object found at " + targetCoordinates);
        //        Transform tile = Instantiate(tilePrefab, targetCoordinates, Quaternion.Euler(0f, 0f, state ? 0 : 180));
        //        tile.GetComponent<Tile>().SetState(!state);
        //    }

        //}
        //Debug.Log("MultiplayerGameManager DoMove()");
    }

    //public void SelectDice()
    //{
    //    Debug.Log("Selected");
    //}
}
