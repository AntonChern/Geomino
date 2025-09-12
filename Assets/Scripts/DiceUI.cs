using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour
{
    private void Awake()
    {
        gameObject.GetComponent<Button>().enabled = false;
    }
}