using UnityEngine;
using UnityEngine.UI;

public class PanelAnimator : MonoBehaviour
{
    [SerializeField] private GameObject particles;
    public float speed;

    private void FixedUpdate()
    {
        particles.GetComponent<RawImage>().uvRect = new Rect(particles.GetComponent<RawImage>().uvRect.x - speed * Time.deltaTime, 0f, 1f, 1f);
    }
}
