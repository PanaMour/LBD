using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerDeck : NetworkBehaviour
{
    public List<Card> deck = new List<Card>();
    public List<Card> container = new List<Card>();
    public static List<Card> staticDeck = new List<Card>();

    public int x;
    public static int deckSize;

    public GameObject cardInDeck1;
    public GameObject cardInDeck2;
    public GameObject cardInDeck3;
    public GameObject cardInDeck4;

    public GameObject CardToHand;

    public GameObject CardBack;
    public GameObject Deck;

    public GameObject[] Clones;

    public GameObject PlayerArea;

    public PlayerManager PlayerManager;

    public GameObject Canvas;
    public GameObject ConfirmationBox;
    public Button YesButton;
    public Button NoButton;

    void Start()
    {
        /*if (this.tag != "Unusable") //error in console with unusable cards
        {
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        }*/
        Canvas = GameObject.Find("Main Canvas");
        x = 0;
        deckSize = 40;

        /*
        for(int i = 0; i < deckSize; i++)
        {
            x = Random.Range(1, 3);
            deck[i] = CardDataBase.cardList[x];
        }

        StartCoroutine(StartGame());*/
    }

    // Update is called once per frame
    void Update()
    {
        staticDeck = deck;

        if (deckSize < 30)
        {
            cardInDeck1.SetActive(false);
        }
        if (deckSize < 20)
        {
            cardInDeck2.SetActive(false);
        }
        if (deckSize < 2)
        {
            cardInDeck3.SetActive(false);
        }
        if (deckSize < 1)
        {
            cardInDeck4.SetActive(false);
        }
        
        
        if(ThisCard.drawX > 0)
        {
            StartCoroutine(Draw(ThisCard.drawX));
            ThisCard.drawX = 0;
        }
    }

    IEnumerator Example()
    {
        yield return new WaitForSeconds(1);
        Clones = GameObject.FindGameObjectsWithTag("Clone");

        foreach(GameObject Clone in Clones)
        {
            Destroy(Clone);
        }
    }

    IEnumerator StartGame()
    {
        for(int i = 0; i <= 4; i++)
        {
            yield return new WaitForSeconds(1);
            GameObject card = Instantiate(CardToHand, transform.position, transform.rotation);
            NetworkServer.Spawn(card, connectionToClient);
            //PlayerManager.CmdFiveCardHand(card);
        }
    }
    IEnumerator Confirmation(GameObject box)
    {
        var waitForButton = new WaitForUIButtons(YesButton, NoButton);
        yield return waitForButton.Reset();
        if (waitForButton.PressedButton == YesButton)
        {
            for (int i = 0; i < deckSize; i++)
            {
                container[0] = deck[i];
                int randomIndex = Random.Range(i, deckSize);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = container[0];
            }
            GameObject card = Instantiate(CardBack, transform.position, transform.rotation);
            card.AddComponent<NetworkIdentity>();
            NetworkServer.Spawn(card, connectionToClient);
            card.tag = "Clone";
            StartCoroutine(Example());
        }
        else
        {

        }
        Destroy(box);
    }

    public void Shuffle()
    {
        GameObject box = Instantiate(ConfirmationBox);
        NetworkServer.Spawn(box, connectionToClient);
        box.GetComponentInChildren<Text>().text = "Do you REALLY wanna shuffle already randomized cards?";
        box.transform.SetParent(Canvas.transform);
        YesButton = GameObject.Find("YESButton").GetComponent<Button>();
        NoButton = GameObject.Find("NOButton").GetComponent<Button>();
        StartCoroutine(Confirmation(box));
    }

    IEnumerator Draw(int x)
    {
        for(int i = 1; i < x; i++)
        {
            yield return new WaitForSeconds(1);
            GameObject card = Instantiate(CardToHand, transform.position, transform.rotation);
            NetworkServer.Spawn(card, connectionToClient);
        }
    }
}
