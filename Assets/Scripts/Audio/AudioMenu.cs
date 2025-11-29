using UnityEngine;
using UnityEngine.UI;

public class AudioMenu : MonoBehaviour
{
    public static AudioMenu Instance;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    private float musicMultiplier = 1f;
    private float soundMultiplier = 1f;

    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
