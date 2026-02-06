using UnityEngine;
using Mirror;
using System.Collections;
public class LabyrinthObject : NetworkBehaviour
{
    public GameObject gridGenerator;
    public GameObject card;

    [SyncVar(hook = nameof(OnMonsterIDChanged))]
    public int monsterID;

    [SyncVar(hook = nameof(OnTileNameChanged))]
    public string currentTileName;

    [SyncVar]
    public int moveRange;
    [SyncVar]
    public bool attackMode = true;
    [SyncVar]
    public int turnSummoned = 0;

    [SyncVar]
    public bool hasMovedThisTurn = false;
    public float targetWorldSize = 0.5f;
    public override void OnStartClient()
    {
        base.OnStartClient();
        gridGenerator = GameObject.Find("GridGenerator");

        if (monsterID != 0) OnMonsterIDChanged(0, monsterID);
        if (!string.IsNullOrEmpty(currentTileName)) StartCoroutine(WaitForTileAndSnap(currentTileName));
    }

    void OnMonsterIDChanged(int oldID, int newID)
    {
        if (newID > 0 && newID < CardDataBase.cardList.Count)
        {
            Card cardData = CardDataBase.cardList[newID];
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = cardData.thisImage;

                AdjustSize();
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
            transform.SetParent(targetTileTransform, true);
            transform.localPosition = new Vector3(0, 0.55f, 0);
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            AdjustSize();
        }
    }

    void AdjustSize()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        if (transform.parent == null) return;

        Vector3 spriteSize = sr.sprite.bounds.size;

        float maxSpriteDimension = Mathf.Max(spriteSize.x, spriteSize.y);

        float parentScale = transform.parent.lossyScale.x;

        if (parentScale == 0 || maxSpriteDimension == 0) return;

        float finalScale = targetWorldSize / (parentScale * maxSpriteDimension);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    public void ObjectToMove()
    {
        if (!hasAuthority) return;
        if (NetworkClient.connection.identity == null) return;

        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        if (pm == null || !pm.IsMyTurn) return;
        if (hasMovedThisTurn) return;

        if (!attackMode)
        {
            Debug.Log("Cannot move or attack in Defense Mode!");
            return;
        }

        if (gridGenerator == null) gridGenerator = GameObject.Find("GridGenerator");

        if (gridGenerator != null)
            gridGenerator.GetComponent<GridBehavior>().ShowPossiblePaths(gameObject);
    }

    [Command]
    public void CmdMoveToTile(string tileName)
    {
        PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
        currentTileName = tileName;
        hasMovedThisTurn = true;
    }

    [Command]
    public void CmdAttackMonster(GameObject targetObj)
    {
        LabyrinthObject targetScript = targetObj.GetComponent<LabyrinthObject>();
        if (targetScript == null) return;

        ThisCard myCard = card.GetComponent<ThisCard>();
        ThisCard enemyCard = targetScript.card.GetComponent<ThisCard>();

        if (myCard == null || enemyCard == null) return;

        int myAtk = myCard.actualATK; // Use actualATK for buffs
        int enemyAtk = enemyCard.actualATK;
        int enemyDef = enemyCard.def;
        bool enemyIsAttackMode = targetScript.attackMode;

        hasMovedThisTurn = true;

        if (enemyIsAttackMode)
        {
            // --- ATK vs ATK ---
            if (myAtk > enemyAtk)
            {
                // Destroy Enemy
                PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
                pm.CmdOpponentDestroyCard(targetScript.card, 0);
                // Damage
                int damage = myAtk - enemyAtk;
                pm.CmdGMChangeLP(0, damage);
            }
            else if (myAtk < enemyAtk)
            {
                // Destroy Me
                PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
                pm.CmdPlayerDestroyCard(this.card, 0);
                // Damage Me
                int damage = enemyAtk - myAtk;
                pm.CmdGMChangeLP(damage, 0);
            }
            else // Tie
            {
                PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
                pm.CmdPlayerDestroyCard(this.card, 0);
                pm.CmdOpponentDestroyCard(targetScript.card, 0);
            }
        }
        else
        {
            // --- ATK vs DEF ---
            if (myAtk > enemyDef)
            {
                // Destroy Enemy (No Damage to LP)
                PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
                pm.CmdOpponentDestroyCard(targetScript.card, 0);
            }
            else if (myAtk < enemyDef)
            {
                // I lose Life Points (No destroy)
                int damage = enemyDef - myAtk;
                PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
                pm.CmdGMChangeLP(damage, 0);
            }
            // Tie = Nothing happens
        }
    }

    [Command]
    public void CmdDirectAttack()
    {
        ThisCard myCard = card.GetComponent<ThisCard>();
        if (myCard == null) return;

        hasMovedThisTurn = true;

        int damage = myCard.actualATK;

        // Deal damage to Opponent
        PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
        pm.CmdGMChangeLP(0, damage);
    }
}