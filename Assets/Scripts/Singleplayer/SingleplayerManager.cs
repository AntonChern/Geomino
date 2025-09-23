using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleplayerManager : MonoBehaviour, IGameManager
{
    public static SingleplayerManager Instance;

    private int faces = 3;
    private int dices = 4;
    private int skippedPlayers = 0;

    public int currentIdTurn = 0;

    public event System.Action<int> OnCurrentIdTurnChanged;
    public int CurrentIdTurn
    {
        get { return currentIdTurn; }
        set
        {
            if (currentIdTurn == value) return; // Prevent unnecessary events if value is the same
            currentIdTurn = value;
            // Invoke the event if there are any listeners
            OnCurrentIdTurnChanged?.Invoke(currentIdTurn);
        }
    }
    public List<int> scores = new List<int>();
    public List<int> winners = new List<int>();

    private List<int[]> bag;
    private List<int[]> history = new List<int[]>();

    private int players;
    private List<int[][]> hands;
    private List<int> disabledComputerDices = new List<int>();

    [SerializeField] private Transform tilePrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one SingleplayerManager instance!");
        }
        Instance = this;

        players = PlayerPrefs.GetInt("players");

        OnCurrentIdTurnChanged += OnTurnChanged;
        //Debug.Log(players);

        foreach (var board in GameObject.FindGameObjectsWithTag("Board"))
        {
            if (board.name == "BoardMultiplayer")
            {
                Destroy(board);
            }
        }
        //Destroy(GameObject.FindGameObjectWithTag("Board").GetComponent<NetworkObject>());
    }

    //private void OnTurnChanged(int obj)
    //{
    //    throw new NotImplementedException();
    //}

    private void Start()
    {
        GenerateBag();
        InitiateScores();
        SpawnInitialObject();
        GenerateHands();
        ShowNames();

        HandleTurn();
    }

    private void HandleTurn()
    {
        //VisualManager.Instance.RefreshHand();
        if (currentIdTurn == 0)
        {
            //VisualManager.Instance.ShowPlaces();
            VisualManager.Instance.RefreshHand();
            //OnBeginMove?.Invoke(this, EventArgs.Empty);
            // RefreshHand
            //CheckMoves();
        }
        else
        {
            VisualManager.Instance.CheckComputerMoves(hands[currentIdTurn - 1]);
            // computer
            // проверить, есть ли хоть что-то, что можно поставить
            // если да  => ничего не делаем
            // если нет => SkipMove();
        }
    }

    private void ShowNames()
    {
        for (int i = 0; i < players; i++)
        {
            //Debug.Log(i == 0 ? PlayerPrefs.GetString("playerName") : $"Компьютер {i}");
            Scoreboard.Instance.SetPlayer(i, i == 0 ? PlayerPrefs.GetString("playerName") : $"Компьютер {i}");
        }
    }

    private void UpdateBagSize(int size)
    {
        Bag.Instance.UpdateBag(size);
    }

    //private void ReturnDiceRpc(ulong clientId, int[] result)
    //{
    //    if (NetworkManager.Singleton.LocalClientId == clientId)
    //    {
    //        //if (result != null)
    //        //    Debug.Log("Recieved " + result[0] + ", " + result[1] + ", " + result[2]);
    //        VisualManager.Instance.GenerateDiceUI(result);
    //    }
    //}

    public int[] GetDice(int index)
    {
        if (bag.Count == 0)
        {
            //ReturnDiceRpc(clientId, null);
            return null;
        }
        int[] result = bag[index];
        bag.RemoveAt(index);
        // изменить переменную, ответственную за количество оставшихся фишек
        UpdateBagSize(bag.Count);
        //return result;
        //Debug.Log("Bag count = " + bag.Count);
        //ReturnDiceRpc(clientId, result);
        return result;
        //int[] result = new int[3] { bag[index] / 100, bag[index] / 10 % 10, bag[index] % 10 };
        //bag.RemoveAt(index);
        //return result;
    }

    public int[] GetDice()
    {
        //return GetDiceRpc(UnityEngine.Random.Range(0, bag.Count - 1));
        //int[] result = GetDiceRpc(clientId, UnityEngine.Random.Range(0, bag.Count - 1));
        return GetDice(Random.Range(0, bag.Count - 1));
    }

    private void GenerateHands()
    {
        foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
        {
            VisualManager.Instance.GenerateDiceUI(GetDice());
        }

        //Debug.Log($"Player Hand Generated");
        hands = new List<int[][]>();
        for (int i = 0; i < players - 1; i++)
        {
            //Debug.Log(GameObject.FindGameObjectsWithTag("DiceUI").Length);
            int[][] hand = new int[dices][];
            for (int j = 0; j < dices; j++)
            {
                hand[j] = GetDice();
            }
            hands.Add(hand);
        }
        //Debug.Log($"Computer Hands Generated");
    }

    private void SpawnInitialObject()
    {
        Vector2 position = Vector2.zero;
        SpawnPlace(position, false);
        ConstructTilesAround(position, false, new int[3] { 0, 0, 0 });
    }

    private void Dice(Vector2 position, int[] code)
    {
        VisualManager.Instance.Dice(position, code);
    }

    private void SpawnPlace(Vector2 position, bool state)
    {
        //Debug.Log("SpawnPlace");
        Transform tile = Instantiate(tilePrefab, position, Quaternion.Euler(0f, 0f, state ? 180 : 0));
        //Destroy(tile.gameObject.GetComponent<NetworkObject>());
        //Destroy(tile.gameObject.GetComponent<NetworkTransform>());
        //Destroy(tile.gameObject.GetComponent<MultiplayerTile>());
        tile.GetComponent<SingleplayerTile>().SetState(state);
        //Debug.Log($"trans { tile == null}");
        tile.SetParent(GameObject.FindGameObjectWithTag("Board").transform);
        //tile.gameObject.GetComponent<Tile>().code[index] = value;
    }

    private void SetCode(Vector2 targetCoordinates, int index, int value)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);

        if (hit != null)
        {
            if (hit.CompareTag("Place"))
            {
                hit.gameObject.GetComponent<SingleplayerTile>().SetCode(index, value);
            }
        }
    }
    
    private void ConstructTilesAround(Vector2 position, bool state, int[] code)
    {
        Dice(position, code);

        Collider2D hit = Physics2D.OverlapPoint(position);

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
                SpawnPlace(targetCoordinates, !state);
            }
            SetCode(targetCoordinates, i, code[i]);
            UpdateScores(targetCoordinates, code[i]);
        }
    }

    private void UpdateScores(Vector2 targetCoordinates, int value)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);
        if (hit != null)
        {
            if (hit.CompareTag("Dice"))
            {
                scores[currentIdTurn] += value;
                OnScoresChanged();
            }
        }
    }

    private void OnScoresChanged()
    {
        if (players != scores.Count)
        {
            return;
        }
        CalculateWinners();
        UpdateHistory();
        Scoreboard.Instance.UpdateScores();
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

    private void CalculateWinners()
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
        OnWinnersChanged();
    }

    private void OnWinnersChanged()
    {
        for (int index = 0; index < players; index++)
        {
            if (winners.Contains(index))
            {
                Scoreboard.Instance.HighlightPlayer(index);
            }
            else
            {
                Scoreboard.Instance.LowlightPlayer(index);
            }
        }
    }

    private void InitiateScores()
    {
        for (int i = 0; i < players; i++)
        {
            scores.Add(0);
            OnScoresChanged();
        }
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

    //public bool RefreshHand()
    //{
    //    bool answer = false;
    //    foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
    //    {
    //        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
    //        for (int i = 0; i < board.childCount; i++)
    //        {
    //            GameObject place = board.GetChild(i).gameObject;
    //            if (!place.CompareTag("Place"))
    //                continue;

    //            if (VisualManager.Instance.Suits(place, diceUI.GetComponent<DiceUI>().Code))
    //            {
    //                answer = true;
    //                diceUI.GetComponent<DiceUI>().Enable();
    //            }
    //            else
    //            {
    //                diceUI.GetComponent<DiceUI>().Disable();
    //            }
    //        }
    //    }
    //    return answer;
    //}

    public void MakeMove(Vector2 position, bool state, int[] code)
    {
        ConstructTilesAround(position, state, code);
        VisualManager.Instance.GenerateDiceUI(GetDice());
        NullSkippedPlayers();
        ChangeTurn();

        //if (players == skippedPlayers)
        //{
        //    return;
        //}

        //for (int i = 0; i < players - 1; i++)
        //{
        //    MakeComputerMove();
        //}
    }

    public void MakeComputerMove()
    {
        //for (int i = 0; i < players - 1; i++)
        //{
        //    Debug.Log($"{i + 1} Disabled Dices {string.Join(" ", disabledComputerDices)} {disabledComputerDices.Count}");
        //}
        int index = GetPlayableDiceComputer();
        //Debug.Log($"INDEX {index}");
        DiceComputer(hands[currentIdTurn - 1][index]);
        hands[currentIdTurn - 1][index] = GetDice();
        // выбрать index в hands[currentTurnId]
        // поставить его на рандомное место
        // GetDice() в этот индекс
        NullSkippedPlayers();
        ChangeTurn();
    }

    private void DiceComputer(int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        //Debug.Log("playableDice");
        for (int j = 0; j < board.childCount; j++)
        {
            //Debug.Log("Inside loop 2 " + j);
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            //if (VisualManager.Instance.Suits(place.GetComponent<ITile>().GetCode(), code))
            if (VisualManager.Instance.Suits(place, code))
            {
                //Dice(place.transform.position, code);
                place.GetComponent<SingleplayerTile>().SynchronizeCodes();
                ConstructTilesAround(place.transform.position, place.GetComponent<SingleplayerTile>().GetState(), place.GetComponent<SingleplayerTile>().GetCode());
                return;
            }
        }
    }

    private int GetPlayableDiceComputer()
    {
        //Debug.Log($"GetPlayableDiceComputer");
        //for (int i = 0; i < players - 1; i++)
        //{
        //    string res = "[";
        //    for (int j = 0; j < 4; j++)
        //    {
        //        if (hands[i][j] == null) { res += "NULL, "; continue; }
        //        res += string.Join(" ", hands[i][j]) + ", ";
        //    }
        //    res += "]";
        //    Debug.Log($"{i + 1} {res}");
        //}
        int index = Random.Range(0, dices);
        //int index = -1;
        //Debug.Log($"{string.Join(" ", disabledComputerDices)}");
        //for (int i = 0; i < 4; i++)
        //{
        //    if (hands[currentIdTurn - 1][i] != null && !disabledComputerDices.Contains(i))
        //    {
        //        index = i;
        //        break;
        //    }
        //}
        while (hands[currentIdTurn - 1][index] == null || disabledComputerDices.Contains(index))
        {
            index = Random.Range(0, dices);
        }
        return index;
    }

    private void ChangeTurn()
    {
        CurrentIdTurn = (CurrentIdTurn + 1) % players;
        // on change turn
        //OnTurnChanged();
    }

    private void OnTurnChanged(int obj)
    {
        HandleTurn();
        Scoreboard.Instance.UpdateTurn(currentIdTurn);
        //int idTurn = currentIdTurn;

        //if (players == skippedPlayers || currentIdTurn != idTurn)
        //{
        //    return;
        //}
        //for (int i = 0; i < players - 1; i++)
        //{
        //    string res = "[";
        //    for (int j = 0; j < 4; j++)
        //    {
        //        if (hands[i][j] == null) { res += "NULL"; continue; }
        //        res += string.Join(" ", hands[i][j]) + ", ";
        //    }
        //    res += "]";
        //    Debug.Log($"{i + 1} {res}");
        //}

        //if (currentIdTurn != 0)
        //{
        //    disabledComputerDices.Clear();
        //    MakeComputerMove();
        //}
    }

    private void NullSkippedPlayers()
    {
        skippedPlayers = 0;
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

    public void HandleButton(int index, int[] code)
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
        }
        if (currentIdTurn == 0)
        {
            VisualManager.Instance.ReturnHandledButton(index, playableDice);
        }
        else
        {
            VisualManager.Instance.ReturnHandledComputerButton(index, playableDice);
            if (!playableDice) disabledComputerDices.Add(index);
        }
    }

    public List<int> GetScores()
    {
        return scores;
    }

    public void EndGame()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void SkipMove()
    {
        //Debug.Log($"Skip Move {currentIdTurn}");
        skippedPlayers++;
        if (players == skippedPlayers)
        {
            //EndGame();

            ShowWinner();
            GenerateGraph();
            //Debug.Log("END");
            return;
        }
        ChangeTurn();
    }

    private void ShowWinner()
    {
        WinnerGraph.Instance.ShowScoreboard();
        VisualManager.Instance.HideBackPanel();
    }

    void GenerateGraph()
    {
        WinnerGraph.Instance.Draw();
    }

    public List<int[]> GetHistory()
    {
        return history;
    }

    public List<int> GetWinners()
    {
        return winners;
    }
}
