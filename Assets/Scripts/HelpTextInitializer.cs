using TMPro;
using UnityEngine;
using YG;

public class HelpTextInitializer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI placeTextUI, dragTextUI, zoomTextUI;

    private string[] placeText = new string[]
    {
        "Разместить",
        "Place"
    };

    private string[] dragText = new string[]
    {
        "Двигать",
        "Drag"
    };

    private string[] zoomText = new string[]
    {
        "Зумить",
        "Zoom"
    };

    private void Start()
    {
        placeTextUI.text = placeText[CorrectLang.langIndices[YG2.lang]];
        dragTextUI.text = dragText[CorrectLang.langIndices[YG2.lang]];
        zoomTextUI.text = zoomText[CorrectLang.langIndices[YG2.lang]];
    }
}
