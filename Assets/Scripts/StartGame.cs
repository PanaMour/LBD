using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StartGame : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;
    public GameObject CardToHand;
    public int x;

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnClick()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();

        /*
        x = 0;
        PlayerDeck.deckSize = 40;

        for (int i = 0; i < PlayerDeck.deckSize; i++)
        {
            x = Random.Range(1, 3);
            PlayerDeck.staticDeck[i] = CardDataBase.cardList[x];
        }

        StartCoroutine(DealFive());*/
        PlayerManager.CmdGMChangeLP(0, 100);
    }

    IEnumerator DealFive()
    {
        for (int i = 0; i <= 4; i++)
        {
            yield return new WaitForSeconds(1);
            GameObject card = Instantiate(CardToHand, transform.position, transform.rotation);
            NetworkServer.Spawn(card, connectionToClient);
            PlayerManager.CmdFiveCardHand(card);
        }
    }
}
