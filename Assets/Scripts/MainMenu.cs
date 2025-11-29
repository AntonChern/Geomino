using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [SerializeField] private Button singleplayerButton;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button audioButton;

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

        starsCounter.text = PlayerPrefs.GetInt("stars").ToString();

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
