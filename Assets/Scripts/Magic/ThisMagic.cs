using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ThisMagic : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;
    public GameObject Magic;
    public List<Magic> thisMagic = new List<Magic>();

    [SyncVar] public int thisId;

    public int id;
    public string magicName;
    public string magicDescription;
    public Text magicnameText;
    public Text magicdescriptionText;
    public Sprite thisSprite;
    public Image thatImage;
    public Image frame;

    public MagicTargetType targetType;

    public bool cardBack;
    public static bool staticCardBack;

    public GameObject PlayerArea;
    public int numberOfCardsInDeck;

    public bool canBeActivated;
    public bool activated;

    public GameObject battleZone;

    public static int drawX;
    public int drawXcards;

    public GameObject Target;
    public GameObject Enemy;

    public static bool staticTargeting;
    public static bool staticTargetingEnemy;
    public bool targeting;
    public bool targetingEnemy;

    public bool canBeDestroyed = false;
    public bool beInGraveyard = false;
    public int returnXcards;
    public bool useReturn;
    public static bool UcanReturn;
    public bool isTarget;
    public GameObject PlayerSlots;
    public GameObject EnemySlots;
    public bool monstersExist;

    public int damageHealedBySpell;
    public int damageDealtBySpell;
    public bool dealDamage;
    public bool stopDealDamage;
    public bool activationcomplete = false;
    public bool targetDestroy;
    public bool faceup;
    public bool changeAttack;
    public bool changeDefense;
    public bool equip;
    public int equipBoost;
    public GameObject equippedTo;

    private bool initialized = false;

    void Start()
    {
        GameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();

        if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
            PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();

        if (transform.Find("MagicCanvas"))
        {
            Transform canvas = transform.Find("MagicCanvas");
            Transform bg = canvas.Find("MagicBackground");

            if (bg.Find("MagicName/MagicNameText"))
                magicnameText = bg.Find("MagicName/MagicNameText").GetComponent<Text>();

            if (bg.Find("MagicDescription/MagicDescriptionText"))
                magicdescriptionText = bg.Find("MagicDescription/MagicDescriptionText").GetComponent<Text>();

            if (bg.Find("MagicImage"))
                thatImage = bg.Find("MagicImage").GetComponent<Image>();

            frame = canvas.GetComponent<Image>();
        }

        if (thisId > 0 && thisId < MagicDataBase.magicList.Count)
        {
            thisMagic.Add(MagicDataBase.magicList[thisId]);
        }
        else
        {
            thisMagic.Add(MagicDataBase.magicList[0]);
        }

        numberOfCardsInDeck = PlayerDeck.deckSize;
        canBeActivated = false;
        activated = false;
        drawX = 0;

        Enemy = GameObject.Find("OpponentLP");
        PlayerSlots = GameObject.Find("PlayerSlots");
        EnemySlots = GameObject.Find("EnemySlots");
    }

    void Update()
    {
        if (PlayerManager == null)
        {
            if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
                PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        }

        if (PlayerArea == null) PlayerArea = GameObject.Find("Hand_Anchor");
        if (PlayerSlots == null) PlayerSlots = GameObject.Find("PlayerSlots");
        if (EnemySlots == null) EnemySlots = GameObject.Find("EnemySlots");

        if (this.transform.parent != null && PlayerArea != null)
        {
            if (this.transform.parent == PlayerArea.transform)
            {
                cardBack = false;
            }
        }

        if (thisId != 0 && thisId != id)
        {
            if (thisId < MagicDataBase.magicList.Count)
            {
                thisMagic.Clear();
                thisMagic.Add(MagicDataBase.magicList[thisId]);
                initialized = false;
            }
        }

        if (!initialized && thisMagic.Count > 0 && thisMagic[0] != null)
        {
            id = thisMagic[0].id;
            magicName = thisMagic[0].cardName;
            magicDescription = thisMagic[0].cardDescription;
            thisSprite = thisMagic[0].thisImage;
            targetType = thisMagic[0].targetType;
            drawXcards = thisMagic[0].drawXcards;
            returnXcards = thisMagic[0].returnXcards;
            damageHealedBySpell = thisMagic[0].damageHealedBySpell;
            damageDealtBySpell = thisMagic[0].damageDealtBySpell;
            targetDestroy = thisMagic[0].targetDestroy;
            changeAttack = thisMagic[0].changeAttack;
            changeDefense = thisMagic[0].changeDefense;
            equip = thisMagic[0].equip;
            equipBoost = thisMagic[0].equipBoost;
            initialized = true;
        }

        if (initialized)
        {
            if (magicnameText) magicnameText.text = "" + magicName;
            if (magicdescriptionText) magicdescriptionText.text = "" + magicDescription;

            if (thatImage != null && thisSprite != null)
            {
                thatImage.sprite = thisSprite;
            }

            if (frame)
            {
                if (thisMagic[0].color == "Magic") frame.color = new Color32(62, 69, 90, 255);
                else frame.color = new Color32(255, 255, 255, 255);
            }
        }

        staticCardBack = cardBack;

        if (tag != "Unusable")
        {
            if (PlayerManager != null && PlayerManager.IsMyTurn && !activated)
            {
                HandleActivationLogic();
            }
            else
            {
                canBeActivated = false;
            }

            DragDrop dd = gameObject.GetComponent<DragDrop>();
            if (dd != null)
            {
                dd.enabled = canBeActivated;
            }

            bool inActionSlot = false;
            if (this.transform.parent != null)
            {
                if (this.transform.parent.name.Contains("ActionSlot"))
                {
                    inActionSlot = true;
                }
            }

            if (activated == false && inActionSlot && targetType == MagicTargetType.None)
            {
                Activate();
                drawX = drawXcards;
            }

            if (initialized)
            {
                HandleSpellEffects();
            }

            if (activated == true && PlayerManager != null && PlayerManager.IsMyTurn == false && beInGraveyard == false && equip == false)
            {
                activationcomplete = true;
                StartCoroutine(SmoothDestruction(2));
            }

            if (activationcomplete)
            {
                Destroy();
            }
        }
    }

    void HandleActivationLogic()
    {
        if (activated == false && beInGraveyard == false && !equip && !targetDestroy && !changeAttack && !changeDefense)
        {
            canBeActivated = true;
        }
        else if (equip && PlayerSlots != null)
        {
            canBeActivated = HasValidEquipTarget();
        }
        else if ((targetDestroy || changeAttack || changeDefense) && EnemySlots != null)
        {
            canBeActivated = CheckForMonsters(EnemySlots);
        }
        else
        {
            canBeActivated = false;
        }
    }

    bool CheckForMonsters(GameObject slotsContainer)
    {
        if (slotsContainer == null) return false;

        foreach (Transform child in slotsContainer.transform)
        {
            foreach (Transform grandChild in child)
            {
                if (grandChild.GetComponent<ThisCard>() != null) return true;
            }
        }
        return false;
    }

    bool HasValidEquipTarget()
    {
        bool requiresSpecificType = false;

        Type requiredType = Type.Alien;

        if (id == 11) { requiresSpecificType = true; requiredType = Type.Mineral; }
        else if (id == 12) { requiresSpecificType = true; requiredType = Type.Alien; }
        else if (id == 13) { requiresSpecificType = true; requiredType = Type.Human; }
        else if (id == 14) { requiresSpecificType = true; requiredType = Type.Animal; }

        List<GameObject> containersToCheck = new List<GameObject>();
        if (targetType == MagicTargetType.AnyAlly || targetType == MagicTargetType.AnyUnit) containersToCheck.Add(PlayerSlots);
        if (targetType == MagicTargetType.AnyEnemy || targetType == MagicTargetType.AnyUnit) containersToCheck.Add(EnemySlots);

        if (containersToCheck.Count == 0) containersToCheck.Add(PlayerSlots);

        foreach (GameObject container in containersToCheck)
        {
            if (container == null) continue;

            foreach (Transform slot in container.transform)
            {
                foreach (Transform cardObj in slot)
                {
                    ThisCard monsterCard = cardObj.GetComponent<ThisCard>();
                    if (monsterCard != null)
                    {
                        if (!requiresSpecificType) return true;

                        if (monsterCard.currentTypes.Contains(requiredType)) return true;
                    }
                }
            }
        }

        return false;
    }

    void HandleSpellEffects()
    {
        if (thisMagic == null || thisMagic.Count == 0) return;

        targeting = staticTargeting;
        targetingEnemy = staticTargetingEnemy;
        Target = targetingEnemy ? Enemy : null;

        if (targeting) TargetMonster();

        if (returnXcards > 0 && activated && !useReturn)
        {
            Return(returnXcards);
            useReturn = true;
        }

        if (drawX > 0 && activated && hasAuthority)
        {
            if (PlayerManager != null) PlayerManager.CmdDrawCard();
            drawX--;
            canBeDestroyed = true;
        }

        if (drawX <= 0 && !activationcomplete && canBeDestroyed)
        {
            activationcomplete = true;
            StartCoroutine(SmoothDestruction(2));
        }

        if (PlayerManager != null && PlayerManager.IsMyTurn && activated && !activationcomplete && hasAuthority)
        {
            if (damageHealedBySpell > 0)
            {
                PlayerManager.CmdGMChangeLP(damageHealedBySpell, 0);
                canBeDestroyed = true;
            }
            if (damageDealtBySpell > 0)
            {
                PlayerManager.CmdGMChangeLP(0, damageDealtBySpell);
                canBeDestroyed = true;
            }
        }

        if (PlayerManager != null && !PlayerManager.IsMyTurn) UcanReturn = false;

        if (equippedTo != null)
        {
            if (equippedTo == null)
            {
                canBeDestroyed = true;
                beInGraveyard = true;
            }
            else
            {
                ThisCard tc = equippedTo.GetComponent<ThisCard>();
                if (tc != null && tc.beInGraveyard)
                {
                    canBeDestroyed = true;
                    beInGraveyard = true;
                    equippedTo = null;
                }
            }
        }
    }

    IEnumerator SmoothDestruction(int sec)
    {
        yield return new WaitForSeconds(sec);
        if (Magic != null && PlayerManager != null)
            PlayerManager.CmdPlayerDestroyCard(Magic, 0);
    }

    public void Activate()
    {
        activated = true;
        drawX = drawXcards;
    }

    public void UntargetEnemy() { staticTargetingEnemy = false; }
    public void TargetEnemy() { staticTargetingEnemy = true; }
    public void StartAttack() { staticTargeting = true; }
    public void StopAttack() { staticTargeting = false; }
    public void Destroy() { canBeDestroyed = false; beInGraveyard = true; }

    public void Return(int x)
    {
        for (int i = 0; i <= x; i++) ReturnCard();
    }
    public void ReturnCard() { UcanReturn = true; }

    public void ReturnThis()
    {
        if (beInGraveyard == true && UcanReturn == true && PlayerArea != null)
        {
            this.transform.SetParent(PlayerArea.transform);
            UcanReturn = false;
            beInGraveyard = false;
        }
    }

    public void TargetMonster() { }
}