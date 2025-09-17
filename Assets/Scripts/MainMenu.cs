using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [SerializeField] private Button singleplayerButton;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject exitMessage;
    [SerializeField] private SingleplayerMenu singleplayerMenu;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        singleplayerButton.onClick.AddListener(() =>
        {
            //SceneManager.LoadScene("MultiplayerGame", LoadSceneMode.Single);
            singleplayerMenu.Show();
            Hide();
        });
        multiplayerButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("RoomSystem", LoadSceneMode.Single);
        });
        exitButton.onClick.AddListener(() =>
        {
            exitMessage.SetActive(true);
        });

        if (RoomManager.Instance != null)
        {
            Destroy(RoomManager.Instance.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitMessage.SetActive(!exitMessage.activeInHierarchy);
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
