using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

public class ItemController : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public Button sampleButton;                         // sample button prefab
    private List<ContextMenuItem> contextMenuItems;     // list of items in menu

    void Awake()
    {
        // Here we are creating and populating our future Context Menu.
        // I do it in Awake once, but as you can see, 
        // it can be edited at runtime anywhere and anytime.

        contextMenuItems = new List<ContextMenuItem>();
        Action<Image> atk = new Action<Image>(ChangeAttack);
        Action<Image> def = new Action<Image>(ChangeDefense);
        Action<Image> cancel = new Action<Image>(Cancel);

        contextMenuItems.Add(new ContextMenuItem("Attack Mode", sampleButton, atk));
        contextMenuItems.Add(new ContextMenuItem("Defense Mode", sampleButton, def));
        contextMenuItems.Add(new ContextMenuItem("Cancel", sampleButton, cancel));
    }

    public void Start()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();
    }

    public void RightClickForContextMenu()
    {
        if (Input.GetMouseButtonDown(1) && PlayerManager.IsMyTurn == true && gameObject.transform.parent.transform.parent == GameObject.Find("PlayerSlots").transform && gameObject.GetComponent<ThisCard>().alreadychanged == false)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            ContextMenu.Instance.CreateContextMenu(contextMenuItems, new Vector2(pos.x, pos.y));
        }

    }

    void ChangeAttack(Image contextPanel)
    {
        if (!gameObject.GetComponent<ThisCard>().attackmode == true && PlayerManager.IsMyTurn == true)
        {
            gameObject.GetComponent<ThisCard>().alreadychanged = true;
            gameObject.GetComponent<ThisCard>().changemode = true;
            PlayerManager.CmdChangeBattlePosition(gameObject, true);
            Debug.Log("Switched " + gameObject.GetComponent<ThisCard>().cardName + " to Attack");
        }
        Destroy(contextPanel.gameObject);
    }

    void ChangeDefense(Image contextPanel)
    {
        if (gameObject.GetComponent<ThisCard>().attackmode && PlayerManager.IsMyTurn == true && gameObject.GetComponent<ThisCard>().canAttack == true)
        {
            gameObject.GetComponent<ThisCard>().alreadychanged = true;
            gameObject.GetComponent<ThisCard>().changemode = true;
            PlayerManager.CmdChangeBattlePosition(gameObject, false);
            Debug.Log("Switched " + gameObject.GetComponent<ThisCard>().cardName + " to Defense");
        }
        Destroy(contextPanel.gameObject);
    }

    void Cancel(Image contextPanel)
    {
        Destroy(contextPanel.gameObject);
    }
}