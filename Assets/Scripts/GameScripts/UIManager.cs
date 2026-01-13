using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;

    [Header("UI References")]
    public GameObject Button;
    public GameObject EndButton;
    public Text PlayerLPText;
    public Text OpponentLPText;
    public Text TurnText;

    Color blueColor = new Color32(17, 216, 238, 255);

    void Start()
    {
        if (GameManager == null)
            GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (PlayerLPText != null && OpponentLPText != null)
            updatePlayerText();

        if (TurnText != null)
            updateTurnText();
    }

    public void updatePlayerText()
    {
        PlayerLPText.text = PlayerLP.staticLP + " LP";
        OpponentLPText.text = OpponentLP.staticLP + " LP";
    }

    public void updateButtonText(string gameState)
    {
        if (Button != null)
        {
            Button.GetComponentInChildren<Text>().text = gameState;
        }
    }

    public void updateTurnText()
    {
        if (TurnText != null && GameManager != null)
        {
            TurnText.text = "Turn: " + GameManager.turn;
        }
    }

    public void updateEndButtonColourMagenta()
    {
        if (EndButton != null)
            EndButton.GetComponent<Outline>().effectColor = Color.magenta;
    }

    public void updateEndButtonColourBlue()
    {
        if (EndButton != null)
            EndButton.GetComponent<Outline>().effectColor = blueColor;
    }
}