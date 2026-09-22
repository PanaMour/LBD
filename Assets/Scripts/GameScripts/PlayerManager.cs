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
    public GameObject Action;
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

    public GameObject GraveyardInspectorPanelPrefab;
    public GameObject GraveyardCardTilePrefab;
    private GameObject activeGraveyardPanel;

    public GameObject TreasureChestPrefab;
    public Color TreasureTileColor = Color.yellow;

    public bool isTargeting = false;
    public bool isTargetingTile = false;
    public LabyrinthObject teleportMonsterCandidate;
    public bool sprintBoostActive = false;
    public GameObject sprintBoostTarget = null;
    public GameObject activeMagicCard;
    public MagicTargetType currentTargetCriteria;
    public int pendingSlotIndex;
    public GameObject activeMonsterEffectCard;
    public string pendingMonsterEffect;
    private LabyrinthObject pendingSwapFirst; // Shy Magician: first of the two monsters picked to swap
    [SyncVar] public bool honeySnareActive = false;
    public bool isTargetingDiscard = false;
    public GameObject pendingAttacker;
    public GameObject pendingDefender;
    public GameObject pendingTrap;

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
            int r = Random.Range(0, 87);
            if (r < 60)
            {
                Card.GetComponent<ThisCard>().thisId = Random.Range(1, 59);
                GameObject card = Instantiate(Card, new Vector2(0, 0), Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);
                RpcShowCard(card, "Dealt", 0);
            }
            else if (r >= 60 && r<83)
            {
                Magic.GetComponent<ThisMagic>().thisId = Random.Range(1, 24);
                GameObject card = Instantiate(Magic, new Vector2(0, 0), Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);
                RpcShowCard(card, "Dealt", 0);
            }
            else if (r >= 83)
            {
                Action.GetComponent<ThisAction>().thisId = Random.Range(1, 6);
                GameObject card = Instantiate(Action, new Vector2(0, 0), Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);
                RpcShowCard(card, "Dealt", 0);
            }
        }
    }

    IEnumerator DrawCard()
    {
        yield return new WaitForSeconds(1);
        int r = Random.Range(0, 87);
        if (r < 60)
        {
            Card.GetComponent<ThisCard>().thisId = Random.Range(1, 59);
            GameObject card = Instantiate(Card, new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", 0);
        }
        else if (r >= 60 && r <83)
        {
            Magic.GetComponent<ThisMagic>().thisId = Random.Range(1, 24);
            GameObject card = Instantiate(Magic, new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", 0);
        }
        else if (r >= 83)
        {
            Action.GetComponent<ThisAction>().thisId = Random.Range(1, 6);
            GameObject card = Instantiate(Action, new Vector2(0, 0), Quaternion.identity);
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
        if (!hasAuthority) return;
        if (!IsMyTurn && !isTargetingDiscard) return;

        if (Input.GetMouseButtonDown(1))

            if (Input.GetMouseButtonDown(1))
        {
            if (isTargeting) CancelTargeting();
            if (isTargetingTile) CancelTileTargeting();
            if (isTargetingDiscard)
            {
                CmdAnswerTrapPrompt(false, pendingTrap, pendingAttacker, pendingDefender);
                isTargetingDiscard = false;
                pendingTrap = null; pendingAttacker = null; pendingDefender = null;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isTargetingDiscard)
            {
                Ray rayDiscard = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] discardHits = Physics.RaycastAll(rayDiscard);

                bool clickedValidCard = false;

                foreach (RaycastHit hit in discardHits)
                {
                    NetworkIdentity netId = hit.collider.GetComponentInParent<NetworkIdentity>();

                    if (netId != null && netId.hasAuthority)
                    {
                        if (netId.transform.parent != null && netId.transform.parent.name == "Hand_Anchor")
                        {
                            CmdResolveEchoOfSilence(netId.gameObject, pendingTrap, pendingAttacker, pendingDefender);

                            isTargetingDiscard = false;
                            pendingTrap = null; pendingAttacker = null; pendingDefender = null;
                            clickedValidCard = true;
                            break;
                        }
                    }
                }

                if (!clickedValidCard)
                {
                    Debug.Log("Please click a valid card in your hand to discard, or right-click to cancel.");
                }

                return;
            }
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
                    else if (activeMagicCard != null && activeMagicCard.GetComponent<ThisMagic>().id == 21)
                    {
                        if (targetTileCandidate.GetComponentInChildren<LabyrinthObject>() == null)
                        {
                            CmdSetFatalSquare(targetTileCandidate.gameObject.name);
                            CancelTileTargeting();
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
                    if (pendingMonsterEffect == "Aetherwing")
                    {
                        ThisMagic targetMagic = hit.collider.GetComponent<ThisMagic>();
                        if (targetMagic == null) targetMagic = hit.collider.GetComponentInParent<ThisMagic>();

                        if (targetMagic != null && targetMagic.activated)
                        {
                            if (!targetMagic.hasAuthority)
                            {
                                CmdOpponentDestroyCard(targetMagic.gameObject, 0);
                                Debug.Log("Aetherwing Butterfly destroyed an enemy spell!");

                                CancelTargeting();
                                return;
                            }
                            else
                            {
                                Debug.Log("Invalid Target: You must select an ENEMY magic card!");
                            }
                            continue;
                        }

                        ThisAction targetAction = hit.collider.GetComponent<ThisAction>();
                        if (targetAction == null) targetAction = hit.collider.GetComponentInParent<ThisAction>();

                        if (targetAction != null && targetAction.activated)
                        {
                            if (!targetAction.hasAuthority)
                            {
                                CmdOpponentDestroyCard(targetAction.gameObject, 0);
                                Debug.Log("Aetherwing Butterfly destroyed an enemy action card!");

                                CancelTargeting();
                                return;
                            }
                            else
                            {
                                Debug.Log("Invalid Target: You must select an ENEMY action card!");
                            }
                        }
                        continue;
                    }
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

                                    // Clear the "valid ally monster" highlight
                                    // from the picking-a-monster phase before
                                    // switching to the destination-tile
                                    // highlight, otherwise the two overlap.
                                    foreach (LabyrinthTile t in FindObjectsOfType<LabyrinthTile>())
                                        if (t.isHighlighted) t.StopGlowBlock();

                                    HighlightTeleportTiles(targetCandidate);
                                    return;
                                }

                                CmdExecuteMagicEffect(activeMagicCard, targetCandidate.gameObject, pendingSlotIndex);
                                CancelTargeting();
                            }
                            else Debug.Log("Invalid Target selected.");
                            return;
                        }

                        else if (activeMonsterEffectCard != null)
                        {
                            if (pendingMonsterEffect == "ShadowImp")
                            {
                                if (IsValidMonsterEffectTarget("ShadowImp", activeMonsterEffectCard, targetCandidate))
                                {
                                    CmdApplyTempAtk(targetCandidate.card, 200);
                                    CancelTargeting();
                                }
                                else
                                {
                                    Debug.Log("Invalid Target: Shadow Imp can only target a DARK attribute monster!");
                                }
                            }
                            else if (pendingMonsterEffect == "Rattlesnake")
                            {
                                if (IsValidMonsterEffectTarget("Rattlesnake", activeMonsterEffectCard, targetCandidate))
                                {
                                    GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
                                    if (gridGen != null)
                                    {
                                        gridGen.GetComponent<GridBehavior>().ShowPossiblePaths(targetCandidate.gameObject, 3);

                                        CancelTargeting();
                                    }
                                }
                            }
                            else if (pendingMonsterEffect == "FrostWraith")
                            {
                                if (IsValidMonsterEffectTarget("FrostWraith", activeMonsterEffectCard, targetCandidate))
                                {
                                    CmdTributeFrostWraith(activeMonsterEffectCard, targetCandidate.gameObject);
                                    CancelTargeting();
                                }
                                else
                                {
                                    Debug.Log("Invalid Target: You must select a monster inside a Card Base!");
                                }
                            }
                            else if (pendingMonsterEffect == "ShyMagician")
                            {
                                if (!IsValidMonsterEffectTarget("ShyMagician", activeMonsterEffectCard, targetCandidate))
                                {
                                    Debug.Log("Invalid Target: You must select one of YOUR Mage-type monsters!");
                                }
                                else if (pendingSwapFirst == null)
                                {
                                    pendingSwapFirst = targetCandidate;
                                    Debug.Log("Shy Magician: Select the second Mage-type monster to swap with!");
                                }
                                else if (pendingSwapFirst == targetCandidate)
                                {
                                    Debug.Log("Invalid Target: Select a DIFFERENT monster for the second target!");
                                }
                                else
                                {
                                    CmdSwapMonsterPositions(activeMonsterEffectCard, pendingSwapFirst.gameObject, targetCandidate.gameObject);
                                    pendingSwapFirst = null;
                                    CancelTargeting();
                                }
                            }
                            else if (pendingMonsterEffect == "SpiritTribute")
                            {
                                if (!IsValidMonsterEffectTarget("SpiritTribute", activeMonsterEffectCard, targetCandidate))
                                {
                                    bool isLabyrinthSpirit = activeMonsterEffectCard.GetComponent<ThisCard>().id == 35;
                                    string need = isLabyrinthSpirit ? "one of YOUR monsters" : "one of YOUR matching-attribute monsters";
                                    Debug.Log("Invalid Target: You must select " + need + "!");
                                }
                                else
                                {
                                    CmdGrantWallwalk(activeMonsterEffectCard, targetCandidate.gameObject);
                                    CancelTargeting();
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
    public override void OnStartServer()
    {
        if (connectionToClient == NetworkServer.localConnection)
        {
            ServerSpawnTreasure();
        }
    }

    // The host rolls the starting maze in GridBehavior.Start; a joining
    // client built its grid from the scene's authored reference layout, so it
    // asks the server for the real one here.
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (isClientOnly) CmdRequestMazeLayout();
    }

    GridBehavior FindGridBehavior()
    {
        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        return gridGen != null ? gridGen.GetComponent<GridBehavior>() : null;
    }

    [Command]
    void CmdRequestMazeLayout()
    {
        GridBehavior gb = FindGridBehavior();
        if (gb != null && gb.CurrentServerLayout != null)
            TargetApplyMazeLayout(connectionToClient, gb.CurrentServerLayout);
    }

    [TargetRpc]
    void TargetApplyMazeLayout(NetworkConnection target, int[] layout)
    {
        GridBehavior gb = FindGridBehavior();
        if (gb != null) gb.ApplyMazeLayout(layout);
    }

    // Magical Labyrinth: the server rolls a new interior and every machine
    // (host included) applies it to its own tiles.
    [Command]
    public void CmdRegenerateLabyrinth()
    {
        GridBehavior gb = FindGridBehavior();
        if (gb != null) RpcApplyMazeLayout(gb.RegenerateServerLayout());
    }

    [ClientRpc]
    void RpcApplyMazeLayout(int[] layout)
    {
        GridBehavior gb = FindGridBehavior();
        if (gb != null) gb.ApplyMazeLayout(layout);
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

    bool HasValidTargets(MagicTargetType type)
    {
        if (type == MagicTargetType.None || type == MagicTargetType.EmptySquare) return true;

        LabyrinthObject[] allMonsters = FindObjectsOfType<LabyrinthObject>();

        if (type == MagicTargetType.AnyEnemy || type == MagicTargetType.EnemyAttack || type == MagicTargetType.EnemyDefense)
        {
            foreach (var m in allMonsters)
            {
                if (!m.hasAuthority) 
                {
                    if (type == MagicTargetType.AnyEnemy) return true;
                    if (type == MagicTargetType.EnemyAttack && m.attackMode) return true;
                    if (type == MagicTargetType.EnemyDefense && !m.attackMode) return true;
                }
            }
            return false;
        }

        if (type == MagicTargetType.AnyAlly || type == MagicTargetType.UnmovedAlly)
        {
            foreach (var m in allMonsters)
            {
                if (m.hasAuthority)
                {
                    if (type == MagicTargetType.AnyAlly) return true;
                    if (type == MagicTargetType.UnmovedAlly && !m.hasMovedThisTurn) return true;
                }
            }
            return false;
        }

        if (type == MagicTargetType.AnyUnit) return allMonsters.Length > 0;

        return true;
    }

    public void PlayMagicCard(GameObject card, int index)
    {
        ThisMagic magicScript = card.GetComponent<ThisMagic>();

        if (magicScript != null)
        {
            if (!HasValidTargets(magicScript.targetType))
            {
                Debug.Log("Cannot activate: No valid targets on the board!");
                DragDrop dd = card.GetComponent<DragDrop>();
                if (dd != null) dd.ReturnToHand();
                return;
            }

            CmdPlayCard(card, index);

            card.transform.SetParent(PlayerActionSockets[index].transform, true);
            card.transform.localPosition = new Vector3(0, 1.0f, 0);
            card.transform.localScale = new Vector3(0.01f, 0.0075f, 0.01f);
            card.transform.localRotation = Quaternion.Euler(90, 0, 0);

            if (magicScript.targetType == MagicTargetType.EmptySquare)
            {
                isTargetingTile = true;
                activeMagicCard = card;
                pendingSlotIndex = index;

                GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
                if (gridGen != null)
                {
                    foreach (Transform child in gridGen.transform)
                    {
                        GridStat tile = child.GetComponent<GridStat>();
                        if (tile != null && child.GetComponentInChildren<LabyrinthObject>() == null)
                        {
                            Renderer r = child.GetComponentInChildren<Renderer>();
                            if (r != null) r.material.color = Color.yellow;
                        }
                    }
                }
                Debug.Log("Fatal Square: Select an empty square on the grid!");
                return;
            }

            if (magicScript.targetType != MagicTargetType.None)
            {
                pendingSlotIndex = index;
                StartTargetingMode(card, magicScript.targetType);
                return;
            }

            magicScript.Activate();
        }
        else
        {
            CmdPlayCard(card, index);
        }
    }

    public void PlayActionCard(GameObject card, int index)
    {
        ThisAction actionScript = card.GetComponent<ThisAction>();

        if (actionScript != null)
        {
            CmdPlayCard(card, index);
        }
    }
    void StartTargetingMode(GameObject card, MagicTargetType type)
    {
        isTargeting = true;
        activeMagicCard = card;
        currentTargetCriteria = type;

        // Playing the card from hand is already the deliberate "yes, use this
        // effect" (unlike Shadow Imp/Aetherwing, which piggyback on summoning
        // a monster and never got their own confirmation click), so this only
        // needs the same highlight-valid-targets treatment, not a confirm box.
        foreach (LabyrinthObject candidate in FindObjectsOfType<LabyrinthObject>())
        {
            if (!CheckTargetValidity(card, candidate, type)) continue;
            LabyrinthTile tile = candidate.GetComponentInParent<LabyrinthTile>();
            if (tile != null) tile.GlowBlock();
        }

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

    // Shared validity check for the monster-ability targeting flow (Shadow
    // Imp, Rattlesnake, Frost Wraith, Shy Magician, Spirit tributes). Used
    // both to decide what to highlight the moment targeting starts and to
    // validate the actual click, so the two can never drift apart.
    bool IsValidMonsterEffectTarget(string effect, GameObject effectCard, LabyrinthObject candidate)
    {
        if (candidate == null) return false;

        switch (effect)
        {
            case "ShadowImp":
                {
                    ThisCard targetCard = candidate.card != null ? candidate.card.GetComponent<ThisCard>() : null;
                    return targetCard != null && targetCard.currentAttributes.Contains(Attribute.Dark);
                }
            case "Rattlesnake":
                // "you target one monster on the field" -- no ownership restriction.
                return true;
            case "FrostWraith":
                return IsTileInAnyCardBase(candidate);
            case "ShyMagician":
                {
                    ThisCard targetCard = candidate.card != null ? candidate.card.GetComponent<ThisCard>() : null;
                    return candidate.hasAuthority && targetCard != null && targetCard.currentTypes.Contains(Type.Mage);
                }
            case "SpiritTribute":
                {
                    ThisCard spiritCard = effectCard.GetComponent<ThisCard>();
                    ThisCard targetCard = candidate.card != null ? candidate.card.GetComponent<ThisCard>() : null;
                    bool isLabyrinthSpirit = spiritCard.id == 35;
                    bool attributeMatches = isLabyrinthSpirit
                        || (targetCard != null && spiritCard.currentAttributes.Count > 0 && targetCard.currentAttributes.Contains(spiritCard.currentAttributes[0]));
                    bool isNotSelf = candidate.card != effectCard;
                    return candidate.hasAuthority && targetCard != null && isNotSelf && attributeMatches;
                }
        }
        return false;
    }

    bool HasAnyValidMonsterEffectTarget(string effect, GameObject effectCard)
    {
        foreach (LabyrinthObject candidate in FindObjectsOfType<LabyrinthObject>())
            if (IsValidMonsterEffectTarget(effect, effectCard, candidate)) return true;
        return false;
    }

    // Glows the tile under every currently-valid target green, mirroring the
    // existing movement/attack highlight convention (GlowBlock/RedGlowBlock),
    // so the player sees their legal options up front instead of finding out
    // only after an "Invalid Target" log message.
    void HighlightValidMonsterTargets(string effect, GameObject effectCard)
    {
        foreach (LabyrinthObject candidate in FindObjectsOfType<LabyrinthObject>())
        {
            if (!IsValidMonsterEffectTarget(effect, effectCard, candidate)) continue;
            LabyrinthTile tile = candidate.GetComponentInParent<LabyrinthTile>();
            if (tile != null) tile.GlowBlock();
        }
    }

    public void CancelTargeting()
    {
        isTargeting = false;

        if (activeMagicCard != null)
        {
            CmdPlayerDestroyCard(activeMagicCard, 0);
        }

        // StopGlowBlock (not GridBehavior.ResetTileColors -- that only clears
        // the Quad child, leaving the tile's own renderer stuck green and
        // isHighlighted stuck true, which then skews future hover colors)
        // clears everything GlowBlock touched, and only on tiles this
        // targeting session actually highlighted.
        foreach (LabyrinthTile tile in FindObjectsOfType<LabyrinthTile>())
            if (tile.isHighlighted) tile.StopGlowBlock();

        activeMagicCard = null;
        activeMonsterEffectCard = null;
        pendingMonsterEffect = "";
        pendingSwapFirst = null;
        Debug.Log("Targeting Cancelled. Spell fizzled and went to Graveyard.");
    }

    // Frost Wraith: "You can tribute this card to target one monster in the
    // Card Base and it gains 'Immobile' property." Entry point called from
    // ItemController's right-click context menu on a summoned Frost Wraith.
    public void StartFrostWraithTribute(GameObject frostWraithCard)
    {
        isTargeting = true;
        activeMonsterEffectCard = frostWraithCard;
        pendingMonsterEffect = "FrostWraith";
        HighlightValidMonsterTargets("FrostWraith", frostWraithCard);
        Debug.Log("Frost Wraith: Select a monster in a Card Base to make Immobile!");
    }

    // Matches the summon/tribute-zone rule elsewhere (GridBehavior.cs,
    // ThisCard.IsInsideOwnCardBase()): a Card Base tile is row 0 or row 15,
    // columns 2-8. Frost Wraith's target can be in either player's Card Base.
    bool IsTileInAnyCardBase(LabyrinthObject target)
    {
        if (target == null) return false;
        GridStat tile = target.GetComponentInParent<GridStat>();
        if (tile == null) return false;
        return (tile.y == 0 || tile.y == 15) && tile.x >= 2 && tile.x <= 8;
    }

    [Command]
    public void CmdTributeFrostWraith(GameObject frostWraithCard, GameObject targetMonsterObj)
    {
        LabyrinthObject targetMonster = targetMonsterObj.GetComponent<LabyrinthObject>();
        if (targetMonster == null) return;

        targetMonster.isImmobile = true;
        ThisCard targetCardScript = targetMonster.card != null ? targetMonster.card.GetComponent<ThisCard>() : null;
        if (targetCardScript != null) targetCardScript.isImmobile = true;
        if (targetMonster.card != null) RpcShowCard(targetMonster.card, "SetImmobile", 0);

        Debug.Log("Frost Wraith tributed! " + (targetCardScript != null ? targetCardScript.cardName : targetMonsterObj.name) + " is now Immobile.");

        LabyrinthObject[] allMonsters = FindObjectsOfType<LabyrinthObject>();
        foreach (LabyrinthObject lo in allMonsters)
        {
            if (lo.card == frostWraithCard)
            {
                NetworkServer.Destroy(lo.gameObject);
                break;
            }
        }

        CmdPlayerDestroyCard(frostWraithCard, 0);
    }

    // Water/Fire/Nature/Wind/Labyrinth Spirit: "You can tribute this card to
    // grant another [attribute] monster you control this card's
    // properties." All five share this one implementation -- the elemental
    // four restrict the target to their own attribute (read dynamically off
    // the spirit's own card data), Labyrinth Spirit (id 35) allows any
    // monster. Entry point called from ItemController's right-click context
    // menu on a summoned Spirit.
    public void StartSpiritTribute(GameObject spiritCard)
    {
        isTargeting = true;
        activeMonsterEffectCard = spiritCard;
        pendingMonsterEffect = "SpiritTribute";
        HighlightValidMonsterTargets("SpiritTribute", spiritCard);
        Debug.Log("Select one of your monsters to grant Wallwalk!");
    }

    [Command]
    public void CmdGrantWallwalk(GameObject spiritCard, GameObject targetMonsterObj)
    {
        ThisCard spirit = spiritCard != null ? spiritCard.GetComponent<ThisCard>() : null;
        LabyrinthObject targetMonster = targetMonsterObj != null ? targetMonsterObj.GetComponent<LabyrinthObject>() : null;
        if (spirit == null || targetMonster == null || targetMonster.card == spiritCard) return;

        ThisCard targetCardScript = targetMonster.card != null ? targetMonster.card.GetComponent<ThisCard>() : null;
        if (targetCardScript != null) targetCardScript.grantedWallwalk = true;
        if (targetMonster.card != null) RpcShowCard(targetMonster.card, "GrantWallwalk", 0);

        Debug.Log(spirit.cardName + " tributed! " + (targetCardScript != null ? targetCardScript.cardName : targetMonsterObj.name) + " gained Wallwalk.");

        LabyrinthObject[] allMonsters = FindObjectsOfType<LabyrinthObject>();
        foreach (LabyrinthObject lo in allMonsters)
        {
            if (lo.card == spiritCard)
            {
                NetworkServer.Destroy(lo.gameObject);
                break;
            }
        }

        CmdPlayerDestroyCard(spiritCard, 0);
    }

    // Shy Magician: "Once per duel, you can select two of your Mage-type
    // monsters and those monsters exchange positions on the labyrinth."
    // Entry point called from ItemController's right-click context menu on
    // a summoned, not-yet-used Shy Magician.
    public void StartShyMagicianSwap(GameObject shyMagicianCard)
    {
        isTargeting = true;
        activeMonsterEffectCard = shyMagicianCard;
        pendingMonsterEffect = "ShyMagician";
        pendingSwapFirst = null;
        HighlightValidMonsterTargets("ShyMagician", shyMagicianCard);
        Debug.Log("Shy Magician: Select the first Mage-type monster to swap!");
    }

    [Command]
    public void CmdSwapMonsterPositions(GameObject shyMagicianCard, GameObject monsterAObj, GameObject monsterBObj)
    {
        ThisCard shyCard = shyMagicianCard != null ? shyMagicianCard.GetComponent<ThisCard>() : null;
        if (shyCard == null || shyCard.abilityUsed) return;

        LabyrinthObject a = monsterAObj != null ? monsterAObj.GetComponent<LabyrinthObject>() : null;
        LabyrinthObject b = monsterBObj != null ? monsterBObj.GetComponent<LabyrinthObject>() : null;
        if (a == null || b == null || a == b) return;

        // currentTileName is a SyncVar with a hook (OnTileNameChanged) that
        // reparents/snaps the monster to its new tile on every client, so
        // swapping the strings alone is enough -- no manual reparenting here.
        string tileA = a.currentTileName;
        string tileB = b.currentTileName;
        a.currentTileName = tileB;
        b.currentTileName = tileA;

        shyCard.abilityUsed = true;
        RpcShowCard(shyMagicianCard, "MarkAbilityUsed", 0);

        Debug.Log("Shy Magician: swapped positions of "
            + (a.card != null ? a.card.GetComponent<ThisCard>().cardName : a.name) + " and "
            + (b.card != null ? b.card.GetComponent<ThisCard>().cardName : b.name) + ".");
    }

    // Spooky Man: "When this card is destroyed by battle you can revive one
    // Undead-type monster with less squares than this card from your
    // Graveyard to the same square as this card." Called by
    // LabyrinthObject.CheckSpookyManRevival on the dead monster's OWN
    // PlayerManager, right before it's destroyed.
    // Not [Server]-attributed: Mirror's weaver produces invalid IL for this
    // method signature on this Unity/Mirror combo (same issue hit earlier
    // with OnStartServer/ServerSpawnTreasure after the 2022.3 upgrade), so
    // this guards manually instead.
    public void ServerOfferSpookyManRevival(string deathTileName, int spookyStars)
    {
        if (!Mirror.NetworkServer.active) return;

        bool anyEligible = false;
        foreach (ThisCard c in FindObjectsOfType<ThisCard>())
        {
            if (c.connectionToClient != connectionToClient) continue;
            if (!c.beInGraveyard) continue;
            if (!c.currentTypes.Contains(Type.Undead)) continue;
            if (c.stars >= spookyStars) continue;
            anyEligible = true;
            break;
        }
        if (!anyEligible) return; // nothing to choose from -- no prompt

        TargetChooseSpookyManRevival(connectionToClient, deathTileName, spookyStars);
    }

    // Tells the owning client which death square and star ceiling to offer,
    // then opens their own Graveyard inspector so they can pick exactly
    // which eligible Undead monster to revive (see PopulateGraveyardColumn).
    [TargetRpc]
    void TargetChooseSpookyManRevival(NetworkConnection target, string deathTileName, int spookyStars)
    {
        pendingRevivalTile = deathTileName;
        pendingRevivalMinStars = spookyStars;
        OpenGraveyardInspector();
    }

    [Command]
    public void CmdResolveSpookyManRevival(GameObject reviveCard, string tileName)
    {
        ThisCard cardScript = reviveCard != null ? reviveCard.GetComponent<ThisCard>() : null;
        if (cardScript == null || !cardScript.beInGraveyard) return; // already resolved or invalid

        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        Transform tile = gridGen != null ? gridGen.transform.Find(tileName) : null;
        if (tile != null && tile.GetComponentInChildren<LabyrinthObject>() != null)
        {
            Debug.Log("Spooky Man revival failed: the square is no longer empty.");
            return;
        }

        int freeIndex = -1;
        for (int i = 0; i < PlayerSockets.Count; i++)
        {
            if (PlayerSockets[i] == null) continue;
            bool occupied = false;
            foreach (Transform child in PlayerSockets[i].transform)
            {
                if (child.GetComponent<ThisCard>() != null || child.GetComponent<ThisMagic>() != null || child.GetComponent<ThisAction>() != null)
                { occupied = true; break; }
            }
            if (!occupied) { freeIndex = i; break; }
        }
        if (freeIndex == -1)
        {
            Debug.Log("Spooky Man revival failed: no free monster slot.");
            return;
        }

        cardScript.beInGraveyard = false;
        cardScript.summoned = true;
        cardScript.attackmode = true;

        // Reuses the same cross-client reparent-into-socket logic a normal
        // summon uses, so the revived card appears correctly on both
        // players' screens.
        RpcShowCard(reviveCard, "Played", freeIndex);

        CmdSpawnMonster(cardScript.thisId, tileName, reviveCard.GetComponent<NetworkIdentity>());

        Debug.Log("Spooky Man revived " + cardScript.cardName + "!");
    }

    bool HasValidAetherwingTarget()
    {
        if (EnemyActionSockets == null) return false;

        foreach (GameObject slot in EnemyActionSockets)
        {
            if (slot == null) continue;

            foreach (Transform child in slot.transform)
            {
                ThisMagic tm = child.GetComponent<ThisMagic>();
                if (tm != null && tm.activated) return true;

                ThisAction ta = child.GetComponent<ThisAction>();
                if (ta != null && ta.activated) return true;
            }
        }

        return false;
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

                    if (card.GetComponent<ThisMagic>() != null)
                        card.GetComponent<ThisMagic>().cardBack = false;

                    if (card.GetComponent<ThisAction>() != null)
                    {
                        card.GetComponent<ThisAction>().cardBack = false;
                        CanvasGroup cg = card.GetComponent<CanvasGroup>();
                        if (cg != null) cg.alpha = 1.0f;
                    }
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

                    if (card.GetComponent<ThisMagic>() != null)
                        card.GetComponent<ThisMagic>().cardBack = true;

                    if (card.GetComponent<ThisAction>() != null)
                        card.GetComponent<ThisAction>().cardBack = true;
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
                if (card.GetComponent<ThisAction>() != null) targetSocket = PlayerActionSockets[index].transform;

                CmdGMCardPlayed();
            }
            else
            {
                if (card.GetComponent<ThisMagic>() != null) targetSocket = EnemyActionSockets[index].transform;
                if (card.GetComponent<ThisCard>() != null) targetSocket = EnemySockets[index].transform;
                if (card.GetComponent<ThisAction>() != null) targetSocket = EnemyActionSockets[index].transform;
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
                if (card.GetComponent<ThisAction>() != null)
                {
                    ThisAction ta = card.GetComponent<ThisAction>();
                    ta.activated = false;
                    ta.faceup = false;

                    CanvasGroup cg = card.GetComponent<CanvasGroup>();

                    if (hasAuthority)
                    {
                        ta.cardBack = false;
                        if (cg != null) cg.alpha = 0.5f;
                    }
                    else
                    {
                        ta.cardBack = true;
                        if (cg != null) cg.alpha = 1.0f;
                    }
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
            if (card.GetComponent<ThisAction>() != null) card.GetComponent<ThisAction>().beInGraveyard = true;

            GameObject targetYard = hasAuthority ? EnemyYard : PlayerYard;

            SendCardToGraveyardPile(card, targetYard);
        }
        else if (type == "PlayerDestroyed")
        {
            if (card.GetComponent<ThisCard>() != null)
                card.GetComponent<ThisCard>().beInGraveyard = true;
            if (card.GetComponent<ThisAction>() != null) card.GetComponent<ThisAction>().beInGraveyard = true;

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
        else if (type == "SetImmobile")
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.isImmobile = true;
            }
        }
        else if (type == "MarkAbilityUsed")
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.abilityUsed = true;
            }
        }
        else if (type == "GrantWallwalk")
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.grantedWallwalk = true;
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

            player.honeySnareActive = false; 

            if (player.sprintBoostActive && player.sprintBoostTarget != null)
            {
                LabyrinthObject lo = player.sprintBoostTarget.GetComponent<LabyrinthObject>();
                if (lo != null) lo.moveRange -= 4;
            }
        }

        GridStat[] allTiles = FindObjectsOfType<GridStat>();
        foreach (GridStat tile in allTiles)
        {
            if (tile.isFatalSquare)
            {
                tile.trapDuration--;
                if (tile.trapDuration <= 0)
                {
                    tile.isFatalSquare = false;
                    RpcResetTileColor(tile.gameObject.name);
                }
            }
        }

        RpcGMChangeTurn();

        ThisAction[] allActions = FindObjectsOfType<ThisAction>();
        foreach (ThisAction action in allActions)
        {
            if (action.id == 3 && !action.faceup && !action.beInGraveyard)
            {
                if (action.transform.parent != null && action.transform.parent.name.Contains("ActionSlot"))
                {
                    NetworkIdentity trapIdentity = action.GetComponent<NetworkIdentity>();
                    PlayerManager ownerPM = trapIdentity.connectionToClient.identity.GetComponent<PlayerManager>();

                    if (ownerPM != null)
                    {
                        ownerPM.TargetAskActivateTrap(trapIdentity.connectionToClient, action.gameObject, null, null);
                    }
                }
            }
        }
    }

    [ClientRpc]
    void RpcResetTileColor(string tileName)
    {
        GameObject tileObj = GameObject.Find(tileName);
        if (tileObj != null)
        {
            Renderer r = tileObj.GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = Color.white;
        }
    }

    [ClientRpc]
    public void RpcGMChangeTurn()
    {
        ThisCard[] allCardsOnBoard = FindObjectsOfType<ThisCard>();
        foreach (ThisCard c in allCardsOnBoard)
        {
            c.tempAtk = 0;
            c.tempDef = 0;
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
        ThisCard tc = card.GetComponent<ThisCard>();
        if (tc != null)
        {
            tc.attackmode = ATKDEF;

            LabyrinthObject[] allMonsters = FindObjectsOfType<LabyrinthObject>();
            foreach (LabyrinthObject m in allMonsters)
            {
                if (m.card == card)
                {
                    m.attackMode = ATKDEF;

                    if (ATKDEF && m.hasAuthority && IsMyTurn)
                    {
                        Debug.Log("Switched to Attack! Movement unlocked.");
                    }
                    break;
                }
            }
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

        // A card whose own base property is Immobile (e.g. Labyrinth Shield,
        // Frost Wraith) is bound the instant it's summoned, not just when some
        // other effect grants it the property.
        if (cardScript.cardProperty == Property.Immobile)
        {
            script.isImmobile = true;
            cardScript.isImmobile = true;
        }

        NetworkServer.Spawn(monster, connectionToClient);
        RpcLinkMonsterToCard(monster, cardNetId.gameObject);

        if (script.isImmobile) RpcShowCard(cardNetId.gameObject, "SetImmobile", 0);
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
                // Printed text is "you can target..." -- unlike Rattlesnake
                // (below), this is optional, and the player never got a
                // deliberate activation click the way tribute/equip abilities
                // do (those are opted into by choosing a context-menu item),
                // so it needs its own Activate/Decline prompt before forcing
                // the player into targeting mode.
                GameObject shadowImpCard = tempCard;
                if (HasAnyValidMonsterEffectTarget("ShadowImp", shadowImpCard))
                {
                    SpawnBox("Use Shadow Imp's effect? Target a DARK attribute monster to give it +200 ATK until the end of the turn.", "Activate", "Decline",
                        () =>
                        {
                            Destroy(activeUIBox);
                            isTargeting = true;
                            activeMonsterEffectCard = shadowImpCard;
                            pendingMonsterEffect = "ShadowImp";
                            HighlightValidMonsterTargets("ShadowImp", shadowImpCard);
                            Debug.Log("Targeting Mode Started: Select a Dark Attribute Monster!");
                        },
                        () =>
                        {
                            Destroy(activeUIBox);
                            Debug.Log("Shadow Imp: Effect declined.");
                        }
                    );
                }
                else
                {
                    Debug.Log("Shadow Imp: No Dark attribute monster to target.");
                }
            }

            if (tempCard.GetComponent<ThisCard>().id == 51) // Rattlesnake
            {
                // Printed text is "you target..." (no "can") -- treated as a
                // mandatory on-summon effect, so it skips the Activate/Decline
                // prompt and goes straight to targeting, same as before.
                isTargeting = true;
                activeMonsterEffectCard = tempCard;
                pendingMonsterEffect = "Rattlesnake";
                HighlightValidMonsterTargets("Rattlesnake", tempCard);
                Debug.Log("Rattlesnake: Select a monster to move!");
            }

            if (tempCard.GetComponent<ThisCard>().id == 56) // Aetherwing Butterfly
            {
                // Same reasoning as Shadow Imp: printed text is "you can
                // destroy...", and this on-summon trigger never gave the
                // player a deliberate activation click, so it needs its own
                // Activate/Decline prompt instead of forcing targeting mode.
                GameObject aetherwingCard = tempCard;
                if (HasValidAetherwingTarget())
                {
                    SpawnBox("Use Aetherwing Butterfly's effect? Destroy 1 Magic or Action card your opponent controls.", "Activate", "Decline",
                        () =>
                        {
                            Destroy(activeUIBox);
                            isTargeting = true;
                            activeMonsterEffectCard = aetherwingCard;
                            pendingMonsterEffect = "Aetherwing";
                            Debug.Log("Aetherwing Butterfly: Select an Enemy Magic/Action card to destroy!");
                        },
                        () =>
                        {
                            Destroy(activeUIBox);
                            Debug.Log("Aetherwing Butterfly: Effect declined.");
                        }
                    );
                }
                else
                {
                    Debug.Log("Aetherwing Butterfly: No enemy Magic/Action card to destroy.");
                }
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

            Collider[] cardColliders = card.GetComponentsInChildren<Collider>(true);
            foreach (Collider cardCollider in cardColliders)
            {
                cardCollider.enabled = false;
            }

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

    [System.Serializable]
    public class GraveyardEntry
    {
        public string cardName;
        public string category; // "Monster" | "Spell" | "Action"
        public Sprite icon;
        public GameObject cardObject; // needed so a tile click can resolve back to the actual card (e.g. Spooky Man's revival choice)
    }

    // Spooky Man: which death square is awaiting a revival pick, and the
    // star ceiling a graveyard monster must be under to qualify. Non-null
    // means the graveyard inspector's PlayerColumn is currently offering a
    // revival choice instead of just being a read-only view.
    private string pendingRevivalTile;
    private int pendingRevivalMinStars;

    private List<GraveyardEntry> GatherGraveyardEntries(GameObject yard)
    {
        List<GraveyardEntry> entries = new List<GraveyardEntry>();

        if (yard == null) return entries;
        if (yard.transform.childCount == 0) return entries;

        foreach (Transform child in yard.transform)
        {
            if (child == null) continue;

            ThisCard monster = child.GetComponent<ThisCard>();
            if (monster != null)
            {
                entries.Add(new GraveyardEntry { cardName = monster.cardName, category = "Monster", icon = monster.thisSprite, cardObject = child.gameObject });
                continue;
            }

            ThisMagic spell = child.GetComponent<ThisMagic>();
            if (spell != null)
            {
                entries.Add(new GraveyardEntry { cardName = spell.magicName, category = "Spell", icon = spell.thisSprite, cardObject = child.gameObject });
                continue;
            }

            ThisAction action = child.GetComponent<ThisAction>();
            if (action != null)
            {
                entries.Add(new GraveyardEntry { cardName = action.cardName, category = "Action", icon = action.thisImage, cardObject = child.gameObject });
                continue;
            }
        }

        return entries;
    }

    public void OpenGraveyardInspector()
    {
        List<GraveyardEntry> myGraveyard = GatherGraveyardEntries(PlayerYard);
        List<GraveyardEntry> enemyGraveyard = GatherGraveyardEntries(EnemyYard);

        DisplayGraveyardInspector(myGraveyard, enemyGraveyard);
    }

    private void DisplayGraveyardInspector(List<GraveyardEntry> mine, List<GraveyardEntry> enemy)
    {
        if (GraveyardInspectorPanelPrefab == null || Canvas == null) return;

        if (activeGraveyardPanel != null) Destroy(activeGraveyardPanel);

        activeGraveyardPanel = Instantiate(GraveyardInspectorPanelPrefab, Canvas.transform);
        activeGraveyardPanel.transform.localPosition = Vector3.zero;
        activeGraveyardPanel.transform.localScale = Vector3.one;

        Text playerHeader = activeGraveyardPanel.transform.Find("PlayerColumn/PlayerHeaderText")?.GetComponent<Text>();
        if (playerHeader != null) playerHeader.text = pendingRevivalTile != null ? "Select an Undead monster to revive!" : "Your Graveyard";

        PopulateGraveyardColumn(activeGraveyardPanel.transform.Find("PlayerColumn"), mine, true);
        PopulateGraveyardColumn(activeGraveyardPanel.transform.Find("EnemyColumn"), enemy, false);

        Button closeBtn = activeGraveyardPanel.transform.Find("CloseButton")?.GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() =>
            {
                pendingRevivalTile = null; // closing while a revival choice is pending declines it
                Destroy(activeGraveyardPanel);
                activeGraveyardPanel = null;
            });
        }
    }

    private Color GetGraveyardCategoryColor(string category)
    {
        if (category == "Monster") return new Color(1f, 0.8110068f, 0f);
        if (category == "Spell") return new Color(0.07450981f, 0.5803922f, 0.45098042f);
        if (category == "Action") return new Color(0.5f, 0f, 0.5f);
        return Color.white;
    }

    private void PopulateGraveyardColumn(Transform column, List<GraveyardEntry> entries, bool isPlayerColumn)
    {
        if (column == null) return;

        Transform content = column.Find("ScrollView/Viewport/Content");
        if (content == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        Text emptyText = column.Find("EmptyText")?.GetComponent<Text>();
        if (emptyText != null) emptyText.gameObject.SetActive(entries.Count == 0);

        if (GraveyardCardTilePrefab == null) return;

        bool revivalPending = isPlayerColumn && pendingRevivalTile != null;

        foreach (GraveyardEntry entry in entries)
        {
            GameObject tile = Instantiate(GraveyardCardTilePrefab, content);
            tile.transform.localScale = Vector3.one;

            Image border = tile.GetComponent<Image>();
            if (border != null) border.color = GetGraveyardCategoryColor(entry.category);

            Image icon = tile.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && entry.icon != null) icon.sprite = entry.icon;

            Text label = tile.transform.Find("NameLabel")?.GetComponent<Text>();
            if (label != null) label.text = entry.cardName;

            if (!revivalPending) continue;

            ThisCard tc = entry.cardObject != null ? entry.cardObject.GetComponent<ThisCard>() : null;
            bool eligible = tc != null && tc.currentTypes.Contains(Type.Undead) && tc.stars < pendingRevivalMinStars;

            if (eligible)
            {
                GameObject capturedCard = entry.cardObject;
                Button btn = tile.GetComponent<Button>();
                if (btn == null) btn = tile.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    string tileName = pendingRevivalTile;
                    pendingRevivalTile = null;
                    CmdResolveSpookyManRevival(capturedCard, tileName);
                    Destroy(activeGraveyardPanel);
                    activeGraveyardPanel = null;
                });
            }
            else
            {
                // Dim tiles that don't qualify so the eligible ones read as clickable.
                if (border != null) { Color c = border.color; c.a *= 0.3f; border.color = c; }
                if (icon != null) { Color c = icon.color; c.a *= 0.35f; icon.color = c; }
                if (label != null) { Color c = label.color; c.a *= 0.4f; label.color = c; }
            }
        }
    }

    public void ServerSpawnTreasure()
    {
        if (!NetworkServer.active) return;

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

    // Not [Server]-attributed: Mirror's weaver produces invalid IL for this
    // method on this Unity/Mirror combo once its body references the new
    // SpecialSummonOrDrawLabyrinthMonster coroutine (same class of issue hit
    // earlier with OnStartServer/ServerSpawnTreasure and
    // ServerOfferSpookyManRevival), so this guards manually instead.
    public void ServerCollectTreasure(GameObject chest)
    {
        if (!NetworkServer.active) return;

        GridStat tileStat = chest.GetComponentInParent<GridStat>();
        LabyrinthObject collector = (tileStat != null) ? tileStat.GetComponentInChildren<LabyrinthObject>() : null;

        NetworkServer.Destroy(chest);

        List<KeyValuePair<string, int>> labyrinthPool = new List<KeyValuePair<string, int>>();
        for (int i = 1; i < CardDataBase.cardList.Count; i++)
            if (CardDataBase.cardList[i].type == Type.Labyrinth) labyrinthPool.Add(new KeyValuePair<string, int>("Monster", i));

        for (int i = 0; i < MagicDataBase.magicList.Count; i++)
            if (MagicDataBase.magicList[i].magicType == MagicType.Labyrinth) labyrinthPool.Add(new KeyValuePair<string, int>("Magic", i));

        if (labyrinthPool.Count > 0)
        {
            var choice = labyrinthPool[Random.Range(0, labyrinthPool.Count)];

            if (choice.Key == "Magic" && choice.Value == 15) // Labyrinth Love
            {
                StartCoroutine(AutoplayLabyrinthLove(choice.Value, collector));
                return;
            }

            if (choice.Key == "Monster") StartCoroutine(SpecialSummonOrDrawLabyrinthMonster(choice.Value));
            else StartCoroutine(DrawSpecificMagic(choice.Value));
        }
    }
    // Every Type.Labyrinth monster (Labyrinth Minotaur, Labyrinth Shield, ...)
    // shares this same printed effect: "Once picked up: Special Summon this
    // card to your base. If you do not have an open zone add it to your
    // hand instead." Picked up here means drawn from the treasure-chest
    // labyrinthPool above, not a normal draw.
    IEnumerator SpecialSummonOrDrawLabyrinthMonster(int cardId)
    {
        yield return new WaitForSeconds(0.5f); // matches DrawSpecificMagic's pacing

        GameObject cardObj = Instantiate(Card, Vector2.zero, Quaternion.identity);
        ThisCard cardScript = cardObj.GetComponent<ThisCard>();
        cardScript.thisId = cardId;
        NetworkServer.Spawn(cardObj, connectionToClient);

        // ThisCard.Update() populates stars/cardProperty/etc. from thisId on
        // its own next Update() tick, not synchronously -- CmdSpawnMonster
        // reads those fields, so give it a moment before using them.
        yield return new WaitForSeconds(0.1f);

        // Card Base row matches the summon-placement/tribute rule used
        // elsewhere (GridBehavior.cs, ThisCard.IsInsideOwnCardBase()): the
        // host's own base is row 0, the joining client's is row 15.
        bool ownerIsHost = (hasAuthority == NetworkServer.active);
        int homeRow = ownerIsHost ? 0 : 15;

        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        string openTileName = null;
        if (gridGen != null)
        {
            for (int col = 2; col <= 8 && openTileName == null; col++)
            {
                foreach (Transform child in gridGen.transform)
                {
                    GridStat stat = child.GetComponent<GridStat>();
                    if (stat != null && stat.x == col && stat.y == homeRow && child.GetComponentInChildren<LabyrinthObject>() == null)
                    {
                        openTileName = child.name;
                        break;
                    }
                }
            }
        }

        int freeSlotIndex = -1;
        if (openTileName != null)
        {
            for (int i = 0; i < PlayerSockets.Count; i++)
            {
                if (PlayerSockets[i] == null) continue;
                bool occupied = false;
                foreach (Transform child in PlayerSockets[i].transform)
                {
                    if (child.GetComponent<ThisCard>() != null || child.GetComponent<ThisMagic>() != null || child.GetComponent<ThisAction>() != null)
                    { occupied = true; break; }
                }
                if (!occupied) { freeSlotIndex = i; break; }
            }
        }

        if (openTileName != null && freeSlotIndex != -1)
        {
            cardScript.summoned = true;
            cardScript.attackmode = true;

            RpcShowCard(cardObj, "Played", freeSlotIndex);
            CmdSpawnMonster(cardId, openTileName, cardObj.GetComponent<NetworkIdentity>());

            Debug.Log("Labyrinth pickup: Special Summoned " + cardScript.cardName + " to the Card Base!");
        }
        else
        {
            RpcShowCard(cardObj, "Dealt", 0);
            Debug.Log("Labyrinth pickup: No open zone, added " + cardScript.cardName + " to hand.");
        }
    }

    IEnumerator DrawSpecificMagic(int magicId)
    {
        yield return new WaitForSeconds(0.5f);
        GameObject magicObj = Instantiate(Magic, Vector2.zero, Quaternion.identity);
        magicObj.GetComponent<ThisMagic>().thisId = magicId;
        NetworkServer.Spawn(magicObj, connectionToClient);
        RpcShowCard(magicObj, "Dealt", 0);
    }

    IEnumerator AutoplayLabyrinthLove(int magicId, LabyrinthObject collector)
    {
        GameObject magicObj = Instantiate(Magic, Vector2.zero, Quaternion.identity);
        ThisMagic magicScript = magicObj.GetComponent<ThisMagic>();
        magicScript.thisId = magicId;
        NetworkServer.Spawn(magicObj, connectionToClient);

        int targetSlotIndex = 0;
        for (int i = 0; i < PlayerActionSockets.Count; i++)
        {
            if (PlayerActionSockets[i].transform.childCount == 0)
            {
                targetSlotIndex = i;
                break;
            }
        }

        RpcShowCard(magicObj, "Played", targetSlotIndex);

        yield return new WaitForSeconds(1.5f);

        if (collector != null)
        {
            ThisCard stats = collector.card.GetComponent<ThisCard>();
            int gainAmount = Mathf.Max(stats.actualATK, stats.actualDEF);
            RpcGMChangeLP(-gainAmount, 0);
            Debug.Log($"Autoplay: Labyrinth Love healed {gainAmount}!");
        }

        yield return new WaitForSeconds(0.5f);

        RpcShowCard(magicObj, "PlayerDestroyed", 0);
    }
    [Command]
    public void CmdExecuteMagicEffect(GameObject magicCard, GameObject targetMonster, int slotIndex)
    {
        ThisMagic magicScript = magicCard.GetComponent<ThisMagic>();
        LabyrinthObject monsterScript = targetMonster.GetComponent<LabyrinthObject>();

        if (magicScript == null || monsterScript == null) return;
        magicScript.lastTargetedMonster = monsterScript.card;

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
                if (magicScript.id == 9) // Exhaust
                {
                    RpcApplyTempAtk(monsterScript.card, -400);
                    Debug.Log($"Exhaust Activated! {targetMonster.name} loses 400 ATK until the end of the turn.");
                }
                else
                {
                    NetworkServer.Destroy(targetMonster);
                    RpcShowCard(monsterScript.card, "OpponentDestroyed", 0);
                }
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

                            thisCardScript.equippedTo = magicCard;
                            magicScript.equippedTo = monsterCardObj;

                            NetworkIdentity monsterNi = monsterCardObj.GetComponent<NetworkIdentity>();
                            Debug.Log("[CmdExecuteMagicEffect] Equipped " + magicScript.magicName + " to " + monsterCardObj.name
                                + " (netId=" + (monsterNi != null ? monsterNi.netId.ToString() : "none") + ", cardName=" + thisCardScript.cardName + ")"
                                + " | target.summoned=" + thisCardScript.summoned + " target.beInGraveyard=" + thisCardScript.beInGraveyard
                                + " | magicScript.canBeDestroyed=" + magicScript.canBeDestroyed + " magicScript.activated=" + magicScript.activated + " magicScript.activationcomplete=" + magicScript.activationcomplete + " ThisMagic.drawX=" + ThisMagic.drawX);

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

                    if (thisCardScript != null) thisCardScript.equippedTo = magicCard;
                    magicScript.equippedTo = monsterCardObj;

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

        if (!magicScript.equip)
        {
            CmdPlayerDestroyCard(magicCard, 0);
        }
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

        if (activeMagicCard != null)
        {
            CmdPlayerDestroyCard(activeMagicCard, 0);
        }

        activeMagicCard = null;

        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        if (gridGen != null)
        {
            gridGen.GetComponent<GridBehavior>().ResetTileColors();
        }
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
        bool smallogIsOnField = false;
        int plantGraveyardCount = 0;

        foreach (var magic in allMagics)
        {
            if (magic.hasAuthority && magic.activated && !magic.beInGraveyard) myCardCount++;
        }

        foreach (var m in allMonsters)
        {
            if (m.hasAuthority) myCardCount++;
            if (m.monsterID == 34) smallogIsOnField = true; // Check for Smallog
        }

        foreach (ThisCard card in allCards)
        {
            if (card.beInGraveyard && card.currentTypes.Contains(Type.Plant))
            {
                plantGraveyardCount++;
            }
        }

        foreach (var m in allMonsters)
        {
            if (!m.hasAuthority || m.card == null) continue;

            ThisCard tc = m.card.GetComponent<ThisCard>();
            if (tc == null) continue;

            int targetAuraAtk = tc.auraAtk;
            int targetAuraDef = tc.auraDef;
            bool hasAuraEffect = false;

            if (m.monsterID == 57) // Snalien
            {
                targetAuraAtk = (myCardCount == 1) ? 400 : 0;
                targetAuraDef = (myCardCount == 1) ? 200 : 0;
                hasAuraEffect = true;
            }
            else if (m.monsterID == 25) // Thorn Fairy
            {
                targetAuraAtk = plantGraveyardCount * 200;
                targetAuraDef = 0;
                hasAuraEffect = true;
            }
            else if (m.monsterID == 33) // Logigas
            {
                targetAuraAtk = smallogIsOnField ? 500 : 0;
                targetAuraDef = smallogIsOnField ? 500 : 0;
                hasAuraEffect = true;
            }

            if (hasAuraEffect && (tc.auraAtk != targetAuraAtk || tc.auraDef != targetAuraDef))
            {
                tc.auraAtk = targetAuraAtk;
                tc.auraDef = targetAuraDef;
                CmdUpdateAuraStats(m.card, targetAuraAtk, targetAuraDef);
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

    // Takes the new ABSOLUTE tempDef value, not a delta -- ClientRpc delivery
    // (even to the host's own local client) is queued to the next network
    // tick, not synchronous, so the caller already applies this same value
    // directly on the server's own object first (RpcApplyDemonLadyShred's
    // battleDefPenalty follows the same idempotent-absolute-set pattern for
    // the same reason). Using a delta here would double-count once this
    // eventually runs redundantly against that already-updated object.
    [ClientRpc]
    public void RpcApplyTempDef(GameObject card, int newTempDef)
    {
        if (card != null)
        {
            ThisCard tc = card.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.tempDef = newTempDef;
                tc.RecalculateStats();
            }
        }
    }

    [Command]
    public void CmdApplyPlagueDebuff(GameObject targetCard)
    {
        RpcApplyPlagueDebuff(targetCard);
    }

    [ClientRpc]
    public void RpcApplyPlagueDebuff(GameObject targetCard)
    {
        if (targetCard != null)
        {
            ThisCard tc = targetCard.GetComponent<ThisCard>();
            if (tc != null)
            {
                tc.plagueAtkLoss += 500;
                tc.plagueDefLoss += 500;

                Debug.Log($"{tc.cardName} was infected by the Plague and permanently lost 500 ATK & DEF!");
            }
        }
    }

    [Command]
    public void CmdForceMoveMonster(GameObject monsterToMove, string targetTileName)
    {
        LabyrinthObject monsterScript = monsterToMove.GetComponent<LabyrinthObject>();
        if (monsterScript != null)
        {
            monsterScript.currentTileName = targetTileName;
            monsterScript.hasMovedThisTurn = true;

            Debug.Log($"[SERVER] Rattlesnake forced {monsterToMove.name} to move to {targetTileName}");
        }
    }

    [ClientRpc]
    public void RpcApplyDemonLadyShred(GameObject targetCard)
    {
        if (targetCard != null)
        {
            ThisCard tc = targetCard.GetComponent<ThisCard>();
            if (tc != null) tc.battleDefPenalty = 500;
        }
    }

    [ClientRpc]
    public void RpcResetDemonLadyShred(GameObject targetCard)
    {
        if (targetCard != null)
        {
            ThisCard tc = targetCard.GetComponent<ThisCard>();
            if (tc != null) tc.battleDefPenalty = 0;
        }
    }

    [Command]
    public void CmdSetFatalSquare(string tileName)
    {
        GameObject tileObj = GameObject.Find(tileName);

        if (tileObj != null)
        {
            GridStat gs = tileObj.GetComponent<GridStat>();
            if (gs != null)
            {
                gs.isFatalSquare = true;
                gs.trapDuration = 2;
                RpcShowTrapEffect(tileName);
            }
        }
    }

    [ClientRpc]
    void RpcShowTrapEffect(string tileName)
    {
        GameObject tileObj = GameObject.Find(tileName);
        if (tileObj != null)
        {
            Renderer r = tileObj.GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = new Color(0.5f, 0, 0.5f);
        }
    }

    [TargetRpc]
    public void TargetAskActivateTrap(NetworkConnection target, GameObject trapCard, GameObject attacker, GameObject defender)
    {
        string trapName = trapCard.GetComponent<ThisAction>().cardName;

        if (trapCard.GetComponent<ThisAction>().id == 1 && PlayerArea.transform.childCount == 0)
        {
            Debug.Log("Cannot activate Echo of Silence: Hand is empty!");
            CmdAnswerTrapPrompt(false, trapCard, attacker, defender);
            return;
        }

        string msg = (attacker != null) ? $"Opponent is attacking! Activate {trapName}?" : $"Start of turn! Activate {trapName}?";

        SpawnBox(msg, "Activate", "Decline",
            () =>
            {
                Destroy(activeUIBox);
                CmdAnswerTrapPrompt(true, trapCard, attacker, defender);
            },
            () =>
            {
                Destroy(activeUIBox);
                CmdAnswerTrapPrompt(false, trapCard, attacker, defender);
            }
        );
    }

    [Command]
    public void CmdAnswerTrapPrompt(bool wantsToActivate, GameObject trapCard, GameObject attacker, GameObject defender)
    {
        if (wantsToActivate)
        {
            ThisAction actionScript = trapCard.GetComponent<ThisAction>();

            // Echo of Silence's cost (discard 1 card) hasn't been paid yet at
            // this point -- CmdResolveEchoOfSilence reveals/plays the trap
            // once the discard is actually chosen. Doing it here too would
            // fire RpcShowCard("Played")/CmdGMCardPlayed() twice for a single
            // activation, double-incrementing GameManager.TurnOrder, and
            // would leave the trap stuck marked as played/beInGraveyard if
            // the player cancels the discard prompt instead of completing it.
            if (actionScript.id == 1) // Echo of Silence
            {
                TargetStartDiscardForEcho(connectionToClient, trapCard, attacker, defender);
                return;
            }

            RpcShowCard(trapCard, "Played", 0);
            actionScript.faceup = true;
            actionScript.beInGraveyard = true;

            if (actionScript.id == 4) // Last Stand Barrier
            {
                GameObject defenderCardObj = defender.GetComponent<LabyrinthObject>().card;
                ThisCard defenderCard = defenderCardObj.GetComponent<ThisCard>();
                RpcGMChangeBattlePosition(defenderCardObj, false);

                // "...gains DEF equal to its ATK until the end of this turn"
                // -- tempDef persists (reset alongside tempAtk in
                // RpcGMChangeTurn) so a second attack against this same
                // monster later in the turn still sees the DEF boost, not
                // just the single battle that triggered the trap.
                //
                // Applied directly on the server's own object (not just via
                // the RPC below) because the ServerResolveAttack call right
                // after this needs actualDEF to already reflect the boost --
                // ClientRpc delivery is queued to the next network tick even
                // for the host's own local client, so it wouldn't be visible
                // in time for this same battle.
                int newTempDef = defenderCard.tempDef + defenderCard.actualATK;
                defenderCard.tempDef = newTempDef;
                defenderCard.RecalculateStats();
                RpcApplyTempDef(defenderCardObj, newTempDef);
            }
            else if (actionScript.id == 3) // Honey Snare
            {
                Debug.Log("Honey Snare Activated! Locking down the board.");
                this.honeySnareActive = true;
            }

            if (attacker != null && defender != null)
            {
                attacker.GetComponent<LabyrinthObject>().ServerResolveAttack(defender, true, actionScript.id);
            }

            CmdPlayerDestroyCard(trapCard, 0);
        }
        else
        {
            Debug.Log("Player declined to use the trap.");
            if (attacker != null && defender != null)
            {
                attacker.GetComponent<LabyrinthObject>().ServerResolveAttack(defender, false, 0);
            }
        }
    }

    [TargetRpc]
    public void TargetStartDiscardForEcho(NetworkConnection target, GameObject trap, GameObject attacker, GameObject defender)
    {
        isTargetingDiscard = true;
        pendingTrap = trap;
        pendingAttacker = attacker;
        pendingDefender = defender;
        Debug.Log("Echo of Silence: Select a card in your hand to discard!");
    }

    [Command]
    public void CmdResolveEchoOfSilence(GameObject discardedCard, GameObject trapCard, GameObject attacker, GameObject defender)
    {
        if (discardedCard.GetComponent<ThisCard>() != null) discardedCard.GetComponent<ThisCard>().beInGraveyard = true;
        if (discardedCard.GetComponent<ThisMagic>() != null) discardedCard.GetComponent<ThisMagic>().beInGraveyard = true;
        if (discardedCard.GetComponent<ThisAction>() != null) discardedCard.GetComponent<ThisAction>().beInGraveyard = true;

        RpcShowCard(discardedCard, "PlayerDestroyed", 0);

        RpcShowCard(trapCard, "Played", 0);
        ThisAction actionScript = trapCard.GetComponent<ThisAction>();
        if (actionScript != null)
        {
            actionScript.faceup = true;
            actionScript.beInGraveyard = true;
        }
        CmdPlayerDestroyCard(trapCard, 0);

        if (attacker != null && defender != null)
        {
            attacker.GetComponent<LabyrinthObject>().ServerResolveAttack(defender, true, 1);
        }
    }
}
