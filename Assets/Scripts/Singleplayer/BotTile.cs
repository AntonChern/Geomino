using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BotTile : MonoBehaviour
{
    [SerializeField] private Button removeButton;
    [SerializeField] private Button difficultyButton;

    private Difficulty difficulty;

    public Difficulty GetDifficulty()
    {
        return difficulty;
    }

    private void Start()
    {
        difficulty = Difficulty.Medium;
        difficultyButton.onClick.AddListener(() =>
        {
            difficulty = difficulty.Next();
            difficultyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = DifficultyHandler.Translate(difficulty);
            difficultyButton.GetComponent<Image>().color = GetColor(difficulty);
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
                return Color.blue;
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
    public static string Translate(Difficulty difficulty)
    {
        string result = string.Empty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                result = "Лёгкий";
                break;
            case Difficulty.Medium:
                result = "Средний";
                break;
            case Difficulty.Hard:
                result = "Трудный";
                break;
            case Difficulty.Impossible:
                result = "Невозможный";
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