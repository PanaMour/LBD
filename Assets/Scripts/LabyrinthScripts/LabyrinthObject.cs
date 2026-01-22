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

    public override void OnStartClient()
    {
        base.OnStartClient();

        gridGenerator = GameObject.Find("GridGenerator");

        if (monsterID != 0)
            OnMonsterIDChanged(0, monsterID);

        if (!string.IsNullOrEmpty(currentTileName))
        {
            StartCoroutine(WaitForTileAndSnap(currentTileName));
        }
    }

    void OnMonsterIDChanged(int oldID, int newID)
    {
        if (newID > 0 && newID < CardDataBase.cardList.Count)
        {
            Card cardData = CardDataBase.cardList[newID];
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = cardData.thisImage;
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
        GameObject targetTile = null;

        float timeout = 2.0f;
        while (targetTile == null && timeout > 0)
        {
            targetTile = GameObject.Find(tileName);
            if (targetTile == null)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }
        }

        if (targetTile != null)
        {
            transform.SetParent(targetTile.transform, false);
            transform.localPosition = new Vector3(0, 0.5f, 0);
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError($"[Client] LabyrinthObject timed out finding '{tileName}'. Is the Grid generated?");
        }
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