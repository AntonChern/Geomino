using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SingleplayerMenu : MonoBehaviour
{
    public static SingleplayerMenu Instance { get; private set; }

    [SerializeField] private GameObject table;

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_Dropdown map;

    [SerializeField] private GameObject[] players;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        startGameButton.onClick.AddListener(() =>
        {
            int playersCount = 0;
            int botsCount = 0;
            foreach (GameObject player in players)
            {
                if (player.activeInHierarchy)
                {
                    if (player.GetComponent<BotTile>() != null)
                    {
                        PlayerPrefs.SetInt($"difficulty{botsCount++}", (int)player.GetComponent<BotTile>().GetDifficulty());
                    }
                    playersCount++;
                }
            }
            PlayerPrefs.SetInt("players", playersCount);
            PlayerPrefs.SetString("playerName", playerName.text);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MultiplayerGame", LoadSceneMode.Single);
        });
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
            MainMenu.Instance.Show();
        });
        playerName.onValueChanged.AddListener((string value) =>
        {
            PlayerPrefs.SetString("playerName", value);
            PlayerPrefs.Save();
        });
        map.onValueChanged.AddListener((int value) =>
        {
            PlayerPrefs.SetString("map", MapHandler.TranslateToEnglish(map.options[value].text));
            PlayerPrefs.Save();
        });

        PlayerPrefs.SetString("map", "infinity");
        PlayerPrefs.SetInt("players", 2);
        PlayerPrefs.Save();

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        table.SetActive(true);
        if (PlayerPrefs.GetString("playerName") != string.Empty) 
            playerName.text = PlayerPrefs.GetString("playerName");
    }

    public void Hide()
    {
        table.SetActive(false);
        gameObject.SetActive(false);
    }
}
