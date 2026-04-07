using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ActionZoom : NetworkBehaviour
{
    public ThisAction actionCardData;

    [Header("Global Zoom UI")]
    public Text zoomCardNameText;
    public Text zoomText;
    public Image zoomImage;
    public Text zoomDescriptionText;
    public GameObject zoomStars;
    public GameObject zoomATK;
    public GameObject zoomDEF;
    public Image zoomCardBack;

    public void Awake()
    {
        actionCardData = GetComponent<ThisAction>();

        zoomCardNameText = GameObject.Find("ZoomNameText").GetComponent<Text>();
        zoomImage = GameObject.Find("ZoomImage").GetComponent<Image>();
        zoomDescriptionText = GameObject.Find("ZoomDescriptionText").GetComponent<Text>();
        zoomText = GameObject.Find("ExplainZoomText").GetComponent<Text>();

        zoomStars = GameObject.Find("ZoomStars");
        zoomATK = GameObject.Find("ZoomATK");
        zoomDEF = GameObject.Find("ZoomDEF");
        zoomCardBack = GameObject.Find("ZoomCardBack").GetComponent<Image>();
    }

    public void OnHoverEnter()
    {
        if (hasAuthority || actionCardData.faceup == true)
        {
            if (zoomCardBack != null) zoomCardBack.transform.localScale = new Vector3(0, 0, 0);
            if (zoomATK != null) zoomATK.transform.localScale = new Vector3(0, 0, 0);
            if (zoomDEF != null) zoomDEF.transform.localScale = new Vector3(0, 0, 0);
            if (zoomStars != null) zoomStars.transform.localScale = new Vector3(0, 0, 0);

            if (zoomCardNameText != null) zoomCardNameText.text = actionCardData.cardName;
            if (zoomImage != null) zoomImage.sprite = actionCardData.thisImage;
            if (zoomDescriptionText != null) zoomDescriptionText.text = actionCardData.cardDescription;

            if (zoomText != null) zoomText.text = "Requirement: " + actionCardData.cardRequirement + "\n\n" + actionCardData.cardDescription;
        }
        else
        {
            if (zoomCardBack != null) zoomCardBack.transform.localScale = new Vector3(1, 1, 1);
            if (zoomText != null) zoomText.text = "Opponent's Face-Down Card.";
        }
    }

    public void OnHoverExit()
    {

    }
}