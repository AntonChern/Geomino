using UnityEngine;

public interface ITile
{
    public void Dice();
    public void SetState(bool state);
    public bool GetState();
    public int[] GetCode();
    public void SetTemporaryCode(int index, int value);
    public void ChangeColor(Color color);

    //public void Rotate(int offset);
}
