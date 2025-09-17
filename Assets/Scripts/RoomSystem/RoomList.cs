using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{
    public static RoomList Instance;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button backButton;
    //[SerializeField] private Button joinRoomButton;
    [SerializeField] private Button updateButton;
    //[SerializeField] private TMP_InputField joinCode;
    //[SerializeField] private TMP_InputField roomName;
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
            //RoomManager.Instance.CreateSession();
            Hide();
            CreateRoom.Instance.Show();
        });
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        });
        //joinRoomButton.onClick.AddListener(() =>
        //{
        //    //RoomManager.Instance.JoinSessionByCode(joinCode.text);
        //});
        updateButton.onClick.AddListener(() =>
        {
            //Debug.Log($"NetworkManager.Singleton.LocalClientId {NetworkManager.Singleton.LocalClientId}");
            UpdateRoomList();
        });

        if (RoomManager.Instance.ActiveSession != null)
        {
            Hide();
        }

        //try
        //{
        //    await UnityServices.InitializeAsync();
        //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //    Debug.Log($"Signed in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
        //}
        //catch (Exception e)
        //{
        //    Debug.LogException(e);
        //}
        //UpdateRoomList();
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
    //private async void UpdateRoomList(IList<ISessionInfo> roomList)
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
            //ActiveSession.AsHost().SetProperties(???);
            //if (session.HasPassword) continue;

            Transform roomTransform = Instantiate(roomSample, container);
            roomTransform.gameObject.SetActive(true);
            Room room = roomTransform.GetComponent<Room>();
            room.UpdateRoom(session);
        }
    }

    //private async void UpdateSessions()
    //{
    //    foreach (ISessionInfo session in await RoomManager.Instance.QuerySessions())
    //    {
    //        Debug.Log($"Session {session.Name} with ID = {session.Id}");
    //        Instantiate(roomSample);
    //        //RoomManager.Instance.JoinSessionById(session.Id);
    //    }


    //}

    //private async void CreateRoom()
    //{

    //    var session = await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions()
    //    {
    //        //Type = k_SessionType,
    //        MaxPlayers = 2,
    //        IsPrivate = false,
    //        //Password = string.IsNullOrEmpty(Password) ? null : Password,
    //        //IsLocked = IsLocked,
    //        //SessionProperties = LocalSessionProperties,
    //        //PlayerProperties = LocalPlayerProperties
    //    }.WithRelayNetwork());
    //    Debug.Log($"Session {session.Id} created! Join code: {session.Code}");
    //}

    //private async void JoinRoom(string code)
    //{
    //    var session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
    //    Debug.Log($"Joined to session {session.Id}! Join code: {session.Code}");
    //}

    //public void Fuf()
    //{
    //    Transform roomTransform = Instantiate(roomSample, container);
    //    roomTransform.gameObject.SetActive(true);
    //    //LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
    //}
}
