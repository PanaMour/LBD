using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class LabyrinthObject : NetworkBehaviour
{
    public GameObject labyrinthObject;
    public GameObject gridGenerator;
    public GameObject card;

    [SyncVar(hook = nameof(OnMonsterIDChanged))]
    public int monsterID;
    [SyncVar(hook = nameof(OnTileNameChanged))]
    public string currentTileName;

    void Start()
    {
        gridGenerator = GameObject.Find("GridGenerator");

        Transform targetTile = gridGenerator.transform.Find("GridObject(4,0)");

        if (targetTile != null)
        {
            transform.SetParent(targetTile, false);
            transform.localPosition = Vector3.zero;
        }

        if (monsterID != 0) OnMonsterIDChanged(0, monsterID);
    }

    void OnMonsterIDChanged(int oldID, int newID)
    {
        Card cardData = CardDataBase.cardList[newID];

        Sprite monsterSprite = cardData.thisImage;

        GetComponent<Image>().sprite = monsterSprite;
    }

    public void ObjectToMove()
    {
        if (!hasAuthority) return;

        if (gridGenerator == null)
            gridGenerator = GameObject.Find("GridGenerator");

        if (gridGenerator == null) return;

        GridBehavior gb = gridGenerator.GetComponent<GridBehavior>();

        bool canMoveNow = true;
        if (card != null)
        {
            canMoveNow = card.GetComponent<ThisCard>().canMove;
        }

        if (gb.objectToMove != null && gb.objectToMove != labyrinthObject)
        {
            gb.HighlightRange(false);
            gb.ShowPossiblePaths(labyrinthObject);
        }
        else if (gb.objectToMove == labyrinthObject)
        {
            gb.HighlightRange(false);
            gb.objectToMove = null;
        }
        else if (canMoveNow)
        {
            gb.ShowPossiblePaths(labyrinthObject);
        }
    }

    void OnTileNameChanged(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(newName)) return;

        GameObject targetTile = GameObject.Find(newName);
        if (targetTile != null)
        {
            transform.SetParent(targetTile.transform, false);
            transform.localPosition = Vector3.zero;
        }
    }

    [Command]
    public void CmdMoveToTile(string tileName)
    {
        currentTileName = tileName;
    }
}