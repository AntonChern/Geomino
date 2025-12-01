using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class LanguageSwitchButton : MonoBehaviour
{
    [SerializeField] private Sprite[] flags;
    [SerializeField] private TextMeshProUGUI text;
    private string[] texts;

    private int index = 0;
    private Image image;

    private void Awake()
    {
        texts = new string[]
        {
            "рус",
            "eng"
        };
        image = GetComponent<Image>();
    }

    private void SetUIElements(string lang)
    {
        index = CorrectLang.langIndices[lang];
        image.sprite = flags[index];
        text.text = texts[index];
    }

    private void OnEnable()
    {
        SetUIElements(YG2.lang);
        YG2.onSwitchLang += SetUIElements;
    }

    private void OnDisable()
    {
        YG2.onSwitchLang -= SetUIElements;
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            index ^= 1;
            YG2.SwitchLanguage(CorrectLang.langIndices.Keys.ToArray()[index]);
        });
    }
}
