using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;


public class BotTile : MonoBehaviour
{
    [SerializeField] private Button removeButton;
    [SerializeField] private Button difficultyButton;

    private Difficulty difficulty = Difficulty.Medium;

    private TextMeshProUGUI difficultyText;
    private Image difficultyColor;

    public Difficulty GetDifficulty()
    {
        return difficulty;
    }

    private void OnEnable()
    {
        OnSwitch(YG2.lang);
        YG2.onSwitchLang += OnSwitch;
    }

    private void OnDisable()
    {
        YG2.onSwitchLang -= OnSwitch;
    }

    private void OnSwitch(string lang)
    {
        difficultyText.text = DifficultyHandler.Translate(difficulty);
    }

    private void Awake()
    {
        difficultyText = difficultyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        difficultyColor = difficultyButton.GetComponent<Image>();
    }

    private void Start()
    {
        difficultyButton.onClick.AddListener(() =>
        {
            difficulty = difficulty.Next();
            difficultyText.text = DifficultyHandler.Translate(difficulty);
            difficultyColor.color = GetColor(difficulty);
        });
        if (removeButton == null) return;
        removeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private Color GetColor(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return new Color(0f, 0.75f, 0f, 1f);
            case Difficulty.Medium:
                return new Color(0.5f, 0.5f, 1f, 1f);
            case Difficulty.Hard:
                return new Color(0.75f, 0f, 0.75f, 1f);
            case Difficulty.Impossible:
                return new Color(1f, 0.5f, 0f, 1f);
            default:
                return Color.white;
        }
    }
}

public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2,
    Impossible = 3
}

public static class DifficultyHandler
{
    private static Dictionary<string, string> easyText = new Dictionary<string, string>()
        {
            { "ru", "Лёгкий" },
            { "en", "Easy" }
        };
    private static Dictionary<string, string> mediumText = new Dictionary<string, string>()
        {
            { "ru", "Средний" },
            { "en", "Medium" }
        };
    private static Dictionary<string, string> hardText = new Dictionary<string, string>()
        {
            { "ru", "Трудный" },
            { "en", "Hard" }
        };
    private static Dictionary<string, string> impossibleText = new Dictionary<string, string>()
        {
            { "ru", "Невозможный" },
            { "en", "Impossible" }
        };

    public static string Translate(Difficulty difficulty)
    {
        string result = string.Empty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                result = easyText[YG2.lang];
                break;
            case Difficulty.Medium:
                result = mediumText[YG2.lang];
                break;
            case Difficulty.Hard:
                result = hardText[YG2.lang];
                break;
            case Difficulty.Impossible:
                result = impossibleText[YG2.lang];
                break;
        }
        return result;
    }
}

public static class Extensions
{
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }
}