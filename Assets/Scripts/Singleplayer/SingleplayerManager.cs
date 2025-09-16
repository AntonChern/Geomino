using System.Collections.Generic;
using UnityEngine;

public class SingleplayerManager : MonoBehaviour, IGameManager
{
    public static SingleplayerManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one MultiplayerGameManager instance!");
        }
        Instance = this;
    }

    private void Start()
    {
        
    }

    public bool RefreshHand()
    {
        bool answer = false;
        foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
        {
            Transform board = GameObject.FindGameObjectWithTag("Board").transform;
            for (int i = 0; i < board.childCount; i++)
            {
                GameObject place = board.GetChild(i).gameObject;
                if (!place.CompareTag("Place"))
                    continue;

                if (VisualManager.Instance.Suits(place, diceUI.GetComponent<DiceUI>().Code))
                {
                    answer = true;
                    diceUI.GetComponent<DiceUI>().Enable();
                }
                else
                {
                    diceUI.GetComponent<DiceUI>().Disable();
                }
            }
        }
        return answer;
    }

    public void MakeMove(Vector2 position, bool state, int[] code)
    {
        throw new System.NotImplementedException();
    }

    public void HandleButton(int index, int[] code)
    {
        throw new System.NotImplementedException();
    }

    public List<int> GetScores()
    {
        throw new System.NotImplementedException();
    }

    public void EndGame()
    {
        throw new System.NotImplementedException();
    }

    public void SkipMove()
    {
        throw new System.NotImplementedException();
    }

    public List<int[]> GetHistory()
    {
        throw new System.NotImplementedException();
    }

    public List<int> GetWinners()
    {
        throw new System.NotImplementedException();
    }
}
