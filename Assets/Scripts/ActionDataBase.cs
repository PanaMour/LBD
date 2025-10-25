using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ActionDataBase : NetworkBehaviour
{
    public GameManager GameManager;
    public GameObject Canvas;
    public PlayerManager PlayerManager;

    public static List<Action> actionList = new List<Action>();

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Canvas = GameObject.Find("Main Canvas");
    }
    private void Awake()
    {
        actionList.Add(new Action(0, "None", "None","None", Resources.Load<Sprite>("noimg"), "None"));
        actionList.Add(new Action(1, "Echo of Silence", "When an opponent's monster declares an attack, send 1 card from your hand to the Graveyard.","The attacking monster loses 100 ATK for each monster in your Graveyard until the end of the battle.", Resources.Load<Sprite>("echo_of_silence"), "Action"));
        actionList.Add(new Action(2, "Phantom Binding", "When a monster you control is destroyed by battle.", "The monster that destroyed it gains the property 'Immobile'.", Resources.Load<Sprite>("phantom_binding"), "Action"));
        actionList.Add(new Action(3, "Honey Snare", "At the start of this turn.", "All monsters on the field and monsters summoned this turn gain the property 'Immobile' until the end of this turn.", Resources.Load<Sprite>("honey_snare"), "Action"));
        actionList.Add(new Action(4, "Last Stand Barrier", "When an opponent's monster declares an attack against an Attack position monster.", "Switch the monster being attacked to defense and it gains DEF equal to its ATK until the end of this turn.", Resources.Load<Sprite>("last_stand_barrier"), "Action"));
        actionList.Add(new Action(5, "Intercept", "When an opponent's monster attacks a monster you control that is adjacent to a WALL or in the Card Base.", "Negate that attack.", Resources.Load<Sprite>("intercept"), "Action"));
    }
}
