//using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GraphDrawer : NetworkBehaviour
{
    public RectTransform graphContainer;
    [SerializeField]
    private GameObject[] pointPrefab; // A small UI Image prefab for data points
    [SerializeField]
    private GameObject[] lineSegmentPrefab; // A UI Image prefab for line segments

    //public List<float> dataPoints = new List<float>() { 5f, 10f, 7f, 12f, 8f };
    //public float yMax = 15f; // Maximum Y value for scaling

    void Start()
    {
        Debug.Log("Start GraphDrawer");
        MultiplayerGameManager.Instance.OnGameEnded += MultiplayerGameManager_OnGameEnded;
        gameObject.SetActive(false);
    }

    private void MultiplayerGameManager_OnGameEnded(object sender, System.EventArgs e)
    {
        Debug.Log("OnGameEnded GraphDrawer");
        
        GenerateGraphRpc();
    }

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
        Debug.Log("GenerateGraph");

        gameObject.SetActive(true);

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

                // Create and position data point
                GameObject point = Instantiate(pointPrefab[j], graphContainer);
                point.GetComponent<RectTransform>().anchoredPosition = currentPointPos;

                // Draw line segment to previous point (if not the first point)
                if (i > 0)
                {
                    DrawLineSegment(lastPointPos, currentPointPos, j);
                }
                lastPointPos = currentPointPos;
            }
        }
    }

    void DrawLineSegment(Vector2 startPos, Vector2 endPos, int index)
    {
        GameObject lineSegment = Instantiate(lineSegmentPrefab[index], graphContainer);
        RectTransform lineRect = lineSegment.GetComponent<RectTransform>();

        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);

        lineRect.anchoredPosition = startPos + direction * (distance / 2f);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y); // Adjust line thickness if needed
        lineRect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }
}
