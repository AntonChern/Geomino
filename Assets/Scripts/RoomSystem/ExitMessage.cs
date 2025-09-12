using UnityEngine;
using UnityEngine.UI;

public class ExitMessage : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button cancelButton;

    private void Start()
    {
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });
        Hide();
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
