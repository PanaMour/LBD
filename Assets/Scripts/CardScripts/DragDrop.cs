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

        if (GetComponent<ThisCard>() != null && PlayerManager.nomoresummons)
        {
            Debug.Log("Already summoned this turn.");
            return;
        }

        if (GetComponent<ThisMagic>() != null)
        {
            if (!GetComponent<ThisMagic>().canBeActivated)
            {
                Debug.Log("Cannot activate spell: No valid targets or conditions not met.");
                return;
            }
        }

        isDragging = true;

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

        GameObject existingCard = GetCardInSlot(slot);

        if (magicScript != null && magicScript.equip)
        {
            if (slot.name.Contains("PlayerSlot") && existingCard != null)
            {
                ThisCard monsterScript = existingCard.GetComponent<ThisCard>();
                if (monsterScript != null)
                {
                    monsterScript.isTarget = true;

                    GameObject openSpellSlot = FindEmptyActionSlot();

                    if (openSpellSlot != null)
                    {
                        PlaceCard(openSpellSlot, false);
                        return;
                    }
                    else
                    {
                        Debug.Log("No empty Spell/Trap zones available!");
                        monsterScript.isTarget = false;
                        ReturnToHand();
                        return;
                    }
                }
            }

            Debug.Log("Equip Spell must be dropped ON TOP of a Monster!");
            ReturnToHand();
            return;
        }

        if (cardScript != null && slot.name.Contains("PlayerSlot"))
        {
            if (PlayerManager.nomoresummons)
            {
                Debug.Log("Already summoned this turn.");
                ReturnToHand();
                return;
            }

            if (cardScript.stars >= 5)
            {
                if (existingCard != null)
                {
                    ThisCard targetScript = existingCard.GetComponent<ThisCard>();
                    if (targetScript != null && targetScript.canBeTributed)
                    {
                        if (PlayerManager != null)
                            PlayerManager.StartTributeProcess(gameObject, slot, existingCard);
                        return;
                    }
                }
                Debug.Log("Level 5+ must tribute a monster!");
                ReturnToHand();
                return;
            }
            else 
            {
                if (existingCard == null)
                {
                    PlaceCard(slot, true);
                    return;
                }
            }
        }

        if (magicScript != null && slot.name.Contains("ActionSlot"))
        {
            if (existingCard == null)
            {
                PlaceCard(slot, false);
                return;
            }
        }

        ReturnToHand();
    }

    GameObject FindEmptyActionSlot()
    {
        for (int i = 1; i <= 4; i++)
        {
            GameObject s = GameObject.Find("ActionSlot" + i);
            if (s != null && GetCardInSlot(s) == null)
            {
                return s;
            }
        }
        return null;
    }

    GameObject GetCardInSlot(GameObject slot)
    {
        foreach (Transform child in slot.transform)
        {
            if (child.GetComponent<ThisCard>() != null || child.GetComponent<ThisMagic>() != null)
                return child.gameObject;
        }
        return null;
    }
    void PlaceCard(GameObject slot, bool isMonster)
    {
        isDraggable = false;

        if (isMonster)
        {

            ThisCard cardScript = GetComponent<ThisCard>();

            if (PlayerManager != null)
            {
                PlayerManager.StartSummonProcess(gameObject, slot, false);
            }
        }
        else
        {
            transform.SetParent(slot.transform);
            transform.localPosition = slotPosition;
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            transform.localScale = slotScale;

            if (GetComponent<ThisMagic>() != null)
                GetComponent<ThisMagic>().activated = true;

            string numberOnly = System.Text.RegularExpressions.Regex.Match(slot.name, @"\d+").Value;
            int index = 0;
            if (int.TryParse(numberOnly, out int result)) index = result - 1;

            PlayerManager.PlayMagicCard(gameObject, index);
            this.enabled = false;
        }
    }

    public void ReturnToHand()
    {
        transform.SetParent(startParent.transform);
        transform.localPosition = Vector3.zero;
    }
}