using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private Button kickButton;

    private IReadOnlyPlayer player;

    private void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            RoomManager.Instance.KickPlayer(player.Id);
        });
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        kickButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(IReadOnlyPlayer player)
    {
        this.player = player;

        playerName.text = $"{player.Properties["playerName"].Value}";
    }
}
