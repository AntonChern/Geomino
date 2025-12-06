using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI starCounter;
    [SerializeField] private Button kickButton;
    [SerializeField] private GameObject readyAura;

    private IReadOnlyPlayer player;

    private void Awake()
    {
        if (kickButton == null) return;
        kickButton.onClick.AddListener(() =>
        {
            RoomManager.Instance.KickPlayer(player.Id);
        });
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        if (kickButton == null) return;
        kickButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(IReadOnlyPlayer player)
    {
        this.player = player;

        playerName.text = $"{player.Properties[RoomManager.playerNamePropertyKey].Value}";
        starCounter.text = $"{player.Properties[RoomManager.starCounterPropertyKey].Value}";
        if (readyAura != null)
        {
            readyAura.SetActive(bool.Parse(player.Properties[RoomManager.readinessPropertyKey].Value));
        }
    }
}
