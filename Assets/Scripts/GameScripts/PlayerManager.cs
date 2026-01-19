using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public GameManager GameManager;
    public UIManager UIManager;

    public GameObject Canvas;
    public GameObject PlayerArea;
    public GameObject EnemyArea;
    public GameObject PlayerSlot1;
    public GameObject PlayerSlot2;
    public GameObject PlayerSlot3;
    public GameObject PlayerSlot4;
    public GameObject ActionSlot1;
    public GameObject ActionSlot2;
    public GameObject ActionSlot3;
    public GameObject ActionSlot4;
    public GameObject EnemySlot1;
    public GameObject EnemySlot2;
    public GameObject EnemySlot3;
    public GameObject EnemySlot4;
    public GameObject EnemyActionSlot1;
    public GameObject EnemyActionSlot2;
    public GameObject EnemyActionSlot3;
    public GameObject EnemyActionSlot4;
    public GameObject PlayerYard;
    public GameObject EnemyYard;
    public GameObject LabyrinthObjectPrefab;
    public List<GameObject> PlayerSockets = new List<GameObject>();
    public List<GameObject> EnemySockets = new List<GameObject>();
    public List<GameObject> PlayerActionSockets = new List<GameObject>();
    public List<GameObject> EnemyActionSockets = new List<GameObject>();

    private GameObject zoomCard;

    public GameObject Card;
    public GameObject Magic;
    public GameObject CardToHand;

    public int CardsPlayed = 0;
    public bool IsMyTurn = false;
    public bool nomoresummons = false;
    public int MonstersPlayed = 0;

    private List<GameObject> cards = new List<GameObject>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        Canvas = GameObject.Find("Main Canvas");
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        UIManager = GameObject.Find("UIManager").GetComponent<UIManager>();

        PlayerArea = GameObject.Find("Hand_Anchor");
        EnemyArea = GameObject.Find("EnemyArea");
        PlayerYard = GameObject.Find("PlayerYard");
        EnemyYard = GameObject.Find("EnemyYard");

        PlayerSlot1 = GameObject.Find("PlayerSlot1");
        PlayerSlot2 = GameObject.Find("PlayerSlot2");
        PlayerSlot3 = GameObject.Find("PlayerSlot3");
        PlayerSlot4 = GameObject.Find("PlayerSlot4");
        ActionSlot1 = GameObject.Find("ActionSlot1");
        ActionSlot2 = GameObject.Find("ActionSlot2");
        ActionSlot3 = GameObject.Find("ActionSlot3");
        ActionSlot4 = GameObject.Find("ActionSlot4");
        EnemySlot1 = GameObject.Find("EnemySlot1");
        EnemySlot2 = GameObject.Find("EnemySlot2");
        EnemySlot3 = GameObject.Find("EnemySlot3");
        EnemySlot4 = GameObject.Find("EnemySlot4");
        EnemyActionSlot1 = GameObject.Find("EnemyActionSlot1");
        EnemyActionSlot2 = GameObject.Find("EnemyActionSlot2");
        EnemyActionSlot3 = GameObject.Find("EnemyActionSlot3");
        EnemyActionSlot4 = GameObject.Find("EnemyActionSlot4");

        PlayerSockets.Add(PlayerSlot1);
        PlayerSockets.Add(PlayerSlot2);
        PlayerSockets.Add(PlayerSlot3);
        PlayerSockets.Add(PlayerSlot4);
        EnemySockets.Add(EnemySlot1);
        EnemySockets.Add(EnemySlot2);
        EnemySockets.Add(EnemySlot3);
        EnemySockets.Add(EnemySlot4);
        PlayerActionSockets.Add(ActionSlot1);
        PlayerActionSockets.Add(ActionSlot2);
        PlayerActionSockets.Add(ActionSlot3);
        PlayerActionSockets.Add(ActionSlot4);
        EnemyActionSockets.Add(EnemyActionSlot1);
        EnemyActionSockets.Add(EnemyActionSlot2);
        EnemyActionSockets.Add(EnemyActionSlot3);
        EnemyActionSockets.Add(EnemyActionSlot4);

        if (isClientOnly)
        {
            IsMyTurn = true;
            UIManager.updateEndButtonColourMagenta();
            nomoresummons = false;
        }

/*
        x = 0;
        PlayerDeck.deckSize = 40;

        for (int i = 0; i < PlayerDeck.deckSize; i++)
        {
            x = Random.Range(1, 3);
            PlayerDeck.staticDeck[i] = CardDataBase.cardList[x];
        }

        StartCoroutine(StartGame());*/
    }
    /*
    IEnumerator StartGame()
    {
        for (int i = 0; i <= 4; i++)
        {
            yield return new WaitForSeconds(1);
            GameObject card = Instantiate(CardToHand, transform.position, transform.rotation);
            NetworkServer.Spawn(card, connectionToClient);
            CmdFiveCardHand(card);
        }
    }*/
    IEnumerator DealFiveCards()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1);
            int r = Random.Range(0, 38);
            if (r < 25)
            {
                Card.GetComponent<ThisCard>().thisId = Random.Range(1, 24);
                GameObject card = Instantiate(Card, new Vector2(0, 0), Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);
                RpcShowCard(card, "Dealt", 0);
            }
            else if (r >= 25)
            {
                Magic.GetComponent<ThisMagic>().thisId = Random.Range(1, 14);
                GameObject card = Instantiate(Magic, new Vector2(0, 0), Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);
                RpcShowCard(card, "Dealt", 0);
            }
        }
    }

    IEnumerator DrawCard()
    {
        yield return new WaitForSeconds(1);
        int r = Random.Range(0, 38);
        if (r < 25)
        {
            Card.GetComponent<ThisCard>().thisId = Random.Range(1, 24);
            GameObject card = Instantiate(Card, new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", 0);
        }
        else if (r >= 25)
        {
            Magic.GetComponent<ThisMagic>().thisId = Random.Range(1, 14);
            GameObject card = Instantiate(Magic, new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", 0);
        }
    }

    public void Update()
    {
    }

    [Server]
    public override void OnStartServer()
    {
        //cards.Add(Ping);
        //cards.Add(Card1);
        //Card.GetComponent<ThisCard>().thisId = 2;
        //cards.Add(Card);
    }

    [Command]
    public void CmdDealCards()
    {
        StartCoroutine(DealFiveCards());
        RpcGMChangeState("Compile {}");
    }

    [Command]
    public void CmdDrawCard()
    {
        StartCoroutine(DrawCard());
    }

    [Command]
    public void CmdFiveCardHand(GameObject card)
    {
        NetworkServer.Spawn(card, connectionToClient);
        RpcShowCard(card, "Dealt", 0);
    }

    public void PlayCard(GameObject card, int index)
    {
        if (card.GetComponent<CardAbilities>() != null)
        {
            card.GetComponent<CardAbilities>().OnCompile();
        }

        CmdPlayCard(card, index);
    }

    public void PlayMagicCard(GameObject card, int index)
    {
        if (card.GetComponent<ThisMagic>() != null)
        {
            card.GetComponent<ThisMagic>().Activate();
        }

        CmdPlayCard(card, index);
    }
    [Command]
    public void CmdPlayCard(GameObject card, int index)
    {
        RpcShowCard(card, "Played",index);
    }

    [Command]
    public void CmdOpponentDestroyCard(GameObject card, int index)
    {
        RpcShowCard(card, "OpponentDestroyed", index);
    }

    [Command]
    public void CmdPlayerDestroyCard(GameObject card, int index)
    {
        RpcShowCard(card, "PlayerDestroyed", index);
    }

    [Command]
    public void CmdChangeAttack(GameObject card,int index)
    {
        RpcShowCard(card, "ChangeAttack", index);
    }

    [Command]
    public void CmdChangeDefense(GameObject card, int index)
    {
        RpcShowCard(card, "ChangeDefense", index);
    }

    [Command]
    public void CmdEquipBoost(GameObject card,int equipBoost)
    {
        RpcShowCard(card, "EquipBoost", equipBoost);
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, string type, int index)
    {
        if (card == null) return;

        if (type == "Dealt")
        {
            if (hasAuthority)
            {
                GameObject handAnchor = GameObject.Find("Hand_Anchor");
                if (handAnchor != null)
                {
                    card.transform.SetParent(handAnchor.transform);
                    card.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                    card.transform.localPosition = new Vector3(index * 0.1f, 0, 0);
                    card.transform.localRotation = Quaternion.identity;

                    if (card.GetComponent<ThisCard>() != null)
                        card.GetComponent<ThisCard>().cardBack = false;
                }
            }
            else
            {
                if (EnemyArea != null)
                {
                    card.transform.SetParent(EnemyArea.transform);
                    card.transform.localPosition = new Vector3(index * 0.1f, 0, 0);
                    card.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                    if (card.GetComponent<ThisCard>() != null)
                        card.GetComponent<ThisCard>().cardBack = true;
                }
            }
        }
        else if (type == "Played")
        {
            Transform targetSocket = null;

            if (hasAuthority)
            {
                if (card.GetComponent<ThisMagic>() != null) targetSocket = PlayerActionSockets[index].transform;
                if (card.GetComponent<ThisCard>() != null) targetSocket = PlayerSockets[index].transform;

                CmdGMCardPlayed();
            }
            else
            {
                if (card.GetComponent<ThisMagic>() != null) targetSocket = EnemyActionSockets[index].transform;
                if (card.GetComponent<ThisCard>() != null) targetSocket = EnemySockets[index].transform;
            }

            if (targetSocket != null)
            {
                card.transform.SetParent(targetSocket, true);

                card.transform.localPosition = new Vector3(0, 1.0f, 0);
                card.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);

                if (hasAuthority)
                {
                    card.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    card.transform.localRotation = Quaternion.Euler(0, 0, 180);
                }

                if (card.GetComponent<ThisMagic>() != null)
                {
                    card.GetComponent<ThisMagic>().cardBack = false;
                    card.GetComponent<ThisMagic>().activated = true;
                    card.GetComponent<ThisMagic>().faceup = true;
                }
                if (card.GetComponent<ThisCard>() != null)
                {
                    card.GetComponent<ThisCard>().cardBack = false;
                    card.GetComponent<ThisCard>().summoned = true;
                    card.GetComponent<ThisCard>().faceup = true;
                }
            }
        }
        else if (type == "OpponentDestroyed")
        {
            if (card.GetComponent<ThisCard>() != null)
                card.GetComponent<ThisCard>().beInGraveyard = true;

            GameObject targetYard = hasAuthority ? EnemyYard : PlayerYard;
            if (targetYard != null)
            {
                card.transform.SetParent(targetYard.transform, false);
            }

            if (card.GetComponent<ThisCard>() != null)
            {
                MonstersPlayed--;
            }
        }
        else if (type == "PlayerDestroyed")
        {
            if (card.GetComponent<ThisCard>() != null)
                card.GetComponent<ThisCard>().beInGraveyard = true;

            GameObject targetYard = hasAuthority ? PlayerYard : EnemyYard;
            if (targetYard != null)
            {
                card.transform.SetParent(targetYard.transform, false);
            }
        }
        else if (type == "ChangeAttack")
        {
            float zRot = hasAuthority ? 90 : -90;
            if (!hasAuthority) zRot = -zRot;

            card.transform.Rotate(0, 0, zRot);
            card.GetComponent<ThisCard>().attackmode = true;
        }
        else if (type == "ChangeDefense")
        {
            float zRot = hasAuthority ? -90 : 90;
            if (!hasAuthority) zRot = -zRot;

            card.transform.Rotate(0, 0, zRot);
            card.GetComponent<ThisCard>().attackmode = false;
        }
        else if (type == "EquipBoost")
        {
            if (card.GetComponent<ThisCard>() != null)
                card.GetComponent<ThisCard>().boost = index;
        }
    }
    [Command]
    public void CmdGMChangeState(string stateRequest)
    {
        RpcGMChangeState(stateRequest);
    }

    [ClientRpc]
    void RpcGMChangeState(string stateRequest)
    {
        if (stateRequest == "Compile {}" && hasAuthority == true)
        {
            UIManager.updateButtonText("Compile {}");
            UIManager.updateTurnText();
        }
        else if (stateRequest == "Draw Cards")
        {
            UIManager.updateButtonText("Draw Cards");
            UIManager.updateTurnText();
        }
    }

    [Command]
    void CmdGMCardPlayed()
    {
        RpcGMCardPlayed();
    }

    [ClientRpc]
    void RpcGMCardPlayed()
    {
        GameManager.CardPlayed();
    }

    [Command]
    public void CmdGMChangeVariables(int variables)
    {
        RpcGMChangeVariables(variables);
    }

    [ClientRpc]
    public void RpcGMChangeVariables(int variables)
    {
        GameManager.ChangeVariables(variables, hasAuthority);
    }

    [Command]
    public void CmdGMChangeKEKW(GameObject yourKEKW, GameObject myKEKW)
    {
        RpcGMChangeKEKW(yourKEKW, myKEKW);
    }

    [ClientRpc]
    public void RpcGMChangeKEKW(GameObject yourKEKW, GameObject myKEKW)
    {
        GameManager.ChangeKEKW(yourKEKW, myKEKW, hasAuthority);
    }

    [Command]
    public void CmdGMChangeLP(int playerLP,int opponentLP)
    {
        RpcGMChangeLP(playerLP, opponentLP);
    }

    [ClientRpc]
    public void RpcGMChangeLP(int playerLP, int opponentLP)
    {
        GameManager.ChangeLP(playerLP, opponentLP, hasAuthority);
    }

    [Command]
    public void CmdChangeTurn()
    {
        RpcGMChangeTurn();
    }

    [ClientRpc]
    public void RpcGMChangeTurn()
    {
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        pm.IsMyTurn = !pm.IsMyTurn;
        GameManager.turn++;
        UIManager.updateTurnText();

        if (!hasAuthority)
            UIManager.updateEndButtonColourMagenta();
        if (hasAuthority)
            UIManager.updateEndButtonColourBlue();

        LabyrinthObject[] allMonsters = FindObjectsOfType<LabyrinthObject>();
        foreach (LabyrinthObject monster in allMonsters)
        {
            if (isServer)
            {
                monster.hasMovedThisTurn = false;
            }
        }

        nomoresummons = false;
    }

    [Command]
    public void CmdChangeBattlePosition(GameObject card, bool ATKDEF)
    {
        RpcGMChangeBattlePosition(card, ATKDEF);
    }

    [ClientRpc]
    public void RpcGMChangeBattlePosition(GameObject card, bool ATKDEF)
    {
        if (ATKDEF)
        {
            card.GetComponent<ThisCard>().attackmode = true;
        }
        else
        {
            card.GetComponent<ThisCard>().attackmode = false;
        }
    }

    // /// // // / // / / / /// // //////////////////////////////////////
    [Command]
    public void CmdZoomCard(string card)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].name == card)
            {
                //Input.mousePosition.x - Canvas.GetComponent<RectTransform>().rect.width / 2, Input.mousePosition.y
                zoomCard = Instantiate(cards[i], new Vector2(Input.mousePosition.x - Canvas.GetComponent<RectTransform>().rect.width / 2, Input.mousePosition.y - 200), Quaternion.identity);
                NetworkServer.Spawn(zoomCard);
                zoomCard.layer = LayerMask.NameToLayer("Zoom");
                TargetZoomCard(connectionToClient, zoomCard);
            }
        }
    }

    [TargetRpc]
    public void TargetZoomCard(NetworkConnection target, GameObject card)
    {
        if (hasAuthority)
        {
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 354);
            card.transform.SetParent(Canvas.transform, false);
        }
    }

    [Command]
    public void CmdDestroyZoomCard()
    {
        NetworkServer.Destroy(zoomCard);
    }
    // /// // // / // / / / /// // //////////////////////////////////////

    [Command]
    public void CmdSpawnMonster(int id, string tileName, NetworkIdentity cardNetId)
    {
        GameObject monster = Instantiate(LabyrinthObjectPrefab);
        LabyrinthObject script = monster.GetComponent<LabyrinthObject>();

        // Get the stars from the card on the Server side
        script.moveRange = cardNetId.gameObject.GetComponent<ThisCard>().stars;
        script.monsterID = id;
        script.currentTileName = tileName;

        NetworkServer.Spawn(monster, connectionToClient);
    }

    [ClientRpc]
    void RpcLinkMonsterToCard(GameObject monster, GameObject cardObj)
    {
        if (monster != null)
        {
            monster.GetComponent<LabyrinthObject>().card = cardObj;
        }
    }
}
