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
    public GameObject current3DModel;
    [SyncVar]
    public int moveRange;
    [SyncVar]
    public bool attackMode = true;
    [SyncVar]
    public int turnSummoned = 0;

    public bool waitingToAttack = false;

    [SyncVar]
    public bool hasMovedThisTurn = false;
    public float targetWorldSize = 0.5f;

    public override void OnStartClient()
    {
        base.OnStartClient();
        gridGenerator = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

        if (monsterID != 0) OnMonsterIDChanged(0, monsterID);
        if (!string.IsNullOrEmpty(currentTileName)) StartCoroutine(WaitForTileAndSnap(currentTileName));
    }

    void OnMonsterIDChanged(int oldID, int newID)
    {
        if (newID > 0 && newID < CardDataBase.cardList.Count)
        {
            Card cardData = CardDataBase.cardList[newID];

            if (current3DModel != null) Destroy(current3DModel);

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
            transform.SetParent(targetTileTransform, true);
            transform.localPosition = new Vector3(0, 0.55f, 0);

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
            yield return new WaitForEndOfFrame();

            if (hasAuthority && hasMovedThisTurn)
            {
                CheckSurroundingsForEnemies();
            }
        }
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

        if (gridGenerator == null) gridGenerator = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

        if (gridGenerator != null)
            gridGenerator.GetComponent<GridBehavior>().ShowPossiblePaths(gameObject);
    }

    [Command]
    public void CmdMoveToTile(string tileName)
    {
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

        int myAtk = myCard.actualATK;
        int enemyAtk = enemyCard.actualATK;
        int enemyDef = enemyCard.def;
        bool enemyIsAttackMode = targetScript.attackMode;

        hasMovedThisTurn = true;
        waitingToAttack = false;
        ResetGridColors();

        PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();

        if (enemyIsAttackMode)
        {
            if (myAtk > enemyAtk)
            {
                NetworkServer.Destroy(targetScript.gameObject);
                pm.RpcShowCard(targetScript.card, "OpponentDestroyed", 0);

                int damage = myAtk - enemyAtk;
                pm.RpcGMChangeLP(0, damage);
            }
            else if (myAtk < enemyAtk)
            {
                NetworkServer.Destroy(this.gameObject);
                pm.RpcShowCard(this.card, "PlayerDestroyed", 0);

                int damage = enemyAtk - myAtk;
                pm.RpcGMChangeLP(damage, 0);
            }
            else
            {
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
                NetworkServer.Destroy(targetScript.gameObject);
                pm.RpcShowCard(targetScript.card, "OpponentDestroyed", 0);
            }
            else if (myAtk < enemyDef)
            {
                int damage = enemyDef - myAtk;
                pm.RpcGMChangeLP(damage, 0);
            }
        }
    }

    [Command]
    public void CmdDirectAttack()
    {
        ThisCard myCard = card.GetComponent<ThisCard>();
        if (myCard == null) return;

        GridStat currentGrid = GetComponentInParent<GridStat>();
        if (currentGrid == null) return;

        int y = currentGrid.y;
        bool validHostPos = (y == 14);
        bool validClientPos = (y == 1);

        bool validRow = (y == 14 || y == 15 || y == 1 || y == 0);

        if (validRow)
        {
            hasMovedThisTurn = true;
            int damage = myCard.actualATK;

            PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
            pm.CmdGMChangeLP(0, damage);

            waitingToAttack = false;
            ResetGridColors();
        }
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
}