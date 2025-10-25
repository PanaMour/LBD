using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Text victoryText;
    public GameObject textObject;
    public GameObject playAgainButton;

    void Start()
    {
        textObject.SetActive(false);
    }

    void Update()
    {
        if(PlayerLP.staticLP <= 0)
        {
            textObject.SetActive(true);
            victoryText.text = "You Lose!";
            playAgainButton.SetActive(true);

        }
        if(OpponentLP.staticLP <= 0)
        {
            textObject.SetActive(true);
            victoryText.text = "Victory!";
            playAgainButton.SetActive(true);
        }
        if(PlayerLP.staticLP <= 0 && OpponentLP.staticLP <= 0)
        {
            textObject.SetActive(true);
            victoryText.text = "Draw!";
            playAgainButton.SetActive(true);
        }
    }
}
