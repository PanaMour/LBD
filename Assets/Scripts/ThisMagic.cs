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
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (this.tag != "Unusable")
        {
            if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
                PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        }

        thisMagic[0] = MagicDataBase.magicList[thisId];

        if (transform.Find("MagicCanvas"))
        {
            magicnameText = transform.Find("MagicCanvas/MagicBackground/MagicName/MagicNameText").GetComponent<Text>();
            magicdescriptionText = transform.Find("MagicCanvas/MagicBackground/MagicDescription/MagicDescriptionText").GetComponent<Text>();
            thatImage = transform.Find("MagicCanvas/MagicBackground/MagicImage").GetComponent<Image>();
            frame = transform.Find("MagicCanvas").GetComponent<Image>();
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

        if (!initialized && thisMagic.Count > 0)
        {
            id = thisMagic[0].id;
            magicName = thisMagic[0].cardName;
            magicDescription = thisMagic[0].cardDescription;
            thisSprite = thisMagic[0].thisImage;
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
            magicnameText.text = "" + magicName;
            magicdescriptionText.text = "" + magicDescription;
            if (thisSprite != null) thatImage.sprite = thisSprite;

            if (thisMagic[0].color == "Magic") frame.color = new Color32(62, 69, 90, 255);
            else frame.color = new Color32(255, 255, 255, 255);
        }

        staticCardBack = cardBack;

        if (tag != "Unusable")
        {
            HandleActivationLogic();

            DragDrop dd = gameObject.GetComponent<DragDrop>();
            if (dd != null) dd.enabled = canBeActivated;

            bool inActionSlot = false;
            if (this.transform.parent != null)
            {
                if (this.transform.parent.name.Contains("ActionSlot"))
                {
                    inActionSlot = true;
                }
            }

            if (activated == false && inActionSlot)
            {
                Activate();
                drawX = drawXcards;
            }

            HandleSpellEffects();

            if (activated == true && PlayerManager.IsMyTurn == false && beInGraveyard == false && equip == false)
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
            canBeActivated = CheckForMonsters(PlayerSlots);
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
        foreach (Transform child in slotsContainer.transform)
        {
            foreach (Transform grandChild in child)
            {
                if (grandChild.GetComponent<ThisCard>() != null) return true;
            }
        }
        return false;
    }

    void HandleSpellEffects()
    {
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
            PlayerManager.CmdDrawCard();
            drawX--;
            canBeDestroyed = true;
        }

        if (drawX <= 0 && !activationcomplete && canBeDestroyed)
        {
            activationcomplete = true;
            StartCoroutine(SmoothDestruction(2));
        }

        if (PlayerManager.IsMyTurn && activated && !activationcomplete && hasAuthority)
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

        if (!PlayerManager.IsMyTurn) UcanReturn = false;

        if (equippedTo != null)
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

    IEnumerator SmoothDestruction(int sec)
    {
        yield return new WaitForSeconds(sec);
        if (Magic != null)
            PlayerManager.CmdPlayerDestroyCard(Magic, 0);
    }
    public void Activate()
    {
        activated = true;
    }
    public void UntargetEnemy()
    {
        staticTargetingEnemy = false;
    }

    public void TargetEnemy()
    {
        staticTargetingEnemy = true;
    }
    public void Activating()
    {
        //PlayerManager.nomoresummons = false;
    }
    public void NotActivating()
    {
        //PlayerManager.nomoresummons = true;
    }

    public void StartAttack()
    {
        staticTargeting = true;
    }

    public void StopAttack()
    {
        staticTargeting = false;
    }

    public void OneCardAttack()
    {
        //onlyThisCardAttack = true;
    }

    public void OneCardAttackStop()
    {
        //onlyThisCardAttack = false;
    }

    public void Destroy()
    {
        canBeDestroyed = false;
        beInGraveyard = true;
    }

    public void Return(int x)
    {
        for (int i = 0; i <= x; i++)
        {
            ReturnCard();//not working now
        }
    }

    public void ReturnCard()
    {
        UcanReturn = true;
    }

    public void ReturnThis()
    {
        if (beInGraveyard == true && UcanReturn == true)
        {
            this.transform.SetParent(PlayerArea.transform);
            UcanReturn = false;
            beInGraveyard = false;
        }
    }

    public void BeingTarget()
    {
        isTarget = true;
    }

    public void NotBeingTarget()
    {
        isTarget = false;
    }

    /*
    public void dealxDamage(int x)
    {
        if (Target != null)
        {
            if (Target == Enemy && stopDealDamage == false && Input.GetMouseButton(0))
            {
                PlayerManager.CmdGMChangeLP(0, damageDealtBySpell);
                stopDealDamage = true;
            }
        }
        else
        {

        }
    }*/

    public void TargetMonster()
    {
        if (targetDestroy == true && activated == true && activationcomplete == false && beInGraveyard == false)
        {
            if(Target != null)
            {
                if (Target == Enemy)
                {
                    monstersExist = false;
                    foreach (Transform child in EnemySlots.transform)//child.child
                    {
                        if (child.transform.childCount != 0)
                        {
                            monstersExist = true;
                        }
                    }
                }
            }
            else
            {
                foreach (Transform child in EnemySlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>().isTarget == true)
                        {
                            if (grandChild.GetComponent<ThisCard>() != null)
                            {
                                grandChild.GetComponent<ThisCard>().decreased = grandChild.GetComponent<ThisCard>().atk;
                                PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                canBeDestroyed = true;
                                beInGraveyard = true;
                            }
                        }
                    }
                }
            }
        }
        else if(changeAttack == true && activated == true && activationcomplete == false && beInGraveyard == false)
        {
            if (Target != null)
            {
                if (Target == Enemy)
                {
                    monstersExist = false;
                    foreach (Transform child in EnemySlots.transform)//child.child
                    {
                        if (child.transform.childCount != 0)
                        {
                            monstersExist = true;
                        }
                    }
                }
            }
            else
            {
                foreach (Transform child in EnemySlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>().isTarget == true)
                        {
                            if (grandChild.GetComponent<ThisCard>() != null)
                            {
                                PlayerManager.CmdChangeAttack(grandChild.gameObject, 0);
                                canBeDestroyed = true;
                                beInGraveyard = true;
                            }
                        }
                    }
                }
            }
        }
        else if(changeDefense == true && activated == true && activationcomplete == false && beInGraveyard == false)
        {
            if (Target != null)
            {
                if (Target == Enemy)
                {
                    monstersExist = false;
                    foreach (Transform child in EnemySlots.transform)//child.child
                    {
                        if (child.transform.childCount != 0)
                        {
                            monstersExist = true;
                        }
                    }
                }
            }
            else
            {
                foreach (Transform child in EnemySlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>().isTarget == true)
                        {
                            if (grandChild.GetComponent<ThisCard>() != null)
                            {
                                PlayerManager.CmdChangeDefense(grandChild.gameObject, 0);
                                canBeDestroyed = true;
                                beInGraveyard = true;
                            }
                        }
                    }
                }
            }
        }
        else if(equip == true && activated == true && beInGraveyard == false)
        {
            if (Target != null)
            {
                if (Target == Enemy)
                {
                    monstersExist = false;
                    foreach (Transform child in PlayerSlots.transform)//child.child
                    {
                        if (child.transform.childCount != 0)
                        {
                            monstersExist = true;
                        }
                    }
                }
            }
            else
            {
                foreach (Transform child in PlayerSlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if(grandChild.GetComponent<ThisCard>() != null)
                        {
                            if (grandChild.GetComponent<ThisCard>().isTarget == true)
                            {
                                equippedTo = grandChild.gameObject;
                                grandChild.gameObject.GetComponent<ThisCard>().equippedTo = Magic;
                                PlayerManager.CmdEquipBoost(grandChild.gameObject, equipBoost);
                            }
                        }
                    }
                }
            }
        }
    }
}
