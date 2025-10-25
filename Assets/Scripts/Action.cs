using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Action
{
    public int id;
    public string cardName;
    public string cardRequirement;
    public string cardDescription;

    public Sprite thisImage;

    public string color;

    public Action()
    {

    }

    public Action(int Id, string CardName,string CardRequirement, string CardDescription, Sprite ThisImage, string Color)
    {
        id = Id;
        cardName = CardName;
        cardRequirement = CardRequirement;
        cardDescription = CardDescription;

        thisImage = ThisImage;

        color = Color;
    }
}
