using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    public GameObject textObject;
    public GameObject playAgainButton;

    public string menuSceneName = "MainMenu";

    public void RestartGame()
    {
        PlayerLP.staticLP = 4000;
        OpponentLP.staticLP = 4000;

        playAgainButton.SetActive(false);
        textObject.SetActive(false);

        if (NetworkManager.singleton != null)
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            else if (NetworkServer.active)
            {
                NetworkManager.singleton.StopServer();
            }
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}