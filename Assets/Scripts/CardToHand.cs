using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardToHand : NetworkBehaviour
{
    public GameObject PlayerArea;
    public GameObject It;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        PlayerArea = GameObject.Find("PlayerArea");
        It.transform.SetParent(PlayerArea.transform);
        It.transform.localScale = Vector3.one;
        It.transform.position = new Vector3(transform.position.x, transform.position.y, -48);
        It.transform.eulerAngles = new Vector3(25, 0, 0);
    }
}
