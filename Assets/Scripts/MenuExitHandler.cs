using UnityEngine;
using UnityEngine.UI;

public class MenuExitHandler : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject exitMessage;

    private void Start()
    {
        exitButton.onClick.AddListener(() =>
        {
            MainMenu.Instance.Hide();
            exitMessage.SetActive(true);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SingleplayerMenu.Instance.gameObject.activeInHierarchy)
            {
                SingleplayerMenu.Instance.Hide();
                MainMenu.Instance.Show();
                return;
            }
            //MainMenu.Instance.gameObject.SetActive(exitMessage.activeInHierarchy);
            //exitMessage.SetActive(!exitMessage.activeInHierarchy);
        }
    }
}
