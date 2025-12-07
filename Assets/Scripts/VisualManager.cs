using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VisualManager : MonoBehaviour
{
    public static VisualManager Instance;

    private int freePanelDice;
    private int pressed;
    private int availableDice;

    [SerializeField] private Sprite[] pointSprites;
    [SerializeField] private Button[] panel;
    private int[][] panelCodes;
    [SerializeField] private GameObject backPanel;

    private GameObject lastDice;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        panelCodes = new int[panel.Length][];
    }

    public void GenerateDiceUI(int[] code)
    {
        if (code == null)
        {
            panel[pressed].gameObject.SetActive(false);
            panelCodes[pressed] = null;
            ClearAvailablePlaces();
            return;
        }

        if (freePanelDice == panel.Length)
        {
            freePanelDice = pressed;
            SetNewDice(code);
            ClearAvailablePlaces();
            panel[freePanelDice].GetComponent<DiceUI>().PlayNewDiceAnimation();
            freePanelDice = panel.Length;
            return;
        }

        SetNewDice(code);
        panel[freePanelDice].GetComponent<DiceUI>().PlayNewDiceAnimation();
        freePanelDice++;
    }

    private void ClearAvailablePlaces()
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            SetLayer(place, LayerMask.NameToLayer("Places"));
        }
    }

    private void SetNewDice(int[] code)
    {
        panelCodes[freePanelDice] = new int[3] { -1, -1, -1 };
        for (int j = 0; j < 3; j++)
        {
            panel[freePanelDice].transform.GetChild(j).gameObject.GetComponent<Image>().sprite = pointSprites[code[j]];
            panelCodes[freePanelDice][j] = code[j];
        }
        panel[freePanelDice].GetComponent<DiceUI>().Code = code;
    }

    public void Dice(Vector2 position, int[] code)
    {
        if (lastDice != null)
            lastDice.GetComponent<ITile>().ChangeColor(Color.white);

        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null)
        {
            hit.gameObject.tag = "Dice";
            Color color = new Color(0.5f, 1f, 1f);
            if (lastDice != null)
            {
                hit.gameObject.GetComponent<ITile>().ChangeColor(color);
            }
            else
            {
                hit.gameObject.GetComponent<ITile>().ChangeColor(Color.white);
            }
            lastDice = hit.gameObject;
            SetLayer(hit.gameObject, LayerMask.NameToLayer("Default"));
            for (int i = 0; i < code.Length; i++)
            {
                hit.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = pointSprites[code[i]];
            }
        }
    }

    private void Try(GameObject place, int[] chosenCode, int offset)
    {
        for (int i = 0; i < chosenCode.Length; i++)
        {
            place.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = pointSprites[chosenCode[(i + offset) % chosenCode.Length]];
            place.GetComponent<ITile>().SetTemporaryCode(i, chosenCode[(i + offset) % chosenCode.Length]);
        }
    }

    public bool Suits(GameObject place, int[] chosenCode)
    {
        int[] code = place.GetComponent<ITile>().GetCode();

        for (int i = 0; i < chosenCode.Length; i++)
        {
            bool result = true;
            for (int j = 0; j < chosenCode.Length; j++)
            {
                if (code[j] != chosenCode[(j + i) % chosenCode.Length] && code[j] != -1)
                {
                    result = false;
                    break;
                }
            }
            if (result)
            {
                Try(place, chosenCode, i);
                return true;
            }
        }
        return false;
    }

    public void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }

    public void HidePlaces()
    {
        int layerIndex = LayerMask.NameToLayer("CurrentPlaces");
        Camera.main.cullingMask &= ~(1 << layerIndex);
    }

    private void ShowPlaces()
    {
        int layerIndex = LayerMask.NameToLayer("CurrentPlaces");
        Camera.main.cullingMask |= (1 << layerIndex);
    }

    public void RefreshHand()
    {
        ShowPlaces();

        CheckMoves();
    }

    public void ReturnHandledComputerButton(int index, bool playableDice)
    {
        if (!playableDice)
        {
            DecreaseAvailableDice();
        }
    }

    private bool PlayableComputerCode(int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int j = 0; j < board.childCount; j++)
        {
            //Debug.Log("Inside loop 2 " + j);
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Suits(place, code))
            {
                return true;
            }
        }
        return false;
    }

    public void CheckComputerMoves(int[][] hand)
    {
        int availableDice = panel.Length;
        for (int i = 0; i < panel.Length; i++)
        {
            int[] code = hand[i];
            if (code == null || !PlayableComputerCode(code))
            {
                availableDice--;
            }
        }
        if (availableDice == 0)
        {
            GameHandler.Instance.SkipMove();
        }
        else
        {
            StartCoroutine(moveWaiter());
        }
    }

    IEnumerator moveWaiter()
    {
        float timer = 0f;
        float waitingTime = Random.Range(0.5f, 1f);
        while (timer < waitingTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        GameHandler.Instance.MakeComputerMove();
    }

    private bool PlayableCode(int index, int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int j = 0; j < board.childCount; j++)
        {
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Suits(place, code))
            {
                panel[index].GetComponent<DiceUI>().Enable();
                return true;
            }
        }
        panel[index].GetComponent<DiceUI>().Disable();
        return false;
    }

    private void CheckMoves()
    {
        int availableDice = panel.Length;
        for (int i = 0; i < panel.Length; i++)
        {
            int[] code = panelCodes[i];
            if (code == null || !PlayableCode(i, code))
            {
                availableDice--;
            }
        }
        if (availableDice == 0)
        {
            GameHandler.Instance.SkipMove();
        }
    }

    public void DecreaseAvailableDice()
    {
        availableDice--;
        if (availableDice == 0)
        {
            GameHandler.Instance.SkipMove();
        }
    }

    public void ReturnHandledButton(int index, bool playableDice)
    {
        if (playableDice)
        {
            panel[index].GetComponent<DiceUI>().Enable();
        }
        else
        {
            panel[index].GetComponent<DiceUI>().Disable();
            DecreaseAvailableDice();
        }
    }

    private void DisableDicesUI()
    {
        for (int i = 0; i < panel.Length; i++)
        {
            panel[i].GetComponent<DiceUI>().Disable();
            panel[i].GetComponent<DiceUI>().HideAura();
        }
    }

    public void MakeMove()
    {
        HidePlaces();
        DisableDicesUI();
        Unchoose();
    }

    public void HideBackPanel()
    {
        backPanel.SetActive(false);
    }

    public void Choose(int index, int[] code)
    {
        pressed = index;
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;

            if (Suits(place, code))
            {
                SetLayer(place, LayerMask.NameToLayer("CurrentPlaces"));
            }
            else
            {
                SetLayer(place, LayerMask.NameToLayer("Places"));
            }
        }
    }

    public void Unchoose()
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        for (int i = 0; i < board.childCount; i++)
        {
            GameObject place = board.GetChild(i).gameObject;
            if (!place.CompareTag("Place"))
                continue;

            SetLayer(place, LayerMask.NameToLayer("Places"));
        }
    }
}
