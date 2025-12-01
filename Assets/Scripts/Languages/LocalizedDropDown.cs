using TMPro;
using UnityEngine;
using YG;

public class LocalizedDropDown : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
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
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            dropdown.options[i].text = MapHandler.GetMapByIndex(i.ToString());
        }
        dropdown.RefreshShownValue();
    }
}
