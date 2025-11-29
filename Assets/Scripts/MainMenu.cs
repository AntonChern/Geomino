using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [SerializeField] private Button singleplayerButton;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button leaderboardButton;

    [SerializeField] private TextMeshProUGUI starsCounter;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        singleplayerButton.onClick.AddListener(() =>
        {
            SingleplayerMenu.Instance.Show();
            Hide();
        });
        multiplayerButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("RoomSystem", LoadSceneMode.Single);
        });
        audioButton.onClick.AddListener(() =>
        {
            AudioMenu.Instance.Show();
            Hide();
        });
        leaderboardButton.onClick.AddListener(() =>
        {
            LeaderboardController.Instance.Show();
            Hide();
        });

        int stars = PlayerPrefs.GetInt("stars");
        starsCounter.text = stars.ToString();
        YG2.SetLeaderboard("starsLeaderboard", stars);

        if (RoomManager.Instance != null)
        {
            Destroy(RoomManager.Instance.gameObject);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
