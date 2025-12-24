using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public NetworkManager manager;
    public TMP_InputField ipInputField;

    public void HostDuel()
    {
        manager.StartHost();
    }

    public void JoinDuel()
    {
        manager.networkAddress = ipInputField.text;
        manager.StartClient();
    }
}