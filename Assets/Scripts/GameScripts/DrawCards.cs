using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEditor;

public class DrawCards : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnClick()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();

        if (transform.Find("Text").GetComponent<Text>().text == "Draw Cards")
        {
            InitializeClick();
        }
        else if (transform.Find("Text").GetComponent<Text>().text == "Execute {}")
        {
            ExecuteClick();
        }
    }

    void InitializeClick()
    {
        PlayerManager.CmdDealCards();
        PlayerManager.CardsPlayed = 0;
    }

    void ExecuteClick()
    {
        PlayerManager.CmdGMChangeState("Draw Cards");
    }
}
