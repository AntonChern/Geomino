using UnityEngine;
using UnityEngine.UI;

public class GameExitMessage : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    private float musicMultiplier = 1f;
    private float soundMultiplier = 1f;

    private void Start()
    {
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

        musicSlider.onValueChanged.AddListener((float value) =>
        {
            musicMultiplier = value;
            PlayerPrefs.SetFloat("musicSilencer", 1f - value);
            PlayerPrefs.Save();
            AudioManager.Instance.UpdateVolumes(Audio.Music, musicMultiplier);
        });
        soundSlider.onValueChanged.AddListener((float value) =>
        {
            soundMultiplier = value;
            PlayerPrefs.SetFloat("soundSilencer", 1f - value);
            PlayerPrefs.Save();
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
        gameObject.SetActive(false);
    }
}
