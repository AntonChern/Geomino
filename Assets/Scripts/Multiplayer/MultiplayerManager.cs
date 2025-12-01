using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class MultiplayerManager : NetworkBehaviour, IGameManager
{
    public static MultiplayerManager Instance;

    private int faces = 3;
    private int skippedPlayers = 0;

    private Dictionary<int, bool> sceneLoaded = new Dictionary<int, bool>();
    public NetworkVariable<int> currentIdTurn = new NetworkVariable<int>(0);
    public NetworkList<int> scores = new NetworkList<int>();
    public NetworkList<int> winners = new NetworkList<int>();
    private List<int> stars = null;

    private List<int[]> bag;
    private List<int[]> history = new List<int[]>();

    [SerializeField] private Transform tilePrefab;

    private Dictionary<ulong, int> clientIndex = new Dictionary<ulong, int>();

    private List<List<int[]>> hands = new List<List<int[]>>();
    private string[] returnText = new string[]
    {
        "Возвращение в комнату через ",
        "Return to the room in "
    };

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

    private void UpdateClientIndices()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            clientIndex[NetworkManager.Singleton.ConnectedClientsIds[i]] = i;
        }
    }

    public override void OnNetworkSpawn()
    {
        UpdateClientIndices();

        if (IsServer)
        {
            foreach (ulong index in NetworkManager.Singleton.ConnectedClientsIds)
            {
                sceneLoaded[clientIndex[index]] = false;
                hands.Add(new List<int[]>());
            }
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

        NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

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
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }

    private void OnLeaveProcedure(int index)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 0) return;
        AddToBag(hands[index].ToArray());
        ChangeClientIndicesRpc(index);
        if (currentIdTurn.Value == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            currentIdTurn.Value = currentIdTurn.Value % NetworkManager.Singleton.ConnectedClientsIds.Count;
        }
        else
        {
            HandleTurnRpc();
        }
        scores.RemoveAt(index);
        hands.RemoveAt(index);
        UpdateInterfaceRpc(bag.Count, index);
    }

    private void OnClientDisconnectCallback(ulong obj)
    {
        if (!IsHost) return;
        Debug.Log($"ClientDisconnect {obj}");

        OnLeaveProcedure(clientIndex[obj]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HandleTurnRpc()
    {
        HandleTurn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateInterfaceRpc(int bagSize, int index)
    {
        Scoreboard.Instance.RemoveByIndex(index);
        Bag.Instance.UpdateBag(bagSize);
    }

    private void AddToBag(int[][] hand)
    {
        foreach (var code in hand)
        {
            if (code == null) continue;
            bag.Add(code);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeClientIndicesRpc(int index)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
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
            float returningTime = 5f;
            float timer = returningTime;
            while (timer > 0f)
            {
                timer = Mathf.Clamp(timer - Time.deltaTime, 0f, returningTime);
                WinnerGraph.Instance.SetExitButtonText(returnText[CorrectLang.langIndices[YG2.lang]] + $"{Mathf.Ceil(timer)}");
                yield return null;
            }
            if (NetworkManager.Singleton.IsHost)
            {
                QuitGame();
                RoomManager.Instance.UnlockSession();
            }
        }

        StartCoroutine(quitWaiter());
    }

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
        GenerateBag();
        InitiateScores();
        SpawnInitialObjectRpc();
        GenerateHandsRpc();
        ShowNamesRpc();

        HandleTurn();
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
        UpdateBagSizeRpc(bag.Count);
        ReturnDiceRpc(clientId, result);
        hands[clientId].Add(result);
    }

    [Rpc(SendTo.Server)]
    public void GetDiceRpc(int clientId)
    {
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

    [Rpc(SendTo.Server)]
    private void RemoveCodeFromHandRpc(int clientId, int[] code)
    {
        System.Func<int[], int[], bool> equals = (lhs, rhs) =>
        {
            bool result = true; ;
            for (int i = 0; i < lhs.Length; i++)
            {
                result = true;
                for (int j = 0; j < lhs.Length; j++)
                {
                    if (lhs[j] != rhs[(i + j) % rhs.Length])
                    {
                        result = false;
                        break;
                    }
                }
                if (result) return result;
            }
            return result;
        };

        hands[clientId].RemoveAll(x => equals(x, code));
    }

    private void ConstructTilesAround(Vector2 position, bool state, int[] code)
    {
        RemoveCodeFromHandRpc(clientIndex[NetworkManager.Singleton.LocalClientId], code);

        DiceRpc(position, code);

        float originAngle = (state ? Mathf.PI : 0) - Mathf.PI / 2;
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

    [Rpc(SendTo.Server)]
    public void UpdateScoresRpc(int clientId, int value)
    {
        scores[(int)clientId] += value;
    }

    [Rpc(SendTo.Server)]
    private void SetCodeRpc(Vector2 targetCoordinates, int index, int value)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);

        if (hit != null)
        {
            if (hit.CompareTag("Place"))
            {
                hit.gameObject.GetComponent<MultiplayerTile>().SetCodeRpc(index, value);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DiceRpc(Vector2 position, int[] code)
    {
        if (Vector2.Distance(Vector2.zero, position) > Mathf.Epsilon)
            AudioManager.Instance.Play("Dice");
        VisualManager.Instance.Dice(position, code);
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlaceRpc(Vector2 position, bool state)
    {
        Transform tile = Instantiate(tilePrefab, position, Quaternion.Euler(0f, 0f, state ? 180 : 0));
        tile.GetComponent<NetworkObject>().Spawn(true);
        tile.GetComponent<ITile>().SetState(state);
        tile.SetParent(GameObject.FindGameObjectWithTag("Board").transform);
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
    }

    private void InitiateScores()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            scores.Add(0);
        }
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == RoomManager.Instance.GameScene)
            SceneLoadedRpc(clientIndex[NetworkManager.Singleton.LocalClientId]);
    }

    private void HandleTurn()
    {
        if (currentIdTurn.Value == clientIndex[NetworkManager.Singleton.LocalClientId])
        {
            VisualManager.Instance.RefreshHand();
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
        for (int j = 0; j < board.childCount; j++)
        {
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Suits(place.GetComponent<ITile>().GetCode(), code))
            {
                playableDice = true;
                break;
            }
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
        GetDiceRpc(clientIndex[NetworkManager.Singleton.LocalClientId]);
        NullSkippedPlayersRpc();
        ChangeTurnRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameRpc()
    {
        List<int> stars = GetStars();
        for (int i = 0; i < stars.Count; i++)
        {
            if (clientIndex[NetworkManager.Singleton.LocalClientId] == i)
            {
                int newStars = PlayerPrefs.GetInt("stars") + stars[i];
                PlayerPrefs.SetInt("stars", newStars);
                PlayerPrefs.Save();
                YG2.SetLeaderboard("starsLeaderboard", newStars);
                RoomManager.Instance.UpdateStarCounter(newStars.ToString());
            }
        }
        VisualManager.Instance.HidePlaces();
    }

    public void MakeComputerMove()
    {
        throw new System.NotImplementedException();
    }

    private void OnPreShutdown()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnApplicationQuit()
    {
        Exit();
    }

    public void Exit()
    {
        RoomManager.Instance.LeaveSession();
    }

    private int CountStarsFor(int index)
    {
        int result = 0;
        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i] < scores[index])
            {
                result++;
            }
        }
        return result;
    }

    public List<int> GetStars()
    {
        if (stars != null) return stars;
        stars = new List<int>();
        for (int i = 0; i < scores.Count; i++)
        {
            stars.Add(0);
        }
        for (int i = 0; i < stars.Count; i++)
        {
            stars[i] = CountStarsFor(i);
        }
        return stars;
    }
}
