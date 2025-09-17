using Unity.Netcode;
using UnityEngine;

public class MultiplayerTile : NetworkBehaviour, ITile
{
    private NetworkVariable<bool> state = new NetworkVariable<bool>(false);
    //public int[] code;
    public NetworkList<int> code = new NetworkList<int>(new int[3] { -1, -1, -1 });
    public int[] temporaryCode;

    public void SetState(bool state)
    {
        this.state.Value = state;
    }

    public void ChangeColor(Color color)
    {
        gameObject.GetComponent<Tile>().ChangeColor(gameObject, color);
    }

    public int[] GetCode()
    {
        return new int[] { code[0], code[1], code[2] };
    }

    public void SetTemporaryCode(int index, int value)
    {
        temporaryCode[index] = value;
    }

    [Rpc(SendTo.Server)]
    public void SetCodeRpc(int index, int value)
    {
        code[index] = value;
    }

    public void Dice()
    {
        //gameObject.tag = "Dice";
        //gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        //for (int i = 0; i < code.Count; i++) {
        //    code[i] = temporaryCode[i];
        //}
        //RewriteCodeRpc();
        for (int i = 0; i < code.Count; i++)
        {
            SetCodeRpc(i, temporaryCode[i]);
        }
        //Debug.Log("CODE = " + code[0] + ", " + code[1] + ", " + code[2]);
        //Debug.Log("TEMP CODE = " + temporaryCode[0] + ", " + temporaryCode[1] + ", " + temporaryCode[2]);
        //MultiplayerGameManager.Instance.DoMove(transform.position, state.Value, new int[3] { temporaryCode[0], temporaryCode[1], temporaryCode[2] });
        GameHandler.Instance.MakeMove(transform.position, state.Value, temporaryCode);
        //code = temporaryCode;
        //MultiplayerGameManager.instance.DoMove(transform.position, state.Value, code);
    }

    private void Awake()
    {
        //code = new int[3] { -1, -1, -1 };
        temporaryCode = new int[3] { -1, -1, -1 };
        //state.Value = false;
    }
}
