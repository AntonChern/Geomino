using UnityEngine;

public interface ITile
{
    public void Dice();
    public void SetState(bool state);
    public int[] GetCode();
    public void SetTemporaryCode(int index, int value);
    public void ChangeColor(Color color);
}
