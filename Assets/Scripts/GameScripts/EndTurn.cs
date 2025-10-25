using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EndTurn : NetworkBehaviour
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

        if (PlayerManager.IsMyTurn)
        {
            InitializeClick();
        }
    }

    void InitializeClick()
    {
        PlayerManager.CmdChangeTurn();
    }
}
