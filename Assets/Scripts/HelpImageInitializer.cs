using UnityEngine;
using UnityEngine.UI;

public class HelpImageInitializer : MonoBehaviour
{
    [SerializeField] private Image placeIcon;
    [SerializeField] private Image dragIcon;
    [SerializeField] private Image zoomIcon;

    [SerializeField] private Sprite placeSpriteMobile, dragSpriteMobile, zoomSpriteMobile;
    [SerializeField] private Sprite placeSpriteDesktop, dragSpriteDesktop, zoomSpriteDesktop;

    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            placeIcon.sprite = placeSpriteMobile;
            dragIcon.sprite = dragSpriteMobile;
            zoomIcon.sprite = zoomSpriteMobile;
        }
        else
        {
            placeIcon.sprite = placeSpriteDesktop;
            dragIcon.sprite = dragSpriteDesktop;
            zoomIcon.sprite = zoomSpriteDesktop;
        }
    }
}
