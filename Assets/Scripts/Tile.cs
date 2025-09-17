using UnityEngine;
using UnityEngine.EventSystems;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class Tile : MonoBehaviour//, ITile
{
    //private NetworkVariable<bool> state = new NetworkVariable<bool>(false);
    ////public int[] code;
    //public NetworkList<int> code = new NetworkList<int>(new int[3]{ -1,-1,-1 });
    //public int[] temporaryCode;

    //public void SetState(bool state)
    //{
    //    this.state.Value = state;
    //}

    //public int[] GetCode()
    //{
    //    return new int[] { code[0], code[1], code[2] };
    //}

    //public void SetTemporaryCode(int index, int value)
    //{
    //    temporaryCode[index] = value;
    //}
    //public void ChangeColor(Color color)
    //{
    //    ChangeColor(gameObject, color);
    //}

    //public int GetFacesNumber()
    //{
    //    //return code.Length;
    //    return code.Count;
    //}

    //[Rpc(SendTo.Server)]
    //public void SetCodeRpc(int index, int value)
    //{
    //    code[index] = value;
    //}

    //[Rpc(SendTo.Server)]
    //private void RewriteCodeRpc()
    //{
    //    for (int i = 0; i < code.Count; i++)
    //    {
    //        code[i] = temporaryCode[i];
    //    }
    //}

    //public void Dice()
    //{
    //    //gameObject.tag = "Dice";
    //    //gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    //    //for (int i = 0; i < code.Count; i++) {
    //    //    code[i] = temporaryCode[i];
    //    //}
    //    //RewriteCodeRpc();
    //    for (int i = 0; i < code.Count; i++)
    //    {
    //        SetCodeRpc(i, temporaryCode[i]);
    //    }
    //    //Debug.Log("CODE = " + code[0] + ", " + code[1] + ", " + code[2]);
    //    //Debug.Log("TEMP CODE = " + temporaryCode[0] + ", " + temporaryCode[1] + ", " + temporaryCode[2]);
    //    //MultiplayerGameManager.Instance.DoMove(transform.position, state.Value, new int[3] { temporaryCode[0], temporaryCode[1], temporaryCode[2] });
    //    GameHandler.Instance.MakeMove(transform.position, state.Value, temporaryCode);
    //    //code = temporaryCode;
    //    //MultiplayerGameManager.instance.DoMove(transform.position, state.Value, code);
    //}

    //private void Awake()
    //{
    //    //code = new int[3] { -1, -1, -1 };
    //    temporaryCode = new int[3] { -1, -1, -1 };
    //    //state.Value = false;
    //}

    private void OnMouseDown()
    {
        // Check if the pointer is currently over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // If over UI, do nothing or return early
            return;
        }

        if (gameObject.CompareTag("Dice"))
            return;
        //Debug.Log("Clicked! " + transform.position);
        gameObject.GetComponent<ITile>().Dice();
        //MultiplayerGameManager.instance.Boob();
    }

    public void ChangeColor(GameObject obj, Color color)
    {
        obj.GetComponent<SpriteRenderer>().color = color;
        foreach (Transform child in obj.transform)
        {
            ChangeColor(child.gameObject, color);
        }
    }

    private void OnMouseEnter()
    {
        // Check if the pointer is currently over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // If over UI, do nothing or return early
            return;
        }

        if (gameObject.CompareTag("Place"))
            //ChangeColor(gameObject, new Color(1f, 1f, 0f, 0.5f));
            ChangeColor(gameObject, Color.yellow);
    }

    private void OnMouseExit()
    {
        // Check if the pointer is currently over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // If over UI, do nothing or return early
            return;
        }

        if (gameObject.CompareTag("Place"))
            //ChangeColor(gameObject, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            ChangeColor(gameObject, Color.gray);
    }
}
