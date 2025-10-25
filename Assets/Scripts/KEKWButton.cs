using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class KEKWButton : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;
    public GameObject myKEKW;
    public GameObject yourKEKW;

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //yourKEKW = transform.Find("yourKEKW").GetComponent<GameObject>();
        //myKEKW = transform.Find("myKEKW").GetComponent<GameObject>();
    }

    public void OnClick()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        PlayerManager.CmdGMChangeKEKW(yourKEKW, myKEKW);
    }
}
