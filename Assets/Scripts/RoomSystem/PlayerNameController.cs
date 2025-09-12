using TMPro;
using UnityEngine;

public class PlayerNameController : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TMP_InputField>().text = RoomManager.Instance.PlayerName;
        GetComponent<TMP_InputField>().onValueChanged.AddListener((string value) =>
        {
            RoomManager.Instance.UpdatePlayerName(value);
        });
    }
}
