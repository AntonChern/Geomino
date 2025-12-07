using UnityEngine;
using UnityEngine.UI;

public class HelpViewHandler : MonoBehaviour
{
    [SerializeField] private Image[] helpIcons;

    [SerializeField] private Sprite[] desktopSprites;
    [SerializeField] private Sprite[] mobileSprites;

    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            for (int i = 0; i < 3; i++)
            {
                helpIcons[i].sprite = mobileSprites[i];
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                helpIcons[i].sprite = desktopSprites[i];
            }
        }
    }
}
