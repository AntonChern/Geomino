using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    //public List<int> GetPlayers();

    //public void MakeMove();

    public bool RefreshHand();
    public void MakeMove(Vector2 position, bool state, int[] code);
    public void HandleButton(int index, int[] code);

    public List<int> GetScores();

    public void EndGame();

    public void SkipMove();

    public List<int[]> GetHistory();

    public List<int> GetWinners();

}
