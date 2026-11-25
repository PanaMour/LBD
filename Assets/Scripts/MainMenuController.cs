using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public NetworkManager manager;
    public TMP_InputField ipInputField;

    // Rematch: the host comes back to this menu and hosts again straight away
    // (in the same mode: two players or vs CPU); the other player comes back
    // here too and rejoins the same address.
    public static bool AutoHost;
    public static string RejoinAddress;
    public static int RejoinAttempts;

    // How many players may join over the network when hosting normally; CPU
    // mode sets it to 0 so nobody can wander into a game against the CPU.
    static int normalMaxConnections = -1;

    // Coming back to the menu after a match loads a second NetworkManager,
    // which is thrown away in favour of the one kept from the first visit, so
    // the reference set in the scene can point at a destroyed object. Always
    // use the surviving one.
    NetworkManager Manager => NetworkManager.singleton != null ? NetworkManager.singleton : manager;

    void Start()
    {
        AddCpuButton();

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

    // A "VS CPU" button, made from a copy of the Host button and placed just
    // under it.
    void AddCpuButton()
    {
        GameObject host = GameObject.Find("HostButton");
        GameObject join = GameObject.Find("JoinButton");
        if (host == null || GameObject.Find("CpuButton") != null) return;

        GameObject cpu = Instantiate(host, host.transform.parent);
        cpu.name = "CpuButton";
        RectTransform hostRect = host.GetComponent<RectTransform>();
        RectTransform cpuRect = cpu.GetComponent<RectTransform>();
        RectTransform joinRect = join != null ? join.GetComponent<RectTransform>() : null;
        Vector2 step = joinRect != null
            ? joinRect.anchoredPosition - hostRect.anchoredPosition
            : new Vector2(0f, -(hostRect.rect.height + 20f));
        LayoutGroup layout = host.transform.parent.GetComponent<LayoutGroup>();
        if (layout != null)
        {
            cpu.transform.SetSiblingIndex(host.transform.GetSiblingIndex() + 1); // the layout puts it under Host
            // Make the menu taller so the buttons keep their size: squeezed
            // shorter than the label's line height, the labels disappear.
            RectTransform content = layout.GetComponent<RectTransform>();
            content.sizeDelta += new Vector2(0f, 126f);
        }
        else
            cpuRect.anchoredPosition = (joinRect != null ? joinRect.anchoredPosition : hostRect.anchoredPosition) + step;

        Text label = cpu.GetComponentInChildren<Text>(true);
        if (label != null) label.text = "VS CPU";
        TMP_Text tmpLabel = cpu.GetComponentInChildren<TMP_Text>(true);
        if (tmpLabel != null) tmpLabel.text = "VS CPU";

        Button button = cpu.GetComponent<Button>();
        if (button != null)
        {
            button.onClick = new Button.ButtonClickedEvent(); // drop the copied HostDuel call
            button.onClick.AddListener(PlayCpu);
        }
    }

    // One frame's pause so the menu (and the kept NetworkManager) is fully
    // set up before starting again.
    IEnumerator HostSoon()
    {
        yield return null;
        StartHosting(CpuPlayer.Requested); // same mode as the match that just ended
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
        StartHosting(false);
    }

    public void PlayCpu()
    {
        StartHosting(true);
    }

    void StartHosting(bool vsCpu)
    {
        NetworkManager m = Manager;
        if (normalMaxConnections < 0) normalMaxConnections = m.maxConnections;
        CpuPlayer.Requested = vsCpu;
        m.maxConnections = vsCpu ? 0 : normalMaxConnections;
        m.StartHost();
    }

    public void JoinDuel()
    {
        RejoinAddress = null; // a manual join cancels any pending rematch rejoin
        CpuPlayer.Requested = false;
        Manager.networkAddress = ipInputField.text;
        Manager.StartClient();
    }
}
