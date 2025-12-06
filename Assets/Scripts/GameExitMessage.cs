using UnityEngine;
using UnityEngine.UI;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class GameExitMessage : MonoBehaviour
{
    [SerializeField] private Button showButton;

    [SerializeField] private Button exitButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    private float musicMultiplier = 1f;
    private float soundMultiplier = 1f;

    private void Start()
    {
        showButton.onClick.AddListener(() =>
        {
            Show();
        });
        exitButton.onClick.AddListener(() =>
        {
            GameHandler.Instance.Exit();
        });
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });

        musicMultiplier = 1f - PlayerPrefs.GetFloat("musicSilencer");
        soundMultiplier = 1f - PlayerPrefs.GetFloat("soundSilencer");

        musicSlider.value = musicMultiplier;
        soundSlider.value = soundMultiplier;

        AudioManager.Instance.UpdateVolumes(Audio.Music, musicMultiplier);
        AudioManager.Instance.UpdateVolumes(Audio.Sound, soundMultiplier);

        musicSlider.onValueChanged.AddListener((float value) =>
        {
            musicMultiplier = value;
            PlayerPrefs.SetFloat("musicSilencer", 1f - value);
            AudioManager.Instance.UpdateVolumes(Audio.Music, musicMultiplier);
        });
        soundSlider.onValueChanged.AddListener((float value) =>
        {
            soundMultiplier = value;
            PlayerPrefs.SetFloat("soundSilencer", 1f - value);
            AudioManager.Instance.UpdateVolumes(Audio.Sound, soundMultiplier);
        });
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
