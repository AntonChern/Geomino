using DG.Tweening;
using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Instance;

    [SerializeField] private TextMeshProUGUI[] names;
    [SerializeField] private TextMeshProUGUI[] scores;
    [SerializeField] private GameObject frame;

    private void Awake()
    {
        Instance = this;
    }

    public string GetPlayerScore(int index)
    {
        return scores[index].text;
    }

    public string GetPlayer(int index)
    {
        return names[index].text;
    }

    public void SetPlayer(int index, string name)
    {
        names[index].text = name;
    }

    public void Null()
    {
        for (int i = 0; i < names.Length; i++)
        {
            names[i].text = string.Empty;
            scores[i].text = string.Empty;
        }
    }

    public void RemoveByIndex(int index)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (i >= index && i + 1 < names.Length)
            {
                names[i].text = names[i + 1].text;

                scores[i].text = scores[i + 1].text;
            }
            else if (i == names.Length - 1)
            {
                names[i].text = string.Empty;
                scores[i].text = string.Empty;
            }
        }
    }

    public void UpdateScores()
    {
        var scores = GameHandler.Instance.gameManager.GetScores();
        for (int i = 0; i < scores.Count; i++)
        {
            UpdateScore(i, scores[i]);
        }
    }

    public void UpdateScore(int index, int value)
    {
        scores[index].text = value.ToString();
    }

    public void UpdateTurn(int index)
    {
        frame.transform.DOLocalMoveY(names[index].transform.localPosition.y, 0.25f);
        //frame.transform.DOMove(new Vector3(frame.transform.position.x, names[index].transform.position.y, frame.transform.position.z), 0.25f);
        //frame.transform.position = new Vector3(frame.transform.position.x, names[index].transform.position.y, frame.transform.position.z);
    }

    public void HighlightPlayer(int index)
    {
        names[index].color = Color.yellow;
        scores[index].color = Color.yellow;
    }

    public void LowlightPlayer(int index)
    {
        names[index].color = Color.white;
        scores[index].color = Color.white;
    }
}
