using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    public GameObject textObject;
    public GameObject playAgainButton;

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        PlayerLP.staticLP = 1;
        OpponentLP.staticLP = 1;
        playAgainButton.SetActive(false);
        textObject.SetActive(false);
    }
}
