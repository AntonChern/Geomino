using TMPro;
using UnityEngine;
using YG;

public class TESTTEXT : MonoBehaviour
{
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
        GetComponent<TextMeshProUGUI>().text = lang;
    }
}
