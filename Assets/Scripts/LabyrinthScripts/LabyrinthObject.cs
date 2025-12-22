using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthObject : MonoBehaviour
{
    public GameObject labyrinthObject;
    public GameObject gridGenerator;
    public GameObject card;

    // Start is called before the first frame update
    void Start()
    {
        gridGenerator = transform.parent.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ObjectToMove()
    {
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
