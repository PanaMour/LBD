using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ToggleLabyrinthVisibility : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;
    public GameObject Labyrinth;
    public bool Visible = false;

    public void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnClick()
    {
            if (!Visible)
            {
                Labyrinth.transform.localScale = new Vector3(1, 1, 1);
                Visible = true;
            }
            else
            {
                Labyrinth.transform.localScale = new Vector3(0, 0, 0);
                Visible = false;
            }
    }
}
