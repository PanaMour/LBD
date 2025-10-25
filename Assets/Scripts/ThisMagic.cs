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

    [SyncVar]
    public int thisId;

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

    public GameObject PlayerArea;////////////////////////////////

    public int numberOfCardsInDeck;///

    public bool canBeActivated;
    public bool activated;
    public GameObject battleZone;///d///////////////////

    public static int drawX;
    public int drawXcards;

    public GameObject Target;
    public GameObject Enemy;

    public static bool staticTargeting;
    public static bool staticTargetingEnemy;

    public bool targeting;
    public bool targetingEnemy;

    public bool canBeDestroyed = false;
    public GameObject Graveyard;

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

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (this.tag != "Unusable") //error in console with unusable cards
        {
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        }

        thisMagic[0] = MagicDataBase.magicList[thisId];

        magicnameText = transform.Find("MagicCanvas").Find("MagicBackground").Find("MagicName").Find("MagicNameText").GetComponent<Text>();
        magicdescriptionText = transform.Find("MagicCanvas").Find("MagicBackground").Find("MagicDescription").Find("MagicDescriptionText").GetComponent<Text>();
        thatImage = transform.Find("MagicCanvas").Find("MagicBackground").Find("MagicImage").GetComponent<Image>();
        frame = transform.Find("MagicCanvas").GetComponent<Image>();
        numberOfCardsInDeck = PlayerDeck.deckSize;

        canBeActivated = false;
        activated = false;

        drawX = 0;

        Enemy = GameObject.Find("OpponentLP");
        PlayerSlots = GameObject.Find("PlayerSlots");
        EnemySlots = GameObject.Find("EnemySlots");
        targeting = false;
        targetingEnemy = false;
    }

    void Update()
    {
        PlayerArea = GameObject.Find("PlayerArea");
        if (this.transform.parent == PlayerArea.transform.parent)
        {
            cardBack = false;
        }

        id = thisMagic[0].id;
        magicName = thisMagic[0].cardName;
        magicDescription = thisMagic[0].cardDescription;
        thisSprite = thisMagic[0].thisImage;

        drawXcards = thisMagic[0].drawXcards;
        returnXcards = thisMagic[0].returnXcards;

        damageHealedBySpell = thisMagic[0].damageHealedBySpell;
        damageDealtBySpell = thisMagic[0].damageDealtBySpell;

        magicnameText.text = "" + magicName;
        magicdescriptionText.text = "" + magicDescription;

        thatImage.sprite = thisSprite;

        targetDestroy = thisMagic[0].targetDestroy;
        changeAttack = thisMagic[0].changeAttack;
        changeDefense = thisMagic[0].changeDefense;
        equip = thisMagic[0].equip;
        equipBoost = thisMagic[0].equipBoost;

        if (thisMagic[0].color == "None")
        {
            frame.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        if (thisMagic[0].color == "Brown")
        {
            frame.GetComponent<Image>().color = new Color32(156, 73, 0, 255);
        }
        if (thisMagic[0].color == "Red")
        {
            frame.GetComponent<Image>().color = new Color32(255, 0, 0, 255);
        }
        if (thisMagic[0].color == "Magic")
        {
            frame.GetComponent<Image>().color = new Color32(62, 69, 90, 255);
        }

        staticCardBack = cardBack;

        /*if (this.tag == "Clone")
        {
            thisMagic[0] = PlayerDeck.staticDeck[numberOfCardsInDeck - 1];
            numberOfCardsInDeck -= 1;
            PlayerDeck.deckSize -= 1;
            cardBack = false;
            this.tag = "Untagged";
        }*/

        if (tag != "Unusable")
        {

            if (activated == false && beInGraveyard == false && equip == false && targetDestroy == false && changeAttack == false && changeDefense == false)
            {
                canBeActivated = true;
            }
            else if (equip)
            {
                foreach (Transform child in PlayerSlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>() != null)
                        {
                            canBeActivated = true;
                        }
                    }
                }
            }
            else if (targetDestroy == true || changeAttack == true || changeDefense == true)
            {
                foreach (Transform child in EnemySlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>() != null)
                        {
                            canBeActivated = true;
                        }
                    }
                }
            }
            else canBeActivated = false;

            if (canBeActivated)
            {
                gameObject.GetComponent<DragDrop>().enabled = true;
            }
            else gameObject.GetComponent<DragDrop>().enabled = false;

            battleZone = GameObject.Find("PlayerSlots");

            if (activated == false && this.transform.parent.transform.parent == battleZone.transform)
            {
                Activate();
                drawX = drawXcards;
            }
            if (activated == true && PlayerManager.IsMyTurn == false && beInGraveyard == false && equip == false)
            {
                activationcomplete = true;
                StartCoroutine(SmoothDestruction(2));
            }
            /*
            if (canAttack == true && beInGraveyard == false && activated == true)
            {
                attackBorder.SetActive(true);
            }
            else
            {
                attackBorder.SetActive(false);
            }

            if (PlayerManager.IsMyTurn == false && activated == true)
            {
                cantAttack = false;
            }

            if (PlayerManager.IsMyTurn == true && cantAttack == false && this.transform.parent.transform.parent == battleZone.transform && GameManager.turn != 0)
            {
                canAttack = true;
            }
            else
            {
                canAttack = false;
            }*/

            targeting = staticTargeting;
            targetingEnemy = staticTargetingEnemy;

            if (targetingEnemy)
            {
                Target = Enemy;
            }
            else
            {
                Target = null;
            }

            if (targeting == true)
            {
                TargetMonster();
            }

            IEnumerator SmoothDestruction(int sec)
            {
                yield return new WaitForSeconds(sec);
                PlayerManager.CmdPlayerDestroyCard(Magic, 0);
            }

            if (returnXcards > 0 && activated == true && useReturn == false)
            {
                Return(returnXcards);
                useReturn = true;
            }
            if (drawX > 0 && activated == true && hasAuthority == true)
            {
                PlayerManager.CmdDrawCard();
                drawX--;
                canBeDestroyed = true;
            }
            if (drawX <= 0 && activationcomplete == false && canBeDestroyed == true)//possible error if want to retrieve pot of greed from grave drawX=0
            {
                activationcomplete = true;
                StartCoroutine(SmoothDestruction(2));
            }
            if(PlayerManager.IsMyTurn == true && damageHealedBySpell > 0 && activated == true && activationcomplete == false && hasAuthority == true)
            {
                PlayerManager.CmdGMChangeLP(damageHealedBySpell, 0);
                canBeDestroyed = true;
            }
            if(PlayerManager.IsMyTurn == true && damageDealtBySpell > 0 && activated == true && activationcomplete == false && hasAuthority == true)
            {
                PlayerManager.CmdGMChangeLP(0, damageDealtBySpell);
                canBeDestroyed = true;
            }
            if (PlayerManager.IsMyTurn == false)
            {
                UcanReturn = false;
            }
            if(equippedTo!= null)
            {
                if (equippedTo.GetComponent<ThisCard>().beInGraveyard)
                {
                    canBeDestroyed = true;
                    beInGraveyard = true;
                    equippedTo = null;
                }
            }
            /*
            if (damageDealtBySpell > 0 && drawX == 100)
            {
                dealDamage = true;
            }
            if (dealDamage == true && this.transform.parent == battleZone.transform)
            {
                //attackBorder.SetActive(true);
            }
            else
            {
                //attackBorder.SetActive(false);
            }
            if (dealDamage == true && this.transform.parent == battleZone.transform)
            {
                dealxDamage(damageDealtBySpell);
            }

            if (stopDealDamage == true)
            {
                //attackBorder.SetActive(false);
                dealDamage = false;
            }

            if (this.transform.parent == battleZone.transform && dealDamage == false)
            {
                Destroy();
            }*/
            if (activationcomplete)
            {
                Destroy();
            }
        }
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
