using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionName;
    [SerializeField] private TextMeshProUGUI map;
    [SerializeField] private TextMeshProUGUI players;

    private ISessionInfo session;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(async () =>
        {
            //if (session..AvailableSlots == session.MaxPlayers)
            //{
            //    return;
            //}
            //if (session == null) return;
            var existingSessions = await RoomManager.Instance.QuerySessions();
            //var sessions = await MultiplayerService.Instance.QuerySessionsAsync();
            foreach (var existingSession in existingSessions)
            {
                if (existingSession.Id == session.Id)
                {
                    Debug.Log($"Session {session.Id} exists");
                    RoomList.Instance.Hide();
                    RoomSpace.Instance.Show();
                    RoomManager.Instance.JoinSessionById(session.Id);
                    return;
                }
            }
            Debug.Log($"Session {session.Id} doesn't exist");
            //if (!sessions.Contains(session.Id))
            //{
            //    return;
            //}
        });
    }

    public void UpdateRoom(ISessionInfo session)
    {
        this.session = session;

        sessionName.text = session.Name;
        Debug.Log(session.Properties[RoomManager.mapProperty].Value);
        map.text = MapHandler.TranslateToRussian(session.Properties[RoomManager.mapProperty].Value);
        players.text = $"{session.MaxPlayers - session.AvailableSlots}/{session.MaxPlayers}";
        //info.text = $"{session.Name}\t{session.MaxPlayers - session.AvailableSlots}/{session.MaxPlayers}";
    }
}
