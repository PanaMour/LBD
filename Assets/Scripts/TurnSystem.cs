using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class TurnSystem : NetworkBehaviour
{
    public bool isYourTurn;
    public int yourTurn;
    public int yourOpponentTurn;
    public Text turnText;

    // Start is called before the first frame update
    void Start()
    {
        turnText = GameObject.Find("TurnText").GetComponent<Text>();
        isYourTurn = true;
        yourTurn = 1;
        yourOpponentTurn = 0;
    }

    void Update()
    {
        if (isYourTurn)
        {
            turnText.text = "Your Turn";
        }
        else turnText.text = "Opponent Turn";
    }

    public void EndYourTurn()
    {
        isYourTurn = false;
        yourOpponentTurn += 1;
    }

    public void EndYourOpponentTurn()
    {
        isYourTurn = true;
        yourTurn += 1;
    }
}
