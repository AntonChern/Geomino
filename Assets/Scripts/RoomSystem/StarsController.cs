using TMPro;
using UnityEngine;

public class StarsController : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("stars").ToString();
    }
}
