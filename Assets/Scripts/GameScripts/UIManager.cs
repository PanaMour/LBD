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
    private Text phaseButtonText;

    void Start()
    {
        if (GameManager == null)
            GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (Button != null)
            phaseButtonText = Button.GetComponentInChildren<Text>();

        RefreshUI();
    }

    void Update()
    {
        if (phaseButtonText == null || NetworkClient.connection == null || NetworkClient.connection.identity == null)
            return;

        PlayerManager localPM = NetworkClient.connection.identity.GetComponent<PlayerManager>();

        if (localPM != null)
        {
            if (!localPM.IsMyTurn)
            {
                phaseButtonText.text = "Enemy Turn";
                updateEndButtonColourBlue();
            }
            else if (localPM.hasDrawnThisTurn)
            {
                phaseButtonText.text = "Action Phase";
                updateEndButtonColourMagenta();
            }
            else if (!localPM.hasDrawnInitialHand)
            {
                phaseButtonText.text = "Draw Cards";
                updateEndButtonColourMagenta();
            }
            else
            {
                phaseButtonText.text = "Draw Card";
                updateEndButtonColourMagenta();
            }
        }
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
        if (Button != null && phaseButtonText != null)
        {
            phaseButtonText.text = gameState;
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
        if (EndButton != null && EndButton.GetComponent<Outline>() != null)
            EndButton.GetComponent<Outline>().effectColor = Color.magenta;
    }

    public void updateEndButtonColourBlue()
    {
        if (EndButton != null && EndButton.GetComponent<Outline>() != null)
            EndButton.GetComponent<Outline>().effectColor = blueColor;
    }
}