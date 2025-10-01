using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private Button kickButton;
    //[SerializeField] private TMP_InputField nameField;

    private IReadOnlyPlayer player;

    private void Awake()
    {
        if (kickButton == null) return;
        kickButton.onClick.AddListener(() =>
        {
            RoomManager.Instance.KickPlayer(player.Id);
        });
    }

    //public void SetNameFieldVisible(bool visible)
    //{
    //    nameField.gameObject.SetActive(visible);
    //}

    public void SetKickPlayerButtonVisible(bool visible)
    {
        if (kickButton == null) return;
        kickButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(IReadOnlyPlayer player)
    {
        this.player = player;

        playerName.text = $"{player.Properties["playerName"].Value}";
    }
}
