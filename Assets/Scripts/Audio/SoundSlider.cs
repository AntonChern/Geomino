using UnityEngine;
using UnityEngine.EventSystems;

public class SoundSlider : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.Play("Dice");
    }
}
