using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public UIManager UIManager;
    public int TurnOrder = 0;
    public int turn = 0;
    public int PlayerVariables = 0;
    public int OpponentVariables = 0;
    public Image KEKW;

    private int ReadyClicks = 0;

    void Start()
    {
        UIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        //UIManager.updatePlayerText();
        //UIManager.updateButtonText(GameState);
        //UIManager.updateTurnText();
    }

    public void ChangeGameState(string stateRequest)
    {
        /*if (stateRequest == "Draw Cards")
        {
            ReadyClicks = 0;
            GameState = "Draw Cards";
        }
        else if (stateRequest == "Compile {}")
        {
            if (ReadyClicks == 1)
            {
                GameState = "Compile {}";
                //UIManager.HighlightTurn(TurnOrder);
            }
        }
        else */
        if (stateRequest == "Execute {}")
        {
            TurnOrder = 0;
        }
        UIManager.updateButtonText(stateRequest);
        UIManager.updateTurnText();
    }

    public void ChangeReadyClicks()
    {
        ReadyClicks++;
    }

    public void CardPlayed()
    {
        TurnOrder++;

        //UIManager.HighlightTurn(TurnOrder);
        UIManager.updateTurnText();
        if (TurnOrder == 10)
        {
            ChangeGameState("Execute {}");
        }
    }

    public void ChangeLP(int playerLP, int opponentLP, bool hasAuthority)
    {
        if (hasAuthority)
        {
            PlayerLP.staticLP += playerLP;
            OpponentLP.staticLP -= opponentLP;
        }
        else
        {
            PlayerLP.staticLP -= opponentLP;
            OpponentLP.staticLP += playerLP;
        }
    }

    public void ChangeVariables(int variables, bool hasAuthority)
    {
        if (hasAuthority)
        {
            PlayerVariables += variables;
        }
        else
        {
            OpponentVariables += variables;
        }
        UIManager.updatePlayerText();
    }

    public void ChangeKEKW(GameObject yourKEKW, GameObject myKEKW ,bool hasAuthority)
    {
        if (hasAuthority)
        {
            KEKW = myKEKW.GetComponent<Image>();
            KEKW.enabled = true;
        }
        else
        {
            KEKW = yourKEKW.GetComponent<Image>();
            KEKW.enabled = true;
        }
    }
}
