using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SingleplayerMenu : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_Dropdown computers;

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

        //Debug.Log(computers.value);
        PlayerPrefs.SetInt("players", int.Parse(computers.options[computers.value].text) + 1);
        PlayerPrefs.Save();

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
