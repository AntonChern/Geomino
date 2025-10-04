using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitHandler : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (RoomList.Instance.gameObject.activeInHierarchy)
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        if (CreateRoom.Instance.gameObject.activeInHierarchy)
        {
            CreateRoom.Instance.Cancel();
        }

        if (RoomSpace.Instance.gameObject.activeInHierarchy && RoomManager.Instance.ActiveSession != null)
        {
            RoomSpace.Instance.Leave();
        }
    }
}
