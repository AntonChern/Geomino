using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class SingleplayerManager : MonoBehaviour, IGameManager
{
    public static SingleplayerManager Instance;

    private int faces = 3;
    private int dices = 4;
    private int skippedPlayers = 0;

    public int currentIdTurn = 0;

    private List<(int, int, float)> availableMoves = new List<(int, int, float)>();

    public event System.Action<int> OnCurrentIdTurnChanged;
    public int CurrentIdTurn
    {
        get { return currentIdTurn; }
        set
        {
            if (currentIdTurn == value) return;
            currentIdTurn = value;
            OnCurrentIdTurnChanged?.Invoke(currentIdTurn);
        }
    }
    public List<int> scores = new List<int>();
    public List<int> winners = new List<int>();
    private List<int> stars = null;

    private List<int[]> bag;
    private List<int[]> history = new List<int[]>();

    private int players;
    private List<int[][]> hands;
    private List<int> disabledComputerDices = new List<int>();

    [SerializeField] private Transform tilePrefab;

    private Difficulty[] difficulties;
    private Dictionary<string, string> botText = new Dictionary<string, string>()
    {
        { "ru", "Бот" },
        { "en", "Bot" }
    };

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one SingleplayerManager instance!");
        }
        Instance = this;

        players = PlayerPrefs.GetInt("players");

        OnCurrentIdTurnChanged += OnTurnChanged;

        foreach (var board in GameObject.FindGameObjectsWithTag("Board"))
        {
            if (board.name == "BoardMultiplayer")
            {
                Destroy(board);
            }
        }
    }

    private void Start()
    {
        GenerateBag();
        InitiateScores();
        SpawnInitialObject();
        GenerateHands();
        SetDifficulties();
        ShowNames();

        HandleTurn();
    }

    private void SetDifficulties()
    {
        difficulties = new Difficulty[players - 1];
        for (int i = 0; i < players - 1; i++)
        {
            difficulties[i] = (Difficulty)PlayerPrefs.GetInt($"difficulty{i}");
        }
    }

    private void HandleTurn()
    {
        if (currentIdTurn == 0)
        {
            VisualManager.Instance.RefreshHand();
        }
        else
        {
            VisualManager.Instance.CheckComputerMoves(hands[currentIdTurn - 1]);
        }
    }

    private void ShowNames()
    {
        for (int i = 0; i < players; i++)
        {
            Scoreboard.Instance.SetPlayer(i, i == 0 ? PlayerPrefs.GetString("playerName") : $"{DifficultyHandler.Translate(difficulties[i - 1])} {botText[YG2.lang]}");
        }
    }

    private void UpdateBagSize(int size)
    {
        Bag.Instance.UpdateBag(size);
    }

    public int[] GetDice(int index)
    {
        if (bag.Count == 0)
        {
            return null;
        }
        int[] result = bag[index];
        bag.RemoveAt(index);
        UpdateBagSize(bag.Count);
        return result;
    }

    public int[] GetDice()
    {
        return GetDice(Random.Range(0, bag.Count - 1));
    }

    private void GenerateHands()
    {
        foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
        {
            VisualManager.Instance.GenerateDiceUI(GetDice());
        }

        hands = new List<int[][]>();
        for (int i = 0; i < players - 1; i++)
        {
            int[][] hand = new int[dices][];
            for (int j = 0; j < dices; j++)
            {
                hand[j] = GetDice();
            }
            hands.Add(hand);
        }
    }

    private void SpawnInitialObject()
    {
        Vector2 position = Vector2.zero;
        SpawnPlace(position, false);
        ConstructTilesAround(position, false, new int[3] { 0, 0, 0 });

        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null)
        {
            for (int i = 0; i < faces; i++)
            {
                hit.gameObject.GetComponent<SingleplayerTile>().SetCode(i, 0);
            }
        }
    }

    private void Dice(Vector2 position, int[] code)
    {
        if (Vector2.Distance(Vector2.zero, position) > Mathf.Epsilon)
            AudioManager.Instance.Play("Dice");
        VisualManager.Instance.Dice(position, code);
    }

    private void SpawnPlace(Vector2 position, bool state)
    {
        Transform tile = Instantiate(tilePrefab, position, Quaternion.Euler(0f, 0f, state ? 180 : 0));
        tile.GetComponent<SingleplayerTile>().SetState(state);
        tile.SetParent(GameObject.FindGameObjectWithTag("Board").transform);
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
                SpawnPlace(targetCoordinates, !state);
            }
            SetCode(targetCoordinates, i, code[i]);
            value += UpdateScores(targetCoordinates, code[i]);
        }
        scores[currentIdTurn] += value;
        OnScoresChanged();

        if (value == 0) return;
        WorldCanvas.Instance.SpawnScore(position, value);
    }

    private int UpdateScores(Vector2 targetCoordinates, int value)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);
        if (hit != null)
        {
            if (hit.CompareTag("Dice"))
            {
                return value;
            }
        }
        return 0;
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

    public void MakeMove(Vector2 position, bool state, int[] code)
    {
        ConstructTilesAround(position, state, code);
        VisualManager.Instance.GenerateDiceUI(GetDice());
        NullSkippedPlayers();
        ChangeTurn();
    }

    private int CountDicesInPractice(int[] code)
    {
        int dicesInPractice = 0;
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject dice = board.GetChild(i).gameObject;
            if (!dice.CompareTag("Dice"))
                continue;
            dicesInPractice++;
        }
        return dicesInPractice;
    }

    private int CountSuitableDicesInPractice(int[] code)
    {
        int suitableDicesInPractice = 0;
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 1; i < board.childCount; i++)
        {
            GameObject dice = board.GetChild(i).gameObject;
            if (!dice.CompareTag("Dice"))
                continue;
            if (Suits(code, dice.GetComponent<ITile>().GetCode()))
            {
                suitableDicesInPractice++;
            }
        }
        foreach (int[] diceCode in hands[currentIdTurn - 1])
        {
            if (diceCode == null) continue;
            if (Suits(code, diceCode))
            {
                suitableDicesInPractice++;
            }
        }
        return suitableDicesInPractice;
    }

    private int CountSuitableDicesInTheory(int[] code)
    {
        int suitableDicesInTheory = 0;
        int colors = 7;
        for (int i = 0; i < colors; i++)
        {
            for (int j = i + 1; j < colors; j++)
            {
                for (int k = j + 1; k < colors; k++)
                {
                    if (Suits(code, new int[3] { i, j, k }))
                    {
                        suitableDicesInTheory++;
                    }
                    if (Suits(code, new int[3] { i, k, j }))
                    {
                        suitableDicesInTheory++;
                    }
                }
            }
        }
        return suitableDicesInTheory;
    }

    private float CountProbabilityForPlace(int[] code)
    {
        if ((float)(CountSuitableDicesInTheory(code) - CountSuitableDicesInPractice(code)) / (71 - (hands[currentIdTurn - 1].Count(x => x != null) + CountDicesInPractice(code))) < 0f)
        {
            Debug.Log($"code {string.Join(" ", code)}");
            Debug.Log($"{hands[currentIdTurn - 1].Count(x => x != null)}");
            Debug.Log($"({CountSuitableDicesInTheory(code)} - {CountSuitableDicesInPractice(code)}) / ({71} - {hands[currentIdTurn - 1].Count(x => x != null) + CountDicesInPractice(code)}) = {(CountSuitableDicesInTheory(code) - CountSuitableDicesInPractice(code))} / {71 - (4 + CountDicesInPractice(code))} = {(float)(CountSuitableDicesInTheory(code) - CountSuitableDicesInPractice(code)) / (71 - (4 + CountDicesInPractice(code)))}");
            Debug.Log($"Prob {(float)(CountSuitableDicesInTheory(code) - CountSuitableDicesInPractice(code)) / (71 - (4 + CountDicesInPractice(code)))}");
        }
        return (float)(CountSuitableDicesInTheory(code) - CountSuitableDicesInPractice(code)) / (71 - (hands[currentIdTurn - 1].Count(x => x != null) + CountDicesInPractice(code)));
    }

    private float CountValueForPlace(GameObject place)
    {
        float result = 0f;
        float originAngle = (place.GetComponent<ITile>().GetState() ? Mathf.PI : 0) - Mathf.PI / 2;
        for (int i = 0; i < faces; i++)
        {
            float angle = originAngle + 2 * Mathf.PI * i / faces;
            Vector2 targetCoordinates = new Vector2(place.transform.position.x, place.transform.position.y) + CoordinateConverter.PolarToCartesian(1f / Mathf.Sqrt(3), angle);

            Collider2D hitCollider = Physics2D.OverlapPoint(targetCoordinates);

            if (hitCollider != null)
            {
                if (hitCollider.CompareTag("Dice"))
                {
                    result += place.GetComponent<ITile>().GetCode()[i];
                }
            }
        }
        return result;
    }

    private void CalculateMovesForDiceAndPlace(int diceIndex, int placeIndex, int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        GameObject candidatePlace = board.GetChild(placeIndex).gameObject;
        float value = 0f;
        float originAngle = (candidatePlace.GetComponent<ITile>().GetState() ? Mathf.PI : 0) - Mathf.PI / 2;
        for (int i = 0; i < faces; i++)
        {
            float angle = originAngle + 2 * Mathf.PI * i / faces;
            Vector2 targetCoordinates = new Vector2(candidatePlace.transform.position.x, candidatePlace.transform.position.y) + CoordinateConverter.PolarToCartesian(1f / Mathf.Sqrt(3), angle);

            Collider2D hitCollider = Physics2D.OverlapPoint(targetCoordinates);

            if (hitCollider == null)
            {
                if (MapManager.Instance.CanBePlaced(targetCoordinates))
                {
                    int[] placeCode = new int[3] { -1, -1, -1 }; ;
                    placeCode[i] = code[i];
                    value -= code[i] * CountProbabilityForPlace(placeCode);
                }
            }
            else
            {
                if (hitCollider.CompareTag("Dice"))
                {
                    value += candidatePlace.GetComponent<ITile>().GetCode()[i];
                }
                if (hitCollider.CompareTag("Place"))
                {
                    int[] placeCode = (int[])hitCollider.GetComponent<ITile>().GetCode().Clone();
                    placeCode[i] = code[i];

                    value -= (CountValueForPlace(hitCollider.gameObject) + code[i]) * CountProbabilityForPlace(placeCode);
                }
            }
        }

        for (int i = 0; i < board.childCount; i++)
        {
            if (i == placeIndex) continue;

            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Vector2.Distance(place.transform.position, board.GetChild(placeIndex).position) < Mathf.Sqrt(3) / 3 + Mathf.Epsilon)
            {
                continue;
            }
            value -= CountValueForPlace(place) * CountProbabilityForPlace(place.GetComponent<ITile>().GetCode());
        }

        availableMoves.Add((diceIndex, placeIndex, value));
    }

    private void CalculateMovesForDice(int diceIndex, int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (VisualManager.Instance.Suits(place, code))
            {
                CalculateMovesForDiceAndPlace(diceIndex, i, place.GetComponent<SingleplayerTile>().GetTemporaryCode());
            }
        }
    }

    private (int, int) CalculateMove()
    {
        int[][] hand = hands[currentIdTurn - 1];
        for (int i = 0; i < hand.Length; i++)
        {
            int[] code = hand[i];
            if (code == null || disabledComputerDices.Contains(i)) continue;
            CalculateMovesForDice(i, code);
        }
        availableMoves.Sort((x, y) => y.Item3.CompareTo(x.Item3));
        return GetMoveByDifficulty();
    }

    private (int, int) GetMoveByDifficulty()
    {
        int index = 0;
        switch (difficulties[currentIdTurn - 1])
        {
            case Difficulty.Easy:
                index = Random.Range(0, availableMoves.Count / 2);
                break;
            case Difficulty.Medium:
                index = Random.Range(0, 3 * availableMoves.Count / 10);
                break;
            case Difficulty.Hard:
                index = Random.Range(0, availableMoves.Count / 10);
                break;
            case Difficulty.Impossible:
                index = 0;
                break;
        }
        return (availableMoves[index].Item1, availableMoves[index].Item2);
    }

    public void MakeComputerMove()
    {
        int diceIndex, placeIndex = 0;
        (diceIndex, placeIndex) = CalculateMove();
        DiceComputer(diceIndex, placeIndex);
        hands[currentIdTurn - 1][diceIndex] = GetDice();
        availableMoves.Clear();
        NullSkippedPlayers();
        ChangeTurn();
    }

    private void DiceComputer(int diceIndex, int placeIndex)
    {
        GameObject place = GameObject.FindGameObjectWithTag("Board").transform.GetChild(placeIndex).gameObject;
        VisualManager.Instance.Suits(place, hands[currentIdTurn - 1][diceIndex]);
        place.GetComponent<SingleplayerTile>().SynchronizeCodes();
        ConstructTilesAround(place.transform.position, place.GetComponent<SingleplayerTile>().GetState(), place.GetComponent<SingleplayerTile>().GetCode());
    }

    private void ChangeTurn()
    {
        CurrentIdTurn = (CurrentIdTurn + 1) % players;
    }

    private void OnTurnChanged(int obj)
    {
        HandleTurn();
        Scoreboard.Instance.UpdateTurn(currentIdTurn);
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
        skippedPlayers++;
        if (players == skippedPlayers)
        {
            FinishGame();
            return;
        }
        ChangeTurn();
    }

    private void FinishGame()
    {
        List<int> stars = GetStars();
        int newStars = PlayerPrefs.GetInt("stars") + stars[0];
        PlayerPrefs.SetInt("stars", newStars);
        PlayerPrefs.Save();
        YG2.SetLeaderboard("starsLeaderboard", newStars);

        ShowWinner();
        GenerateGraph();
    }

    private void ShowWinner()
    {
        YG2.InterstitialAdvShow();
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

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public List<int> GetStars()
    {
        if (stars != null) return stars;
        stars = new List<int>();
        for (int i = 0; i < scores.Count; i++)
        {
            stars.Add(0);
            if (i == 0) continue;
            if (scores[i] < scores[0] && difficulties[i - 1] == Difficulty.Impossible)
            {
                stars[0]++;
            }
        }
        return stars;
    }
}
