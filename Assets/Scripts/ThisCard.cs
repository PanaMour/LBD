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
    public List<Type> currentTypes = new List<Type>();
    public List<Attribute> currentAttributes = new List<Attribute>();

    public Text nameText;
    public Text starsText;
    public Text ATKText;
    public Text DEFText;
    public Text descriptionText;
    public Text typeLineText;
    public RectTransform typeLinePlate;
    public Image typeLinePlateImage;

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
    public int actualDEF;
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

    [SyncVar]
    public bool attackmode = true;
    public bool changemode = false;
    public bool alreadychanged = false;

    // Generic "once per duel" flag for a monster's own activated ability
    // (e.g. Shy Magician's position exchange). Not synced to non-owning
    // clients automatically -- effects that set it should RPC the change
    // the same way SetImmobile/RemoveImmobile do.
    public bool abilityUsed = false;

    // Set by the elemental/Labyrinth Spirits' tribute ability ("grant
    // another monster you control this card's properties"). Kept separate
    // from cardProperty (rather than overwriting it) so a monster that
    // already has its own property -- Plague, say -- doesn't lose it just
    // because it was also granted Wallwalk. GridBehavior's wall-crossing
    // checks treat this the same as cardProperty == Property.Wallwalk.
    public bool grantedWallwalk = false;

    public bool faceup = false;
    public int boost = 0;
    public bool boosted = false;
    public GameObject equippedTo;

    public bool initialized = false;

    public bool canMove = true;
    public bool hasMoved = false;

    [SyncVar]
    public bool isImmobile;

    public int auraAtk = 0;
    public int auraDef = 0;
    public int tempAtk = 0;
    public int tempDef = 0;

    public Property cardProperty;
    public int plagueAtkLoss = 0;
    public int plagueDefLoss = 0;

    public int battleDefPenalty = 0;

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

    void Update()
    {
        if (PlayerArea == null) PlayerArea = GameObject.Find("Hand_Anchor");
        if (battleZone == null) battleZone = GameObject.Find("PlayerSlots");
        if (PlayerSlots == null) PlayerSlots = GameObject.Find("PlayerSlots");

        if (this.transform.parent != null && PlayerArea != null)
        {
            if (this.transform.parent == PlayerArea.transform)
            {
                cardBack = false;
            }
        }

        if (thisId != 0 && thisId != id)
        {
            if (thisId < CardDataBase.cardList.Count)
            {
                thisCard.Clear();
                thisCard.Add(CardDataBase.cardList[thisId]);
                initialized = false;
            }
        }

        if (!initialized && thisCard.Count > 0)
        {
            id = thisCard[0].id;
            cardName = thisCard[0].cardName;
            stars = thisCard[0].stars;
            atk = thisCard[0].atk;
            def = thisCard[0].def;
            cardDescription = thisCard[0].cardDescription;
            cardProperty = thisCard[0].property;
            thisSprite = thisCard[0].thisImage;
            drawXcards = thisCard[0].drawXcards;
            returnXcards = thisCard[0].returnXcards;
            spell = thisCard[0].spell;
            damageDealtBySpell = thisCard[0].damageDealtBySpell;
            canBeTributed = thisCard[0].canBeTributed;
            initialized = true;
        }

        if (initialized)
        {
            nameText.text = "" + cardName;
            starsText.text = "" + stars;
            actualATK = atk + boost + auraAtk + tempAtk - plagueAtkLoss - decreased;
            actualDEF = def + tempDef + auraDef - plagueDefLoss - battleDefPenalty;
            if (actualATK < 0) actualATK = 0;
            if (actualDEF < 0) actualDEF = 0;
            ATKText.text = "" + actualATK;
            DEFText.text = "" + actualDEF;
            descriptionText.text = "" + cardDescription;
            if (thisSprite != null) thatImage.sprite = thisSprite;

            currentTypes.Clear();
            currentAttributes.Clear();
            currentTypes.Add(thisCard[0].type);
            currentAttributes.Add(thisCard[0].attribute);
            if (summoned && !beInGraveyard)
            {
                if (id == 39)
                {
                    if (!currentTypes.Contains(Type.Robot)) currentTypes.Add(Type.Robot);
                }
            }
            if (thisCard[0].color == "None") frame.color = new Color32(255, 255, 255, 255);
            else if (thisCard[0].color == "Brown") frame.color = new Color32(156, 73, 0, 255);
            else if (thisCard[0].color == "Red") frame.color = new Color32(255, 0, 0, 255);
            else if (thisCard[0].color == "Magic") frame.color = new Color32(19, 138, 102, 255);

            if (typeLineText == null)
            {
                Transform typeLine = transform.Find("CardCanvas/Background/TypeLine");
                if (typeLine != null)
                {
                    typeLinePlate = typeLine.GetComponent<RectTransform>();
                    typeLinePlateImage = typeLine.GetComponent<Image>();
                    Transform inner = typeLine.Find("TypeLineText");
                    if (inner != null) typeLineText = inner.GetComponent<Text>();
                }
            }
            if (typeLineText != null)
            {
                typeLineText.text = BuildTypeLine();

                if (typeLinePlateImage != null && currentAttributes.Count > 0)
                    typeLinePlateImage.color = AttributeColor(currentAttributes[0]);

                // The bar is top-pivoted and the artwork leaves room for two
                // lines, so a long type line wraps instead of shrinking the
                // font and making cards inconsistent with each other.
                if (typeLinePlate != null)
                {
                    float needed = typeLineText.preferredHeight + 2f;
                    if (needed < 9f) needed = 9f;
                    if (typeLinePlate.sizeDelta.y != needed)
                        typeLinePlate.sizeDelta = new Vector2(typeLinePlate.sizeDelta.x, needed);
                }
            }
        }

        if (summoned && transform.parent != null)
        {
            if (attackmode)
            {
                transform.localRotation = Quaternion.Euler(90, 0, 0);
                transform.localScale = new Vector3(0.01f, 0.0075f, 0.01f);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(90, 0, 90);
                transform.localScale = new Vector3(0.0075f, 0.01f, 0.01f);
            }
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
            if (summoned && !beInGraveyard && IsInsideOwnCardBase())
            {
                canBeTributed = true;
            }
            else
            {
                canBeTributed = false;
            }

            bool tributeAvailable = false;

            if (stars >= 5 && !summoned && !beInGraveyard)
            {
                ThisCard[] allCardsOnBoard = FindObjectsOfType<ThisCard>();
                foreach (ThisCard c in allCardsOnBoard)
                {
                    if (c.hasAuthority && c.summoned && !c.beInGraveyard && c.canBeTributed)
                    {
                        tributeAvailable = true;
                        break;
                    }
                }
            }

            if (summoned == false && beInGraveyard == false)
            {
                if (PlayerManager.nomoresummons)
                {
                    canBeSummoned = false;
                }
                else
                {
                    if (stars <= 4)
                    {
                        canBeSummoned = true;
                    }
                    else if (stars >= 5 && stars <= 6)
                    {
                        canBeSummoned = tributeAvailable;
                    }
                    else if (stars >= 7)
                    {
                        canBeSummoned = false;
                    }
                }
            }

            DragDrop dd = gameObject.GetComponent<DragDrop>();
            if (dd != null)
            {
                dd.enabled = canBeSummoned;
            }

            bool isInBattleZone = false;
            if (this.transform.parent != null && this.transform.parent.parent != null && battleZone != null)
            {
                if (this.transform.parent.parent == battleZone.transform) isInBattleZone = true;
            }

            HandleStatusBorders(isInBattleZone);
            HandleTurnLogic(isInBattleZone);
            HandleDamageAndDestroy();
            HandleBoosts();
        }
    }

    bool IsInsideOwnCardBase()
    {
        LabyrinthObject[] allTokens = FindObjectsOfType<LabyrinthObject>();
        LabyrinthObject myToken = null;

        foreach (LabyrinthObject token in allTokens)
        {
            if (token.card == gameObject)
            {
                myToken = token;
                break;
            }
        }

        if (myToken == null) return true;

        GridStat myTile = myToken.GetComponentInParent<GridStat>();
        if (myTile == null) return true;

        // Matches the summon-placement rule in GridBehavior.cs: the host's cards
        // belong at row 0, the joining client's cards belong at row 15.
        bool ownerIsHost = (hasAuthority == NetworkServer.active);
        int homeRow = ownerIsHost ? 0 : 15;

        return myTile.y == homeRow && myTile.x >= 2 && myTile.x <= 8;
    }

    public string DebugCardBaseInfo()
    {
        LabyrinthObject[] allTokens = FindObjectsOfType<LabyrinthObject>();
        LabyrinthObject myToken = null;

        foreach (LabyrinthObject token in allTokens)
        {
            if (token.card == gameObject)
            {
                myToken = token;
                break;
            }
        }

        if (myToken == null) return "no LabyrinthObject token found for " + cardName;

        GridStat myTile = myToken.GetComponentInParent<GridStat>();
        if (myTile == null) return "token found but no GridStat parent for " + cardName;

        bool ownerIsHost = (hasAuthority == NetworkServer.active);
        int homeRow = ownerIsHost ? 0 : 15;

        return cardName + " tile=(" + myTile.x + "," + myTile.y + ") homeRow=" + homeRow
            + " hasAuthority=" + hasAuthority + " NetworkServer.active=" + NetworkServer.active
            + " ownerIsHost=" + ownerIsHost + " IsInsideOwnCardBase=" + IsInsideOwnCardBase();
    }

    void HandleStatusBorders(bool isInBattleZone)
    {
        if (summoned && !beInGraveyard)
        {
            attackBorder.SetActive(canAttack && attackmode);
            canBeTributed = IsInsideOwnCardBase();
        }
        else
        {
            attackBorder.SetActive(false);
            if (!summoned) canBeTributed = false;
        }

        if (beInGraveyard && !attackmode)
        {
            transform.Rotate(0, 0, -90);
            attackmode = true;
        }
    }

    void HandleTurnLogic(bool isInBattleZone)
    {
        if (PlayerManager.IsMyTurn == false)
        {
            if (summoned) { cantAttack = false; hasMoved = false; }
            UcanReturn = false;
            alreadychanged = false;
        }

        canAttack = (PlayerManager.IsMyTurn && !cantAttack && attackmode && isInBattleZone && GameManager.turn != 0);
        canMove = (PlayerManager.IsMyTurn && attackmode && isInBattleZone && !hasMoved);

        targeting = staticTargeting;
        targetingEnemy = staticTargetingEnemy;
        Target = targetingEnemy ? Enemy : null;

        if (targeting && onlyThisCardAttack) Attack();
    }

    void HandleDamageAndDestroy()
    {
        if (actualATK <= 0 && initialized) Destroy();

        if (returnXcards > 0 && summoned && !useReturn)
        {
            Return(returnXcards);
            useReturn = true;
        }

        if (drawX > 0 && summoned && !beInGraveyard)
        {
            PlayerManager.CmdDrawCard();
            drawX--;
        }

        if (damageDealtBySpell > 0) dealDamage = true;
    }

    void HandleBoosts()
    {
        if (boost > 0 && !boosted)
        {
            boosted = true;
        }
        if (equippedTo != null)
        {
            ThisMagic tm = equippedTo.GetComponent<ThisMagic>();
            if (tm != null && tm.beInGraveyard)
            {
                boost = 0;
                equippedTo = null;
                boosted = false;
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
                    if (!monstersExist)
                    {
                        PlayerManager.CmdGMChangeLP(0, actualATK);
                        targeting = false;
                        cantAttack = true;
                        hasMoved = true;
                    }
                }
            }
            else
            {
                foreach (Transform child in EnemySlots.transform)//child.child
                {
                    foreach (Transform grandChild in child)
                    {
                        ThisCard enemyCard = grandChild.GetComponent<ThisCard>();
                        if (enemyCard != null && enemyCard.isTarget == true)
                        {
                            enemyCard.decreased = actualATK;
                            decreased = enemyCard.actualATK;
                            cantAttack = true;
                            hasMoved = true;

                            if (enemyCard.attackmode) // Enemy is in Attack Mode
                            {
                                if (enemyCard.actualATK < this.actualATK)
                                {
                                    if (enemyCard.cardProperty == Property.Plague)
                                    {
                                        PlayerManager.CmdApplyPlagueDebuff(this.gameObject);
                                    }

                                    PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                    PlayerManager.CmdGMChangeLP(0, this.actualATK - enemyCard.actualATK);
                                }
                                else if (enemyCard.actualATK > this.actualATK)
                                {
                                    if (this.cardProperty == Property.Plague)
                                    {
                                        PlayerManager.CmdApplyPlagueDebuff(grandChild.gameObject);
                                    }

                                    PlayerManager.CmdPlayerDestroyCard(Card, 0);
                                    PlayerManager.CmdGMChangeLP(this.actualATK - enemyCard.actualATK, 0);
                                }
                                else
                                {
                                    PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                    PlayerManager.CmdPlayerDestroyCard(Card, 0);
                                }
                            }
                            else if (!enemyCard.attackmode) // Enemy is in Defense Mode
                            {
                                if (enemyCard.actualDEF < this.actualATK)
                                {
                                    if (enemyCard.cardProperty == Property.Plague)
                                    {
                                        PlayerManager.CmdApplyPlagueDebuff(this.gameObject);
                                    }

                                    PlayerManager.CmdOpponentDestroyCard(grandChild.gameObject, 0);
                                }
                                else if (enemyCard.actualDEF > this.actualATK)
                                {
                                    PlayerManager.CmdGMChangeLP(this.actualATK - enemyCard.actualDEF, 0);
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

    [Command]
    public void CmdSetBattleMode(bool isAttack)
    {
        attackmode = isAttack;
    }

    public void ActivateSummonEffects()
    {
        drawX = drawXcards;
        useReturn = false;
    }

    // Tints the type bar by attribute so a card is identifiable at a glance in
    // hand, where the text is too small to read. All values are mid-to-light so
    // the near-black label stays legible on top of them.
    public static Color AttributeColor(Attribute a)
    {
        switch (a)
        {
            case Attribute.Fire: return new Color(0.92f, 0.45f, 0.30f);
            case Attribute.Water: return new Color(0.45f, 0.68f, 0.90f);
            case Attribute.Ice: return new Color(0.66f, 0.88f, 0.94f);
            case Attribute.Nature: return new Color(0.48f, 0.75f, 0.38f);
            case Attribute.Toxic: return new Color(0.70f, 0.85f, 0.30f);
            case Attribute.Dark: return new Color(0.58f, 0.52f, 0.68f);
            case Attribute.Radiant: return new Color(0.97f, 0.85f, 0.40f);
            case Attribute.Aerial: return new Color(0.72f, 0.85f, 0.95f);
            case Attribute.Labyrinth: return new Color(0.80f, 0.72f, 0.55f);
        }
        return new Color(0.594f, 0.594f, 0.594f);
    }

    // Reads the live currentTypes/currentAttributes lists rather than the base
    // card, so runtime additions (Cyber Ninja gaining Robot once summoned) show
    // up. Granted properties live in their own fields instead of overwriting
    // cardProperty -- the Spirits' tribute sets grantedWallwalk, Frost Wraith
    // and Honey Snare set isImmobile -- so they are folded in here, otherwise
    // the card would keep advertising its printed property after an ability
    // changed what it actually does.
    string BuildTypeLine()
    {
        string types = "";
        for (int i = 0; i < currentTypes.Count; i++)
            types += (i > 0 ? "/" : "") + currentTypes[i];

        string attributes = "";
        for (int i = 0; i < currentAttributes.Count; i++)
            attributes += (i > 0 ? "/" : "") + currentAttributes[i];

        // The Labyrinth cards are Type.Labyrinth AND Attribute.Labyrinth, which
        // would otherwise print as a redundant "Labyrinth / Labyrinth".
        string line = (types == attributes) ? types : types + " / " + attributes;

        // Gained properties are prefixed with "+" so a player can tell them
        // apart from the card's printed one -- "[Plague, +Immobile]" reads as
        // Plague printed, Immobile granted by some effect.
        //
        // Immobile is a special case: unlike the other printed properties, it
        // has a live on/off flag (isImmobile) because Mechanical Legs can
        // remove it without changing what's printed on the card. So a printed
        // Immobile only shows while isImmobile is still true -- once
        // Mechanical Legs clears it, the label drops "Immobile" entirely
        // instead of continuing to claim the monster can't move.
        string props = "";
        if (cardProperty != Property.None && cardProperty != Property.Immobile)
            props += cardProperty.ToString();
        else if (cardProperty == Property.Immobile && isImmobile)
            props += cardProperty.ToString();

        if (grantedWallwalk && cardProperty != Property.Wallwalk)
            props += (props.Length > 0 ? ", " : "") + "+" + Property.Wallwalk;
        if (isImmobile && cardProperty != Property.Immobile)
            props += (props.Length > 0 ? ", " : "") + "+" + Property.Immobile;

        if (props.Length > 0) line += " [" + props + "]";
        return line;
    }

    public void RecalculateStats()
    {
        actualATK = atk + boost + auraAtk + tempAtk - plagueAtkLoss - decreased;
        // Must match Update()'s formula below -- this omitted tempDef, so any
        // effect that needs a temp DEF boost reflected synchronously (Last
        // Stand Barrier) would see it silently vanish whenever this ran.
        actualDEF = def + tempDef + auraDef - plagueDefLoss - battleDefPenalty;

        if (actualATK < 0) actualATK = 0;
        if (actualDEF < 0) actualDEF = 0;

        if (ATKText != null) ATKText.text = "" + actualATK;
        if (DEFText != null) DEFText.text = "" + actualDEF;
    }
}
