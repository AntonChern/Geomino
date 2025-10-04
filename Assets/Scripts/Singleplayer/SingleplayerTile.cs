using UnityEngine;

public class SingleplayerTile : MonoBehaviour, ITile
{
    private bool state = false;
    public int[] code = new int[3] { -1, -1, -1 };
    public int[] temporaryCode;

    public void ChangeColor(Color color)
    {
        gameObject.GetComponent<Tile>().ChangeColor(gameObject, color);
    }

    public int[] GetCode()
    {
        return code;
    }

    public void SynchronizeCodes()
    {
        for (int i = 0; i < code.Length; i++)
        {
            code[i] = temporaryCode[i];
        }
    }

    public int[] GetTemporaryCode()
    {
        return temporaryCode;
    }

    public void SetTemporaryCode(int index, int value)
    {
        temporaryCode[index] = value;
    }

    public void SetCode(int index, int value)
    {
        code[index] = value;
    }

    public void Dice()
    {
        SynchronizeCodes();
        GameHandler.Instance.MakeMove(transform.position, state, temporaryCode);
    }

    public void SetState(bool state)
    {
        this.state = state;
    }

    public bool GetState()
    {
        return state;
    }

    private void Awake()
    {
        temporaryCode = new int[3] { -1, -1, -1 };
    }
}
