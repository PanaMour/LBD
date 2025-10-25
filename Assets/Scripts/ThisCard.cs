using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ThisCard : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;
    public GameObject Card;
    public List<Card> thisCard = new List<Card>();
    public GameObject LabyrinthObject;

    [SyncVar]
    public int thisId;

    public int id;
    public string cardName;
    public int stars;
    public int atk;
    public int def;
    public string cardDescription;

    public Text nameText;
    public Text starsText;
    public Text ATKText;
    public Text DEFText;
    public Text descriptionText;

    public Sprite thisSprite;
    public Image thatImage;

    public Image frame;

    public bool cardBack;
    public static bool staticCardBack;

    public GameObject PlayerArea;

    public int numberOfCardsInDeck;

    public bool canBeSummoned;
    public bool summoned;
    public GameObject battleZone;///d///////////////////

    public static int drawX;
    public int drawXcards;

    public GameObject attackBorder;

    public GameObject Target;
    public GameObject Enemy;

    public bool cantAttack;

    public bool canAttack;

    public static bool staticTargeting;
    public static bool staticTargetingEnemy;

    public bool targeting;
    public bool targetingEnemy;

    public bool onlyThisCardAttack;

    public bool canBeDestroyed;
    //public GameObject Graveyard;
    public bool beInGraveyard;

    public int decreased;
    public int actualATK;
    public int returnXcards;
    public bool useReturn;

    public static bool UcanReturn;

    public bool isTarget;
    public GameObject PlayerSlots;
    public GameObject EnemySlots;
    public bool monstersExist;

    public bool spell;//-////////////////////////////////////////////////////////////////////
    public int damageDealtBySpell;///-////////////////////////////////////////////////////

    public bool dealDamage;
    public bool stopDealDamage;

    public bool canBeTributed;
    public bool confirmationfinished = false;

    public bool attackmode = true;
    public bool changemode = false;
    public bool alreadychanged = false;

    public bool faceup = false;
    public int boost = 0;
    public bool boosted = false;
    public GameObject equippedTo;

    public bool initialized = false;

    public bool canMove = true;
    public bool hasMoved = false;

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (this.tag != "Unusable") //error in console with unusable cards
        {
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        }

        thisCard[0] = CardDataBase.cardList[thisId];

        nameText = transform.Find("CardCanvas").Find("Background").Find("CardName").Find("NameText").GetComponent<Text>();
        starsText = transform.Find("CardCanvas").Find("Background").Find("Stars").Find("StarsText").GetComponent<Text>();
        ATKText = transform.Find("CardCanvas").Find("Background").Find("ATK").Find("ATKtext").GetComponent<Text>();
        DEFText = transform.Find("CardCanvas").Find("Background").Find("DEF").Find("DEFtext").GetComponent<Text>();
        descriptionText = transform.Find("CardCanvas").Find("Background").Find("CardDescription").Find("DescriptionText").GetComponent<Text>();
        thatImage = transform.Find("CardCanvas").Find("Background").Find("Image").GetComponent<Image>();
        frame = transform.Find("CardCanvas").GetComponent<Image>();
        attackBorder = transform.Find("Attack").transform.gameObject;
        numberOfCardsInDeck = PlayerDeck.deckSize;

        canBeSummoned = false;
        summoned = false;

        drawX = 0;

        canAttack = false;

        Enemy = GameObject.Find("OpponentLP");
        EnemySlots = GameObject.Find("EnemySlots");
        PlayerSlots = GameObject.Find("PlayerSlots");
        targeting = false;
        targetingEnemy = false;
    }
    /*void OnMouseMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Right Click on " + cardName);/////////////////////////////////////////////////////////////////
        }
    }*/

    void Update()
    {
        PlayerArea = GameObject.Find("PlayerArea");
        if (this.transform.parent == PlayerArea.transform.parent)
        {
            cardBack = false;
        }

        if (!initialized)
        {
            id = thisCard[0].id;
            cardName = thisCard[0].cardName;
            stars = thisCard[0].stars;
            atk = thisCard[0].atk;
            def = thisCard[0].def;
            cardDescription = thisCard[0].cardDescription;
            thisSprite = thisCard[0].thisImage;

            drawXcards = thisCard[0].drawXcards;
            returnXcards = thisCard[0].returnXcards;

            spell = thisCard[0].spell;
            damageDealtBySpell = thisCard[0].damageDealtBySpell;

            canBeTributed = thisCard[0].canBeTributed;
            initialized = true;
        }

        nameText.text = "" + cardName;
        starsText.text = "" + stars;
        actualATK = atk - decreased;
        ATKText.text = "" + atk;
        DEFText.text = "" + def;
        descriptionText.text = "" + cardDescription;

        thatImage.sprite = thisSprite;

        if (thisCard[0].color == "None")
        {
            frame.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        if (thisCard[0].color == "Brown")
        {
            frame.GetComponent<Image>().color = new Color32(156, 73, 0, 255);
        }
        if (thisCard[0].color == "Red")
        {
            frame.GetComponent<Image>().color = new Color32(255, 0, 0, 255);
        }
        if (thisCard[0].color == "Magic")
        {
            frame.GetComponent<Image>().color = new Color32(19, 138, 102, 255);
        }

        staticCardBack = cardBack;

        if (this.tag == "Clone")
        {
            thisCard[0] = PlayerDeck.staticDeck[numberOfCardsInDeck - 1];
            numberOfCardsInDeck -= 1;
            PlayerDeck.deckSize -= 1;
            cardBack = false;
            this.tag = "Untagged";
        }

        if (tag != "Unusable")
        {

            if (stars <= 4 && summoned == false && beInGraveyard == false)
            {
                canBeSummoned = true;
            }
            else canBeSummoned = false;

            if (stars >= 5 && stars <= 6 && summoned == false && beInGraveyard == false)
            {
                foreach (Transform child in PlayerSlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>() != null)
                        {
                            if (grandChild.GetComponent<ThisCard>().canBeTributed)
                            {
                                canBeSummoned = true;
                            }
                        }
                    }
                }
            }
            if (canBeSummoned)
            {
                gameObject.GetComponent<DragDrop>().enabled = true;
            }
            else gameObject.GetComponent<DragDrop>().enabled = false;

            battleZone = GameObject.Find("PlayerSlots");

            if ((summoned == false && this.transform.parent.transform.parent == battleZone.transform) || confirmationfinished == true)
            {
                Summon();
                Debug.Log(cardName + " was summoned successfully!");
                alreadychanged = true;
                PlayerManager.nomoresummons = true;
                drawX = drawXcards;
                confirmationfinished = false;
            }

            if (canAttack == true && attackmode == true && beInGraveyard == false && summoned == true)
            {
                attackBorder.SetActive(true);
                canBeTributed = true;
            }
            else if (attackmode == false && beInGraveyard == false && summoned == true)
            {
                attackBorder.SetActive(false);
                canBeTributed = true;
            }
            else
            {
                attackBorder.SetActive(false);
                canBeTributed = false;
            }
            if (beInGraveyard)
            {
                if(!attackmode)
                transform.Rotate(0, 0, -90);
                attackmode = true;
            }
            if (PlayerManager.IsMyTurn == false && summoned == true)
            {
                cantAttack = false;
                hasMoved = false;
            }

            if (PlayerManager.IsMyTurn == true && cantAttack == false && attackmode == true && this.transform.parent.transform.parent == battleZone.transform && GameManager.turn != 0)
            {
                canAttack = true;
            }
            else
            {
                canAttack = false;
            }

            if (PlayerManager.IsMyTurn == true && attackmode == true && this.transform.parent.transform.parent == battleZone.transform && hasMoved == false)
            {
                canMove = true;
            }
            else
            {
                canMove = false;
            }

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

            if (targeting == true /*&& targetingEnemy == true*/ && onlyThisCardAttack == true)
            {
                Attack();
            }

            if (actualATK <= 0)
            {
                Destroy();
            }

            if (returnXcards > 0 && summoned == true && useReturn == false)
            {
                Return(returnXcards);
                useReturn = true;
            }
            if (drawX > 0 && summoned == true && beInGraveyard == false)
            {
                PlayerManager.CmdDrawCard();
                drawX--;
            }
            if(PlayerManager.IsMyTurn == false)
            {
                UcanReturn = false;
            }
            if(damageDealtBySpell > 0)
            {
                dealDamage = true;
            }
            if (dealDamage == true && this.transform.parent == battleZone.transform)
            {
                attackBorder.SetActive(true);
            }
            else
            {
                //attackBorder.SetActive(false);
            }
            if (dealDamage == true && this.transform.parent == battleZone.transform)
            {
                dealxDamage(damageDealtBySpell);
            }

            if(stopDealDamage == true)
            {
                attackBorder.SetActive(false);
                dealDamage = false;
            }

            if(this.transform.parent == battleZone.transform && spell == true && dealDamage == false)
            {
                Destroy();
            }

            if (attackmode == true && changemode == true)
            {
                transform.Rotate(0, 0, 90);
                changemode = false;
            }
            else if (attackmode != true && changemode == true)
            {
                transform.Rotate(0, 0, -90);
                changemode = false;
            }
            if (!PlayerManager.IsMyTurn)
            {
                alreadychanged = false;
            }
            if (boost > 0 && boosted == false)
            {
                atk = atk + boost;
                actualATK = atk;
                boosted = true;
            }
            if (equippedTo != null)
            {
                if (equippedTo.GetComponent<ThisMagic>().beInGraveyard)
                {
                    atk = atk - boost;
                    actualATK = atk;
                    boost = 0;
                    equippedTo = null;
                    boosted = false;
                }
            }
        }
    }

    public void Summon()
    {
        summoned = true;
        faceup = true;
    }

    public void Attack()
    {
        if (canAttack == true && summoned == true && spell == false)
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
                    if (!monstersExist)
                    {
                        PlayerManager.CmdGMChangeLP(0, atk);
                        targeting = false;
                        cantAttack = true;
                        hasMoved = true;
                    }
                }
            }
            else
            {
                foreach(Transform child in EnemySlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ThisCard>().isTarget == true)
                        {
                            grandChild.GetComponent<ThisCard>().decreased = atk;
                            decreased = grandChild.GetComponent<ThisCard>().atk;
                            cantAttack = true;
                            hasMoved = true;
                            if (grandChild.GetComponent<ThisCard>().attackmode)
                            {
                                if (grandChild.GetComponent<ThisCard>().atk < this.atk)
                                {
                                    PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                    PlayerManager.CmdGMChangeLP(0, this.atk - grandChild.GetComponent<ThisCard>().atk);
                                }
                                else if (grandChild.GetComponent<ThisCard>().atk > this.atk)
                                {
                                    PlayerManager.CmdPlayerDestroyCard(Card, 0);
                                    PlayerManager.CmdGMChangeLP(this.atk - grandChild.GetComponent<ThisCard>().atk, 0);
                                }
                                else
                                {
                                    PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                    PlayerManager.CmdPlayerDestroyCard(Card, 0);
                                }
                            }
                            else if (!grandChild.GetComponent<ThisCard>().attackmode)
                            {
                                if (grandChild.GetComponent<ThisCard>().def < this.atk)
                                {
                                    PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                }
                                else if (grandChild.GetComponent<ThisCard>().def > this.atk)
                                {
                                    PlayerManager.CmdGMChangeLP(this.atk - grandChild.GetComponent<ThisCard>().def, 0);
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
            }
        }
        alreadychanged = true;
    }
    public void UntargetEnemy()
    {
        staticTargetingEnemy = false; 
    }

    public void TargetEnemy()
    {
        staticTargetingEnemy = true;
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
        onlyThisCardAttack = true;
    }

    public void OneCardAttackStop()
    {
        onlyThisCardAttack = false;
    }

    public void Destroy()
    {
        canBeDestroyed = false;
        summoned = false;
        beInGraveyard = true;
        decreased = 0;
    }

    public void Return(int x)
    {
        for(int i = 0; i <= x; i++)
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
    }
}
