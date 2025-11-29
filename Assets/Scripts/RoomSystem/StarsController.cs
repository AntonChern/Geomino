using TMPro;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

public class StarsController : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("stars").ToString();
    }
}
