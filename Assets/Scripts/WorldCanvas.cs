using UnityEngine;

public class WorldCanvas : MonoBehaviour
{
    public static WorldCanvas Instance;
    [SerializeField] private GameObject scorePrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnScore(Vector2 position, int value)
    {
        var scoreText = Instantiate(scorePrefab, gameObject.transform);
        scoreText.transform.position = position;
        scoreText.GetComponent<ScoreText>().SetScore(value);
    }
}
