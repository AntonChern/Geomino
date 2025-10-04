using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{
    public static RoomList Instance;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button updateButton;
    [SerializeField] private Transform container;
    [SerializeField] private Transform roomSample;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        createRoomButton.onClick.AddListener(() =>
        {
            Hide();
            CreateRoom.Instance.Show();
        });
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        });
        updateButton.onClick.AddListener(() =>
        {
            UpdateRoomList();
        });

        if (RoomManager.Instance.ActiveSession != null)
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private async void UpdateRoomList()
    {
        var roomList = await RoomManager.Instance.QuerySessions();

        foreach (Transform child in container)
        {
            if (child == roomSample) continue;

            Destroy(child.gameObject);
        }

        if (roomList == null) return;

        foreach (ISessionInfo session in roomList)
        {
            if (session.IsLocked) continue;

            Transform roomTransform = Instantiate(roomSample, container);
            roomTransform.gameObject.SetActive(true);
            Room room = roomTransform.GetComponent<Room>();
            room.UpdateRoom(session);
        }
    }
}
