using UnityEngine;

public interface ITile
{
    public int[] GetCode();
    public void SetTemporaryCode(int index, int value);
    public void ChangeColor(Color color);
}
