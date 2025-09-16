//using System;
//using System.Linq;
//using System;
//using System.Drawing;
using System;
//using System.Drawing;
//using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameVisualManager : NetworkBehaviour
{
    public float scale = 1f;
    [SerializeField]
    private Transform tilePrefab;
    private Vector2[] startButtonPosition;
    [SerializeField]
    private Button[] panel;
    private int[][] panelCodes;
    [SerializeField]
    private Sprite[] sprites;
    [SerializeField]
    private TextMeshProUGUI[] scoreText;
    [SerializeField]
    private TextMeshProUGUI[] nameText;
    [SerializeField]
    private TextMeshProUGUI bagSizeText;

    //[SerializeField]
    //private TextMeshProUGUI winnerText;
    [SerializeField]
    private TextMeshProUGUI[] endName;
    [SerializeField]
    private TextMeshProUGUI[] endScore;
    [SerializeField]
    private Image[] endWinner;

    [SerializeField]
    private GameObject BackPanel;


    [SerializeField]
    private Image paperFront;
    private bool playGraphAnimation = false;
    private float graphAnimationTime = 2f;
    private float graphAnimationTimer = 2f;

    [SerializeField]
    private GameObject winnerGraph;
    [SerializeField]
    private Button exitButton;

    private Color playerTextLightColor = Color.white;
    private Color playerTextDarkColor = Color.gray;
    private Color winnerTextLightColor = Color.yellow;
    private Color winnerTextDarkColor = new Color(0.5f, 0.5f, 0f);


    private int faces = 3;
    private int freePanelDice = 0;
    private int pressed;

    private int availableDice = 0;

    private GameObject lastDice;

    public RectTransform graphContainer;
    [SerializeField]
    private GameObject[] pointPrefab; // A small UI Image prefab for data points
    [SerializeField]
    private GameObject[] lineSegmentPrefab; // A UI Image prefab for line segments

    private bool[] playNewDiceAnimation;
    private float newDiceAnimationTime = 0.4f;
    private float[] newDiceAnimationTimer;
    private float newDiceDistance = 200f;
    //private int animationButton = 0;
    //private bool animationChoose = false;
    //private float animationChooseTimer = 0f;
    //[SerializeField]
    //private float animationChooseSpeed = 1f;
    //public List<float> dataPoints = new List<float>() { 5f, 10f, 7f, 12f, 8f };
    //public float yMax = 15f; // Maximum Y value for scaling

    //void Start()
    //{
    //    Debug.Log("Start GraphDrawer");
    //    MultiplayerGameManager.Instance.OnGameEnded += MultiplayerGameManager_OnGameEnded;
    //    gameObject.SetActive(false);
    //}

    //private void MultiplayerGameManager_OnGameEnded(object sender, System.EventArgs e)
    //{
    //    Debug.Log("OnGameEnded GraphDrawer");

    //    GenerateGraphRpc();
    //}

    private int GetYMax()
    {
        int maxValue = 0;
        for (int i = 0; i < MultiplayerGameManager.Instance.scores.Count; i++)
        {
            if (MultiplayerGameManager.Instance.scores[i] > maxValue)
            {
                maxValue = MultiplayerGameManager.Instance.scores[i];
            }
        }
        return maxValue;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void GenerateGraphRpc()
    {
        //Debug.Log("GenerateGraph");
        winnerGraph.SetActive(true);
        //winnerGraph.SetActive(true);

        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;
        float xSpacing = graphWidth / (MultiplayerGameManager.Instance.history.Count - 1);

        float yMax = GetYMax();

        Vector2 lastPointPos = Vector2.zero;

        for (int j = 0; j < MultiplayerGameManager.Instance.scores.Count; j++)
        {
            for (int i = 0; i < MultiplayerGameManager.Instance.history.Count; i++)
            {
                float xPos = i * xSpacing;
                float yPos = (MultiplayerGameManager.Instance.history[i][j] / yMax) * graphHeight;
                Vector2 currentPointPos = new Vector2(xPos - graphWidth / 2, yPos - graphHeight / 2);

                // Draw line segment to previous point (if not the first point)
                if (i > 0)
                {
                    DrawLineSegment(lastPointPos, currentPointPos, j);
                }
                lastPointPos = currentPointPos;
                // Create and position data point
                //GameObject point = Instantiate(pointPrefab[j], graphContainer);
                ////point.GetComponent<NetworkObject>().Spawn(true);
                ////point.GetComponent<NetworkObject>().TrySetParent(graphContainer, false);
                //point.GetComponent<RectTransform>().anchoredPosition = currentPointPos;

            }

            for (int i = 0; i < MultiplayerGameManager.Instance.history.Count; i++)
            {
                float xPos = i * xSpacing;
                float yPos = (MultiplayerGameManager.Instance.history[i][j] / yMax) * graphHeight;
                Vector2 currentPointPos = new Vector2(xPos - graphWidth / 2, yPos - graphHeight / 2);

                // Draw line segment to previous point (if not the first point)
                //if (i > 0)
                //{
                //    DrawLineSegment(lastPointPos, currentPointPos, j);
                //}
                //lastPointPos = currentPointPos;
                // Create and position data point
                GameObject point = Instantiate(pointPrefab[j], graphContainer);
                //point.GetComponent<NetworkObject>().Spawn(true);
                //point.GetComponent<NetworkObject>().TrySetParent(graphContainer, false);
                point.GetComponent<RectTransform>().anchoredPosition = currentPointPos;
                //point.transform.localScale = Vector3.one * 0.5f;

            }
        }

        playGraphAnimation = true;
    }

    //[Rpc(SendTo.ClientsAndHost)]
    void DrawLineSegment(Vector2 startPos, Vector2 endPos, int index)
    {
        GameObject lineSegment = Instantiate(lineSegmentPrefab[index], graphContainer);
        //lineSegment.GetComponent<NetworkObject>().Spawn(true);
        //lineSegment.GetComponent<NetworkObject>().TrySetParent(graphContainer, false);
        RectTransform lineRect = lineSegment.GetComponent<RectTransform>();

        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);

        lineRect.anchoredPosition = startPos + direction * (distance / 2f);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y * 0.8f); // Adjust line thickness if needed
        lineRect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void InitButtons()
    {
        startButtonPosition = new Vector2[panel.Length];
        playNewDiceAnimation = new bool[panel.Length];
        newDiceAnimationTimer = new float[panel.Length];
        for (int i = 0; i < panel.Length; i++)
        {
            startButtonPosition[i] = panel[i].transform.position;
            playNewDiceAnimation[i] = false;
            newDiceAnimationTimer[i] = newDiceAnimationTime;
        }
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void StopGameRpc()
    //{
    //    SceneManager.LoadScene(0);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //private void StopGameRpc()
    //{
    //    //NetworkManager.Singleton.Shutdown();
    //    Destroy(NetworkManager.Singleton.gameObject);
    //}

    private void Start()
    {
        MultiplayerGameManager.Instance.OnDoMove += MultiplayerGameManager_OnDoMove;
        MultiplayerGameManager.Instance.OnGameStarted += MultiplayerGameManager_OnGameStarted;
        MultiplayerGameManager.Instance.OnAddDice += MultiplayerGameManager_OnAddDice;
        MultiplayerGameManager.Instance.OnUpdateScore += MultiplayerGameManager_OnUpdateScore;
        MultiplayerGameManager.Instance.OnBeginMove += MultiplayerGameManager_OnBeginMove;
        MultiplayerGameManager.Instance.OnRemoveDiceFromBag += MultiplayerGameManager_OnRemoveDiceFromBag;
        MultiplayerGameManager.Instance.OnGameEnded += MultiplayerGameManager_OnGameEnded;
        MultiplayerGameManager.Instance.OnTurnChanged += MultiplayerGameManager_OnTurnChanged;

        winnerGraph.SetActive(false);

        exitButton.onClick.AddListener(() => {
            //Debug.Log("AuthenticateUI.Instance " + AuthenticateUI.Instance);
            //Debug.Log("EditPlayerName.Instance " + EditPlayerName.Instance);
            //Debug.Log("EditPlayerName.Instance.GetPlayerName() " + EditPlayerName.Instance.GetPlayerName());
            //SceneManager.LoadScene(0);
            //Destroy(NetworkManager.Singleton.gameObject);
            NetworkManager.Singleton.SceneManager.LoadScene("RoomSystem", LoadSceneMode.Single);
            //StopGameRpc();
            //NetworkManager.Singleton.Shutdown();
            //LobbyManager.Instance.StopGame();
        });
        if (!IsServer)
        {
            exitButton.gameObject.SetActive(false);
        }
        InitButtons();

        //Transform initialTile = Instantiate(tilePrefab, new Vector2(0f, 0f), Quaternion.identity);
        //initialTile.GetComponent<Tile>().Dice();
        //initialTile.GetComponent<NetworkObject>().Spawn(true);

        //SpawnObjectRpc(new Vector2(0f, 0f), false);
        //SpawnInitialObjectRpc();

        //initialTile.tag = "Dice";
        //initialTile.GetComponent<SpriteRenderer>().color = Color.white;
        //MultiplayerGameManager.instance.SetDice(transform.position, false);
        for (int i = 0; i < nameText.Length; i++)
        {
            nameText[i].color = winnerTextDarkColor;
            scoreText[i].color = winnerTextDarkColor;
            if (i == 0)
            {
                nameText[i].color = winnerTextLightColor;
                scoreText[i].color = winnerTextLightColor;
            }
        }
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void HighlightTurn()
    //{

    //}

    private void MultiplayerGameManager_OnTurnChanged(object sender, EventArgs e)
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            Color darkColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextDarkColor : playerTextDarkColor;
            scoreText[i].color = darkColor;
            nameText[i].color = darkColor;

            //scoreText[i].text = MultiplayerGameManager.Instance.scores[i].ToString();

            if (i == (int)MultiplayerGameManager.Instance.currentIdTurn.Value)
            {
                //result += i + ") " + e.value + "\n";
                Color lightColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextLightColor : playerTextLightColor;
                scoreText[i].color = lightColor;
                nameText[i].color = lightColor;
            }
        }
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void ShowWinnerGraphRpc()
    //{
    //    winnerGraph.SetActive(true);
    //}

    private void MultiplayerGameManager_OnGameEnded(object sender, EventArgs e)
    {
        ShowWinnerRpc();

        //ShowWinnerGraphRpc();
        GenerateGraphRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowWinnerRpc()
    {
        //for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        //{
        //    endName[i].gameObject.SetActive(true);
        //    endScore[i].gameObject.SetActive(true);
        //    if (MultiplayerGameManager.Instance.winners.Contains(i))
        //        endWinner[i].gameObject.SetActive(true);

        //    endName[i].text = nameText[i].text;
        //    endScore[i].text = scoreText[i].text;
        //}
        //winnerText.gameObject.SetActive(true);
        ////winnerGraph.gameObject.SetActive(true);
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            endName[i].gameObject.SetActive(true);
            endScore[i].gameObject.SetActive(true);
            if (MultiplayerGameManager.Instance.winners.Contains(i))
            {
                endWinner[i].gameObject.SetActive(true);
                //winnerText.text += nameText[i].text + " \t " + scoreText[i].text + "\n";

                //nameText[i].color = winnerTextLightColor;
                //scoreText[i].color = winnerTextLightColor;
            }
            //else
            //{
            //    nameText[i].color = playerTextLightColor;
            //    scoreText[i].color = playerTextLightColor;
            //}

            //nameText[i].gameObject.SetActive(false);
            //scoreText[i].gameObject.SetActive(false);

            endName[i].text = nameText[i].text;
            endScore[i].text = scoreText[i].text;
        }
        BackPanel.SetActive(false);
        //winnerText.text = winnerName;
    }

    private void MultiplayerGameManager_OnRemoveDiceFromBag(object sender, MultiplayerGameManager.OnRemoveDiceFromBagArgs e)
    {
        UpdateBagSizeRpc(e.size);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void HighlightNameRpc(int index, Color color)
    //{
    //    nameText[index].color = color;
    //    scoreText[index].color = color;
    //}

    //private void HighlightName()
    //{
    //    int index = (int)NetworkManager.Singleton.LocalClientId;
    //    Color color = playerTextLightColor;
    //    if (MultiplayerGameManager.Instance.winners.Contains(index))
    //    {
    //        color = winnerTextLightColor;
    //    }
    //    HighlightNameRpc(index, color);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //private void HighlightNameRpc()
    //{
    //    HighlightTurn();
    //}

    private void MultiplayerGameManager_OnBeginMove(object sender, EventArgs e)
    {
        CheckMoves();
        //HighlightNameRpc();
    }

    //private void HighlightTurn()
    //{
    //    Debug.Log("HighlightTurn");
    //    for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
    //    {
    //        Color darkColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextDarkColor : playerTextDarkColor;
    //        scoreText[i].color = darkColor;
    //        nameText[i].color = darkColor;

    //        scoreText[i].text = MultiplayerGameManager.Instance.scores[i].ToString();

    //        if (i == (int)MultiplayerGameManager.Instance.currentIdTurn.Value)
    //        {
    //            //result += i + ") " + e.value + "\n";
    //            Color lightColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextLightColor : playerTextLightColor;
    //            scoreText[i].color = lightColor;
    //            nameText[i].color = lightColor;
    //        }
    //    }
    //}

    private void UpdateButtonAnimation(int index)
    {
        if (playNewDiceAnimation[index])
        {
            newDiceAnimationTimer[index] -= Time.deltaTime;
            panel[index].transform.position = startButtonPosition[index] + Vector2.down * (newDiceDistance * Mathf.Clamp(newDiceAnimationTimer[index] / newDiceAnimationTime, 0f, 1f));
            if (newDiceAnimationTimer[index] <= 0f)
            {
                newDiceAnimationTimer[index] = newDiceAnimationTime;
                playNewDiceAnimation[index] = false;
            }
        }
    }

    private void UpdateButtonAnimations()
    {
        for (int i = 0; i < panel.Length; i++)
        {
            UpdateButtonAnimation(i);
        }
    }

    private void Update()
    {
        UpdateButtonAnimations();
        //if (animationChoose)
        //{
        //    animationChooseTimer += Time.deltaTime * animationChooseSpeed;
        //    //animationChooseTimer = animationChooseTimer % (Mathf.PI * 2);
        //    if (animationChooseTimer > Mathf.PI * 2)
        //        animationChooseTimer = 0f;
        //    //animationChooseTimer = Mathf.Clamp(animationChooseTimer + Time.deltaTime, 0f, Mathf.PI * 2f);
        //    float a = 10f;
        //    float t = animationChooseTimer + Mathf.PI / 2;
        //    panel[pressed].transform.position = startButtonPosition[pressed] +
        //        new Vector2(
        //            a * Mathf.Sqrt(2) * Mathf.Cos(t) / (1 + Mathf.Pow(Mathf.Sin(t), 2)),
        //            a * Mathf.Sqrt(2) * Mathf.Cos(t) * Mathf.Sin(t) / (1 + Mathf.Pow(Mathf.Sin(t), 2))
        //        );
        //}
        if (playGraphAnimation)
        {
            graphAnimationTimer -= Time.deltaTime;
            paperFront.GetComponent<Image>().fillAmount = Mathf.Clamp(graphAnimationTimer / graphAnimationTime, 0f, 1f);
            if (graphAnimationTimer <= 0f)
            {
                playGraphAnimation = false;
            }
        }
    }

    private void MultiplayerGameManager_OnUpdateScore(object sender, MultiplayerGameManager.OnUpdateScoreArgs e)
    {
        
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            //Color darkColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextDarkColor : playerTextDarkColor;
            //scoreText[i].color = darkColor;
            //nameText[i].color = darkColor;

            scoreText[i].text = MultiplayerGameManager.Instance.scores[i].ToString();

            //if (i == (int)MultiplayerGameManager.Instance.currentIdTurn.Value)
            //{
            //    //result += i + ") " + e.value + "\n";
            //    Color lightColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextLightColor : playerTextLightColor;
            //    scoreText[i].color = lightColor;
            //    nameText[i].color = lightColor;
            //}
        }
        //HighlightTurn();
        //string result = "";
        //for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        //{
        //    Color darkColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextDarkColor : playerTextDarkColor;
        //    scoreText[i].color = darkColor;
        //    nameText[i].color = darkColor;
        //    //if (i == e.index)
        //    //{
        //    //    //result += i + ") " + e.value + "\n";
        //    //    scoreText[i].text = e.value.ToString();
        //    //}
        //    //else
        //    //{
        //        //result += i + ") " + MultiplayerGameManager.Instance.scores[i] + "\n";
        //        scoreText[i].text = MultiplayerGameManager.Instance.scores[i].ToString();
        //    //}

        //    if (i == (int)MultiplayerGameManager.Instance.currentIdTurn.Value)
        //    {
        //        //result += i + ") " + e.value + "\n";
        //        Color lightColor = MultiplayerGameManager.Instance.winners.Contains(i) ? winnerTextLightColor : playerTextLightColor;
        //        scoreText[i].color = lightColor;
        //        nameText[i].color = lightColor;
        //    }
        //}
        //scoreText.text = result;


    }


    //private void ChangeDice()
    //{
    //    Debug.Log("PLACED");
    //    ChangeButtonColor(pressed, Color.white);
    //    if (bag.Count == 0)
    //    {
    //        panel[pressed].gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        int[] newCode = GetDice();
    //        //GameObject canvas = panel[pressed].transform.GetChild(0).gameObject;
    //        for (int i = 0; i < panel[pressed].transform.childCount; i++)
    //        {
    //            panel[pressed].transform.GetChild(i).gameObject.GetComponent<Image>().sprite = sprites[newCode[i]];
    //            panelCodes[pressed][i] = newCode[i];
    //            //text.GetComponent<TextMeshProUGUI>().text = newCode[i] == 0 ? "" : newCode[i].ToString();
    //        }
    //    }
    //    for (int i = 0; i < field.transform.childCount; i++)
    //    {
    //        GameObject place = field.transform.GetChild(i).gameObject;
    //        if (!place.CompareTag("Place"))
    //            continue;
    //        place.SetActive(false);
    //    }
    //}

    private void ClearAvailablePlaces()
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            SetLayer(place, LayerMask.NameToLayer("Places"));
        }
    }

    private void SetNewDice(int[] code)
    {
        panelCodes[freePanelDice] = new int[3] { -1, -1, -1 };
        for (int j = 0; j < 3; j++)
        {
            //Debug.Log("ShowPanelRpc " + e.code[0] + "," + e.code[1] + "," + e.code[2]);
            //panel[i].transform.GetChild(j).gameObject.GetComponent<TextMeshProUGUI>().text = ints[j].ToString() == "0" ? "" : ints[j].ToString();
            panel[freePanelDice].transform.GetChild(j).gameObject.GetComponent<Image>().sprite = sprites[code[j]];
            panelCodes[freePanelDice][j] = code[j];
        }
        //Debug.Log("panelCodes " + panelCodes[freePanelDice].Length);
    }

    private void PlayNewDiceAnimation()
    {
        playNewDiceAnimation[freePanelDice] = true;
        //animationButton = pressed;
        //panel[pressed].transform.position += Vector3.down * newDiceDistance;
    }

    private void MultiplayerGameManager_OnAddDice(object sender, MultiplayerGameManager.OnAddDiceArgs e)
    {
        //ChangeButtonColor(pressed, Color.white);
        if (e.code == null)
        {
            panel[pressed].gameObject.SetActive(false);
            panelCodes[pressed] = null;
            ClearAvailablePlaces();
            return;
        }

        if (freePanelDice == panel.Length)
        {
            freePanelDice = pressed;
            SetNewDice(e.code);
            ClearAvailablePlaces();
            PlayNewDiceAnimation();
            freePanelDice = panel.Length;
            return;
        }

        SetNewDice(e.code);
        PlayNewDiceAnimation();
        freePanelDice++;
    }

    //private void ShowPanel()
    //{
    //    foreach (Button dice in panel)
    //    {
    //        dice.gameObject.SetActive(true);
    //    }

    //    panelCodes = new int[panel.Length][];
    //    for (int i = 0; i < panel.Length; i++)
    //    {
    //        MultiplayerGameManager.Instance.GetDiceRpc(NetworkManager.Singleton.LocalClientId);
    //    }
    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowPanelRpc()
    {
        foreach (Button dice in panel)
        {
            dice.gameObject.SetActive(true);
        }

        panelCodes = new int[panel.Length][];
        //panelCodes[3] = new int[3] { -1, -1, -1 };
        //int[] zeroBag = MultiplayerGameManager.instance.GetDiceRpc(0);
        //MultiplayerGameManager.Instance.GetDiceRpc(NetworkManager.Singleton.LocalClientId, 0);
        //for (int j = 0; j < 3; j++)
        //{
        //    //panel[3].transform.GetChild(j).gameObject.GetComponent<TextMeshProUGUI>().text = bag[0][j].ToString() == "0" ? "" : bag[0][j].ToString();
        //    Debug.Log("ShowPanelRpc " + zeroBag[0] + "," + zeroBag[1] + "," + zeroBag[2]);
        //    panel[3].transform.GetChild(j).gameObject.GetComponent<Image>().sprite = sprites[zeroBag[j]];
        //    panelCodes[3][j] = zeroBag[j];
        //}
        //for (int i = 0; i < panel.Length - 1; i++)
        for (int i = 0; i < panel.Length; i++)
        {
            MultiplayerGameManager.Instance.GetDiceRpc(NetworkManager.Singleton.LocalClientId);
            //int[] ints = MultiplayerGameManager.instance.GetDiceRpc();
            //panelCodes[i] = new int[3] { -1, -1, -1 };
            //for (int j = 0; j < 3; j++)
            //{
            //    Debug.Log("ShowPanelRpc " + ints[0] + "," + ints[1] + "," + ints[2]);
            //    //panel[i].transform.GetChild(j).gameObject.GetComponent<TextMeshProUGUI>().text = ints[j].ToString() == "0" ? "" : ints[j].ToString();
            //    panel[i].transform.GetChild(j).gameObject.GetComponent<Image>().sprite = sprites[ints[j]];
            //    panelCodes[i][j] = ints[j];
            //}
        }
        //Debug.Log("Showed!!!");
        //Debug.Log("Bag Count = " + MultiplayerGameManager.instance.bag.Count);
        //CheckMovesRpc();
    }

    //[ServerRpc]
    //private void DealOutDecks()
    //{

    //}

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowNameRpc(int i, string name)
    {
        nameText[i].text = name;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FindAndShowNameRpc(int i)
    {
        if ((ulong)i == NetworkManager.Singleton.LocalClientId)
        {
            ShowNameRpc(i, RoomManager.Instance.PlayerName);
        }
    }

    [Rpc(SendTo.Server)]
    private void ShowNamesRpc()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            FindAndShowNameRpc(i);
        }
    }

    private void MultiplayerGameManager_OnGameStarted(object sender, EventArgs e)
    {
        //Debug.Log("Game started");
        SpawnInitialObjectRpc();
        ShowPanelRpc();

        ShowNamesRpc();

        //CheckMovesRpc();
        //DealOutDecks();
        //UpdateBagSizeRpc();
    }

    [Rpc(SendTo.Server)]
    private void SpawnInitialObjectRpc()
    {
        //Debug.Log("WHAT?");
        Vector2 position = Vector2.zero;
        SpawnPlaceRpc(position, false);
        ConstructTilesAround(position, false, new int[3] { 0, 0, 0 });
        //SubscribeTilesAround(position, false, new int[3] { 0, 0, 0 });
        //MultiplayerGameManager.instance.SetDice(position, false);

        //Transform initialTile = Instantiate(tilePrefab, new Vector2(0f, 0f), Quaternion.identity);
        //initialTile.GetComponent<NetworkObject>().Spawn(true);
        //initialTile.GetComponent<Tile>().Dice();
    }

    private void ConstructTilesAround(Vector2 position, bool state, int[] code)
    {
        DiceRpc(position, code);

        Collider2D hit = Physics2D.OverlapPoint(position);
        //int[] code = new int[3] { -1, -1, -1 };
        //if (hit != null)
        //{
        //    code = hit.gameObject.GetComponent<Tile>().code;
        //}

        float originAngle = (state ? Mathf.PI : 0) - Mathf.PI / 2;
        //move++;
        for (int i = 0; i < faces; i++)
        {
            float angle = originAngle + 2 * Mathf.PI * i / faces;
            Vector2 targetCoordinates = position + PolarToCartesian(scale / Mathf.Sqrt(3), angle);
            Collider2D hitCollider = Physics2D.OverlapPoint(targetCoordinates);

            if (hitCollider == null)
            {
                //Debug.Log("No object found at " + targetCoordinates);
                SpawnPlaceRpc(targetCoordinates, !state);

                //hitCollider = Physics2D.OverlapPoint(targetCoordinates);
                //hitCollider.gameObject.GetComponent<Tile>().code[i] = code[i];
            }
            else
            {
                //Debug.Log("Found object at " + targetCoordinates + ": " + hitCollider.gameObject.name);
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
                MultiplayerGameManager.Instance.UpdateScores(clientId, value);
            }
        }
    }

    //private void SubscribeTilesAround(Vector2 position, bool state, int[] code)
    //{
    //    float originAngle = (state ? Mathf.PI : 0) - Mathf.PI / 2;
    //    for (int i = faces - 1; i >= 0; i--)
    //    {
    //        float angle = originAngle + 2 * Mathf.PI * i / faces;
    //        Vector2 targetCoordinates = position + PolarToCartesian(scale / Mathf.Sqrt(3), angle);
    //        SetCodeRpc(targetCoordinates, i, code[i]);
    //    }
    //}

    [Rpc(SendTo.Server)]
    private void SetCodeRpc(Vector2 targetCoordinates, int index, int value)
    {
        //Debug.Log("Tiles " + GameObject.FindGameObjectWithTag("Board").transform.childCount);
        if (Physics2D.OverlapPoint(targetCoordinates))
        {
            Debug.DrawLine(Vector2.zero, targetCoordinates, Color.green, 20);
        }
        else
        {
            Debug.DrawLine(Vector2.zero, targetCoordinates, Color.red, 20);
        }

        Collider2D hit = Physics2D.OverlapPoint(targetCoordinates);
        //Debug.Log("SetCodeRpc " + targetCoordinates);

        if (hit != null)
        {
            if (hit.CompareTag("Place"))
            {
                //Debug.Log(targetCoordinates);
                //Debug.Log("SetCodeRpc " + index + ", " + value);
                //hit.gameObject.GetComponent<Tile>().code[index] = value;
                hit.gameObject.GetComponent<Tile>().SetCodeRpc(index, value);
            }
        }
    }

    private void MultiplayerGameManager_OnDoMove(object sender, MultiplayerGameManager.OnDoMoveArgs e)
    {
        //SpawnObjectRpc(e.position, e.state);
        //Vector2 position = e.position;
        //bool state = e.state;
        //StopAnimationChoose();

        ConstructTilesAround(e.position, e.state, e.code);
        //SubscribeTilesAround(e.position, e.state, e.code);

        //ChangeButtonColor(pressed, Color.white);
        MultiplayerGameManager.Instance.GetDiceRpc(NetworkManager.Singleton.LocalClientId);
        //CheckMovesRpc();
        UnenableButtons();
        //UpdateBagSizeRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateBagSizeRpc(int size)
    {
        bagSizeText.text = size.ToString();
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void UpdateStringBagSizeRpc(string bagSizeString)
    //{
    //    bagSizeText.text = bagSizeString;
    //}


    //private void EnableButtons()
    //{
    //    foreach (Button dice in panel)
    //    {
    //        dice.enabled = true;
    //    }
    //}

    private void UnenableButtons()
    {
        //Debug.Log("UnenableButtons");
        for (int i = 0; i < panel.Length; i++)
        {
            panel[i].enabled = false;
            ChangeButtonColor(i, Color.gray);
        }
    }

    public void ChangeColor(GameObject obj, Color color)
    {
        if (obj == null)
            return;
        obj.GetComponent<SpriteRenderer>().color = color;
        foreach (Transform child in obj.transform)
        {
            ChangeColor(child.gameObject, color);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DiceRpc(Vector2 position, int[] code)
    {
        ChangeColor(lastDice, Color.white);

        //Debug.Log("DiceRpc");
        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null)
        {
            //Debug.Log("Diced");
            hit.gameObject.tag = "Dice";
            // refactor
            Color color = new Color(0.5f, 1f, 1f);
            if (lastDice != null)
            {
                ChangeColor(hit.gameObject, new Color(0.5f, 1f, 1f));
            }
            else
            {
                ChangeColor(hit.gameObject, Color.white);
            }
            lastDice = hit.gameObject;
            //hit.gameObject.GetComponent<SpriteRenderer>().color = color;
            //foreach (Transform child in hit.transform)
            //{
            //    child.gameObject.GetComponent<SpriteRenderer>().color = color;
            //}
            SetLayer(hit.gameObject, LayerMask.NameToLayer("Default"));
            //hit.gameObject.layer = LayerMask.NameToLayer("Default");

            //hit.gameObject.GetComponent<Tile>().code = code;
            for (int i = 0; i < code.Length; i++)
            {
                //hit.gameObject.GetComponent<Tile>().code[i] = code[i];
                hit.gameObject.GetComponent<Tile>().SetCodeRpc(i, code[i]);
                hit.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = sprites[code[i]];
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlaceRpc(Vector2 position, bool state)
    {
        //Debug.Log("SpawnPlace");
        Transform tile = Instantiate(tilePrefab, position, Quaternion.Euler(0f, 0f, state ? 180 : 0));
        tile.GetComponent<NetworkObject>().Spawn(true);
        tile.GetComponent<Tile>().SetState(state);
        tile.SetParent(GameObject.FindGameObjectWithTag("Board").transform);
        //tile.gameObject.GetComponent<Tile>().code[index] = value;
    }

    private Vector2 PolarToCartesian(float radius, float angle)
    {
        return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
    }

    private void ChangeButtonColor(int index, Color color)
    {
        //if (!panel[index].enabled)
        //    return;
        panel[index].transform.GetComponent<Image>().color = color;
        //var colors = panel[index].colors;
        //colors.selectedColor = color;
        //colors.highlightedColor = color;
        //colors.normalColor = color;
        //panel[index].colors = colors;
        foreach (Transform child in panel[index].transform)
        {
            //Debug.Log("CHILD");
            child.GetComponent<Image>().color = color;
        }
    }

    private void ChangeColors(int index)
    {
        for (int i = 0; i < panel.Length; i++)
        {
            if (!panel[i].enabled)
                continue;
            ChangeButtonColor(i, Color.white);
        }
        ChangeButtonColor(index, Color.green);
    }

    //private string GetCurrentDice(int index)
    //{
    //    int digitsNum = panel[index].transform.childCount;
    //    string result = "";
    //    for (int i = 0; i < digitsNum; i++)
    //    {
    //        //string digitString = panel[index].transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().text;
    //        //string digit = digitString == "" ? "0" : digitString;
    //        //result += panel[index].GetComponent<Tile>().code[i].ToString();
    //        result += panelCodes[index][i].ToString();
    //    }
    //    return result;
    //}

    private void Try(GameObject place, int offset)
    {
        int[] code = new int[3] { place.GetComponent<Tile>().code[0], place.GetComponent<Tile>().code[1], place.GetComponent<Tile>().code[2] };
        int[] diceCode = MultiplayerGameManager.Instance.chosenCode;
        //int offset = 0;
        //for (int i = 0; i < place.transform.childCount; i++)
        //{
        //    //GameObject text = place.transform.GetChild(i).gameObject;

        //    //string digit = text.GetComponent<TextMeshProUGUI>().text == "" ? "0" : text.GetComponent<TextMeshProUGUI>().text;
        //    string digit = code[i] == -1 ? "?" : code[i].ToString();
        //    if (diceCode.Contains(digit))
        //    {
        //        offset = (diceCode.IndexOf(digit) - i + 3) % 3;
        //    }
        //}
        //Debug.Log(diceCode);
        //Debug.Log(offset);
        for (int i = 0; i < diceCode.Length; i++)
        {
            //Debug.Log("DiceCode = " + diceCode[0] + ", " + diceCode[1] + ", " + diceCode[2] + "; offset = " + offset);
            place.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = sprites[diceCode[(i + offset) % diceCode.Length]];
            place.GetComponent<Tile>().temporaryCode[i] = diceCode[(i + offset) % diceCode.Length];
        }
        //for (int i = 0; i < place.transform.childCount; i++)
        //{
        //    //place.GetComponent<Tile>().SetCode(i, diceCode[(i + offset) % 3] - '0');
        //    place.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = sprites[diceCode[(i + offset) % 3] - '0'];
        //    //GameObject text = canvas.transform.GetChild(i).gameObject;
        //    //text.SetActive(true);

        //    //text.GetComponent<TextMeshProUGUI>().text = diceCode[(i + offset) % 3].ToString() == "0" ? "" : diceCode[(i + offset) % 3].ToString();
        //}
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

    private bool Suits(GameObject place)
    {
        //int[] code = place.GetComponent<Tile>().code;
        int[] code = new int[3] { place.GetComponent<Tile>().code[0], place.GetComponent<Tile>().code[1], place.GetComponent<Tile>().code[2] };
        int[] diceCode = MultiplayerGameManager.Instance.chosenCode;

        //Debug.Log("DiceCode Length = " + diceCode.Length);
        //Debug.Log("Code Length = " + code.Length);

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
                Try(place, i);
                return true;
            }
        }
        return false;

        ////Func<int, string> toStrDigit = x => x == -1 ? "?" : x.ToString();
        //string stringCode = string.Join("", code.Select(x => x == -1 ? "?" : x.ToString()));
        //if (stringCode[1] == '?')
        //{
        //    stringCode = stringCode[2].ToString() + stringCode[1].ToString() + stringCode[0].ToString();
        //}
        //stringCode = stringCode.Replace("?", "");
        //return (diceCode + diceCode).Contains(stringCode);
    }

    private void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }

    //private void StopAnimationChoose()
    //{
    //    animationChoose = false;
    //    animationChooseTimer = 0f;
    //    for (int i = 0; i < panel.Length; i++)
    //    {
    //        panel[i].transform.position = startButtonPosition[i];
    //    }
    //}

    //private void StartAnimationChoose()
    //{
    //    animationChoose = true;
    //}

    public void Choose()
    {
        string pressedButtonName = EventSystem.current.currentSelectedGameObject.name;
        pressed = pressedButtonName[6] - '0';
        ChangeColors(pressed);
        //StopAnimationChoose();
        //StartAnimationChoose();
        //diceCode = GetCurrentDice(pressed);
        MultiplayerGameManager.Instance.chosenCode = panelCodes[pressed];
        //Debug.Log(diceCode);

        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            //place.SetActive(Suits(place.transform.position, place.GetComponent<Tile>().state));
            if (Suits(place))
            {
                SetLayer(place, LayerMask.NameToLayer("CurrentPlaces"));
                //Try(place);
            }
            else
            {
                SetLayer(place, LayerMask.NameToLayer("Places"));
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReturnHandledButtonRpc(ulong clientId, int index, bool playableDice)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (playableDice)
            {
                //availableDice++;
                panel[index].enabled = true;
                //Debug.Log("Return availableDice = " + availableDice);
                ChangeButtonColor(index, Color.white);
            }
            else
            {
                ChangeButtonColor(index, Color.gray);
                panel[index].enabled = false;
                DecreaseAvailableDice();
            }
        }
    }

    //[ServerRpc]
    //public int[] GetDiceRpc(ulong clientId, int index)
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
            if (Suits(new int[3] { place.GetComponent<Tile>().code[0], place.GetComponent<Tile>().code[1], place.GetComponent<Tile>().code[2] }, code))
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

    private void DecreaseAvailableDice()
    {
        availableDice--;
        if (availableDice == 0)
        {
            MultiplayerGameManager.Instance.SkipMoveRpc();
        }
    }

    //[Rpc(SendTo.ClientsAndHost)]
    private void CheckMoves()
    {
        //Debug.Log("CheckMoves()");
        availableDice = 4;
        for (int i = 0; i < panel.Length; i++)
        {
            //Debug.Log("Inside loop " + i);
            //Debug.Log("panelCodes " + panelCodes.Length);
            int[] code = panelCodes[i];
            if (code == null)
            {
                DecreaseAvailableDice();
                continue;
            }
            // на сервере вызываем метод, передаём clientId и code, он обратно вызывает у нас метод, который либо энейблит эту кнопку, либо нет
            HandleButtonRpc(NetworkManager.Singleton.LocalClientId, i, code);
            //Debug.Log("code length = " + (panelCodes[i] == null));
            //Transform board = GameObject.FindGameObjectWithTag("Board").transform;
            //bool playableDice = false;
            ////Debug.Log("playableDice");
            //for (int j = 0; j < board.childCount; j++)
            //{
            //    //Debug.Log("Inside loop 2 " + j);
            //    GameObject place = board.GetChild(j).gameObject;
            //    if (!place.CompareTag("Place"))
            //        continue;
            //    Debug.Log(string.Join("", code));
            //    for (int k = 0; k < place.GetComponent<Tile>().GetFacesNumber(); k++)
            //    {
            //        Debug.Log(place.GetComponent<Tile>().code[k]);
            //        //Debug.Log("Inside loop 3 " + k);
            //        //Debug.Log("code length = " + code.Length);
            //        if (string.Join("", code).Contains(place.GetComponent<Tile>().code[k].ToString()))
            //        {
            //            playableDice = true;
            //            break;
            //        }
            //    }
            //    if (playableDice) {
            //        break;
            //    }
            //}
            //if (playableDice)
            //{
            //    //panel[i].enabled = true;
            //    ChangeButtonColor(i, Color.white);
            //}
            //else
            //{
            //    //panel[i].enabled = false;
            //    ChangeButtonColor(i, Color.gray);
            //}
        }
        //Debug.Log("availableDice = " + availableDice);
        //if (availableDice == 0)
        //{
        //    MultiplayerGameManager.Instance.SkipMoveRpc();
        //}
    }
}
