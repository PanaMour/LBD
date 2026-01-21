using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DragDrop : NetworkBehaviour
{
    public GameManager GameManager;
    public PlayerManager PlayerManager;

    private bool isDragging = false;
    private bool isDraggable = true;
    private GameObject startParent;

    private Vector3 slotPosition = new Vector3(0, 1.0f, 0);
    private Vector3 slotScale = new Vector3(0.01f, 0.0075f, 0.01f);

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
        {
            PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        }

        if (!hasAuthority) isDraggable = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 1.2f;
            transform.position = Camera.main.ScreenToWorldPoint(mousePos);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void StartDrag()
    {
        if (!isDraggable) return;
        isDragging = true;
        startParent = transform.parent.gameObject;
        transform.SetParent(null);
    }

    public void EndDrag()
    {
        if (!isDraggable) return;
        isDragging = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        GameObject foundSlot = null;

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.name.Contains("Slot"))
            {
                foundSlot = hitObj;
                break;
            }
            else if (hitObj.transform.parent != null && hitObj.transform.parent.name.Contains("Slot"))
            {
                foundSlot = hitObj.transform.parent.gameObject;
                break;
            }
        }

        if (foundSlot != null)
        {
            AttemptDrop(foundSlot);
        }
        else
        {
            Debug.Log("No slot found under mouse. Returning.");
            ReturnToHand();
        }
    }

    void AttemptDrop(GameObject slot)
    {
        bool isMonster = GetComponent<ThisCard>() != null;
        bool isMagic = GetComponent<ThisMagic>() != null;

        bool validMonsterDrop = isMonster && slot.name.Contains("PlayerSlot");
        bool validMagicDrop = isMagic && slot.name.Contains("ActionSlot");

        if (validMonsterDrop || validMagicDrop)
        {
            bool slotOccupied = slot.GetComponentInChildren<ThisCard>() != null ||
                                slot.GetComponentInChildren<ThisMagic>() != null;

            if (!slotOccupied)
            {
                PlaceCard(slot, isMonster);
                return;
            }
            else
            {
                Debug.Log("Slot is already full!");
            }
        }

        ReturnToHand();
    }

    void PlaceCard(GameObject slot, bool isMonster)
    {
        transform.SetParent(slot.transform);
        transform.localPosition = slotPosition;
        transform.localRotation = Quaternion.Euler(90, 0, 0);
        transform.localScale = slotScale;

        isDraggable = false;

        if (isMonster)
        {
            if (GetComponent<ThisCard>() != null)
                GetComponent<ThisCard>().summoned = true;
        }
        else
        {
            if (GetComponent<ThisMagic>() != null)
                GetComponent<ThisMagic>().activated = true;
        }

        string numberOnly = System.Text.RegularExpressions.Regex.Match(slot.name, @"\d+").Value;
        int index = 0;
        if (int.TryParse(numberOnly, out int result)) index = result - 1;

        if (isMonster)
        {
            if (GetComponent<CardAbilities>() != null)
                PlayerManager.PlayCard(gameObject, index);
            else
                PlayerManager.CmdPlayCard(gameObject, index);
        }
        else
        {
            PlayerManager.PlayMagicCard(gameObject, index);
        }

        this.enabled = false;
    }

    void ReturnToHand()
    {
        transform.SetParent(startParent.transform);
        transform.localPosition = Vector3.zero;
    }
}