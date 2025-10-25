using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MagicDataBase : NetworkBehaviour
{
    public GameManager GameManager;
    public GameObject Canvas;
    public PlayerManager PlayerManager;

    public static List<Magic> magicList = new List<Magic>();

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Canvas = GameObject.Find("Main Canvas");
    }
    private void Awake()
    {
        magicList.Add(new Magic(0, "None", "None", Resources.Load<Sprite>("noimg"), "None", 0, 0, 0, 0,false,false,false,false,0));
        magicList.Add(new Magic(1, "Mooyan Curry", "Gain 200 Life Points.", Resources.Load<Sprite>("soupitsa"), "Magic", 0, 0, 200, 0,false, false, false, false, 0));
        magicList.Add(new Magic(2, "Pot of Greed", "Draw 2 cards.", Resources.Load<Sprite>("pot"), "Magic", 2, 0, 0, 0,false, false, false, false, 0));
        magicList.Add(new Magic(3, "Se Mastigwnw", "Deal 500 damage.", Resources.Load<Sprite>("mastigio"), "Magic", 0, 0, 0, 500,false, false, false, false, 0));
        magicList.Add(new Magic(4, "Target Practice", "You can target one monster your opponent controls and destroy it.", Resources.Load<Sprite>("target"), "Magic", 0, 0, 0, 0,true, false, false, false, 0));
        magicList.Add(new Magic(5, "Enrage", "You can target one monster your opponent controls and change its battle position to Attack mode.", Resources.Load<Sprite>("enrage"), "Magic", 0, 0, 0, 0,false,true,false, false, 0));
        magicList.Add(new Magic(6, "Succumb", "You can target one monster your opponent controls and change its battle position to Defense mode.", Resources.Load<Sprite>("succumb"), "Magic", 0, 0, 0, 0,false,false,true, false, 0));
        magicList.Add(new Magic(7, "Divine Sword", "Equip this card to one of your monsters and it gains 400 ATK.", Resources.Load<Sprite>("divinesword"), "Magic", 0, 0, 0, 0,false,false,false, true, 400));
        magicList.Add(new Magic(8, "Blastigator", "Equip this card to one of your monsters and it gains 300 ATK.", Resources.Load<Sprite>("blastigator"), "Magic", 0, 0, 0, 0,false,false,false, true, 300));
        magicList.Add(new Magic(9, "Exhaust", "Choose one monster your opponent controls: that monster loses 400 ATK points until the end of the turn.", Resources.Load<Sprite>("exhaust"), "Magic", 0, 0, 0, 0,false,false,false, true, -400));
        magicList.Add(new Magic(10, "Teleport", "You can target one monster you control that has not moved this turn, move it vertically up or down up to 3 rows, while keeping it in the same column.", Resources.Load<Sprite>("teleport"), "Magic", 0, 0, 0, 0,false,false,false, true, -400));
        magicList.Add(new Magic(11, "Crystaline Enhancement", "A mineral type monster equipped with this card gains 400ATK and 400DEF", Resources.Load<Sprite>("crystaline_enhancement"), "Magic", 0, 0, 0, 0, false, false, false, true, 400));
        magicList.Add(new Magic(12, "Neutron Blaster", "An alien type monster equipped with this card gains 400ATK and 400DEF", Resources.Load<Sprite>("neutron_blaster"), "Magic", 0, 0, 0, 0,false,false,false, true, 400));
        magicList.Add(new Magic(13, "Axe of Ancient Champions", "A human type monster equipped with this card gains 400ATK and 400DEF", Resources.Load<Sprite>("axe_of_ancient_champions"), "Magic", 0, 0, 0, 0,false,false,false, true, 400));
        magicList.Add(new Magic(14, "Beast Claws", "An animal type monster equipped with this card gains 400ATK and 400DEF", Resources.Load<Sprite>("beast_claws"), "Magic", 0, 0, 0, 0,false,false,false, true, 400));
        magicList.Add(new Magic(15, "Labyrinth Love", "Gain Life Points equal to the ATK or DEF of the monster that stepped on the treasure square. (whichever is highest)", Resources.Load<Sprite>("labyrinth_love"), "Magic", 0, 0, 500, 0, false, false, false, false, 0));
        magicList.Add(new Magic(16, "Labyrinth Dice", "Select one monster on the field and move it to a random space on the labyrinth. (You can keep this card and treat it as a magic card if you do not have any other labyrinth cards).", Resources.Load<Sprite>("labyrinth_dice"), "Magic", 0, 0, 0, 0, false, false, false, false, 0));
        magicList.Add(new Magic(17, "Sprint Boost", "Select one monster on your side of the field and increase its 'Square' by 4 until the end of the turn. Other monsters can't attack nor move this turn.", Resources.Load<Sprite>("sprint_boost"), "Magic", 0, 0, 0, 0, false, false, false, false, 0));
        magicList.Add(new Magic(18, "Mechanical Legs", "A monster equipped with this card loses the property 'Immobile'.", Resources.Load<Sprite>("mechanical_legs"), "Magic", 0, 0, 0, 0, false, false, false, true, 0));
        magicList.Add(new Magic(19, "Fleetfoot Blessing", "The equipped monster gains +2 square.", Resources.Load<Sprite>("fleetfoot_blessing"), "Magic", 0, 0, 0, 0, false, false, false, true, 0));
        magicList.Add(new Magic(20, "Weighted Shackles", "The equipped monster gains -2 square.", Resources.Load<Sprite>("weighted_shackles"), "Magic", 0, 0, 0, 0, false, false, false, true, 0));
        magicList.Add(new Magic(21, "Fatal Square", "Select one empty square and until the end of the next turn the first monster that stays on that square is destroyed.", Resources.Load<Sprite>("fatal_square"), "Magic", 0, 0, 0, 0, false, false, false, true, 0));
        magicList.Add(new Magic(22, "Labyrinth Lootbox", "Draw 2 cards from your deck.", Resources.Load<Sprite>("labyrinth_lootbox"), "Magic", 2, 0, 0, 0, false, false, false, false, 0));
    }
}
