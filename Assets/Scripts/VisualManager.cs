using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VisualManager : MonoBehaviour
{
    public static VisualManager Instance;

    //private int dices = 4;
    private int freePanelDice;
    private int pressed;
    private int availableDice;

    [SerializeField] private Sprite[] pointSprites;
    [SerializeField] private Button[] panel;
    private int[][] panelCodes;
    [SerializeField] private GameObject backPanel;

    private GameObject lastDice;
    //private GameObject[] dicesUI;

    //private int[] chosenCode;

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
        // code == null => в мешочке закончились кости

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
            //Debug.Log("ShowPanelRpc " + e.code[0] + "," + e.code[1] + "," + e.code[2]);
            //panel[i].transform.GetChild(j).gameObject.GetComponent<TextMeshProUGUI>().text = ints[j].ToString() == "0" ? "" : ints[j].ToString();
            panel[freePanelDice].transform.GetChild(j).gameObject.GetComponent<Image>().sprite = pointSprites[code[j]];
            panelCodes[freePanelDice][j] = code[j];
        }
        panel[freePanelDice].GetComponent<DiceUI>().Code = code;
        //Debug.Log("panelCodes " + panelCodes[freePanelDice].Length);
    }

    public void Dice(Vector2 position, int[] code)
    {
        if (lastDice != null)
            lastDice.GetComponent<ITile>().ChangeColor(Color.white);
        //ChangeColor(lastDice, Color.white);

        //Debug.Log("DiceRpc");
        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit != null)
        {
            hit.gameObject.tag = "Dice";
            // refactor
            Color color = new Color(0.5f, 1f, 1f);
            if (lastDice != null)
            {
                hit.gameObject.GetComponent<ITile>().ChangeColor(color);
                //ChangeColor(hit.gameObject, color);
            }
            else
            {
                hit.gameObject.GetComponent<ITile>().ChangeColor(Color.white);
                //ChangeColor(hit.gameObject, Color.white);
            }
            lastDice = hit.gameObject;
            SetLayer(hit.gameObject, LayerMask.NameToLayer("Default"));
            for (int i = 0; i < code.Length; i++)
            {
                //hit.gameObject.GetComponent<Tile>().SetCodeRpc(i, code[i]);
                hit.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = pointSprites[code[i]];
            }
        }
    }

    private void Try(GameObject place, int[] chosenCode, int offset)
    {
        //int[] code = place.GetComponent<ITile>().GetCode();

        for (int i = 0; i < chosenCode.Length; i++)
        {
            //place.GetComponent<Tile>().temporaryCode[i] = chosenCode[(i + offset) % chosenCode.Length];
            place.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = pointSprites[chosenCode[(i + offset) % chosenCode.Length]];
            place.GetComponent<ITile>().SetTemporaryCode(i, chosenCode[(i + offset) % chosenCode.Length]);
            //Debug.Log($"{i} {place.transform.GetChild(i).name} {pointSprites[chosenCode[(i + offset) % chosenCode.Length]].name}");
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
        //bool playableDice = false;
        //Debug.Log("playableDice");
        for (int j = 0; j < board.childCount; j++)
        {
            //Debug.Log("Inside loop 2 " + j);
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Suits(place, code))
            {
                //playableDice = true;
                return true;
            }
        }
        return false;
        //if (currentIdTurn == 0)
        //{
        //    VisualManager.Instance.ReturnHandledButton(index, playableDice);
        //}
        //else
        //{
        //    VisualManager.Instance.ReturnHandledComputerButton(index, playableDice);
        //    if (!playableDice) disabledComputerDices.Add(index);
        //}
    }

    public void CheckComputerMoves(int[][] hand)
    {
        int availableDice = panel.Length;
        //Debug.Log($"CheckComputerMoves {string.Join(" ", hand.Select(value => value != null))}");
        for (int i = 0; i < panel.Length; i++)
        {
            int[] code = hand[i];
            if (code == null || !PlayableComputerCode(code))
            {
                //DecreaseAvailableDice();
                availableDice--;
            }

            //GameHandler.Instance.HandleButton(i, code);
            // на сервере вызываем метод, передаём clientId и code, он обратно вызывает у нас метод, который либо энейблит эту кнопку, либо нет
            //HandleButtonRpc(NetworkManager.Singleton.LocalClientId, i, code);
        }
        //Debug.Log($"{availableDice}");
        if (availableDice == 0)
        {
            //MultiplayerGameManager.Instance.SkipMoveRpc();
            // skip move
            GameHandler.Instance.SkipMove();
        }
        else
        {
            GameHandler.Instance.MakeComputerMove();
        }
    }

    private bool PlayableCode(int index, int[] code)
    {
        Transform board = GameObject.FindGameObjectWithTag("Board").transform;
        //bool playableDice = false;
        //Debug.Log("playableDice");
        for (int j = 0; j < board.childCount; j++)
        {
            //Debug.Log("Inside loop 2 " + j);
            GameObject place = board.GetChild(j).gameObject;
            if (!place.CompareTag("Place"))
                continue;
            if (Suits(place, code))
            {
                //playableDice = true;
                panel[index].GetComponent<DiceUI>().Enable();
                return true;
            }
        }
        panel[index].GetComponent<DiceUI>().Disable();
        return false;
        //if (currentIdTurn == 0)
        //{
        //    VisualManager.Instance.ReturnHandledButton(index, playableDice);
        //}
        //else
        //{
        //    VisualManager.Instance.ReturnHandledComputerButton(index, playableDice);
        //    if (!playableDice) disabledComputerDices.Add(index);
        //}
    }

    private void CheckMoves()
    {
        int availableDice = panel.Length;
        for (int i = 0; i < panel.Length; i++)
        {
            int[] code = panelCodes[i];
            if (code == null || !PlayableCode(i, code))
            {
                //Debug.Log($"disabled {i}");
                //DecreaseAvailableDice();
                //continue;
                availableDice--;
            }
            //GameHandler.Instance.HandleButton(i, code);
            // на сервере вызываем метод, передаём clientId и code, он обратно вызывает у нас метод, который либо энейблит эту кнопку, либо нет
            //HandleButtonRpc(NetworkManager.Singleton.LocalClientId, i, code);
        }
        //Debug.Log($"PLAYER {availableDice}");
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
            //MultiplayerGameManager.Instance.SkipMoveRpc();
            // skip move
            GameHandler.Instance.SkipMove();
        }
    }

    public void ReturnHandledButton(int index, bool playableDice)
    {
        if (playableDice)
        {
            panel[index].GetComponent<DiceUI>().Enable();
            //availableDice++;
            //panel[index].enabled = true;
            ////Debug.Log("Return availableDice = " + availableDice);
            //ChangeButtonColor(index, Color.white);
        }
        else
        {
            panel[index].GetComponent<DiceUI>().Disable();
            //ChangeButtonColor(index, Color.gray);
            //panel[index].enabled = false;
            DecreaseAvailableDice();
        }
    }

    private void DisableDicesUI()
    {
        //Debug.Log("UnenableButtons");
        for (int i = 0; i < panel.Length; i++)
        {
            panel[i].GetComponent<DiceUI>().Disable();
        }
    }

    public void MakeMove()
    {
        HidePlaces();
        DisableDicesUI();
        //OnDoMove?.Invoke(this, new OnDoMoveArgs
        //{
        //    position = position,
        //    state = state,
        //    code = code
        //});
        ////Debug.Log("currentIdTurn.Value = " + currentIdTurn.Value);

        //// на сервере обновить skippedPlayers = 0
        //NullSkippedPlayersRpc();
        //ChangeTurnRpc();
    }

    public void HideBackPanel()
    {
        backPanel.SetActive(false);
    }

    public void Choose(int index, int[] code)
    {
        //chosenCode = code;
        pressed = index;
        //Debug.Log($"Chosen one {code}");
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
}
