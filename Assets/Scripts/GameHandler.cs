using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;

    [SerializeField] private GameObject multiplayerPrefab;
    [SerializeField] private GameObject singleplayerPrefab;

    [SerializeField] private GameObject exitMessage;

    public IGameManager gameManager;

    //private int players;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            gameManager = Instantiate(singleplayerPrefab).GetComponent<SingleplayerManager>();
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                var multiplayerManager = Instantiate(multiplayerPrefab);
                multiplayerManager.GetComponent<NetworkObject>().Spawn(true);
                gameManager = multiplayerManager.GetComponent<MultiplayerManager>();
            }
        }

        //AudioManager.Instance.Play("GameBackground");
    }

    public void SkipMove()
    {
        gameManager.SkipMove();
    }

    public void EndGame()
    {
        gameManager.EndGame();
    }

    public void MakeMove(Vector2 position, bool state, int[] code)
    {
        //AudioManager.Instance.Play("Dice");
        VisualManager.Instance.MakeMove();
        gameManager.MakeMove(position, state, code);
    }

    public void HandleButton(int index, int[] code)
    {
        gameManager.HandleButton(index, code);
    }

    public void MakeComputerMove()
    {
        AudioManager.Instance.Play("Dice");
        gameManager.MakeComputerMove();
    }

    public void Exit()
    {
        gameManager.Exit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitMessage.SetActive(!exitMessage.activeInHierarchy);
        }
    }
}
