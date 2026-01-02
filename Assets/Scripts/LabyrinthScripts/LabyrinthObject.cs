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

    void Start()
    {
        gridGenerator = GameObject.Find("GridGenerator");

        if (monsterID != 0)
        {
            OnMonsterIDChanged(0, monsterID);
        }
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

        GridBehavior gb = gridGenerator.GetComponent<GridBehavior>();

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
        else if (card.GetComponent<ThisCard>().canMove == true)
        {
            gb.ShowPossiblePaths(labyrinthObject);
        }
    }
}