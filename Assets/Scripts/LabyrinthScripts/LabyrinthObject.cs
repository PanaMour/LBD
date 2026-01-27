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
        if (gridGenerator == null) gridGenerator = GameObject.Find("GridGenerator");

        Transform targetTileTransform = null;
        float timeout = 2.0f;

        while (targetTileTransform == null && timeout > 0)
        {
            if (gridGenerator != null)
                targetTileTransform = gridGenerator.transform.Find(tileName);

            if (targetTileTransform == null)
            {
                yield return null;
                timeout -= Time.deltaTime;
                if (gridGenerator == null) gridGenerator = GameObject.Find("GridGenerator");
            }
        }

        if (targetTileTransform != null)
        {
            transform.SetParent(targetTileTransform, true);

            transform.localPosition = new Vector3(0, 0.55f, 0);
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            AdjustSize();
        }
        else
        {
            Debug.LogError($"[Client] LabyrinthObject could not find child '{tileName}' inside GridGenerator!");
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

        if (gridGenerator == null) gridGenerator = GameObject.Find("GridGenerator");

        if (gridGenerator != null)
            gridGenerator.GetComponent<GridBehavior>().ShowPossiblePaths(gameObject);
    }

    [Command]
    public void CmdMoveToTile(string tileName)
    {
        PlayerManager pm = connectionToClient.identity.GetComponent<PlayerManager>();
        if (pm.IsMyTurn)
        {
            currentTileName = tileName;
            hasMovedThisTurn = true;
        }
    }
}