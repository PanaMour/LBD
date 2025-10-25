using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Type
{
    Animal,
    Angel,
    Bug,
    Robot,
    Plant,
    Alien,
    Mage,
    Human,
    Undead,
    Marine,
    Spirit,
    Demon,
    Mineral,
    Acid,
    Labyrinth
}

public enum Attribute
{
    Nature,
    Ice,
    Dark,
    Fire,
    Radiant,
    Aerial,
    Water,
    Toxic,
    Labyrinth
}

public enum Property
{
    Hydrowalk,
    Voltstream,
    Flood,
    Immobile,
    Wallwalk,
    Piercing,
    Plague,
    None
}

[System.Serializable]
public class Card
{
    public int id;
    public string cardName;
    public int stars;
    public int atk;
    public int def;

    public Type type;
    public Attribute attribute;
    public Property property;

    public string cardDescription;

    public int drawXcards;

    public Sprite thisImage;

    public string color;
    public int returnXcards;

    public bool spell;
    public int damageDealtBySpell;

    public bool canBeTributed;

    public Card()
    {

    }

    public Card(int Id, string CardName, int Stars, int ATK, int DEF, Type Type, Attribute Attribute, Property Property, string CardDescription, Sprite ThisImage, string Color, int DrawXCards, int ReturnXcards, bool Spell, int DamageDealtBySpell, bool CanBeTributed)
    {
        id = Id;
        cardName = CardName;
        stars = Stars;
        atk = ATK;
        def = DEF;

        type = Type;
        attribute = Attribute;
        property = Property;

        cardDescription = CardDescription;

        thisImage = ThisImage;

        color = Color;

        drawXcards = DrawXCards;

        returnXcards = ReturnXcards;

        spell = Spell;
        damageDealtBySpell = DamageDealtBySpell;

        canBeTributed = CanBeTributed;
    }

    public Card(int Id,string CardName, int Stars, int ATK, int DEF, string CardDescription, Sprite ThisImage, string Color, int DrawXCards, int ReturnXcards, bool Spell, int DamageDealtBySpell, bool CanBeTributed)
    {
        id = Id;
        cardName = CardName;
        stars = Stars;
        atk = ATK;
        def = DEF;
        cardDescription = CardDescription;

        thisImage = ThisImage;

        color = Color;

        drawXcards = DrawXCards;

        returnXcards = ReturnXcards;

        spell = Spell;
        damageDealtBySpell = DamageDealtBySpell;

        canBeTributed = CanBeTributed;
    }
}
