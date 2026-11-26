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

        if (GameManager.IsGameOver) return;

        // Cards can be dealt before the local player object exists; look it up again.
        if (PlayerManager == null && NetworkClient.connection != null && NetworkClient.connection.identity != null)
            PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        if (PlayerManager == null || !PlayerManager.IsMyTurn || PlayerManager.WaitingForTrapAnswer) return;

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
        HighlightEquipTargets();

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        startParent = transform.parent.gameObject;
        transform.SetParent(null);
    }
    public void EndDrag()
    {
        if (!isDragging) return;

        if (!isDraggable) return;
        isDragging = false;
        ClearEquipHighlights();

        gameObject.layer = LayerMask.NameToLayer("Default");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        // An equip card can be dropped straight onto a monster in the
        // labyrinth (its model or its square), not just onto its card.
        ThisMagic dragged = GetComponent<ThisMagic>();
        if (dragged != null && dragged.equip)
        {
            foreach (RaycastHit hit in hits)
            {
                LabyrinthObject monster = hit.collider.GetComponentInParent<LabyrinthObject>();
                if (monster == null)
                {
                    GridStat square = hit.collider.GetComponentInParent<GridStat>();
                    if (square != null) monster = square.GetComponentInChildren<LabyrinthObject>();
                }
                if (monster != null && monster.card != null)
                {
                    TryEquip(monster, 1);
                    return;
                }
            }
        }

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
        ThisAction actionScript = GetComponent<ThisAction>(); // --- NEW ---

        GameObject existingCard = GetCardInSlot(slot);

        if (magicScript != null && magicScript.equip)
        {
            if ((slot.name.Contains("PlayerSlot") || slot.name.Contains("EnemySlot")) && existingCard != null && existingCard.GetComponent<ThisCard>() != null)
            {
                LabyrinthObject target3DMonster = null;
                foreach (LabyrinthObject m in FindObjectsOfType<LabyrinthObject>())
                    if (m.card == existingCard) { target3DMonster = m; break; }

                int preferredIndex = 1;
                string numberOnly = System.Text.RegularExpressions.Regex.Match(slot.name, @"\d+").Value;
                if (int.TryParse(numberOnly, out int result)) preferredIndex = result;
                TryEquip(target3DMonster, preferredIndex);
                return;
            }

            Debug.Log("Equip Spell must be dropped ON TOP of a Monster Card!");
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
                    if (targetScript != null) Debug.Log("[Tribute] canBeTributed=" + targetScript.canBeTributed + " summoned=" + targetScript.summoned + " beInGraveyard=" + targetScript.beInGraveyard + " | " + targetScript.DebugCardBaseInfo());
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

        // --- NEW: ACTION CARD LOGIC ---
        if (actionScript != null && slot.name.Contains("ActionSlot"))
        {
            if (existingCard == null)
            {
                PlaceActionCard(slot);
                return;
            }
        }

        ReturnToHand();
    }

    // ---- equip cards ----

    // Can this equip card go on that monster? Equips that may target any
    // monster (Weighted Shackles, Fleetfoot Blessing, Mechanical Legs) go on
    // either side's; the rest only on your own, and typed armour only on its type.
    string EquipProblem(ThisMagic magic, LabyrinthObject target)
    {
        if (target == null || target.card == null) return "Monster is not on the board yet!";
        if (!target.hasAuthority && magic.targetType != MagicTargetType.AnyUnit)
            return magic.magicName + " can only be equipped to your own monsters.";
        if (ThisMagic.TryGetRequiredEquipType(magic.id, out Type requiredType))
        {
            ThisCard monster = target.card.GetComponent<ThisCard>();
            if (monster == null || !monster.currentTypes.Contains(requiredType))
                return "Cannot equip! This card requires a " + requiredType + ".";
        }
        return null;
    }

    void TryEquip(LabyrinthObject target, int preferredIndex)
    {
        ThisMagic magicScript = GetComponent<ThisMagic>();
        string problem = EquipProblem(magicScript, target);
        if (problem != null)
        {
            Debug.Log(problem);
            ReturnToHand();
            return;
        }

        GameObject openSpellSlot = FindPreferredActionSlot(preferredIndex);
        if (openSpellSlot == null)
        {
            Debug.Log("No empty Spell/Trap zones available!");
            ReturnToHand();
            return;
        }

        transform.SetParent(openSpellSlot.transform);
        transform.localPosition = slotPosition;
        transform.localRotation = Quaternion.Euler(90, 0, 0);
        transform.localScale = slotScale;
        magicScript.activated = true;

        int actualIndex = 0;
        string actualNumberStr = System.Text.RegularExpressions.Regex.Match(openSpellSlot.name, @"\d+").Value;
        if (int.TryParse(actualNumberStr, out int actualResult)) actualIndex = actualResult - 1;
        PlayerManager.CmdExecuteMagicEffect(gameObject, target.gameObject, actualIndex);

        this.enabled = false;
    }

    // While an equip card is being dragged, the squares of the monsters it can
    // go on glow, so it's clear where it can be dropped.
    readonly List<LabyrinthTile> equipHighlights = new List<LabyrinthTile>();

    void HighlightEquipTargets()
    {
        ThisMagic magic = GetComponent<ThisMagic>();
        if (magic == null || !magic.equip) return;
        foreach (LabyrinthObject m in FindObjectsOfType<LabyrinthObject>())
        {
            if (EquipProblem(magic, m) != null) continue;
            LabyrinthTile tile = m.GetComponentInParent<LabyrinthTile>();
            if (tile == null || tile.isHighlighted) continue;
            tile.GlowBlock();
            equipHighlights.Add(tile);
        }
    }

    void ClearEquipHighlights()
    {
        foreach (LabyrinthTile tile in equipHighlights)
            if (tile != null) tile.StopGlowBlock();
        equipHighlights.Clear();
    }

    GameObject FindPreferredActionSlot(int preferredIndex)
    {
        GameObject preferredSlot = GameObject.Find("ActionSlot" + preferredIndex);
        if (preferredSlot != null && GetCardInSlot(preferredSlot) == null)
        {
            return preferredSlot;
        }

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
            // --- NEW: Added Action check so the slot knows it's occupied ---
            if (child.GetComponent<ThisCard>() != null || child.GetComponent<ThisMagic>() != null || child.GetComponent<ThisAction>() != null)
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

    // --- NEW: Specific placement logic for Action Cards ---
    void PlaceActionCard(GameObject slot)
    {
        isDraggable = false;

        transform.SetParent(slot.transform);
        transform.localPosition = slotPosition;
        transform.localRotation = Quaternion.Euler(90, 0, 0);
        transform.localScale = slotScale;

        string numberOnly = System.Text.RegularExpressions.Regex.Match(slot.name, @"\d+").Value;
        int index = 0;
        if (int.TryParse(numberOnly, out int result)) index = result - 1;

        // Tell the server we played an Action card (so the opponent sees it face-down)
        if (PlayerManager != null)
        {
            PlayerManager.PlayActionCard(gameObject, index);
        }

        this.enabled = false; // Turn off dragging now that it's set
    }

    // The host refused this play: make the card draggable from the hand again.
    public void ResetForHand()
    {
        isDragging = false;
        isDraggable = hasAuthority;
        enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void ReturnToHand()
    {
        transform.SetParent(startParent.transform);
        transform.localPosition = Vector3.zero;
    }
}