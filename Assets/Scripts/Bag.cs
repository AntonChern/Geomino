using TMPro;
using UnityEngine;

public class Bag : MonoBehaviour
{
    public static Bag Instance;

    [SerializeField] private TextMeshProUGUI bagSizeText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateBag(int size)
    {
        bagSizeText.text = size.ToString();
    }
}
