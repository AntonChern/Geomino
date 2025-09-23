using UnityEngine;

public interface IMap
{
    public bool CanBePlaced(Vector2 position);
    public void DragMap(Vector2 backgroundOffset);
}