using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MagicType
{
    Normal,
    Armor
}

[System.Serializable]

public class Magic
{
    public int id;
    public string cardName;
    public string cardDescription;

    public int drawXcards;

    public Sprite thisImage;

    public string color;
    public int returnXcards;

    public int damageHealedBySpell;
    public int damageDealtBySpell;

    public bool targetDestroy;

    public bool changeAttack;
    public bool changeDefense;

    public bool equip;
    public int equipBoost;

    public Magic()
    {

    }

    public Magic(int Id, string CardName, string CardDescription, Sprite ThisImage, string Color, int DrawXCards, int ReturnXcards, int DamageHealedBySpell, int DamageDealtBySpell,bool TargetDestroy,bool ChangeAttack,bool ChangeDefense, bool Equip, int EquipBoost)
    {
        id = Id;
        cardName = CardName;
        cardDescription = CardDescription;

        thisImage = ThisImage;

        color = Color;

        drawXcards = DrawXCards;

        returnXcards = ReturnXcards;

        damageHealedBySpell = DamageHealedBySpell;
        damageDealtBySpell = DamageDealtBySpell;
        targetDestroy = TargetDestroy;
        changeAttack = ChangeAttack;
        changeDefense = ChangeDefense;
        equip = Equip;
        equipBoost = EquipBoost;
    }
}
