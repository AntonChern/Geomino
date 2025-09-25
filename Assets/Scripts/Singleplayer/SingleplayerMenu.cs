using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SingleplayerMenu : MonoBehaviour
{
    public static SingleplayerMenu Instance { get; private set; }

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_Dropdown computers;
    [SerializeField] private TMP_Dropdown map;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        startGameButton.onClick.AddListener(() =>
        {
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
        computers.onValueChanged.AddListener((int value) =>
        {
            PlayerPrefs.SetInt("players", int.Parse(computers.options[value].text) + 1);
            PlayerPrefs.Save();
        });
        map.onValueChanged.AddListener((int value) =>
        {
            switch (map.options[value].text)
            {
                case "Бесконечная":
                    PlayerPrefs.SetString("map", "infinity");
                    break;
                case "Коридор":
                    PlayerPrefs.SetString("map", "hallway");
                    break;
                case "Гексагон":
                    PlayerPrefs.SetString("map", "hexagon");
                    break;
                case "Колонны":
                    PlayerPrefs.SetString("map", "columns");
                    break;
            }
            PlayerPrefs.Save();
        });

        //Debug.Log(computers.value);
        PlayerPrefs.SetString("map", "infinity");
        PlayerPrefs.SetInt("players", int.Parse(computers.options[computers.value].text) + 1);
        PlayerPrefs.Save();
        //Debug.Log(PlayerPrefs.GetString("map"));

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        playerName.text = PlayerPrefs.GetString("playerName");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
