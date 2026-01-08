using UnityEngine;
using Mirror;

public class LabyrinthObject : NetworkBehaviour
{
    public GameObject labyrinthObject;
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

    void Start()
    {
        gridGenerator = GameObject.Find("GridGenerator");
        if (!string.IsNullOrEmpty(currentTileName))
        {
            OnTileNameChanged("", currentTileName);
        }
        if (monsterID != 0) OnMonsterIDChanged(0, monsterID);
    }

    void Update()
    {
    }

    void OnMonsterIDChanged(int oldID, int newID)
    {
        Card cardData = CardDataBase.cardList[newID];

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().sprite = cardData.thisImage;
        }
    }

    public void ObjectToMove()
    {
        if (!hasAuthority) return;

        if (NetworkClient.connection.identity == null) return;
        PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();

        if (pm == null || !pm.IsMyTurn)
        {
            Debug.Log("It is not your turn!");
            return;
        }

        if (hasMovedThisTurn)
        {
            Debug.Log("This monster has already moved this turn.");
            return;
        }

        if (gridGenerator == null)
            gridGenerator = GameObject.Find("GridGenerator");

        GridBehavior gb = gridGenerator.GetComponent<GridBehavior>();
        gb.ShowPossiblePaths(gameObject);
    }

    void OnTileNameChanged(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(newName)) return;
        GameObject targetTile = GameObject.Find(newName);
        if (targetTile != null)
        {
            transform.SetParent(targetTile.transform, false);

            transform.localPosition = new Vector3(0, 0.5f, 0);
        }
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