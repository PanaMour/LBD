using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

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
    public bool hasDrawnInitialHand = false;
    public bool hasDrawnThisTurn = false;
    public int MonstersPlayed = 0;

    private List<GameObject> cards = new List<GameObject>();

    public GameObject ConfirmationBoxPrefab;
    private GameObject tempCard;
    private GameObject tempSlot;
    private GameObject tempTributeVictim;
    private GameObject activeUIBox;

    public GameObject TreasureChestPrefab;
    public Color TreasureTileColor = Color.yellow;

    public bool isTargeting = false;
    public bool isTargetingTile = false;
    public LabyrinthObject teleportMonsterCandidate;
    public bool sprintBoostActive = false;
    public GameObject sprintBoostTarget = null;
    public GameObject activeMagicCard;
    public MagicTargetType currentTargetCriteria;
    public GameObject activeMonsterEffectCard;
    public string pendingMonsterEffect;

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
    }

    IEnumerator DealFiveCards()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1);
            int r = Random.Range(0, 43);
            if (r < 30)
            {
                Card.GetComponent<ThisCard>().thisId = Random.Range(1, 29);
                GameObject card = Instantiate(Card, new Vector2(0, 0), Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);
                RpcShowCard(card, "Dealt", 0);
            }
            else if (r >= 30)
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
        int r = Random.Range(0, 43);
        if (r < 30)
        {
            Card.GetComponent<ThisCard>().thisId = Random.Range(1, 29);
            GameObject card = Instantiate(Card, new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", 0);
        }
        else if (r >= 30)
        {
            Magic.GetComponent<ThisMagic>().thisId = Random.Range(1, 14);
            GameObject card = Instantiate(Magic, new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", 0);
        }
    }

    public void OnDrawPhaseClicked()
    {
        if (!hasAuthority || !IsMyTurn || hasDrawnThisTurn) return;

        if (!hasDrawnInitialHand)
        {
            CmdDealCards();
            hasDrawnInitialHand = true;
        }
        else
        {
            CmdDrawCard();
        }

        hasDrawnThisTurn = true;
    }

    public void Update()
    {
        if (!IsMyTurn || !hasAuthority) return;

        if (Input.GetMouseButtonDown(1))
        {
            if (isTargeting) CancelTargeting();
            if (isTargetingTile) CancelTileTargeting();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            LabyrinthObject attackingUnit = null;
            LabyrinthObject[] allUnits = FindObjectsOfType<LabyrinthObject>();

            foreach (var unit in allUnits)
            {
                if (unit.hasAuthority && unit.waitingToAttack)
                {
                    attackingUnit = unit;
                    break;
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            if (isTargetingTile)
            {
                foreach (RaycastHit hit in hits)
                {
                    GridStat targetTileCandidate = hit.collider.GetComponent<GridStat>();
                    if (targetTileCandidate == null) targetTileCandidate = hit.collider.GetComponentInParent<GridStat>();

                    if (targetTileCandidate != null && teleportMonsterCandidate != null)
                    {
                        GridStat monsterTile = teleportMonsterCandidate.GetComponentInParent<GridStat>();

                        if (monsterTile != null)
                        {
                            if (targetTileCandidate.x == monsterTile.x && Mathf.Abs(targetTileCandidate.y - monsterTile.y) > 0 && Mathf.Abs(targetTileCandidate.y - monsterTile.y) <= 3)
                            {
                                if (targetTileCandidate.GetComponentInChildren<LabyrinthObject>() == null)
                                {
                                    CmdTeleportMonster(activeMagicCard, teleportMonsterCandidate.gameObject, targetTileCandidate.gameObject.name);
                                    CancelTileTargeting();
                                }
                                else
                                {
                                    Debug.Log("Tile is occupied!");
                                }
                            }
                            else
                            {
                                Debug.Log("Invalid Teleport Tile!");
                            }
                        }
                        return;
                    }
                }
                return;
            }

            if (isTargeting)
            {
                foreach (RaycastHit hit in hits)
                {
                    LabyrinthObject targetCandidate = hit.collider.GetComponent<LabyrinthObject>();
                    if (targetCandidate == null) targetCandidate = hit.collider.GetComponentInParent<LabyrinthObject>();

                    if (targetCandidate != null)
                    {
                        if (activeMagicCard != null)
                        {
                            if (CheckTargetValidity(activeMagicCard, targetCandidate, currentTargetCriteria))
                            {
                                if (currentTargetCriteria == MagicTargetType.UnmovedAlly)
                                {
                                    isTargeting = false;
                                    isTargetingTile = true;
                                    teleportMonsterCandidate = targetCandidate;
                                    HighlightTeleportTiles(targetCandidate);
                                    return;
                                }

                                CmdExecuteMagicEffect(activeMagicCard, targetCandidate.gameObject, 0);
                                CancelTargeting();
                            }
                            else Debug.Log("Invalid Target selected.");
                            return;
                        }

                        else if (activeMonsterEffectCard != null)
                        {
                            if (pendingMonsterEffect == "ShadowImp")
                            {
                                ThisCard targetCard = targetCandidate.card.GetComponent<ThisCard>();

                                if (targetCard != null && targetCard.currentAttributes.Contains(Attribute.Dark))
                                {
                                    CmdApplyTempAtk(targetCandidate.card, 200);
                                    CancelTargeting();
                                }
                                else
                                {
                                    Debug.Log("Invalid Target: Shadow Imp can only target a DARK attribute monster!");
                                }
                            }
                            return;
                        }
                    }
                }
                return;
            }
            LabyrinthObject clickedMonster = null;
            GridStat clickedTile = null;

            foreach (RaycastHit hit in hits)
            {
                GameObject obj = hit.collider.gameObject;

                if (obj.GetComponent<ThisCard>() != null || obj.GetComponent<ThisMagic>() != null)
                {
                    continue;
                }

                if (clickedMonster == null)
                {
                    clickedMonster = obj.GetComponent<LabyrinthObject>();
                    if (clickedMonster == null) clickedMonster = obj.GetComponentInParent<LabyrinthObject>();
                }

                if (clickedTile == null)
                {
                    clickedTile = obj.GetComponent<GridStat>();
                    if (clickedTile == null) clickedTile = obj.GetComponentInParent<GridStat>();
                }
            }

            if (attackingUnit != null)
            {
                if (sprintBoostActive && attackingUnit.gameObject != sprintBoostTarget)
                {
                    Debug.Log("Sprint Boost restriction: Other monsters cannot attack!");
                    attackingUnit.waitingToAttack = false;
                    return;
                }

                if (clickedMonster != null && !clickedMonster.hasAuthority)
                {
                    attackingUnit.CmdAttackMonster(clickedMonster.gameObject);
                }
                else if (clickedTile != null)
                {
                    int targetRow = isServer ? 15 : 0;
                    if (clickedTile.y == targetRow && clickedTile.x >= 2 && clickedTile.x <= 8)
                    {
                        attackingUnit.CmdDirectAttack();
                    }
                    else
                    {
                        Debug.Log("Attack skipped. Continuing turn.");
                        attackingUnit.waitingToAttack = false;
                    }
                }
                else
                {
                    Debug.Log("Attack skipped. Continuing turn.");
                    attackingUnit.waitingToAttack = false;
                }

                GameObject grid = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
                if (grid != null) grid.GetComponent<GridBehavior>().ResetTileColors();

                return;
            }

            if (clickedMonster != null && clickedMonster.hasAuthority)
            {
                if (sprintBoostActive && clickedMonster.gameObject != sprintBoostTarget)
                {
                    Debug.Log("Sprint Boost restriction: Other monsters cannot move!");
                    return;
                }

                if (clickedMonster.isImmobile)
                {
                    Debug.Log($"{clickedMonster.name} is Immobile and cannot move!");
                    return;
                }

                clickedMonster.ObjectToMove();
                return;
            }

            if (clickedTile != null)
            {
                GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
                if (gridGen != null)
                {
                    gridGen.GetComponent<GridBehavior>().OnTileClicked(clickedTile.x, clickedTile.y);
                }
            }
        }
    }
    [Server]
    public override void OnStartServer()
    {
        if (connectionToClient == NetworkServer.localConnection)
        {
            ServerSpawnTreasure();
        }
    }

    [Command]
    public void CmdDealCards()
    {
        StartCoroutine(DealFiveCards());
        RpcGMChangeState("Draw Card");
    }

    [Command]
    public void CmdDrawCard()
    {
        StartCoroutine(DrawCard());
        RpcGMChangeState("Action Phase");
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
        ThisMagic magicScript = card.GetComponent<ThisMagic>();

        if (magicScript != null)
        {
            if (magicScript.targetType != MagicTargetType.None)
            {
                StartTargetingMode(card, magicScript.targetType);
                return;
            }

            magicScript.Activate();
        }

        CmdPlayCard(card, index);
    }

    void StartTargetingMode(GameObject card, MagicTargetType type)
    {
        isTargeting = true;
        activeMagicCard = card;
        currentTargetCriteria = type;
        Debug.Log($"Targeting Mode Started: Looking for {type}");
    }

    bool CheckTargetValidity(GameObject magicCard, LabyrinthObject monster, MagicTargetType type)
    {
        bool isEnemy = !monster.hasAuthority;
        bool isAttack = monster.attackMode;
        bool isValidTargetType = false;

        switch (type)
        {
            case MagicTargetType.EnemyAttack: isValidTargetType = isEnemy && isAttack; break;
            case MagicTargetType.EnemyDefense: isValidTargetType = isEnemy && !isAttack; break;
            case MagicTargetType.AnyEnemy: isValidTargetType = isEnemy; break;
            case MagicTargetType.AnyAlly: isValidTargetType = !isEnemy; break;
            case MagicTargetType.AnyUnit: isValidTargetType = true; break;
            case MagicTargetType.UnmovedAlly: isValidTargetType = !isEnemy && !monster.hasMovedThisTurn; break;
            default: isValidTargetType = false; break;
        }

        return isValidTargetType;
    }

    public void CancelTargeting()
    {
        isTargeting = false;
        activeMagicCard = null;
        activeMonsterEffectCard = null;
        pendingMonsterEffect = "";
        Debug.Log("Targeting Cancelled");
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
    public void RpcShowCard(GameObject card, string type, int index)
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
                card.transform.localScale = new Vector3(0.01f, 0.0075f, 0.01f);

                if (hasAuthority)
                {
                    card.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }
                else
                {
                    card.transform.localRotation = Quaternion.Euler(90, 0, 180);
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
            {
                card.GetComponent<ThisCard>().beInGraveyard = true;
                MonstersPlayed--;
            }

            GameObject targetYard = hasAuthority ? EnemyYard : PlayerYard;

            SendCardToGraveyardPile(card, targetYard);
        }
        else if (type == "PlayerDestroyed")
        {
            if (card.GetComponent<ThisCard>() != null)
                card.GetComponent<ThisCard>().beInGraveyard = true;

            GameObject targetYard = hasAuthority ? PlayerYard : EnemyYard;

            SendCardToGraveyardPile(card, targetYard);
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
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                if (!isServer)
                {
                    tc.boost += index;
                }

                tc.actualATK = tc.atk + tc.boost;

                tc.decreased = tc.actualATK;
            }
        }
        else if (type == "ChangeStars")
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                if (!isServer)
                {
                    tc.stars += index;
                    if (tc.stars < 0) tc.stars = 0;
                }
            }
        }
        else if (type == "RemoveImmobile")
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.isImmobile = false;
            }
        }

        if (hasAuthority)
        {
            CalculateBoardAuras();
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
        if (stateRequest == "Action Phase" && hasAuthority == true)
        {
            UIManager.updateButtonText("Action Phase");
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
        PlayerManager[] allPlayers = FindObjectsOfType<PlayerManager>();
        foreach (PlayerManager player in allPlayers)
        {
            player.hasDrawnThisTurn = false;

            if (player.sprintBoostActive && player.sprintBoostTarget != null)
            {
                LabyrinthObject lo = player.sprintBoostTarget.GetComponent<LabyrinthObject>();
                if (lo != null) lo.moveRange -= 4;
            }
        }

        RpcGMChangeTurn();
    }

    [ClientRpc]
    public void RpcGMChangeTurn()
    {
        ThisCard[] allCardsOnBoard = FindObjectsOfType<ThisCard>();
        foreach (ThisCard c in allCardsOnBoard)
        {
            c.tempAtk = 0;
        }
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        pm.IsMyTurn = !pm.IsMyTurn;
        pm.hasDrawnThisTurn = false;

        sprintBoostActive = false;
        sprintBoostTarget = null;
        GameManager.turn++;
        UIManager.updateTurnText();

        if (!hasAuthority)
        {
            UIManager.updateEndButtonColourMagenta();
            UIManager.updateButtonText("Enemy Turn");
        }
        if (hasAuthority)
        {
            UIManager.updateEndButtonColourBlue();

            if (!pm.hasDrawnInitialHand)
            {
                UIManager.updateButtonText("Draw Cards");
            }
            else
            {
                UIManager.updateButtonText("Draw Card");
            }
        }

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

        ThisCard cardScript = cardNetId.gameObject.GetComponent<ThisCard>();

        script.moveRange = cardScript.stars;
        script.monsterID = id;
        script.currentTileName = tileName;
        script.attackMode = cardScript.attackmode;
        script.turnSummoned = GameManager.turn;

        script.isImmobile = cardScript.isImmobile;

        NetworkServer.Spawn(monster, connectionToClient);
        RpcLinkMonsterToCard(monster, cardNetId.gameObject);
    }

    [ClientRpc]
    void RpcLinkMonsterToCard(GameObject monster, GameObject cardObj)
    {
        if (monster != null)
        {
            monster.GetComponent<LabyrinthObject>().card = cardObj;
        }
    }

    public void StartTributeProcess(GameObject card, GameObject slot, GameObject victim)
    {
        tempCard = card;
        tempSlot = slot;
        tempTributeVictim = victim;

        if (Canvas == null) Canvas = GameObject.Find("Canvas");

        string victimName = victim.GetComponent<ThisCard>().cardName;
        string newName = card.GetComponent<ThisCard>().cardName;

        SpawnTributeBox($"Tribute {victimName} to summon {newName}?", true);
    }

    public void StartSummonProcess(GameObject card, GameObject slot, bool needsTribute)
    {
        tempCard = card;
        tempSlot = slot;
        tempTributeVictim = null;

        if (needsTribute)
        {
            Debug.Log("Error: Tributes must be targeted!");
            CancelSummon();
        }
        else
        {
            SpawnModeBox();
        }
    }

    void SpawnTributeBox(string msg, bool isTribute)
    {
        SpawnBox(msg, "Yes", "No", () =>
        {
            Destroy(activeUIBox);

            if (isTribute && tempTributeVictim != null)
            {
                CmdPlayerDestroyCard(tempTributeVictim, 0);

                SpawnModeBox();
            }
        },
        () =>
        {
            Destroy(activeUIBox);
            CancelSummon();
        });
    }
    void FinalizeSummon(bool attackMode)
    {
        if (tempCard != null && tempSlot != null)
        {
            tempCard.transform.SetParent(tempSlot.transform);
            tempCard.transform.localPosition = new Vector3(0, 1.0f, 0);

            if (attackMode)
            {
                tempCard.transform.localRotation = Quaternion.Euler(90, 0, 0);
                tempCard.transform.localScale = new Vector3(0.01f, 0.0075f, 0.01f);
            }
            else
            {
                tempCard.transform.localRotation = Quaternion.Euler(90, 0, 90);
                tempCard.transform.localScale = new Vector3(0.0075f, 0.01f, 0.01f);
            }

            ThisCard cardScript = tempCard.GetComponent<ThisCard>();
            if (cardScript != null)
            {
                cardScript.CmdSetBattleMode(attackMode);
                cardScript.summoned = true;
            }

            GameObject gridGen = GameObject.Find("GridGenerator");
            if (gridGen != null)
            {
                GridBehavior gb = gridGen.GetComponent<GridBehavior>();
                gb.ShowSummonZone(tempCard);
            }
        }
    }
    public void CompleteSummonSequence()
    {
        if (tempCard != null && tempSlot != null)
        {
            string numberOnly = System.Text.RegularExpressions.Regex.Match(tempSlot.name, @"\d+").Value;
            int index = 0;
            if (int.TryParse(numberOnly, out int result)) index = result - 1;

            CardAbilities specialEffect = tempCard.GetComponent<CardAbilities>();
            if (specialEffect != null)
            {
                specialEffect.OnCompile();
            }

            ThisCard genericEffect = tempCard.GetComponent<ThisCard>();
            if (genericEffect != null)
            {
                genericEffect.ActivateSummonEffects();
            }

            if (specialEffect != null)
                PlayCard(tempCard, index);
            else
                CmdPlayCard(tempCard, index);

            if (genericEffect != null)
            {
                genericEffect.CmdSetBattleMode(genericEffect.attackmode);
                genericEffect.summoned = true;
            }

            if (tempCard.GetComponent<ThisCard>().id == 46) // Shadow Imp
            {
                isTargeting = true;
                activeMonsterEffectCard = tempCard;
                pendingMonsterEffect = "ShadowImp";
                Debug.Log("Targeting Mode Started: Select a Dark Attribute Monster!");
            }

            nomoresummons = true;
            tempCard = null;
            tempSlot = null;
        }
    }
    void SpawnModeBox()
    {
        SpawnBox("Select Battle Mode", "Attack", "Defense",
            () => { Destroy(activeUIBox); FinalizeSummon(true); },
            () => { Destroy(activeUIBox); FinalizeSummon(false); }
        );
    }

    void SpawnBox(string message, string yesLabel, string noLabel, UnityEngine.Events.UnityAction yesAction, UnityEngine.Events.UnityAction noAction)
    {
        if (activeUIBox != null) Destroy(activeUIBox);

        activeUIBox = Instantiate(ConfirmationBoxPrefab, Canvas.transform);

        activeUIBox.transform.localPosition = Vector3.zero;
        activeUIBox.transform.localScale = Vector3.one;

        Text txt = activeUIBox.transform.Find("MessageText")?.GetComponent<Text>();
        if (txt == null) txt = activeUIBox.GetComponentInChildren<Text>();
        if (txt != null) txt.text = message;

        Button btn1 = activeUIBox.transform.Find("YesButton")?.GetComponent<Button>();
        if (btn1 == null) btn1 = activeUIBox.transform.Find("Button1")?.GetComponent<Button>();

        Button btn2 = activeUIBox.transform.Find("NoButton")?.GetComponent<Button>();
        if (btn2 == null) btn2 = activeUIBox.transform.Find("Button2")?.GetComponent<Button>();

        if (btn1 != null)
        {
            btn1.onClick.RemoveAllListeners();
            btn1.onClick.AddListener(yesAction);
            Text btnTxt = btn1.GetComponentInChildren<Text>();
            if (btnTxt) btnTxt.text = yesLabel;
        }

        if (btn2 != null)
        {
            btn2.onClick.RemoveAllListeners();
            btn2.onClick.AddListener(noAction);
            Text btnTxt = btn2.GetComponentInChildren<Text>();
            if (btnTxt) btnTxt.text = noLabel;
        }
    }

    public void CancelSummon()
    {
        if (tempCard != null)
        {
            DragDrop dd = tempCard.GetComponent<DragDrop>();
            if (dd != null) dd.ReturnToHand();
        }
        tempCard = null;
        tempSlot = null;
    }

    void SendCardToGraveyardPile(GameObject card, GameObject yard)
    {
        if (yard != null)
        {
            card.transform.SetParent(yard.transform);

            int stackIndex = yard.transform.childCount - 1; 
            float baseLift = 1f; 
            float heightOffset = baseLift + (stackIndex * 0.01f);

            card.transform.localPosition = new Vector3(0, heightOffset, 0);
            card.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            card.transform.localScale = new Vector3(0.01f, 0.0075f, 0.01f); 
            LabyrinthObject[] allTokens = FindObjectsOfType<LabyrinthObject>();
            
            foreach (LabyrinthObject token in allTokens)
            {
                if (token.card == card)
                {
                    if (token.hasAuthority || isServer)
                    {
                        
                        if (hasAuthority)
                        {
                            CmdDestroyToken(token.gameObject);
                        }
                    }
                }
            }
        }
    }

    [Command]
    public void CmdDestroyToken(GameObject token)
    {
        NetworkServer.Destroy(token);
    }

    [Server]
    public void ServerSpawnTreasure()
    {
        int rX = Random.Range(0, 11);
        int rY = Random.Range(6, 10);

        Debug.Log($"[SERVER] Spawning Treasure at Grid Coordinates: {rX}, {rY}");

        Vector3 spawnPos = Vector3.zero;
        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

        if (gridGen != null)
        {
            foreach (Transform child in gridGen.transform)
            {
                GridStat stat = child.GetComponent<GridStat>();
                if (stat != null && stat.x == rX && stat.y == rY)
                {
                    spawnPos = child.position;
                    break;
                }
            }
        }

        if (TreasureChestPrefab != null)
        {
            GameObject chest = Instantiate(TreasureChestPrefab, spawnPos, Quaternion.identity);

            TreasureChest chestScript = chest.GetComponent<TreasureChest>();
            if (chestScript != null)
            {
                chestScript.gridX = rX;
                chestScript.gridY = rY;
                chestScript.tileColor = TreasureTileColor;
            }

            NetworkServer.Spawn(chest);
        }
    }

    [Server]
    public void ServerCollectTreasure(GameObject chest)
    {
        NetworkServer.Destroy(chest);

        List<int> labyrinthCardIds = new List<int>();

        for (int i = 1; i < CardDataBase.cardList.Count; i++)
        {
            if (CardDataBase.cardList[i].type == Type.Labyrinth)
            {
                labyrinthCardIds.Add(i);
            }
        }

        if (labyrinthCardIds.Count > 0)
        {
            int randomId = labyrinthCardIds[Random.Range(0, labyrinthCardIds.Count)];
            StartCoroutine(DrawSpecificCard(randomId));

            Debug.Log($"Player collected treasure! Drawing Card ID: {randomId}");
        }
        else
        {
            Debug.LogWarning("No cards of Type.Labyrinth found in CardDataBase!");
        }
    }

    IEnumerator DrawSpecificCard(int cardId)
    {
        yield return new WaitForSeconds(0.5f);

        GameObject cardObj = Instantiate(Card, Vector2.zero, Quaternion.identity);

        cardObj.GetComponent<ThisCard>().thisId = cardId;

        NetworkServer.Spawn(cardObj, connectionToClient);

        RpcShowCard(cardObj, "Dealt", 0);
    }

    [Command]
    public void CmdExecuteMagicEffect(GameObject magicCard, GameObject targetMonster, int slotIndex)
    {
        ThisMagic magicScript = magicCard.GetComponent<ThisMagic>();
        LabyrinthObject monsterScript = targetMonster.GetComponent<LabyrinthObject>();

        if (magicScript == null || monsterScript == null) return;

        switch (magicScript.targetType)
        {
            case MagicTargetType.EnemyAttack:
                monsterScript.attackMode = false;
                RpcShowCard(monsterScript.card, "ChangeDefense", 0);
                break;

            case MagicTargetType.EnemyDefense:
                monsterScript.attackMode = true;
                RpcShowCard(monsterScript.card, "ChangeAttack", 0);
                break;

            case MagicTargetType.AnyEnemy:
                NetworkServer.Destroy(targetMonster);
                RpcShowCard(monsterScript.card, "OpponentDestroyed", 0);
                break;

            case MagicTargetType.AnyAlly:
                if (magicScript.equip)
                {
                    GameObject monsterCardObj = monsterScript.card;
                    if (monsterCardObj != null)
                    {
                        ThisCard thisCardScript = monsterCardObj.GetComponent<ThisCard>();

                        if (thisCardScript != null)
                        {
                            thisCardScript.boost += magicScript.equipBoost;
                            thisCardScript.actualATK = thisCardScript.atk + thisCardScript.boost;
                            thisCardScript.decreased = thisCardScript.actualATK;

                            RpcShowCard(monsterCardObj, "EquipBoost", magicScript.equipBoost);
                        }
                    }
                }
                else if (magicScript.id == 17) // Sprint Boost
                {
                    monsterScript.moveRange += 4;
                    RpcActivateSprintBoost(targetMonster);
                }
                break;

            case MagicTargetType.AnyUnit:
                if (magicScript.equip)
                {
                    GameObject monsterCardObj = monsterScript.card;
                    ThisCard thisCardScript = (monsterCardObj != null) ? monsterCardObj.GetComponent<ThisCard>() : null;

                    if (magicScript.id == 18) // Mechanical Legs
                    {
                        monsterScript.isImmobile = false;
                        if (thisCardScript != null) thisCardScript.isImmobile = false;

                        if (monsterCardObj != null) RpcShowCard(monsterCardObj, "RemoveImmobile", 0);

                        Debug.Log($"Equipped Mechanical Legs. {targetMonster.name} can now move!");
                    }

                    else if (magicScript.id == 19 || magicScript.id == 20)
                    {
                        int starChange = 0;

                        if (magicScript.id == 19) // Fleetfoot Blessing
                        {
                            monsterScript.moveRange += 2;
                            starChange = 2;
                        }
                        else if (magicScript.id == 20) // Weighted Shackles
                        {
                            monsterScript.moveRange -= 2;
                            if (monsterScript.moveRange < 0) monsterScript.moveRange = 0;
                            starChange = -2;
                        }

                        if (thisCardScript != null)
                        {
                            thisCardScript.stars += starChange;
                            if (thisCardScript.stars < 0) thisCardScript.stars = 0;
                        }

                        if (monsterCardObj != null) RpcShowCard(monsterCardObj, "ChangeStars", starChange);

                        Debug.Log($"Equipped {magicScript.name} to {targetMonster.name}. Move Range changed by {starChange}");
                    }
                }
                else if (magicScript.id == 16) // Labyrinth Dice
                {
                    GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

                    if (gridGen != null)
                    {
                        List<GridStat> validTiles = new List<GridStat>();

                        foreach (Transform child in gridGen.transform)
                        {
                            GridStat tile = child.GetComponent<GridStat>();

                            if (tile != null && tile.y > 0 && tile.y < 15)
                            {
                                if (child.GetComponentInChildren<LabyrinthObject>() == null)
                                {
                                    validTiles.Add(tile);
                                }
                            }
                        }

                        if (validTiles.Count > 0)
                        {
                            GridStat randomTile = validTiles[Random.Range(0, validTiles.Count)];

                            monsterScript.CmdMoveToTile(randomTile.gameObject.name);

                            Debug.Log($"Labyrinth Dice tossed {targetMonster.name} to {randomTile.gameObject.name}!");
                        }
                        else
                        {
                            Debug.LogWarning("No empty tiles found for Labyrinth Dice!");
                        }
                    }
                }
                break;
        }

        RpcShowCard(magicCard, "Played", slotIndex);
    }

    [ClientRpc]
    public void RpcActivateSprintBoost(GameObject target)
    {
        sprintBoostActive = true;
        sprintBoostTarget = target;

        Debug.Log($"Sprint Boost Activated! {target.name} has +4 squares. Others are locked.");
    }
    public void CancelTileTargeting()
    {
        isTargetingTile = false;
        teleportMonsterCandidate = null;
        activeMagicCard = null;

        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        if (gridGen != null) gridGen.GetComponent<GridBehavior>().ResetTileColors();
    }

    void HighlightTeleportTiles(LabyrinthObject monster)
    {
        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        if (gridGen == null) return;

        GridStat monsterTile = monster.GetComponentInParent<GridStat>();
        if (monsterTile == null) return;

        foreach (Transform child in gridGen.transform)
        {
            GridStat tile = child.GetComponent<GridStat>();
            if (tile != null)
            {
                if (tile.x == monsterTile.x && Mathf.Abs(tile.y - monsterTile.y) > 0 && Mathf.Abs(tile.y - monsterTile.y) <= 3)
                {
                    if (child.GetComponentInChildren<LabyrinthObject>() == null)
                    {
                        Renderer r = child.GetComponent<Renderer>();
                        if (r == null) r = child.GetComponentInChildren<Renderer>();
                        if (r != null) r.material.color = Color.cyan;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdTeleportMonster(GameObject magicCard, GameObject monster, string targetTileName)
    {
        LabyrinthObject monsterScript = monster.GetComponent<LabyrinthObject>();
        if (monsterScript != null)
        {
            monsterScript.CmdMoveToTile(targetTileName);
        }

        RpcShowCard(magicCard, "Played", 0);
    }

    void CalculateBoardAuras()
    {
        LabyrinthObject[] allMonsters = FindObjectsOfType<LabyrinthObject>();
        ThisMagic[] allMagics = FindObjectsOfType<ThisMagic>();
        ThisCard[] allCards = FindObjectsOfType<ThisCard>();

        int myCardCount = 0;
        foreach (var m in allMonsters) if (m.hasAuthority) myCardCount++;
        foreach (var magic in allMagics) if (magic.hasAuthority && magic.activated && !magic.beInGraveyard) myCardCount++;

        foreach (var m in allMonsters)
        {
            if (m.hasAuthority && m.monsterID == 57 && m.card != null) // Snalien
            {
                ThisCard tc = m.card.GetComponent<ThisCard>();
                if (tc != null)
                {
                    int targetAuraAtk = (myCardCount == 1) ? 400 : 0;
                    int targetAuraDef = (myCardCount == 1) ? 200 : 0;

                    if (tc.auraAtk != targetAuraAtk || tc.auraDef != targetAuraDef)
                    {
                        tc.auraAtk = targetAuraAtk;
                        tc.auraDef = targetAuraDef;
                        CmdUpdateAuraStats(m.card, targetAuraAtk, targetAuraDef);
                    }
                }
            }
        }

        int plantGraveyardCount = 0;
        foreach (ThisCard card in allCards)
        {
            if (card.beInGraveyard && card.currentTypes.Contains(Type.Plant))
            {
                plantGraveyardCount++;
            }
        }

        foreach (var m in allMonsters)
        {
            if (m.hasAuthority && m.monsterID == 25 && m.card != null) // Thorn Fairy
            {
                ThisCard tc = m.card.GetComponent<ThisCard>();
                if (tc != null)
                {
                    int targetAuraAtk = plantGraveyardCount * 200;

                    if (tc.auraAtk != targetAuraAtk)
                    {
                        tc.auraAtk = targetAuraAtk;

                        CmdUpdateAuraStats(m.card, targetAuraAtk, tc.auraDef);
                    }
                }
            }
        }
    }

    [Command]
    public void CmdUpdateAuraStats(GameObject card, int auraAtk, int auraDef)
    {
        RpcUpdateAuraStats(card, auraAtk, auraDef);
    }

    [ClientRpc]
    public void RpcUpdateAuraStats(GameObject card, int auraAtk, int auraDef)
    {
        if (card != null)
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.auraAtk = auraAtk;
                tc.auraDef = auraDef;
            }
        }
    }

    [Command]
    public void CmdApplyTempAtk(GameObject card, int amount)
    {
        RpcApplyTempAtk(card, amount);
    }

    [ClientRpc]
    public void RpcApplyTempAtk(GameObject card, int amount)
    {
        if (card != null)
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.tempAtk += amount;
            }
        }
    }
}
