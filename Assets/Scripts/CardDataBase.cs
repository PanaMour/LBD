using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardDataBase : NetworkBehaviour
{
    public GameManager GameManager;
    public GameObject Canvas;
    public PlayerManager PlayerManager;

    public static List<Card> cardList = new List<Card>();

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Canvas = GameObject.Find("Main Canvas");
    }
    private void Awake()
    {
        cardList.Add(new Card(0, "None", 10, 0, 0, Type.Labyrinth, Attribute.Labyrinth, Property.None, "None", Resources.Load <Sprite>("noimg"), "None", 0, 0, false, 0, false));
        cardList.Add(new Card(1, "Dark Goat", 4, 1000, 700, "Other goats were eaten by the Armored Lizard.", Resources.Load<Sprite>("dgoat"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(2, "Surprised Fish", 2, 500, 400, "+1 Draw. Hey Vidiano! I love you!", Resources.Load<Sprite>("sfish"), "Brown", 1, 0, false, 0, true));
        cardList.Add(new Card(4, "Ninja Squid", 1, 200, 350, "+1 Draw. He can slice bread with his bare arms.", Resources.Load<Sprite>("ninjasquid"), "Brown", 1, 1, false, 0, true));
        cardList.Add(new Card(5, "Gkinia Toad", 2, 700,500, "A toad full of gkinia.", Resources.Load<Sprite>("toad"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(6, "Feral Imp", 4, 1300,1400, "A playful little fiend that lurks in the dark.", Resources.Load<Sprite>("imp"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(3, "Pixel Monster", 5, 1800, 150, Type.Animal, Attribute.Radiant, Property.None, "Found in GoogleSearch Results®", Resources.Load<Sprite>("pixel_monster"), "Red", 0, 0, false, 0, true));
        cardList.Add(new Card(7, "Spider Snowman", 4, 1150,900, Type.Bug, Attribute.Ice, Property.None, "A snowman got bitten by a spider and transformed into this monstrosity.", Resources.Load<Sprite>("spider_snowman"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(8, "Rabbit with Carrot Ears", 4, 1000,1400, Type.Animal, Attribute.Nature, Property.None, "A rabbit that grew carrots as ears after consuming thousands of carrots. They say that when he gets mad, his carrot ears become sharper.", Resources.Load<Sprite>("rabbit_with_carrot_ears"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(9, "Dominalien", 4, 1350,800, Type.Alien, Attribute.Dark, Property.None, "An alien whose purpose is to dominate all the planets in the universe.", Resources.Load<Sprite>("dominalien"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(10, "Fiery Banana Warrior", 4, 1200,400, Type.Plant, Attribute.Fire, Property.None, "Humans burned a forest famous for its bananas and the angered forest spirits combined into this monster.", Resources.Load<Sprite>("fiery_banana_warrior"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(11, "Maiden's Dress", 3, 500,1200, Type.Human, Attribute.Radiant, Property.None, "Legend has it that the maiden was wearing this dress 2000 years ago when her lover disappeared.", Resources.Load<Sprite>("maiden"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(12, "Sharpchine", 3, 750,1100, Type.Robot, Attribute.Nature, Property.None, "This sharpener was given life and now uses its blades to cut its opponents.", Resources.Load<Sprite>("sharpchine"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(13, "Skullfoot", 1, 300,400, Type.Undead, Attribute.Dark, Property.None, "Who comes first... Skull of Foot?", Resources.Load<Sprite>("skullfoot"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(14, "Bloorg", 3, 1000,200, Type.Alien, Attribute.Dark, Property.None, "He wants to devour the entire human race.", Resources.Load<Sprite>("bloorg"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(15, "Daisy Mage", 2, 500,800, Type.Mage, Attribute.Nature, Property.None, "One of the 3 powerful flower mages. He got his powers by experimenting on daisies.", Resources.Load<Sprite>("daisymage"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(16, "Labyrinth Minotaur", 6, 2000,1800, Type.Labyrinth, Attribute.Labyrinth, Property.None, "Once picked up: Special Summon this card to your base. If you do not have an open zone add it to hand instead.", Resources.Load<Sprite>("labyrinth_minotaur"), "Red", 0, 0, false, 0, true));
        cardList.Add(new Card(17, "Quadropticus", 1, 400, 200, Type.Marine, Attribute.Water, Property.Hydrowalk, "With eyes in every direction, it can effortlessly move through the labyrinth's depths. Rumor has it, it enjoys staring contests", Resources.Load<Sprite>(""), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(18, "Crystal Guardian", 3, 900,800, Type.Mineral, Attribute.Radiant, Property.None, "Say my name!", Resources.Load<Sprite>("crystal_guardian"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(19, "Azure Doodlebug", 2, 800,600, Type.Bug, Attribute.Nature, Property.None, "Deep in the forest where the sunlight peeks through\nDances the Doodlebug, a creature so blue.\nNot a shade of crimson, no red in its view,\nJust an azure dancer in nature's own hue.", Resources.Load<Sprite>("azure_doodlebug"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(20, "Treetop Critter", 1, 250,300, Type.Animal, Attribute.Nature, Property.None, "This creature is so lazy it sometimes forget to breathe.", Resources.Load<Sprite>("treetop_critter"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(21, "Baby Alien", 1, 500,0, Type.Alien, Attribute.Dark, Property.None, "Gloop Glop Glee in your mouth I gotta pee.", Resources.Load<Sprite>("alien_baby"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(22, "Pebbler", 2, 750,800, Type.Mineral, Attribute.Nature, Property.None, "When watching it becomes hard as a rock.", Resources.Load<Sprite>("pebbler"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(23, "Pyrognome", 2, 600,400, Type.Human, Attribute.Fire, Property.None, "He is not a gonoblin, he is not a gonelf, he is a gonome!", Resources.Load<Sprite>("pyrognome"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(24, "Yeti", 5, 1800,1000, Type.Demon, Attribute.Ice, Property.None, "Known as the Abominable Snowman many do not believe it exists.", Resources.Load<Sprite>("yeti"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(25, "Thorn Fairy", 4, 1100,300, Type.Angel, Attribute.Nature, Property.None, "This card gains 200 ATK for each Plant type monster in either player's graveyard.", Resources.Load<Sprite>("thorn_fairy"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(26, "Heartstealer", 4, 1200,800, Type.Human, Attribute.Radiant, Property.None, "When this card destroys a monster by battle you gain 500 LP.", Resources.Load<Sprite>("heartstealer"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(27, "Heterochromatic Zombie", 4, 1350,0, Type.Undead, Attribute.Dark, Property.None, "Brain taste good.", Resources.Load<Sprite>("heterochromatic_zombie"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(28, "Wind Turbchine", 4, 1100,1200, Type.Robot, Attribute.Aerial, Property.None, "Its spinning turbines not only provide electricity but also propel it through the labyrinth with agility.", Resources.Load<Sprite>("wind_turbchine"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(29, "Water Spirit", 1, 0,0, Type.Spirit, Attribute.Water, Property.Wallwalk, "You can tribute this card to grant another WATER monster you control this card's properties.", Resources.Load<Sprite>("water_spirit"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(30, "Fire Spirit", 1, 0,0, Type.Spirit, Attribute.Fire, Property.Wallwalk, "You can tribute this card to grant another FIRE monster you control this card's properties.", Resources.Load<Sprite>("fire_spirit"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(31, "Nature Spirit", 1, 0,0, Type.Spirit, Attribute.Nature, Property.Wallwalk, "You can tribute this card to grant another NATURE monster you control this card's properties.", Resources.Load<Sprite>("nature_spirit"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(32, "Wind Spirit", 1, 0,0, Type.Spirit, Attribute.Aerial, Property.Wallwalk, "You can tribute this card to grant another WIND monster you control this card's properties.", Resources.Load<Sprite>("wind_spirit"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(33, "Logigas", 6, 1700,1700, Type.Plant, Attribute.Nature, Property.None, "While 'Smallog' is on the field, this card gains 500 ATK and 500 DEF.", Resources.Load<Sprite>("logigas"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(34, "Smallog", 1, 500,500, Type.Plant, Attribute.Nature, Property.None, "They say this creature grows to be quite dangerous.", Resources.Load<Sprite>("smallog"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(35, "Labyrinth Spirit", 1, 0,0, Type.Labyrinth, Attribute.Labyrinth, Property.Wallwalk, ".", Resources.Load<Sprite>(""), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(36, "Ximp", 1, 350,250, Type.Demon, Attribute.Dark, Property.None, "This imp is so hungry it ate rubies, thinking it was strawberries.", Resources.Load<Sprite>(""), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(37, "Arcane Scholar", 6, 1800,1500, Type.Mage, Attribute.Radiant, Property.None, "Abra Kadabra Alakazam, I will eat all your ham.", Resources.Load<Sprite>(""), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(38, "Robo-Guardian", 4, 1200,1500, Type.Robot, Attribute.Nature, Property.None, "Congrats Master! You won the first LBD tournament! Now I am your slave forever beep boop.", Resources.Load<Sprite>("robo_guardian"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(39, "Picsie", 1, 500,300, Type.Angel, Attribute.Radiant, Property.None, "This card is treated as a Robot while on the field.", Resources.Load<Sprite>("picsie"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(40, "Prismatik Wisp", 1, 100,200, Type.Mage, Attribute.Aerial, Property.None, "This creature looks at you while you poop. If you do not wash your hands after, it attacks.", Resources.Load<Sprite>("prismatic_wisp"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(41, "Zuli, the Gem Knight", 4, 1400,1100, Type.Mineral, Attribute.Nature, Property.None, "The first Gem-Knight born from eating 100 gems of Lapis Lazuli. He claims it tastes like Big Mac", Resources.Load<Sprite>("zuli"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(42, "Sproom", 2, 400,300, Type.Acid, Attribute.Toxic, Property.Plague, "When monster eats Sproom it goes vroom vroom", Resources.Load<Sprite>("sproom"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(43, "Corroda", 4, 1200,1000, Type.Acid, Attribute.Toxic, Property.Plague, "He has impressive tongue skills that will leave you paralyzed.", Resources.Load<Sprite>("corroda"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(44, "Luminara", 4, 1400,400, Type.Angel, Attribute.Radiant, Property.None, "Luminara shines in what the player can't see.", Resources.Load<Sprite>("luminara"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(45, "Demon Lady", 4, 1200,500, Type.Demon, Attribute.Dark, Property.None, "When this card attacks a monster, that monster loses 500 DEF until the end of this battle.", Resources.Load<Sprite>("demon_lady"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(46, "Shadow Imp", 2, 500,400, Type.Demon, Attribute.Dark, Property.None, "When this card is sumoned you can target one Dark attribute monster on the field and it gains 200 ATK until the end of this turn.", Resources.Load<Sprite>("shadow_imp"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(47, "Frost Wraith", 2, 300,100, Type.Undead, Attribute.Ice, Property.Immobile, "You can tribute this card to target one monster in the Card Base and it gains 'Immobile' property.", Resources.Load<Sprite>("frost_wraith"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(47, "Shy Magician", 2, 600,500, Type.Mage, Attribute.Radiant, Property.None, "Once per duel, you can select two of your Mage-type monsters and those monsters exchange positions on the labyrinth.", Resources.Load<Sprite>("shy_magician"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(48, "Legendary Ninja", 3, 1000,1100, Type.Human, Attribute.Radiant, Property.Voltstream, "Legendary Ninja that is a veteran of over 9000 battles.", Resources.Load<Sprite>("legendary_ninja"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(47, "Ashiran", 6, 1850,900, Type.Demon, Attribute.Fire, Property.Piercing, "If you rub his belly 3 times this Genie will burn your butt.", Resources.Load<Sprite>("ashiran"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(48, "Rattlesnake", 3, 800,700, Type.Animal, Attribute.Nature, Property.None, "When this card is Summoned you target one monster on the field and move it up to 3 squares.", Resources.Load<Sprite>("rattlesnake"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(49, "Spooky Man", 3, 1000, 1000, Type.Undead, Attribute.Dark, Property.None, "When this card is destroyed by battle you can revive one undead-type monster with less squares than this card from your Graveyard to the same square as this card.", Resources.Load<Sprite>("spooky_man"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(50, "Popcorn Mage", 1, 0, 0, Type.Mage, Attribute.Radiant, Property.None, ".", Resources.Load<Sprite>("popcorn_mage"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(51, "Labyrinth Shield", 6, 0, 2300, Type.Labyrinth, Attribute.Labyrinth, Property.Immobile, "Once picked up: Special Summon this card to your base. If you do not have an open zone add it to your hand instead.", Resources.Load<Sprite>("labyrinth_shield"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(52, "Mushroom Fun Hat", 3, 900, 850, Type.Acid, Attribute.Toxic, Property.Plague, "This toxic mushroom was created by the genius 'scientist' Dhmhgr. Legend says that if you eat it you shit gold.", Resources.Load<Sprite>("labyrinth_shield"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(53, "Aetherwing Butterfly", 6, 1700, 1600, Type.Bug, Attribute.Aerial, Property.None, "When this card is Summoned from the hand you can destroy 1 Magic or Action card your opponent controls.", Resources.Load<Sprite>("aetherwing_butterfly"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(54, "Snalien", 6, 1500, 1800, Type.Alien, Attribute.Dark, Property.None, "If this is the only card you control on the field then it gains 400 ATK and 200 DEF.", Resources.Load<Sprite>("snalien"), "Brown", 0, 0, false, 0, true));
        cardList.Add(new Card(55, "Solaris", 6, 1800, 1000, Type.Human, Attribute.Radiant, Property.Voltstream, "This warrior wears a special suit powered by electricity. He will burn your toast like a high powered toaster.", Resources.Load<Sprite>("solaris"), "Brown", 0, 0, false, 0, true));
        //cardList.Add(new Card(41, "", 1, 0,0, ".", Resources.Load<Sprite>(""), "Brown", 0, 0, false, 0, true));
        //cardList.Add(new Card(42, "", 1, 0,0, ".", Resources.Load<Sprite>(""), "Brown", 0, 0, false, 0, true));
    }
}
