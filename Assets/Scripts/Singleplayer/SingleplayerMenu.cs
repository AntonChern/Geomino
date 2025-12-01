using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class SingleplayerMenu : MonoBehaviour
{
    public static SingleplayerMenu Instance { get; private set; }

    [SerializeField] private GameObject table;

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_Dropdown map;

    [SerializeField] private TextMeshProUGUI starsCounter;

    [SerializeField] private GameObject[] players;
    private string[] names = new string[]
    {
        "Èìÿ",
        "Name"
    };

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
            //PlayerPrefs.SetString("map", MapHandler.TranslateToEnglish(map.options[value].text));
            PlayerPrefs.SetInt("map", value);
            PlayerPrefs.Save();
        });

        starsCounter.text = PlayerPrefs.GetInt("stars").ToString();
        map.value = PlayerPrefs.GetInt("map");

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
        else
            playerName.text = names[CorrectLang.langIndices[YG2.lang]];
    }

    public void Hide()
    {
        table.SetActive(false);
        gameObject.SetActive(false);
    }
}
