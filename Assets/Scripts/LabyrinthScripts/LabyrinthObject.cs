using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class LabyrinthObject : NetworkBehaviour
{
    public GameObject gridGenerator;
    public GameObject card;

    [SyncVar(hook = nameof(OnMonsterIDChanged))]
    public int monsterID;

    [SyncVar(hook = nameof(OnTileNameChanged))]
    public string currentTileName;
    public GameObject current3DModel;
    [SyncVar]
    public int moveRange;
    [SyncVar]
    public bool isImmobile;
    [SyncVar]
    public bool attackMode = true;
    [SyncVar]
    public int turnSummoned = 0;

    public bool waitingToAttack = false;

    // Server only: an attack by this monster is paused while the defender
    // decides on a trap. Makes sure the answer resolves the battle only once.
    [System.NonSerialized] public bool awaitingTrapAnswer = false;

    // While any attack waits on a trap answer, no other attack (or End Turn)
    // is accepted, so battles can't pile up on the same target.
    public static bool AnyAttackAwaitingTrap()
    {
        foreach (LabyrinthObject m in FindObjectsOfType<LabyrinthObject>())
            if (m.awaitingTrapAnswer) return true;
        return false;
    }

    // Server: the trap question has been answered; resume and drop the
    // attacker's "waiting" message.
    public void ServerEndTrapPause()
    {
        awaitingTrapAnswer = false;
        if (connectionToClient != null && connectionToClient.identity != null)
        {
            PlayerManager owner = connectionToClient.identity.GetComponent<PlayerManager>();
            if (owner != null) owner.TargetShowWaitingForTrap(connectionToClient, false);
        }
    }

    [SyncVar]
    public bool hasMovedThisTurn = false;
    [SyncVar]
    public bool hasAttackedThisTurn = false;
    public float targetWorldSize = 0.5f;

    public override void OnStartClient()
    {
        base.OnStartClient();
        gridGenerator = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

        if (monsterID != 0) OnMonsterIDChanged(0, monsterID);
        if (!string.IsNullOrEmpty(currentTileName)) StartCoroutine(WaitForTileAndSnap(currentTileName));
    }

    // Which monster id the current model was built for: this can be called
    // more than once for the same id (spawn + hook), and rebuilding the model
    // then would cut its drop-in animation short.
    int builtModelId;

    void OnMonsterIDChanged(int oldID, int newID)
    {
        if (current3DModel != null && builtModelId == newID) return;

        Card cardData = newID > 0 ? CardDataBase.GetCard(newID) : null;
        if (cardData != null)
        {

            if (current3DModel != null) Destroy(current3DModel);
            builtModelId = newID;

            if (cardData.modelPrefab != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;

                current3DModel = Instantiate(cardData.modelPrefab, this.transform);

                current3DModel.transform.localPosition = new Vector3(0f, 0f, -0.45f);
                current3DModel.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                current3DModel.transform.localScale = Vector3.one;

                SetLayerRecursively(current3DModel, 2);

                AdjustSize();
            }
            else
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = true;
                    sr.sprite = cardData.thisImage;
                    AdjustSize();
                }
            }
        }
    }

    void OnTileNameChanged(string oldName, string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            StartCoroutine(WaitForTileAndSnap(newName));
        }
    }

    IEnumerator WaitForTileAndSnap(string tileName)
    {
        if (gridGenerator == null)
            gridGenerator = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

        Transform targetTileTransform = null;
        float timeout = 2.0f;

        while (targetTileTransform == null && timeout > 0)
        {
            if (gridGenerator != null)
            {
                foreach (Transform child in gridGenerator.transform)
                {
                    if (child.name == tileName && child.GetComponent<GridStat>() != null)
                    {
                        targetTileTransform = child;
                        break;
                    }
                }
            }

            if (targetTileTransform == null)
            {
                yield return null;
                timeout -= Time.deltaTime;
                if (gridGenerator == null)
                    gridGenerator = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
            }
        }

        if (targetTileTransform != null)
        {
            // Where the model was drawn before this snap: a monster already on
            // a square hops to the new one; a brand-new one pops in.
            Transform previousTile = transform.parent;
            bool wasOnBoard = previousTile != null && previousTile.GetComponent<GridStat>() != null;
            Vector3 modelWasAt = current3DModel != null ? current3DModel.transform.position : transform.position;

            transform.SetParent(targetTileTransform, true);
            transform.localPosition = new Vector3(0, 0.55f, 0);

            GridStat newTile = targetTileTransform.GetComponent<GridStat>();
            if (newTile != null && newTile.isFatalSquare)
            {
                Debug.Log("LANDED ON A FATAL SQUARE!");
                if (isServer)
                {
                    newTile.isFatalSquare = false;
                    // "PlayerDestroyed" goes to the sending player's yard, so send
                    // through the monster's owner, not whoever is hosting.
                    NetworkConnection owner = connectionToClient ?? NetworkServer.localConnection;
                    PlayerManager pm = owner.identity.GetComponent<PlayerManager>();
                    pm.RpcShowCard(this.card, "PlayerDestroyed", 0);
                    pm.RpcResetTileColor(targetTileTransform.name); // the trap is used up
                    ServerDestroyWithVanish();
                    yield break;
                }
            }

            bool isClientMonster = (hasAuthority && !isServer) || (!hasAuthority && isServer);

            if (isClientMonster)
            {
                transform.localRotation = Quaternion.Euler(90, 180, 0);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

            AdjustSize();

            // A repeat snap to the same square (a new monster is snapped twice
            // on arrival) must not cut its drop-in short.
            bool actuallyMoved = !wasOnBoard || previousTile != targetTileTransform;
            if (current3DModel != null && actuallyMoved)
            {
                if (moveEffect != null) StopCoroutine(moveEffect);
                current3DModel.transform.localPosition = ModelRestPosition;
                current3DModel.transform.localScale = Vector3.one;
                moveEffect = StartCoroutine(wasOnBoard ? HopModel(modelWasAt) : PopInModel());
            }

            yield return new WaitForEndOfFrame();

            if (hasAuthority && hasMovedThisTurn)
            {
                CheckSurroundingsForEnemies();
            }
        }
    }

    // ---- board effects (purely visual) ----

    static readonly Vector3 ModelRestPosition = new Vector3(0f, 0f, -0.45f); // as set in OnMonsterIDChanged
    Coroutine moveEffect;

    // A newly summoned monster drops onto its square from above, growing as it
    // falls, then lands with a bounce.
    IEnumerator PopInModel()
    {
        Transform model = current3DModel != null ? current3DModel.transform : null;
        const float DropTime = 0.35f;
        const float LandTime = 0.25f;
        const float DropHeight = 1.5f;

        for (float t = 0f; t < DropTime && model != null; t += Time.deltaTime)
        {
            float k = t / DropTime;
            Vector3 rest = model.parent.TransformPoint(ModelRestPosition);
            model.position = rest + Vector3.up * DropHeight * (1f - k * k); // falls faster as it goes
            model.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, k);
            yield return null;
        }
        if (model != null) model.localPosition = ModelRestPosition;
        if (model != null) SoundFX.Play("summon");

        for (float t = 0f; t < LandTime && model != null; t += Time.deltaTime)
        {
            float k = t / LandTime;
            model.localScale = Vector3.one * (1f + 0.3f * Mathf.Sin(Mathf.PI * k) * (1f - k)); // landing bump
            yield return null;
        }
        if (model != null) model.localScale = Vector3.one;
        moveEffect = null;
    }

    // A monster that changed squares hops there in an arc instead of jumping.
    IEnumerator HopModel(Vector3 from)
    {
        Transform model = current3DModel != null ? current3DModel.transform : null;
        if (model == null) yield break;
        Vector3 to = model.parent.TransformPoint(ModelRestPosition);
        float distance = Vector3.Distance(from, to);
        if (distance < 0.01f) { moveEffect = null; yield break; }
        float time = Mathf.Clamp(0.15f + distance * 0.06f, 0.2f, 0.5f);
        SoundFX.Play("hop", 0.7f);
        float height = Mathf.Clamp(distance * 0.3f, 0.2f, 1.2f);
        for (float t = 0f; t < time && model != null; t += Time.deltaTime)
        {
            float k = t / time;
            to = model.parent.TransformPoint(ModelRestPosition);
            model.position = Vector3.Lerp(from, to, k) + Vector3.up * height * 4f * k * (1f - k);
            yield return null;
        }
        if (model != null) model.localPosition = ModelRestPosition;
        moveEffect = null;
    }

    // Server: remove this monster with a spin-and-shrink on every screen.
    // For effects (spells, traps, tributes, fire); battles have their own animation.
    public void ServerDestroyWithVanish()
    {
        if (!NetworkServer.active) return;
        if (NetworkClient.active) BattleAnimation.PlayVanish(this);
        RpcPlayVanish();
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcPlayVanish()
    {
        if (isServer) return; // the host already played it
        BattleAnimation.PlayVanish(this);
    }

    void CheckSurroundingsForEnemies()
    {
        if (gridGenerator != null)
        {
            GridBehavior gb = gridGenerator.GetComponent<GridBehavior>();
            if (gb != null)
            {
                bool enemiesFound = gb.HighlightAttackOptions(this);

                if (enemiesFound)
                {
                    waitingToAttack = true;
                    Debug.Log("Enemies found! Waiting for attack...");
                }
                else
                {
                    waitingToAttack = false;
                    ResetGridColors();
                }
            }
        }
    }

    void AdjustSize()
    {
        BoxCollider myCollider = GetComponent<BoxCollider>();

        if (current3DModel != null)
        {
            transform.localScale = Vector3.one;

            if (transform.parent != null)
            {
                Vector3 worldScale = transform.lossyScale;

                transform.localScale = new Vector3(
                    1f / worldScale.x,
                    1f / worldScale.y,
                    1f / worldScale.z
                );
            }

            if (myCollider != null)
            {
                myCollider.size = new Vector3(0.8f, 0.5f, 0.8f);
                myCollider.center = new Vector3(0, 0.25f, 0);
            }
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        if (transform.parent == null) return;

        Vector3 spriteSize = sr.sprite.bounds.size;
        float maxSpriteDimension = Mathf.Max(spriteSize.x, spriteSize.y);
        float parentScale = transform.parent.lossyScale.x;

        if (parentScale == 0 || maxSpriteDimension == 0) return;

        float finalScale = targetWorldSize / (parentScale * maxSpriteDimension);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        if (myCollider != null)
        {
            myCollider.size = new Vector3(spriteSize.x, spriteSize.y * 0.5f, 1f);
            myCollider.center = new Vector3(0, -spriteSize.y * 0.25f, 0);
        }
    }

    // Hovering a monster on the board shows its card in the zoom panel, the
    // same as hovering the card itself (opponent's monsters are face-up once
    // on the field, so their cards show too).
    void OnMouseEnter()
    {
        if (card == null) return;
        CardZoom zoom = card.GetComponent<CardZoom>();
        if (zoom != null) zoom.OnHoverEnter();
    }

    public void ObjectToMove()
    {
        if (!hasAuthority) return;
        if (NetworkClient.connection.identity == null) return;

        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        if (pm == null || !pm.IsMyTurn || GameManager.IsGameOver || PlayerManager.WaitingForTrapAnswer) return;

        if (hasMovedThisTurn) return;

        if (isImmobile)
        {
            Debug.Log("This monster is permanently bound and cannot move!");
            return;
        }

        PlayerManager[] allPlayers = FindObjectsOfType<PlayerManager>();
        foreach (PlayerManager player in allPlayers)
        {
            if (player.honeySnareActive)
            {
                Debug.Log("Honey Snare is active! The board is stuck in honey this turn.");
                return;
            }
        }

        if (!attackMode)
        {
            Debug.Log("Cannot move or attack in Defense Mode!");
            return;
        }

        if (gridGenerator == null) gridGenerator = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

        if (gridGenerator != null)
            gridGenerator.GetComponent<GridBehavior>().ShowPossiblePaths(gameObject);
    }

    [Command]
    public void CmdMoveToTile(string tileName)
    {
        ServerTryMove(tileName);
    }

    // Server-side versions of a player's monster actions (the commands, and
    // the CPU player). Same rule checks either way.
    public void ServerTryMove(string tileName)
    {
        if (!NetworkServer.active) return;
        // A normal move by the owner: once per turn, on their own turn, never
        // into the opponent's Card Base. (Effects that move monsters call
        // ServerMoveToTile directly and skip these checks.)
        if (GameManager.IsGameOver || hasMovedThisTurn || isImmobile) return;
        if (AnyAttackAwaitingTrap()) { Debug.Log("[SERVER] Move rejected: an attack is waiting on a trap answer."); return; }
        foreach (PlayerManager p in FindObjectsOfType<PlayerManager>())
            if (p.honeySnareActive) { Debug.Log("[SERVER] Move rejected: Honey Snare is active."); return; }
        bool ownerIsHost = connectionToClient == null || connectionToClient == NetworkServer.localConnection;
        if (ownerIsHost != PlayerManager.ServerHostHasTurn) { Debug.Log("[SERVER] Move rejected: not this player's turn."); return; }

        GridBehavior gb = FindGrid();
        Transform tile = gb != null ? gb.transform.Find(tileName) : null;
        GridStat stat = tile != null ? tile.GetComponent<GridStat>() : null;
        if (stat == null) return;
        if (gb.IsEnemyBaseSquare(this, stat.x, stat.y)) { Debug.Log("[SERVER] Move rejected: can't enter the opponent's Card Base."); return; }

        ServerMoveToTile(tileName);
    }

    // Server-side move, for effects that move a monster from inside another
    // command (Teleport, Labyrinth Dice): a Cmd can't be called from the
    // server for a monster the joining player owns. Not [Server]-attributed
    // (see the weaver note on ServerResolveAttack).
    public void ServerMoveToTile(string tileName)
    {
        if (!NetworkServer.active) return;

        currentTileName = tileName;
        hasMovedThisTurn = true;

        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        if (gridGen != null)
        {
            Transform tile = gridGen.transform.Find(tileName);
            if (tile != null)
            {
                TreasureChest chest = tile.GetComponentInChildren<TreasureChest>();

                if (chest != null && connectionToClient != null && connectionToClient.identity != null)
                {
                    PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
                    if (pm != null)
                    {
                        pm.ServerCollectTreasure(chest.gameObject);
                    }
                }
            }
        }
    }

    [Command]
    public void CmdAttackMonster(GameObject targetObj)
    {
        ServerTryAttack(targetObj);
    }

    public void ServerTryAttack(GameObject targetObj)
    {
        if (!NetworkServer.active) return;
        LabyrinthObject targetScript = targetObj != null ? targetObj.GetComponent<LabyrinthObject>() : null;
        if (targetScript == null) return;
        if (!ServerMayAttack()) return;

        GridBehavior gb = FindGrid();
        if (gb == null || !gb.CanAttackMonster(this, targetScript))
        {
            Debug.Log("[SERVER] Attack rejected: target is not in range.");
            return;
        }

        ThisCard myCard = card.GetComponent<ThisCard>();
        ThisCard enemyCard = targetScript.card.GetComponent<ThisCard>();
        if (myCard == null || enemyCard == null) return;

        // Marked now, not when the battle resolves: a trap prompt may be
        // pending in between, and the attacker mustn't walk off meanwhile.
        hasAttackedThisTurn = true;
        hasMovedThisTurn = true;

        PlayerManager attackerPM = connectionToClient.identity.GetComponent<PlayerManager>();
        NetworkIdentity defenderIdentity = targetObj.GetComponent<NetworkIdentity>();
        PlayerManager defenderPM = defenderIdentity.connectionToClient != null
            ? defenderIdentity.connectionToClient.identity.GetComponent<PlayerManager>()
            : null; // unowned test monsters (solo host sessions) have no player

        int predictedDef = enemyCard.actualDEF;
        if (myCard.id == 45) predictedDef -= 500;

        bool willDefenderDie = false;
        if (targetScript.attackMode && myCard.actualATK >= enemyCard.actualATK) willDefenderDie = true;
        if (!targetScript.attackMode && myCard.actualATK > predictedDef) willDefenderDie = true;


        // Every set trap of the defender's that this attack triggers.
        List<ThisAction> validTraps = new List<ThisAction>();

        ThisAction[] allActions = FindObjectsOfType<ThisAction>();
        foreach (ThisAction action in allActions)
        {
            if (action.connectionToClient == defenderIdentity.connectionToClient && action.IsSetOnField)
            {
                if (action.id == 4) // Last Stand Barrier
                {
                    if (targetScript.attackMode == true) validTraps.Add(action);
                }
                else if (action.id == 5) // Intercept
                {
                    if (CheckIfNextToWallOrInBase(targetObj)) validTraps.Add(action);
                }
                else if (action.id == 1) // Echo of Silence
                {
                    validTraps.Add(action);
                }
                else if (action.id == 2) // Phantom Binding
                {
                    if (willDefenderDie) validTraps.Add(action);
                }
            }
        }

        // A player is asked about the first one; the CPU picks for itself.
        GameObject validTrap = validTraps.Count > 0 ? validTraps[0].gameObject : null;
        CpuPlayer cpuDefender = defenderPM != null ? defenderPM.GetComponent<CpuPlayer>() : null;
        if (cpuDefender != null)
        {
            ThisAction chosen = cpuDefender.ChooseTrap(validTraps, this, targetScript, willDefenderDie);
            if (chosen != null)
            {
                Debug.Log("[CPU] Activates " + chosen.cardName + ".");
                awaitingTrapAnswer = true; // the answer below expects a paused attack
                defenderPM.ServerAnswerTrapPrompt(true, chosen.gameObject, this.gameObject, targetObj);
            }
            else
            {
                ServerResolveAttack(targetObj, false, 0);
            }
        }
        else if (validTrap != null && defenderPM != null)
        {
            Debug.Log("Valid Trap found! Pausing attack to ask defender...");
            awaitingTrapAnswer = true;
            if (attackerPM != null) attackerPM.TargetShowWaitingForTrap(connectionToClient, true);
            defenderPM.TargetAskActivateTrap(defenderIdentity.connectionToClient, validTrap, this.gameObject, targetObj);
        }
        else
        {
            ServerResolveAttack(targetObj, false, 0);
        }
    }
    // Not [Server]-attributed: Mirror's weaver produces invalid IL for this
    // method on this Unity/Mirror combo once its body changes past some size
    // threshold (same recurring issue as OnStartServer/ServerSpawnTreasure,
    // ServerOfferSpookyManRevival and ServerCollectTreasure), so this guards
    // manually instead.
    public void ServerResolveAttack(GameObject targetObj, bool trapActivated, int trapId)
    {
        if (!NetworkServer.active) return;

        if (targetObj == null || card == null) return;
        LabyrinthObject targetScript = targetObj.GetComponent<LabyrinthObject>();
        if (targetScript == null || targetScript.card == null) return;

        ThisCard myCard = card.GetComponent<ThisCard>();
        ThisCard enemyCard = targetScript.card.GetComponent<ThisCard>();

        if (myCard == null || enemyCard == null) return;

        PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
        if (pm == null) return;

        hasAttackedThisTurn = true;

        if (myCard.id == 45) // Demon Lady
        {
            Debug.Log("Demon Lady is attacking! Shredding 500 DEF.");
            enemyCard.battleDefPenalty = 500;
            enemyCard.RecalculateStats();
            pm.RpcApplyDemonLadyShred(targetScript.card);
        }

        int myAtk = myCard.actualATK;
        int enemyAtk = enemyCard.actualATK;
        int enemyDef = enemyCard.actualDEF;

        bool enemyIsAttackMode = targetScript.attackMode;

        if (trapActivated)
        {
            if (trapId == 1) // Echo Of Silence
            {
                int gyMonsterCount = 0;
                ThisCard[] allCards = FindObjectsOfType<ThisCard>();

                foreach (ThisCard c in allCards)
                {
                    if (c.connectionToClient == targetScript.connectionToClient && c.beInGraveyard)
                    {
                        gyMonsterCount++;
                    }
                }

                int atkLoss = gyMonsterCount * 100;
                myAtk -= atkLoss;
                if (myAtk < 0) myAtk = 0; 

                Debug.Log($"Echo of Silence triggered! {gyMonsterCount} monsters in GY. Attacker loses {atkLoss} ATK. New ATK: {myAtk}");
            }
            else if (trapId == 4) // Last Stand Barrier
            {
                // The DEF boost itself was already applied to enemyCard via
                // PlayerManager.RpcApplyTempDef before this call (as tempDef,
                // persisting until end of turn), so enemyDef above already
                // reflects it -- adding enemyAtk again here would double it.
                enemyIsAttackMode = false;
                Debug.Log($"Last Stand Barrier activated! {enemyCard.cardName} DEF is boosted to {enemyDef} for this battle!");
            }
            else if (trapId == 5) // Intercept
            {
                Debug.Log("Intercept activated! The attack is completely negated.");

                enemyCard.battleDefPenalty = 0;
                enemyCard.RecalculateStats();
                pm.RpcResetDemonLadyShred(targetScript.card);

                hasMovedThisTurn = true;
                waitingToAttack = false;
                ResetGridColors();

                ServerShowBattle(targetObj, myAtk, enemyIsAttackMode ? enemyAtk : enemyDef, !enemyIsAttackMode, BattleAnimation.Negated);
                return;
            }
        }

        hasMovedThisTurn = true;
        waitingToAttack = false;
        ResetGridColors();

        // Show the fight on both screens before the loser is removed below
        // (the message arrives ahead of the destroy, so both monsters still
        // exist when the animation copies them).
        int defenderValue = enemyIsAttackMode ? enemyAtk : enemyDef;
        int battleResult;
        if (enemyIsAttackMode)
            battleResult = myAtk > enemyAtk ? BattleAnimation.DefenderDies
                : myAtk < enemyAtk ? BattleAnimation.AttackerDies : BattleAnimation.BothDie;
        else
            battleResult = myAtk > enemyDef ? BattleAnimation.DefenderDies : BattleAnimation.NobodyDies;
        ServerShowBattle(targetObj, myAtk, defenderValue, !enemyIsAttackMode, battleResult);

        if (enemyIsAttackMode)
        {
            if (myAtk > enemyAtk)
            {
                if (trapActivated && trapId == 2) // Phantom Binding
                {
                    this.isImmobile = true;
                    if (this.card != null)
                    {
                        ThisCard atkCardScript = this.card.GetComponent<ThisCard>();
                        if (atkCardScript != null) atkCardScript.isImmobile = true;
                        pm.RpcShowCard(this.card, "SetImmobile", 0);
                    }
                    Debug.Log("Phantom Binding triggered! The attacker is now Immobile.");
                }

                if (enemyCard.cardProperty == Property.Plague) pm.RpcApplyPlagueDebuff(this.card);

                CheckSpookyManRevival(targetScript);
                NetworkServer.Destroy(targetScript.gameObject);
                pm.RpcShowCard(targetScript.card, "OpponentDestroyed", 0);

                int damage = myAtk - enemyAtk;
                pm.RpcGMChangeLP(0, damage);

                if (this.monsterID == 26) // Heartstealer
                {
                    pm.RpcGMChangeLP(-500, 0);
                    Debug.Log("Heartstealer activated! Gained 500 LP.");
                }
            }
            else if (myAtk < enemyAtk)
            {
                if (myCard.cardProperty == Property.Plague) pm.RpcApplyPlagueDebuff(targetScript.card);

                CheckSpookyManRevival(this);
                NetworkServer.Destroy(this.gameObject);
                pm.RpcShowCard(this.card, "PlayerDestroyed", 0);

                int damage = enemyAtk - myAtk;
                pm.RpcGMChangeLP(damage, 0);
            }
            else
            {
                CheckSpookyManRevival(this);
                CheckSpookyManRevival(targetScript);
                NetworkServer.Destroy(this.gameObject);
                NetworkServer.Destroy(targetScript.gameObject);
                pm.RpcShowCard(this.card, "PlayerDestroyed", 0);
                pm.RpcShowCard(targetScript.card, "OpponentDestroyed", 0);
            }
        }
        else
        {
            if (myAtk > enemyDef)
            {
                if (trapActivated && trapId == 2) // Phantom Binding
                {
                    this.isImmobile = true;
                    if (this.card != null)
                    {
                        ThisCard atkCardScript = this.card.GetComponent<ThisCard>();
                        if (atkCardScript != null) atkCardScript.isImmobile = true;
                        pm.RpcShowCard(this.card, "SetImmobile", 0);
                    }
                    Debug.Log("Phantom Binding triggered! The attacker is now Immobile.");
                }

                if (enemyCard.cardProperty == Property.Plague) pm.RpcApplyPlagueDebuff(this.card);

                CheckSpookyManRevival(targetScript);
                NetworkServer.Destroy(targetScript.gameObject);
                pm.RpcShowCard(targetScript.card, "OpponentDestroyed", 0);

                // Normally destroying a Defense-position monster deals no LP
                // damage at all -- Piercing lets the excess ATK over the
                // defender's DEF spill over onto the opponent's LP instead.
                if (myCard.cardProperty == Property.Piercing)
                {
                    int pierceDamage = myAtk - enemyDef;
                    pm.RpcGMChangeLP(0, pierceDamage);
                }

                if (this.monsterID == 26) // Heartstealer
                {
                    pm.RpcGMChangeLP(-500, 0);
                    Debug.Log("Heartstealer activated! Gained 500 LP.");
                }
            }
            else if (myAtk < enemyDef)
            {
                int damage = enemyDef - myAtk;
                pm.RpcGMChangeLP(damage, 0);
            }
        }

        enemyCard.battleDefPenalty = 0;
        enemyCard.RecalculateStats();
        pm.RpcResetDemonLadyShred(targetScript.card);
    }

    // The host's own screen plays the battle right away: a ClientRpc reaches
    // the host's local client a tick later, after the loser is already gone.
    void ServerShowBattle(GameObject defender, int attackerValue, int defenderValue, bool defenderInDefense, int result)
    {
        if (NetworkClient.active)
            BattleAnimation.Play(this, defender.GetComponent<LabyrinthObject>(), attackerValue, defenderValue, defenderInDefense, result);
        RpcPlayBattle(defender.GetComponent<NetworkIdentity>().netId, attackerValue, defenderValue, defenderInDefense, result);
    }

    // Direct attack: same idea as ServerShowBattle -- the host plays it at
    // once, everyone else gets it by message.
    void ServerShowDirectAttack(string baseTileName, int attackerValue)
    {
        if (NetworkClient.active) BattleAnimation.PlayDirect(this, baseTileName, attackerValue);
        RpcPlayDirectAttack(baseTileName, attackerValue);
    }

    [ClientRpc]
    void RpcPlayDirectAttack(string baseTileName, int attackerValue)
    {
        if (isServer) return; // the host already played it
        BattleAnimation.PlayDirect(this, baseTileName, attackerValue);
    }

    // Sent as a netId rather than a GameObject: on the host the defender may
    // already be gone when this arrives, and a plain id avoids a lookup warning.
    [ClientRpc]
    void RpcPlayBattle(uint defenderNetId, int attackerValue, int defenderValue, bool defenderInDefense, int result)
    {
        if (isServer) return; // the host already played it (ServerShowBattle)
        NetworkIdentity defender;
        NetworkIdentity.spawned.TryGetValue(defenderNetId, out defender);
        BattleAnimation.Play(this, defender != null ? defender.GetComponent<LabyrinthObject>() : null,
            attackerValue, defenderValue, defenderInDefense, result);
    }

    // Spooky Man: "When this card is destroyed by battle you can revive one
    // Undead-type monster with less squares than this card from your
    // Graveyard to the same square as this card." Called right before each
    // battle-death NetworkServer.Destroy() call, for whichever side died.
    void CheckSpookyManRevival(LabyrinthObject deadMonster)
    {
        if (deadMonster == null || deadMonster.monsterID != 52) return; // Spooky Man
        if (deadMonster.card == null) return;
        ThisCard deadCard = deadMonster.card.GetComponent<ThisCard>();
        if (deadCard == null) return;
        if (deadMonster.connectionToClient == null || deadMonster.connectionToClient.identity == null) return;

        PlayerManager ownerPM = deadMonster.connectionToClient.identity.GetComponent<PlayerManager>();
        if (ownerPM != null) ownerPM.ServerOfferSpookyManRevival(deadMonster.currentTileName, deadCard.stars);
    }

    [Command]
    public void CmdDirectAttack()
    {
        ServerTryDirectAttack();
    }

    public void ServerTryDirectAttack()
    {
        if (!NetworkServer.active) return;
        ThisCard myCard = card.GetComponent<ThisCard>();
        if (myCard == null) return;
        if (!ServerMayAttack()) return;

        GridBehavior gb = FindGrid();
        if (gb != null && gb.CanDirectAttack(this))
        {
            hasMovedThisTurn = true;
            hasAttackedThisTurn = true;
            int damage = myCard.actualATK;

            // Show the charge into the opponent's base on both screens first,
            // so the LP popup can wait for the moment of impact.
            var baseTiles = gb.AttackableBaseTiles(this);
            ServerShowDirectAttack(baseTiles.Count > 0 ? baseTiles[0].name : null, damage);

            // Already on the server: send the LP change directly. Calling the
            // Cmd from here only worked for the host's own monsters.
            PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
            pm.RpcGMChangeLP(0, damage);

            waitingToAttack = false;
            ResetGridColors();
        }
    }

    GridBehavior FindGrid()
    {
        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        return gridGen != null ? gridGen.GetComponent<GridBehavior>() : null;
    }

    // Server-side guard for attack commands: the owner's turn, attack mode,
    // and at most one attack per monster per turn.
    bool ServerMayAttack()
    {
        if (GameManager.IsGameOver) return false;
        if (AnyAttackAwaitingTrap()) { Debug.Log("[SERVER] Attack rejected: another attack is waiting on a trap answer."); return false; }
        if (hasAttackedThisTurn) { Debug.Log("[SERVER] Attack rejected: this monster already attacked this turn."); return false; }
        if (!attackMode) { Debug.Log("[SERVER] Attack rejected: monster is in Defense mode."); return false; }

        bool ownerIsHost = connectionToClient == null || connectionToClient == NetworkServer.localConnection;
        if (ownerIsHost != PlayerManager.ServerHostHasTurn) { Debug.Log("[SERVER] Attack rejected: not this player's turn."); return false; }
        return true;
    }

    void ResetGridColors()
    {
        GameObject gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");
        if (gridGen) gridGen.GetComponent<GridBehavior>().ResetTileColors();
    }

    public void EndTurn()
    {
        PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
        if (pm) pm.CmdChangeTurn();
    }
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    bool CheckIfNextToWallOrInBase(GameObject defenderObj)
    {
        GridStat defGrid = defenderObj.GetComponentInParent<GridStat>();
        if (defGrid == null) return false;

        if (defGrid.y == 0 || defGrid.y == 1 || defGrid.y == 14 || defGrid.y == 15) return true;

        int[] openBlocks = { 0, 15, 16, 17, 18, 31, 32, 33, 34, 35, 36, 41, 42, 43, 44, 45, 46 };

        for (int i = 0; i < openBlocks.Length; i++)
        {
            if (defGrid.blockID == openBlocks[i])
            {
                return false;
            }
        }

        return true;
    }
}