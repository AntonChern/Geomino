using Unity.Netcode;
using UnityEngine;

public class MultiplayerTile : NetworkBehaviour, ITile
{
    private NetworkVariable<bool> state = new NetworkVariable<bool>(false);
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
        for (int i = 0; i < code.Count; i++)
        {
            SetCodeRpc(i, temporaryCode[i]);
        }
        GameHandler.Instance.MakeMove(transform.position, state.Value, temporaryCode);
    }

    private void Awake()
    {
        temporaryCode = new int[3] { -1, -1, -1 };
    }

    public bool GetState()
    {
        return state.Value;
    }
}
