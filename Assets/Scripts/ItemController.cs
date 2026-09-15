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
    public Button sampleButton;
    private List<ContextMenuItem> contextMenuItems;

    void Awake()
    {
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
        // Note: alreadychanged (battle position already switched this turn)
        // deliberately does NOT gate the menu itself -- it only removes the
        // Attack/Defense options below. A monster's own activated ability
        // (Frost Wraith's tribute, Shy Magician's swap, etc.) is independent
        // of whether its battle position was already changed this turn, and
        // must stay reachable even then.
        if (Input.GetMouseButtonDown(1) && PlayerManager.IsMyTurn == true && gameObject.transform.parent.transform.parent == GameObject.Find("PlayerSlots").transform)
        {
            List<ContextMenuItem> items = new List<ContextMenuItem>();
            if (!gameObject.GetComponent<ThisCard>().alreadychanged)
            {
                items.AddRange(contextMenuItems.GetRange(0, contextMenuItems.Count - 1)); // Attack Mode, Defense Mode
            }
            items.Add(contextMenuItems[contextMenuItems.Count - 1]); // Cancel

            if (gameObject.GetComponent<ThisCard>().id == 47) // Frost Wraith
            {
                Action<Image> tribute = new Action<Image>(TributeToImmobilize);
                items.Insert(items.Count - 1, new ContextMenuItem("Tribute: Immobilize Target", sampleButton, tribute));
            }

            if (gameObject.GetComponent<ThisCard>().id == 48 && !gameObject.GetComponent<ThisCard>().abilityUsed) // Shy Magician
            {
                Action<Image> swap = new Action<Image>(ExchangePositions);
                items.Insert(items.Count - 1, new ContextMenuItem("Exchange Positions", sampleButton, swap));
            }

            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            ContextMenu.Instance.CreateContextMenu(items, new Vector2(pos.x, pos.y));
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
        ContextMenu.Instance.CloseActiveMenu();
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
        ContextMenu.Instance.CloseActiveMenu();
    }

    void Cancel(Image contextPanel)
    {
        ContextMenu.Instance.CloseActiveMenu();
    }

    void TributeToImmobilize(Image contextPanel)
    {
        if (PlayerManager.IsMyTurn == true)
        {
            PlayerManager.StartFrostWraithTribute(gameObject);
        }
        ContextMenu.Instance.CloseActiveMenu();
    }

    void ExchangePositions(Image contextPanel)
    {
        if (PlayerManager.IsMyTurn == true)
        {
            PlayerManager.StartShyMagicianSwap(gameObject);
        }
        ContextMenu.Instance.CloseActiveMenu();
    }
}