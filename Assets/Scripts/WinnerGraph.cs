using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WinnerGraph : MonoBehaviour
{
    public static WinnerGraph Instance;

    [SerializeField] private TextMeshProUGUI[] names;
    [SerializeField] private TextMeshProUGUI[] scores;
    [SerializeField] private Image[] crowns;
    [SerializeField] private Image paperFront;
    [SerializeField] private Button exitButton;

    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private GameObject[] pointPrefab;
    [SerializeField] private GameObject[] lineSegmentPrefab;

    private bool playAnimation = false;
    private float animationTimer = 2f;
    private float animationTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        exitButton.onClick.AddListener(() =>
        {
            GameHandler.Instance.EndGame();
        });

        animationTime = animationTimer;
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!playAnimation) return;

        animationTimer -= Time.deltaTime;
        paperFront.GetComponent<Image>().fillAmount = Mathf.Clamp(animationTimer / animationTime, 0f, 1f);
        if (animationTimer <= 0f)
        {
            playAnimation = false;
        }
    }

    private int GetYMax()
    {
        int maxValue = 0;
        for (int i = 0; i < GameHandler.Instance.gameManager.GetScores().Count; i++)
        {
            if (GameHandler.Instance.gameManager.GetScores()[i] > maxValue)
            {
                maxValue = GameHandler.Instance.gameManager.GetScores()[i];
            }
        }
        return maxValue;
    }

    public void Draw()
    {
        //Debug.Log("GenerateGraph");
        gameObject.SetActive(true);
        //winnerGraph.SetActive(true);

        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;
        var history = GameHandler.Instance.gameManager.GetHistory();
        float xSpacing = graphWidth / (history.Count - 1);

        float yMax = GetYMax();

        Vector2 lastPointPos = Vector2.zero;

        for (int j = 0; j < GameHandler.Instance.gameManager.GetScores().Count; j++)
        {
            for (int i = 0; i < history.Count; i++)
            {
                float xPos = i * xSpacing;
                float yPos = (history[i][j] / yMax) * graphHeight;
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

            for (int i = 0; i < history.Count; i++)
            {
                float xPos = i * xSpacing;
                float yPos = (history[i][j] / yMax) * graphHeight;
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

        playAnimation = true;
    }

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

    public void ShowScoreboard()
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            names[i].gameObject.SetActive(true);
            scores[i].gameObject.SetActive(true);
            if (GameHandler.Instance.gameManager.GetWinners().Contains(i))
            {
                crowns[i].gameObject.SetActive(true);
            }
            names[i].text = Scoreboard.Instance.GetPlayer(i);
            scores[i].text = Scoreboard.Instance.GetPlayerScore(i);
        }
    }
}
