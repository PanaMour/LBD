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
        if(card.GetComponent<ThisCard>().canMove == true)
        gridGenerator.GetComponent<GridBehavior>().ShowPossiblePaths(labyrinthObject);
    }
}
