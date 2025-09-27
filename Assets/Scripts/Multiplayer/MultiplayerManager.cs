using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
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

    private Dictionary<ulong, int> clientIndex = new Dictionary<ulong, int>();
    //[SerializeField] private TextMeshProUGUI exitButtonText;
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

    private void UpdateClientIndices()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            clientIndex[NetworkManager.Singleton.ConnectedClientsIds[i]] = i;
            //Debug.Log("what " + NetworkManager.Singleton.ConnectedClientsIds[i]);
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"On Network Spawn");
        UpdateClientIndices();

        if (IsServer)
        {
            foreach (ulong index in NetworkManager.Singleton.ConnectedClientsIds)
            {
                sceneLoaded[clientIndex[index]] = false;
            }
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

        NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        //NetworkManager.Singleton.OnClientStopped += OnClientStopped;

        currentIdTurn.OnValueChanged += (int oldValue, int newValue) =>
        {
            //Debug.Log($"turn changed");
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
            if (winners.Contains(clientIndex[index]))
            {
                Scoreboard.Instance.HighlightPlayer(clientIndex[index]);
            }
            else
            {
                Scoreboard.Instance.LowlightPlayer(clientIndex[index]);
            }
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= SceneManager_OnLoadComplete;
            }
            NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
        }

        //if (RoomManager.Instance.IsHost())
        //{
        //    ShutdownAllRpc();
        //}
        //else
        //{
        //    AddToBagRpc(GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice.GetComponent<DiceUI>().Code).ToArray());
        //    //ChangeClientIndicesRpc(clientIndex[NetworkManager.Singleton.LocalClientId]);
        //    ChangeClientIndicesRpc();
        //}
    }

    //private void OnClientStopped(bool obj)
    //{
    //    Debug.Log("ClientStopped");
    //}

    //private void OnClientDisconnectCallback(ulong obj)
    //{
    //    Debug.Log("ClientDisconnect");
    //    //if (clientIndex[NetworkManager.Singleton.LocalClientId] == 0) // host disconnected
    //    //{
    //    //    ShutdownAllRpc();
    //    //    return;
    //    //}

    //    if (NetworkManager.Singleton.LocalClientId != obj) return;
    //    //ClientDisconnectedRpc(clientIndex[obj]);
    //    //int index = clientIndex[obj];
    //    //ClientDisconnectedRpc();
    //    //foreach (var sos in GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice.GetComponent<DiceUI>().Code).ToArray())
    //    //{
    //    //    Debug.Log($"Dice {string.Join(" ", sos)}");
    //    //}
    //    //AddToBagRpc(index);
    //    ////AddToBagRpc(GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice.GetComponent<DiceUI>().Code).ToArray());

    //}

    //[Rpc(SendTo.Server)]
    //private void ClientDisconnectedRpc()
    //{
    //    Debug.Log($"Here we go");
    //    //ChangeClientIndicesRpc();
    //    //ChangeTurnRpc();
    //    //RemoveScoreRpc(index);
    //    //UpdateInterfaceRpc(bag.Count, index);
    //}

    [Rpc(SendTo.Server)]
    private void ClientPressedExitButtonRpc(ulong id, int[] code1, int[] code2, int[] code3, int[] code4)
    {
        Debug.Log("ClientDisconnect");
        //Debug.Log($"{string.Join(" ", hand)}");
        //foreach (var sos in hand)
        //{
        //Debug.Log($"Dice1 {string.Join(" ", code1)}");
        //Debug.Log($"Dice2 {string.Join(" ", code2)}");
        //Debug.Log($"Dice3 {string.Join(" ", code3)}");
        //Debug.Log($"Dice4 {string.Join(" ", code4)}");
        //}

        AddToBag(new int[][] { code1, code2, code3, code4 });
        ////AddToBagRpc(GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice.GetComponent<DiceUI>().Code).ToArray());
        int index = clientIndex[id];
        ChangeClientIndicesRpc(index);
        //ChangeTurnRpc();
        if (currentIdTurn.Value == (NetworkManager.Singleton.ConnectedClientsIds.Count - 1))
        {
            currentIdTurn.Value = currentIdTurn.Value % (NetworkManager.Singleton.ConnectedClientsIds.Count - 1);
        }
        else
        {
            HandleTurnRpc();
        }
        RemoveScoreRpc(index);
        UpdateInterfaceRpc(bag.Count, index);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HandleTurnRpc()
    {
        HandleTurn();
    }

    [Rpc(SendTo.Server)]
    private void RemoveScoreRpc(int index)
    {
        scores.RemoveAt(index);
    }

    //[Rpc(SendTo.Server)]
    //private void ClientDisconnectedRpc(int index)
    //{
    //    if (clientIndex[NetworkManager.Singleton.LocalClientId] == index)
    //    {
    //        ShutdownAllRpc();
    //        return;
    //    }
    //    AddToBagRpc(GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice.GetComponent<DiceUI>().Code).ToArray());
    //    ChangeClientIndicesRpc();
    //    //currentIdTurn.Value = currentIdTurn.Value;
    //    //HandleTurn();
    //    //Scoreboard.Instance.UpdateTurn(newValue);
    //    ChangeTurnRpc();

    //    scores.RemoveAt(index);
    //    UpdateInterfaceRpc(bag.Count, index);
    //    //ShowNamesRpc();
    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateInterfaceRpc(int bagSize, int index)
    {
        //Scoreboard.Instance.Null();
        //Scoreboard.Instance.UpdateScores();
        Scoreboard.Instance.RemoveByIndex(index);
        Bag.Instance.UpdateBag(bagSize);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShutdownAllRpc()
    {
        //NetworkManager.Singleton.Shutdown();
        RoomManager.Instance.LeaveSession();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    //[Rpc(SendTo.Server)]
    private void AddToBag(int[][] hand)
    {
        //Debug.Log("No?");
        //Debug.Log(hand == null);
        //Debug.Log(hand.Length);
        //foreach (var sos in hand)
        //{
        //    Debug.Log($"Dice {string.Join(" ", sos)}");
        //}

        foreach (var code in hand)
        {
            if (code == null) continue;
            bag.Add(code);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeClientIndicesRpc(int index)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count - 1 == 1)
        //if (NetworkManager.Singleton.ConnectedClientsIds.Count - 1 == 1 ||
        //    NetworkManager.Singleton.ConnectedClientsIds.Count == 2 && NetworkManager.Singleton.ConnectedClientsIds.Contains(id))
        {
            FinishGame();
            return;
        }
        Dictionary<ulong, int> newClientIndex = new Dictionary<ulong, int>();
        int i = 0;
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientIndex[clientId] == index) continue;
            newClientIndex[clientId] = i++;
        }
        clientIndex = newClientIndex;
        //currentIdTurn.Value = currentIdTurn.Value;
    }

    public void EndGame()
    {
        StartQuitCountDownRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartQuitCountDownRpc()
    {
        WinnerGraph.Instance.DisableExitButton();

        IEnumerator quitWaiter()
        {
            for (int i = 5; i > 0; i--)
            {
                WinnerGraph.Instance.SetExitButtonText($"Возвращение в комнату через {i}");
                yield return new WaitForSeconds(1);
            }
            if (NetworkManager.Singleton.IsHost)
            {
                QuitGame();
                RoomManager.Instance.UnlockSession();
            }
        }

        StartCoroutine(quitWaiter());
    }

    //[Rpc(SendTo.Server)]
    private void QuitGame()
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
    private void SceneLoadedRpc(int clientId)
    {
        sceneLoaded[clientId] = true;
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
        //Debug.Log("1");
        InitiateScores();
        //Debug.Log("2");
        //OnGameStarted?.Invoke(this, EventArgs.Empty);
        SpawnInitialObjectRpc();
        //Debug.Log("3");
        GenerateHandsRpc(); // ???
        //Debug.Log("4");
        ShowNamesRpc();
        //Debug.Log("5");

        HandleTurn();
        //Debug.Log("6");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GenerateHandsRpc()
    {
        foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
        {
            GetDiceRpc(clientIndex[NetworkManager.Singleton.LocalClientId]);
        }
    }

    [Rpc(SendTo.Server)]
    public void GetDiceRpc(int clientId, int index)
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
    public void GetDiceRpc(int clientId)
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
    private void ReturnDiceRpc(int clientId, int[] result)
    {
        if (clientIndex[NetworkManager.Singleton.LocalClientId] == clientId)
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
        int value = 0;
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

            hitCollider = Physics2D.OverlapPoint(targetCoordinates);
            if (hitCollider != null && hitCollider.CompareTag("Dice"))
            {
                value += code[i];
            }
        }
        UpdateScoresRpc(clientIndex[NetworkManager.Singleton.LocalClientId], value);

        if (value == 0) return;
        SpawnScoreRpc(position, value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnScoreRpc(Vector2 position, int value)
    {
        WorldCanvas.Instance.SpawnScore(position, value);
    }

    //[Rpc(SendTo.Server)]
    //private int UpdateScoresRpc(Vector2 targetCoordinates, int value)
    //{
    //    Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);
    //    if (hit != null)
    //    {
    //        if (hit.CompareTag("Dice"))
    //        {
    //            return value;
    //            //UpdateScores(clientId, value);
    //        }
    //    }
    //    return 0;
    //}

    [Rpc(SendTo.Server)]
    public void UpdateScoresRpc(int clientId, int value)
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
        //int maxNumber = 8;
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
        //Debug.Log("Bag generated");
    }

    private void InitiateScores()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            scores.Add(0);
        }
        //Debug.Log("Scores initiated");
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        if (sceneName == RoomManager.Instance.GameScene)
            SceneLoadedRpc(clientIndex[NetworkManager.Singleton.LocalClientId]);
    }

    private void HandleTurn()
    {
        if (currentIdTurn.Value == clientIndex[NetworkManager.Singleton.LocalClientId])
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
        HandleButtonRpc(clientIndex[NetworkManager.Singleton.LocalClientId], index, code);
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
    public void HandleButtonRpc(int clientId, int index, int[] code)
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
    private void ReturnHandledButtonRpc(int clientId, int index, bool playableDice)
    {
        if (clientIndex[NetworkManager.Singleton.LocalClientId] == clientId)
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

    private void FinishGame()
    {
        EndGameRpc();

        ShowWinnerRpc();
        GenerateGraphRpc();
    }

    [Rpc(SendTo.Server)]
    public void SkipMoveRpc()
    {
        skippedPlayers++;
        if (NetworkManager.Singleton.ConnectedClientsList.Count == skippedPlayers)
        {
            FinishGame();
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
        GetDiceRpc(clientIndex[NetworkManager.Singleton.LocalClientId]);
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

    private void OnPreShutdown()
    {
        //Debug.Log("OnPreShutdown");
        //Hide();
        //RoomList.Instance.Show();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quit");
        Exit();
    }

    public void Exit()
    {
        if (!RoomManager.Instance.IsHost())
        {
            int[][] hand = GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice == null ? null : dice.GetComponent<DiceUI>().Code).ToArray();
            //Debug.Log($"{string.Join(" ", GameObject.FindGameObjectsWithTag("DiceUI").Select(dice => dice.GetComponent<DiceUI>().Code).ToArray()[0])}");
            ClientPressedExitButtonRpc(NetworkManager.Singleton.LocalClientId, hand.Length <= 0 ? null : hand[0], hand.Length <= 1 ? null : hand[1], hand.Length <= 2 ? null : hand[2], hand.Length <= 3 ? null : hand[3]);
        }
        //else
        //{
        //    //ShutdownAllRpc();
        //    //return;
        //}
        RoomManager.Instance.LeaveSession();
        //SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    //public bool RefreshHand()
    //{
    //    return true;
    //}
}
