using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    public static LeaderboardController Instance;

    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Hide();
            MainMenu.Instance.Show();
        });

        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
