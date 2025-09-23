
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkBehaviour, IGameManager
{
    public static MultiplayerManager Instance;

    private int faces = 3;
    private int skippedPlayers = 0;

    private Dictionary<int, bool> sceneLoaded = new Dictionary<int, bool>();
    public NetworkVariable<int> currentIdTurn = new NetworkVariable<int>(0);
    public NetworkList<int> scores = new NetworkList<int>();
    public NetworkList<int> winners = new NetworkList<int>();

    private List<int[]> bag;
    private List<int[]> history = new List<int[]>();

    [SerializeField] private Transform tilePrefab;
    //[SerializeField] private GameObject winnerGraph;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one MultiplayerManager instance!");
        }
        Instance = this;

        GameHandler.Instance.gameManager = this;

        foreach (var board in GameObject.FindGameObjectsWithTag("Board"))
        {
            if (board.name == "BoardSingleplayer")
            {
                Destroy(board);
            }
        }
    }

    private void Start()
    {
        //winnerGraph.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"On Network Spawn");
        if (IsServer)
        {
            foreach (ulong index in NetworkManager.Singleton.ConnectedClientsIds)
            {
                sceneLoaded[(int)index] = false;
            }
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

        currentIdTurn.OnValueChanged += (int oldValue, int newValue) =>
        {
            HandleTurn();
            Scoreboard.Instance.UpdateTurn(newValue);
        };

        winners.OnListChanged += Winners_OnListChanged;

        scores.OnListChanged += Scores_OnListChanged;
    }

    private void Winners_OnListChanged(NetworkListEvent<int> changeEvent)
    {
        foreach (var index in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (winners.Contains((int)index))
            {
                Scoreboard.Instance.HighlightPlayer((int)index);
            }
            else
            {
                Scoreboard.Instance.LowlightPlayer((int)index);
            }
        }
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= SceneManager_OnLoadComplete;
    }

    public void EndGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("RoomSystem", LoadSceneMode.Single);
    }

    public List<int> GetScores()
    {
        List<int> result = new List<int>();
        for (int i = 0; i < scores.Count; i++)
        {
            result.Add(scores[i]);
        }
        return result;
    }

    [Rpc(SendTo.Server)]
    private void CalculateWinnersRpc()
    {
        int maxValue = 0;
        winners.Clear();
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

    private void Scores_OnListChanged(NetworkListEvent<int> changeEvent)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count != scores.Count)
        {
            return;
        }
        CalculateWinnersRpc();
        UpdateHistory();
        Scoreboard.Instance.UpdateScores();
    }

    [Rpc(SendTo.Server)]
    private void SceneLoadedRpc(ulong clientId)
    {
        sceneLoaded[(int)clientId] = true;
        Debug.Log($"Client {clientId} is ready");
        if (sceneLoaded.Values.All(value => value))
        {
            StartGame();
        }
    }

    private void UpdateHistory()
    {
        int[] point = new int[scores.Count];
        for (int i = 0; i < scores.Count; i++)
        {
            point[i] = scores[i];
        }
        history.Add(point);
    }

    private void StartGame()
    {
        //Debug.Log($"Start Game");

        GenerateBag();
        InitiateScores();
        //OnGameStarted?.Invoke(this, EventArgs.Empty);
        SpawnInitialObjectRpc();
        GenerateHandsRpc(); // ???
        ShowNamesRpc();

        HandleTurn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GenerateHandsRpc()
    {
        foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
        {
            GetDiceRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

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
        UpdateBagSizeRpc(bag.Count);
        //return result;
        //Debug.Log("Bag count = " + bag.Count);
        ReturnDiceRpc(clientId, result);
        //int[] result = new int[3] { bag[index] / 100, bag[index] / 10 % 10, bag[index] % 10 };
        //bag.RemoveAt(index);
        //return result;
    }

    [Rpc(SendTo.Server)]
    public void GetDiceRpc(ulong clientId)
    {
        //return GetDiceRpc(UnityEngine.Random.Range(0, bag.Count - 1));
        //int[] result = GetDiceRpc(clientId, UnityEngine.Random.Range(0, bag.Count - 1));
        GetDiceRpc(clientId, Random.Range(0, bag.Count - 1));
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateBagSizeRpc(int size)
    {
        Bag.Instance.UpdateBag(size);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReturnDiceRpc(ulong clientId, int[] result)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            //if (result != null)
            //    Debug.Log("Recieved " + result[0] + ", " + result[1] + ", " + result[2]);
            VisualManager.Instance.GenerateDiceUI(result);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowNamesRpc()
    {
        int i = 0;
        foreach (var player in RoomManager.Instance.ActiveSession.Players)
        {
            Scoreboard.Instance.SetPlayer(i++, player.Properties["playerName"].Value);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnInitialObjectRpc()
    {
        Vector2 position = Vector2.zero;
        SpawnPlaceRpc(position, false);
        ConstructTilesAround(position, false, new int[3] { 0, 0, 0 });
    }

    private void ConstructTilesAround(Vector2 position, bool state, int[] code)
    {
        DiceRpc(position, code);

        //Collider2D hit = Physics2D.OverlapPoint(position);

        float originAngle = (state ? Mathf.PI : 0) - Mathf.PI / 2;
        //move++;
        for (int i = 0; i < faces; i++)
        {
            float angle = originAngle + 2 * Mathf.PI * i / faces;
            Vector2 targetCoordinates = position + CoordinateConverter.PolarToCartesian(1f / Mathf.Sqrt(3), angle);

            if (!MapManager.Instance.CanBePlaced(targetCoordinates))
            {
                continue;
            }

            Collider2D hitCollider = Physics2D.OverlapPoint(targetCoordinates);

            if (hitCollider == null)
            {
                SpawnPlaceRpc(targetCoordinates, !state);
            }
            SetCodeRpc(targetCoordinates, i, code[i]);
            UpdateScoresRpc(NetworkManager.Singleton.LocalClientId, targetCoordinates, code[i]);
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateScoresRpc(ulong clientId, Vector2 targetCoordinates, int value)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);
        if (hit != null)
        {
            if (hit.CompareTag("Dice"))
            {
                UpdateScores(clientId, value);
            }
        }
    }

    public void UpdateScores(ulong clientId, int value)
    {
        scores[(int)clientId] += value;
    }

    [Rpc(SendTo.Server)]
    private void SetCodeRpc(Vector2 targetCoordinates, int index, int value)
    {
        //Debug.Log("Tiles " + GameObject.FindGameObjectWithTag("Board").transform.childCount);
        //if (Physics2D.OverlapPoint(targetCoordinates))
        //{
        //    Debug.DrawLine(Vector2.zero, targetCoordinates, Color.green, 20);
        //}
        //else
        //{
        //    Debug.DrawLine(Vector2.zero, targetCoordinates, Color.red, 20);
        //}

        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);
        //Debug.Log("SetCodeRpc " + targetCoordinates);

        if (hit != null)
        {
            if (hit.CompareTag("Place"))
            {
                //Debug.Log(targetCoordinates);
                //Debug.Log("SetCodeRpc " + index + ", " + value);
                //hit.gameObject.GetComponent<Tile>().code[index] = value;
                hit.gameObject.GetComponent<MultiplayerTile>().SetCodeRpc(index, value);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DiceRpc(Vector2 position, int[] code)
    {
        VisualManager.Instance.Dice(position, code);
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlaceRpc(Vector2 position, bool state)
    {
        //Debug.Log("SpawnPlace");
        Transform tile = Instantiate(tilePrefab, position, Quaternion.Euler(0f, 0f, state ? 180 : 0));
        //Destroy(tile.gameObject.GetComponent<SingleplayerTile>());
        tile.GetComponent<NetworkObject>().Spawn(true);
        tile.GetComponent<ITile>().SetState(state);
        tile.SetParent(GameObject.FindGameObjectWithTag("Board").transform);
        //tile.gameObject.GetComponent<Tile>().code[index] = value;
    }

    private void GenerateBag()
    {
        int colors = 7;
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
                }
            }
        }
    }

    private void InitiateScores()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            scores.Add(0);
        }
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        if (sceneName == RoomManager.Instance.GameScene)
            SceneLoadedRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void HandleTurn()
    {
        if (currentIdTurn.Value == (int)NetworkManager.Singleton.LocalClientId)
        {
            //VisualManager.Instance.ShowPlaces();
            VisualManager.Instance.RefreshHand();
            //OnBeginMove?.Invoke(this, EventArgs.Empty);
            // RefreshHand
            //CheckMoves();
        }
    }

    public void HandleButton(int index, int[] code)
    {
        HandleButtonRpc(NetworkManager.Singleton.LocalClientId, index, code);
    }

    private bool Suits(int[] code, int[] diceCode)
    {
        for (int i = 0; i < diceCode.Length; i++)
        {
            bool result = true;
            for (int j = 0; j < diceCode.Length; j++)
            {
                if (code[j] != diceCode[(j + i) % diceCode.Length] && code[j] != -1)
                {
                    result = false;
                    break;
                }
            }
            //return true;
            if (result)
            {
                return true;
            }
        }
        return false;
    }

    [Rpc(SendTo.Server)]
    public void HandleButtonRpc(ulong clientId, int index, int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        bool playableDice = false;
        //Debug.Log("playableDice");
        for (int j = 0; j < board.childCount; j++)
        {
            //Debug.Log("Inside loop 2 " + j);
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Suits(place.GetComponent<ITile>().GetCode(), code))
            {
                playableDice = true;
                break;
            }
            //Debug.Log(string.Join("", code));
            //for (int k = 0; k < place.GetComponent<Tile>().GetFacesNumber(); k++)
            //{
            //    //Debug.Log(place.GetComponent<Tile>().code[k]);
            //    //Debug.Log("Inside loop 3 " + k);
            //    //Debug.Log("code length = " + code.Length);
            //    if (string.Join("", code.Select(v => v == -1 ? "*" : v.ToString())).Contains(place.GetComponent<Tile>().code[k].ToString()))
            //    {
            //        playableDice = true;
            //        break;
            //    }
            //}
            //if (playableDice)
            //{
            //    break;
            //}
        }
        ReturnHandledButtonRpc(clientId, index, playableDice);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReturnHandledButtonRpc(ulong clientId, int index, bool playableDice)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            VisualManager.Instance.ReturnHandledButton(index, playableDice);
        }
    }

    public List<int[]> GetHistory()
    {
        return history;
    }

    public List<int> GetWinners()
    {
        List<int> result = new List<int>();
        foreach (int winner in winners)
        {
            result.Add(winner);
        }
        return result;
    }

    public void SkipMove()
    {
        SkipMoveRpc();
    }

    [Rpc(SendTo.Server)]
    public void SkipMoveRpc()
    {
        skippedPlayers++;
        if (NetworkManager.Singleton.ConnectedClientsList.Count == skippedPlayers)
        {
            EndGameRpc();

            ShowWinnerRpc();
            GenerateGraphRpc();
            //WinnerGraph
            //OnGameEnded?.Invoke(this, EventArgs.Empty);
            //OnGameEnded?.Invoke(this, new OnGameEndedArgs
            //{
            //    winners = winnerId
            //});
            return;
        }
        ChangeTurnRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void GenerateGraphRpc()
    {
        WinnerGraph.Instance.Draw();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowWinnerRpc()
    {
        WinnerGraph.Instance.ShowScoreboard();
        VisualManager.Instance.HideBackPanel();
    }

    [Rpc(SendTo.Server)]
    private void ChangeTurnRpc()
    {
        currentIdTurn.Value = (currentIdTurn.Value + 1) % NetworkManager.Singleton.ConnectedClientsList.Count;
    }

    [Rpc(SendTo.Server)]
    private void NullSkippedPlayersRpc()
    {
        skippedPlayers = 0;
    }

    public void MakeMove(Vector2 position, bool state, int[] code)
    {
        ConstructTilesAround(position, state, code);
        //SubscribeTilesAround(e.position, e.state, e.code);

        //ChangeButtonColor(pressed, Color.white);
        GetDiceRpc(NetworkManager.Singleton.LocalClientId);
        //CheckMovesRpc();
        //VisualManager.Instance.DisableButtons();
        //Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);

        // на сервере обновить skippedPlayers = 0
        NullSkippedPlayersRpc();
        ChangeTurnRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameRpc()
    {
        VisualManager.Instance.HidePlaces();
    }

    public void MakeComputerMove()
    {
        throw new System.NotImplementedException();
    }

    //public bool RefreshHand()
    //{
    //    return true;
    //}
}
