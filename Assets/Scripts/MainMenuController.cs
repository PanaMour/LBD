using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public NetworkManager manager;
    public TMP_InputField ipInputField;

    // Rematch: the host comes back to this menu and hosts again straight away;
    // the other player comes back here too and rejoins the same address.
    public static bool AutoHost;
    public static string RejoinAddress;
    public static int RejoinAttempts;

    // Coming back to the menu after a match loads a second NetworkManager,
    // which is thrown away in favour of the one kept from the first visit, so
    // the reference set in the scene can point at a destroyed object. Always
    // use the surviving one.
    NetworkManager Manager => NetworkManager.singleton != null ? NetworkManager.singleton : manager;

    void Start()
    {
        if (AutoHost)
        {
            AutoHost = false;
            StartCoroutine(HostSoon());
        }
        else if (!string.IsNullOrEmpty(RejoinAddress) && RejoinAttempts > 0)
        {
            StartCoroutine(RejoinSoon());
        }
    }

    // One frame's pause so the menu (and the kept NetworkManager) is fully
    // set up before starting again.
    IEnumerator HostSoon()
    {
        yield return null;
        HostDuel();
    }

    // Gives the host time to restart first; if the host isn't up yet the join
    // fails, the client lands back here, and the next attempt runs.
    IEnumerator RejoinSoon()
    {
        RejoinAttempts--;
        yield return new WaitForSeconds(2f);
        if (string.IsNullOrEmpty(RejoinAddress) || NetworkClient.active) yield break;
        if (ipInputField != null) ipInputField.text = RejoinAddress;
        Manager.networkAddress = RejoinAddress;
        Manager.StartClient();
    }

    public void HostDuel()
    {
        Manager.StartHost();
    }

    public void JoinDuel()
    {
        RejoinAddress = null; // a manual join cancels any pending rematch rejoin
        Manager.networkAddress = ipInputField.text;
        Manager.StartClient();
    }
}
