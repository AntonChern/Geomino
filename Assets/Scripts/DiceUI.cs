using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour
{
    private int[] code;
    public int[] Code
    {
        get => code;
        set { code = value; }
    }

    [SerializeField] private int index;

    [SerializeField] private GameObject aura;

    public int Index
    {
        get => index;
        set { index = value; }
    }

    private bool playNewDiceAnimation = false;
    private float newDiceAnimationTimer = 0.4f;
    private float newDiceAnimationTime;
    private float newDiceDistance = 200f;
    private Vector2 startButtonPosition;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (aura.activeInHierarchy)
            {
                HideAura();
                //ChangeColor(gameObject, Color.white);
                VisualManager.Instance.Unchoose();
                AudioManager.Instance.Play("DiceDropOut");
            }
            else
            {
                NullAuras();
                ShowAura();
                VisualManager.Instance.Choose(index, code);
                AudioManager.Instance.Play("DicePickUp");
            }
        });
        startButtonPosition = transform.localPosition;
        newDiceAnimationTime = newDiceAnimationTimer;

        Disable();
    }

    private void Update()
    {
        if (playNewDiceAnimation)
        {
            newDiceAnimationTimer -= Time.deltaTime;
            gameObject.transform.localPosition = startButtonPosition + Vector2.down * (newDiceDistance * Mathf.Clamp(newDiceAnimationTimer / newDiceAnimationTime, 0f, 1f));
            if (newDiceAnimationTimer <= 0f)
            {
                newDiceAnimationTimer = newDiceAnimationTime;
                playNewDiceAnimation = false;
            }
        }
    }

    public void PlayNewDiceAnimation()
    {
        playNewDiceAnimation = true;
    }

    public void Disable()
    {
        ChangeColor(gameObject, new Color(0.375f, 0.375f, 0.375f, 1f));
        gameObject.GetComponent<Button>().enabled = false;
    }

    public void Enable()
    {
        ChangeColor(gameObject, Color.white);
        gameObject.GetComponent<Button>().enabled = true;
    }

    private void NullAuras()
    {
        foreach (GameObject diceUI in GameObject.FindGameObjectsWithTag("DiceUI"))
        {
            if (!diceUI.GetComponent<Button>().enabled) continue;
            diceUI.GetComponent<DiceUI>().HideAura();
        }
    }

    public void ShowAura()
    {
        aura.SetActive(true);
    }

    public void HideAura()
    {
        aura.SetActive(false);
    }

    private void ChangeColor(GameObject obj, Color color)
    {
        obj.GetComponent<Image>().color = color;
        foreach (Transform child in obj.transform)
        {
            ChangeColor(child.gameObject, color);
        }
    }
}