using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ThisAction : NetworkBehaviour
{
    public List<Action> thisCard = new List<Action>();

    [SyncVar] public int thisId;

    public int id;
    public string cardName;
    public string cardRequirement;
    public string cardDescription;
    public Sprite thisImage;
    public string color;

    [Header("UI References")]
    public Text nameText;
    public Text requirementText;
    public Text descriptionText;
    public Image artworkImage;

    [Header("Card States")]
    [SyncVar] public bool cardBack = false;
    public bool faceup = false;
    public bool activated = false;
    [SyncVar] public bool beInGraveyard = false;

    public GameObject CardBackVisual;

    void Awake()
    {

        nameText = transform.Find("ActionCanvas").Find("ActionBackground").Find("ActionName").Find("ActionNameText").GetComponent<Text>();

        artworkImage = transform.Find("ActionCanvas").Find("ActionBackground").Find("ActionImage").GetComponent<Image>();

        requirementText = transform.Find("ActionCanvas").Find("ActionBackground").Find("ActionRequirement").Find("ActionRequirementText").GetComponent<Text>();

        descriptionText = transform.Find("ActionCanvas").Find("ActionBackground").Find("ActionDescription").Find("ActionDescriptionText").GetComponent<Text>();
    }

    void Start()
    {
        if (ActionDataBase.actionList.Count > 0)
        {
            thisCard.Add(ActionDataBase.actionList[0]);
        }
        else
        {
            thisCard.Add(new Action());
        }
    }

    void Update()
    {
        if (thisId >= 0 && thisId < ActionDataBase.actionList.Count)
        {
            thisCard[0] = ActionDataBase.actionList[thisId];
        }

        id = thisCard[0].id;
        cardName = thisCard[0].cardName;
        cardRequirement = thisCard[0].cardRequirement;
        cardDescription = thisCard[0].cardDescription;
        thisImage = thisCard[0].thisImage;
        color = thisCard[0].color;

        if (nameText != null) nameText.text = cardName;
        if (requirementText != null) requirementText.text = cardRequirement;
        if (descriptionText != null) descriptionText.text = cardDescription;
        if (artworkImage != null && thisImage != null) artworkImage.sprite = thisImage;

        if (CardBackVisual != null) CardBackVisual.SetActive(cardBack);

        Transform canvasTrans = transform.Find("ActionCanvas");
        if (canvasTrans != null)
        {
            canvasTrans.gameObject.SetActive(!cardBack);
        }
    }
}