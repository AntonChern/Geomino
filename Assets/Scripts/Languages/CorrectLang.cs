using System.Collections.Generic;
using UnityEngine;
using YG;

public static class CorrectLang
{
    public static Dictionary<string, int> langIndices = new Dictionary<string, int>()
    {
        { "ru", 0 },
        { "en", 1 }
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        YG2.onCorrectLang += OnСhangeLang;
    }

    public static void OnСhangeLang(string lang)
    {
        if (lang == "ru")
        {
            YG2.lang = "ru";
        }
        else
        {
            YG2.lang = "en";
        }
    }
}
