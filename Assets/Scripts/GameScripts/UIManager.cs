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

    // Turns until the next treasure chest appears; 0 while one is on the board.
    public static int treasureCountdown;
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

        Text treasure = TreasureText();
        if (treasure != null)
        {
            treasure.gameObject.SetActive(treasureCountdown > 0);
            treasure.text = "Next treasure in " + treasureCountdown + (treasureCountdown == 1 ? " turn" : " turns");
        }
    }

    Text treasureText;

    // A copy of the turn counter placed just below it (the turn counter's own
    // box is too small and cuts off longer text).
    Text TreasureText()
    {
        if (treasureText != null || TurnText == null) return treasureText;

        treasureText = Instantiate(TurnText, TurnText.transform.parent);
        treasureText.name = "TreasureText";
        treasureText.horizontalOverflow = HorizontalWrapMode.Overflow;
        treasureText.verticalOverflow = VerticalWrapMode.Overflow;
        treasureText.alignment = TextAnchor.UpperLeft;
        treasureText.fontSize = Mathf.RoundToInt(TurnText.fontSize * 0.85f);
        treasureText.color = new Color(0.85f, 0.5f, 0f);
        Outline outline = treasureText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform rt = treasureText.rectTransform;
        rt.anchoredPosition = TurnText.rectTransform.anchoredPosition + new Vector2(0, -TurnText.rectTransform.rect.height);
        return treasureText;
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