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
        if (!hasAuthority) return;
        if (!isDraggable) return;

        if (PlayerManager != null && !PlayerManager.IsMyTurn) return;

        // 1. MONSTER CHECK: Can't pick up if already summoned
        if (GetComponent<ThisCard>() != null && PlayerManager.nomoresummons)
        {
            Debug.Log("Already summoned this turn.");
            return;
        }

        // 2. MAGIC CHECK: Can't pick up if there are no targets (e.g., Exhaust with no enemies)
        if (GetComponent<ThisMagic>() != null)
        {
            if (!GetComponent<ThisMagic>().canBeActivated)
            {
                Debug.Log("Cannot activate spell: No valid targets or conditions not met.");
                return;
            }
        }

        isDragging = true;

        // Hide from Raycast so we can see the slot behind it
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        startParent = transform.parent.gameObject;
        transform.SetParent(null);
    }
    public void EndDrag()
    {
        if (!isDragging) return;

        if (!isDraggable) return;
        isDragging = false;

        gameObject.layer = LayerMask.NameToLayer("Default");

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
            ReturnToHand();
        }
    }

    void AttemptDrop(GameObject slot)
    {
        ThisCard cardScript = GetComponent<ThisCard>();
        ThisMagic magicScript = GetComponent<ThisMagic>();

        bool isMonster = cardScript != null;
        bool isMagic = magicScript != null;

        // --- 1. MAGIC "GATEKEEPER" CHECK ---
        // If the Magic card logic says "False" (e.g. no enemies to destroy), reject the drop immediately.
        if (isMagic && !magicScript.canBeActivated)
        {
            Debug.Log("Conditions not met for this Magic card!");
            ReturnToHand();
            return;
        }

        // --- 2. EQUIP SPELL LOGIC ---
        if (isMagic && magicScript.equip)
        {
            if (slot.name.Contains("PlayerSlot"))
            {
                bool hasMonster = slot.transform.childCount > 0;
                if (hasMonster)
                {
                    PlaceCard(slot, false);
                    return;
                }
                else
                {
                    Debug.Log("Equip Spell must be placed on a Monster!");
                    ReturnToHand();
                    return;
                }
            }
            else
            {
                Debug.Log("Equip Spells must target a Monster Slot!");
                ReturnToHand();
                return;
            }
        }

        // --- 3. STANDARD DROP LOGIC (Monsters & Normal Spells) ---
        bool validMonsterDrop = isMonster && slot.name.Contains("PlayerSlot");
        bool validMagicDrop = isMagic && slot.name.Contains("ActionSlot");

        if (validMonsterDrop || validMagicDrop)
        {
            // Check if Slot is Full
            bool slotOccupied = slot.GetComponentInChildren<ThisCard>() != null ||
                                slot.GetComponentInChildren<ThisMagic>() != null;

            if (slotOccupied)
            {
                Debug.Log("Slot is already full!");
                ReturnToHand();
                return;
            }

            // Monster Specific Rules
            if (isMonster)
            {
                if (PlayerManager.nomoresummons)
                {
                    Debug.Log("You have already summoned a monster this turn!");
                    ReturnToHand();
                    return;
                }

                if (!cardScript.canBeSummoned)
                {
                    Debug.Log("Cannot summon: Needs Tribute or invalid level.");
                    ReturnToHand();
                    return;
                }
            }

            // Success!
            PlaceCard(slot, isMonster);
            return;
        }

        // If nothing matched, return to hand
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

            PlayerManager.nomoresummons = true;
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