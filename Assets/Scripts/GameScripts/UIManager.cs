using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;
    public GameObject Button;
    public GameObject EndButton;
    public Text PlayerLPText;
    public Text OpponentLPText;
    public Text TurnText;

    Color blueColor = new Color32(17, 216, 238, 255);

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        PlayerLPText = GameObject.Find("PlayerLPtext").GetComponent<Text>();
        OpponentLPText = GameObject.Find("OpponentLPtext").GetComponent<Text>();
        TurnText = GameObject.Find("TurnText").GetComponent<Text>();
        updatePlayerText();
        updateButtonText("Draw Cards");
        updateTurnText();

    }
    public void updatePlayerText()
    {
        PlayerLPText.text = PlayerLP.staticLP + "LP";
        OpponentLPText.text = OpponentLP.staticLP + "LP";
    }

    public void updateButtonText(string gameState)
    {
        Button = GameObject.Find("Button");
        Button.GetComponentInChildren<Text>().text = gameState;
    }

    public void updateTurnText()
    {
        TurnText.GetComponent<Text>().text = "Turn: " + GameManager.turn;
    }

    public void updateEndButtonColourMagenta()
    {
        EndButton.GetComponent<Outline>().effectColor = Color.magenta;
    }
    
    public void updateEndButtonColourBlue()
    {
        EndButton.GetComponent<Outline>().effectColor = blueColor;
    }

    /*
    public void HighlightTurn(int turnOrder)
    {
        PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        if (turnOrder < 10)
        {
            if (turnOrder == 0)
            {
                if (PlayerManager.IsMyTurn)
                {
                    PlayerManager.PlayerSockets[PlayerManager.CardsPlayed].GetComponent<Outline>().effectColor = Color.magenta;
                }
                else
                {
                    PlayerManager.EnemySockets[PlayerManager.CardsPlayed].GetComponent<Outline>().effectColor = Color.magenta;
                }
            }
            else if (turnOrder > 0)
            {
                if (PlayerManager.IsMyTurn)
                {
                    PlayerManager.PlayerSockets[PlayerManager.CardsPlayed].GetComponent<Outline>().effectColor = Color.magenta;

                    if (isClientOnly && turnOrder > 1)
                    {
                        PlayerManager.EnemySockets[PlayerManager.CardsPlayed - 1].GetComponent<Outline>().effectColor = blueColor;
                    }
                    else
                    {
                        PlayerManager.EnemySockets[PlayerManager.CardsPlayed].GetComponent<Outline>().effectColor = blueColor;
                    }
                }
                else
                {
                    PlayerManager.PlayerSockets[PlayerManager.CardsPlayed - 1].GetComponent<Outline>().effectColor = blueColor;

                    if (isClientOnly)
                    {
                        PlayerManager.EnemySockets[PlayerManager.CardsPlayed - 1].GetComponent<Outline>().effectColor = Color.magenta;
                    }
                    else
                    {
                        PlayerManager.EnemySockets[PlayerManager.CardsPlayed].GetComponent<Outline>().effectColor = Color.magenta;
                    }
                }
            }
        }
        else if(turnOrder == 10)
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerManager.PlayerSockets[i].GetComponent<Outline>().effectColor = blueColor;
                PlayerManager.EnemySockets[i].GetComponent<Outline>().effectColor = blueColor;
            }
        }
    }*/
}
