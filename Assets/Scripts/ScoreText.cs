using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    private float elapsedTime = 0f;
    private float animationTime = 1.5f;
    private float distance = 0.7f;
    private Vector3 initialPos = Vector3.zero;

    private void Start()
    {
        initialPos = transform.position;
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;
        transform.position = new Vector3(initialPos.x, initialPos.y + distance * elapsedTime / animationTime, initialPos.z);
        if (elapsedTime >= animationTime)
        {
            Destroy(gameObject);
        }
    }

    private Color GetColor(int value)
    {
        Color result = Color.white;
        switch (value)
        {
            case 1:
            case 2:
                result = new Color(0f, 0.75f, 0f, 1f);
                break;
            case 3:
            case 4:
                result = Color.blue;
                break;
            case 5:
            case 6:
                result = new Color(0.5f, 0f, 0.5f, 1f);
                break;
            default:
                result = new Color(0.75f, 0.75f, 0f, 1f);
                break;
        }
        return result;
    }

    public void SetScore(int value)
    {
        GetComponent<TextMeshProUGUI>().text = $"+{value}";
        GetComponent<TextMeshProUGUI>().color = GetColor(value);
    }
}
