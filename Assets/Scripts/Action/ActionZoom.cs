using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ActionZoom : NetworkBehaviour
{
    public ThisAction actionCardData;

    [Header("Local Card Visuals")]
    public Image localBackground;
    public Image localCanvas;

    [Header("Global Zoom UI")]
    public Text zoomCardNameText;
    public Text zoomText;
    public Image zoomImage;

    // The specific Text components
    public Text zoomDescriptionText;
    public Text zoomRequirementText;
    public Text zoomActionDescriptionText;

    // The Parent Containers (to turn on/off)
    public GameObject zoomStandardDescContainer;
    public GameObject zoomReqContainer;
    public GameObject zoomActionDescContainer;

    public GameObject zoomStars;
    public GameObject zoomATK;
    public GameObject zoomDEF;
    public Image zoomCardBack;

    public Image zoomBackground;
    public Image zoomCanvas;

    public void Awake()
    {
        actionCardData = GetComponent<ThisAction>();

        zoomCardNameText = GameObject.Find("ZoomNameText")?.GetComponent<Text>();
        zoomImage = GameObject.Find("ZoomImage")?.GetComponent<Image>();
        zoomText = GameObject.Find("ExplainZoomText")?.GetComponent<Text>();

        zoomStars = GameObject.Find("ZoomStars");
        zoomATK = GameObject.Find("ZoomATK");
        zoomDEF = GameObject.Find("ZoomDEF");
        zoomCardBack = GameObject.Find("ZoomCardBack")?.GetComponent<Image>();

        zoomBackground = GameObject.Find("ZoomBackground")?.GetComponent<Image>();
        zoomCanvas = GameObject.Find("ZoomCardCanvas")?.GetComponent<Image>();

        if (zoomBackground != null)
        {
            Transform bg = zoomBackground.transform;

            zoomStandardDescContainer = bg.Find("CardDescription")?.gameObject;
            if (zoomStandardDescContainer != null)
                zoomDescriptionText = zoomStandardDescContainer.transform.Find("ZoomDescriptionText")?.GetComponent<Text>();

            zoomReqContainer = bg.Find("ZoomRequirement")?.gameObject;
            if (zoomReqContainer != null)
                zoomRequirementText = zoomReqContainer.transform.Find("ZoomRequirementText")?.GetComponent<Text>();

            zoomActionDescContainer = bg.Find("ZoomActionDescription")?.gameObject;
            if (zoomActionDescContainer != null)
                zoomActionDescriptionText = zoomActionDescContainer.transform.Find("ZoomActionDescriptionText")?.GetComponent<Text>();
        }

        localCanvas = transform.Find("ActionCanvas")?.GetComponent<Image>();
        localBackground = transform.Find("ActionCanvas")?.Find("ActionBackground")?.GetComponent<Image>();
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

            if (zoomStandardDescContainer != null) zoomStandardDescContainer.SetActive(false);
            if (zoomReqContainer != null) zoomReqContainer.SetActive(true);
            if (zoomActionDescContainer != null) zoomActionDescContainer.SetActive(true);

            if (zoomRequirementText != null) zoomRequirementText.text = actionCardData.cardRequirement;
            if (zoomActionDescriptionText != null) zoomActionDescriptionText.text = actionCardData.cardDescription;

            if (zoomText != null) zoomText.text = "Requirement: " + actionCardData.cardRequirement + "\n\n" + actionCardData.cardDescription;

            if (zoomBackground != null && localBackground != null)
            {
                zoomBackground.sprite = localBackground.sprite;
                zoomBackground.color = localBackground.color;
            }
            if (zoomCanvas != null && localCanvas != null)
            {
                zoomCanvas.sprite = localCanvas.sprite;
                zoomCanvas.color = localCanvas.color;
            }
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