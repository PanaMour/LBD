using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// End-of-game buttons. "Rematch" asks the host to restart the match with the
// same two players (both have to click it); "Main Menu" leaves the game.
// If the opponent has already left, Rematch falls back to leaving as well.
public class PlayAgain : MonoBehaviour
{
    public GameObject textObject;
    public GameObject playAgainButton;

    public string menuSceneName = "MainMenu";

    // Shared by every PlayAgain in the scene (the component sits on the
    // button prefab and on another scene object); reset when a new match
    // scene loads because the objects they point at are destroyed with it.
    static GameObject menuButton;
    static Text rematchLabel;
    static bool askedForRematch;

    void Start()
    {
        if (playAgainButton == null || menuButton != null) return;

        askedForRematch = false;
        rematchLabel = playAgainButton.GetComponentInChildren<Text>(true);
        if (rematchLabel != null) rematchLabel.text = "Rematch";

        menuButton = Instantiate(playAgainButton, playAgainButton.transform.parent);
        menuButton.name = "MainMenuButton";
        foreach (PlayAgain copy in menuButton.GetComponentsInChildren<PlayAgain>(true))
            DestroyImmediate(copy);

        RectTransform src = playAgainButton.GetComponent<RectTransform>();
        RectTransform dst = menuButton.GetComponent<RectTransform>();
        if (src != null && dst != null)
            dst.anchoredPosition = src.anchoredPosition + new Vector2(0f, -(src.rect.height + 20f));

        Text menuLabel = menuButton.GetComponentInChildren<Text>(true);
        if (menuLabel != null) menuLabel.text = "Main Menu";

        Button menuBtn = menuButton.GetComponent<Button>();
        if (menuBtn != null)
        {
            menuBtn.onClick = new Button.ButtonClickedEvent(); // drop the copied "Rematch" call
            menuBtn.onClick.AddListener(LeaveToMenu);
        }
    }

    void Update()
    {
        if (playAgainButton == null || menuButton == null) return;
        if (menuButton.activeSelf != playAgainButton.activeSelf) menuButton.SetActive(playAgainButton.activeSelf);

        if (playAgainButton.activeSelf && !askedForRematch && rematchLabel != null && !OpponentPresent())
            rematchLabel.text = "Opponent left";
    }

    static bool OpponentPresent()
    {
        return FindObjectsOfType<PlayerManager>().Length >= 2;
    }

    public void RestartGame()
    {
        PlayerManager pm = NetworkClient.connection != null && NetworkClient.connection.identity != null
            ? NetworkClient.connection.identity.GetComponent<PlayerManager>()
            : null;

        // Against the CPU there's nobody to ask: restart straight away
        // (the menu hosts a new CPU game by itself).
        if (CpuPlayer.Requested && NetworkServer.active && GameManager.IsGameOver)
        {
            if (rematchLabel != null) rematchLabel.text = "Starting rematch...";
            PlayerLP.staticLP = 4000;
            OpponentLP.staticLP = 4000;
            MainMenuController.AutoHost = true;
            NetworkManager.singleton.StopHost();
            return;
        }

        if (pm != null && GameManager.IsGameOver && OpponentPresent())
        {
            if (!askedForRematch)
            {
                askedForRematch = true;
                pm.CmdRequestRematch();
            }
            if (rematchLabel != null) rematchLabel.text = "Waiting for opponent...";
            return;
        }

        LeaveToMenu();
    }

    // Called on every machine when a player asks for a rematch.
    public static void OnRematchVote(bool voterIsHost)
    {
        bool itWasMe = voterIsHost == NetworkServer.active;
        if (!itWasMe && !askedForRematch && rematchLabel != null)
            rematchLabel.text = "Rematch (opponent is ready)";
    }

    // The rematch is agreed: this player's game restarts and rejoins by itself.
    public static void ShowRematchStarting()
    {
        if (rematchLabel != null) rematchLabel.text = "Starting rematch...";
    }

    public void LeaveToMenu()
    {
        MainMenuController.AutoHost = false;
        MainMenuController.RejoinAddress = null;
        CpuPlayer.Requested = false;
        PlayerLP.staticLP = 4000;
        OpponentLP.staticLP = 4000;

        if (playAgainButton != null) playAgainButton.SetActive(false);
        if (textObject != null) textObject.SetActive(false);

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
