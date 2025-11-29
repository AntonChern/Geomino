using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerGraph : MonoBehaviour
{
    public static WinnerGraph Instance;

    [SerializeField] private TextMeshProUGUI[] names;
    [SerializeField] private TextMeshProUGUI[] scores;
    [SerializeField] private Image[] crowns;
    [SerializeField] private Image[] starIcons;
    [SerializeField] private Image paperFront;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button slideButton;

    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private GameObject[] pointPrefab;
    [SerializeField] private GameObject[] lineSegmentPrefab;

    private bool playDrawAnimation = false;
    private float drawTimer;
    private float drawTime = 2f;

    private bool hidden = false;
    private float slideTime = 0.25f;

    private RectTransform rectTransform;

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
        exitButton.gameObject.SetActive(false);
        slideButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.Play("Sliding");
            SlideGraph();
        });

        rectTransform = GetComponent<RectTransform>();

        drawTimer = drawTime;
        gameObject.SetActive(false);
    }

    public void DisableExitButton()
    {
        exitButton.enabled = false;
        exitButton.GetComponent<Image>().color = Color.gray;
    }

    public void SetExitButtonText(string value)
    {
        foreach (Transform child in exitButton.transform)
        {
            var textMeshPro = child.GetComponent<TextMeshProUGUI>();
            if (textMeshPro == null) continue;
            textMeshPro.text = value;
        }
    }

    private void SlideGraph()
    {
        hidden = !hidden;
        if (hidden)
        {
            Vector3 position = rectTransform.position;
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.position = position;
            rectTransform.DOAnchorPosY(260f, slideTime);
            slideButton.transform.DORotate(new Vector3(0f, 0f, 180f), slideTime);
        }
        else
        {
            Vector3 position = rectTransform.position;
            rectTransform.anchorMin = Vector2.one / 2f;
            rectTransform.anchorMax = Vector2.one / 2f;
            rectTransform.position = position;
            rectTransform.DOAnchorPosY(0f, slideTime);
            slideButton.transform.DORotate(new Vector3(0f, 0f, 0f), slideTime);
        }
    }

    private void Update()
    {
        if (!playDrawAnimation) return;

        drawTimer -= Time.deltaTime;
        paperFront.GetComponent<Image>().fillAmount = Mathf.Clamp(drawTimer / drawTime, 0f, 1f);
        if (drawTimer <= 0f)
        {
            playDrawAnimation = false;
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
        AudioManager.Instance.GraduallyStop("GameBackground");
        AudioManager.Instance.Play("Drawing");

        gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);

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

                if (i > 0)
                {
                    DrawLineSegment(lastPointPos, currentPointPos, j);
                }
                lastPointPos = currentPointPos;

            }

            for (int i = 0; i < history.Count; i++)
            {
                float xPos = i * xSpacing;
                float yPos = (history[i][j] / yMax) * graphHeight;
                Vector2 currentPointPos = new Vector2(xPos - graphWidth / 2, yPos - graphHeight / 2);
                GameObject point = Instantiate(pointPrefab[j], graphContainer);
                point.GetComponent<RectTransform>().anchoredPosition = currentPointPos;

            }
        }

        playDrawAnimation = true;
    }

    void DrawLineSegment(Vector2 startPos, Vector2 endPos, int index)
    {
        GameObject lineSegment = Instantiate(lineSegmentPrefab[index], graphContainer);
        RectTransform lineRect = lineSegment.GetComponent<RectTransform>();

        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);

        lineRect.anchoredPosition = startPos + direction * (distance / 2f);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y * 0.8f);
        lineRect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void ShowScoreboard()
    {
        List<int> stars = GameHandler.Instance.gameManager.GetStars();
        for (int i = 0; i < GameHandler.Instance.gameManager.GetScores().Count; i++)
        {
            names[i].gameObject.SetActive(true);
            scores[i].gameObject.SetActive(true);
            if (GameHandler.Instance.gameManager.GetWinners().Contains(i))
            {
                crowns[i].gameObject.SetActive(true);
            }
            ShowStars(i, stars[i]);
            names[i].text = Scoreboard.Instance.GetPlayer(i);
            scores[i].text = Scoreboard.Instance.GetPlayerScore(i);
        }
    }

    private void ShowStars(int index, int stars)
    {
        for (int i = 0; i < stars; i++)
        {
            starIcons[index * 3 + i].gameObject.SetActive(true);
        }
    }
}
