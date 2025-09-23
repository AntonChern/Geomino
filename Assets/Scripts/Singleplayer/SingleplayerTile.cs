using UnityEngine;

public class SingleplayerTile : MonoBehaviour, ITile
{
    //[SerializeField] private Sprite[] pointSprites;

    private bool state = false;
    //public int[] code;
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
        //transform.GetChild(index).gameObject.GetComponent<SpriteRenderer>().sprite = pointSprites[value];
    }

    public void SetTemporaryCode(int index, int value)
    {
        temporaryCode[index] = value;
        //transform.GetChild(index).gameObject.GetComponent<SpriteRenderer>().sprite = pointSprites[value];
    }

    public void SetCode(int index, int value)
    {
        code[index] = value;
    }

    public void Dice()
    {
        SynchronizeCodes();
        //Debug.Log("CODE = " + code[0] + ", " + code[1] + ", " + code[2]);
        //Debug.Log("TEMP CODE = " + temporaryCode[0] + ", " + temporaryCode[1] + ", " + temporaryCode[2]);
        //MultiplayerGameManager.Instance.DoMove(transform.position, state.Value, new int[3] { temporaryCode[0], temporaryCode[1], temporaryCode[2] });
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
        //code = new int[3] { -1, -1, -1 };
        temporaryCode = new int[3] { -1, -1, -1 };
        //state.Value = false;
    }

    //public void Rotate(int offset)
    //{
    //    //transform.GetChild(3).rotation = Quaternion.Euler(0f, 0f, (state ? 180f : 0f) + (state ? -1f : 1f) * 120f * offset);
    //    transform.GetChild(3).rotation = Quaternion.Euler(0f, 0f, (state ? 180f : 0f) - 120f * offset);
    //}
}
