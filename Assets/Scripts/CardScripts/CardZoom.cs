using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CardZoom : NetworkBehaviour
{
    public GameObject Canvas;
    public PlayerManager PlayerManager;
    public GameObject Card;
    public Text NameText;
    public Image Image;
    public Text DescriptionText;
    public Text ATKtext;
    public Text DEFtext;
    public Text StarsText;
    public Image Background;
    public Image CardCanvas;

    public Text zoomCardNameText;
    public Text zoomText;
    public Image zoomImage;

    public Text zoomDescriptionText;
    public GameObject zoomStandardDescContainer;
    public GameObject zoomReqContainer;
    public GameObject zoomActionDescContainer;

    public Text zoomATKtext;
    public Text zoomDEFtext;
    public Text zoomStarstext;
    public GameObject zoomStars;
    public GameObject zoomATK;
    public GameObject zoomDEF;
    public Image zoomBackground;
    public Image zoomCanvas;
    public Image zoomCardBack;
    public GameObject zoomTypeLine;
    public Text zoomTypeLineText;
    public RectTransform zoomTypeLinePlate;

    public void Awake()
    {
        Canvas = GameObject.Find("Main Canvas");
        zoomCardNameText = GameObject.Find("ZoomNameText").GetComponent<Text>();
        zoomImage = GameObject.Find("ZoomImage").GetComponent<Image>();
        zoomText = GameObject.Find("ExplainZoomText").GetComponent<Text>();
        zoomATKtext = GameObject.Find("ZoomATKtext").GetComponent<Text>();
        zoomDEFtext = GameObject.Find("ZoomDEFtext").GetComponent<Text>();
        zoomStarstext = GameObject.Find("ZoomStarsText").GetComponent<Text>();
        zoomStars = GameObject.Find("ZoomStars");
        zoomATK = GameObject.Find("ZoomATK");
        zoomDEF = GameObject.Find("ZoomDEF");
        zoomBackground = GameObject.Find("ZoomBackground").GetComponent<Image>();
        zoomCanvas = GameObject.Find("ZoomCardCanvas").GetComponent<Image>();
        zoomCardBack = GameObject.Find("ZoomCardBack").GetComponent<Image>();

        zoomTypeLine = GameObject.Find("ZoomTypeLine");
        if (zoomTypeLine != null)
        {
            zoomTypeLinePlate = zoomTypeLine.GetComponent<RectTransform>();
            Transform inner = zoomTypeLine.transform.Find("ZoomTypeLineText");
            if (inner != null) zoomTypeLineText = inner.GetComponent<Text>();
        }

        if (zoomBackground != null)
        {
            Transform bg = zoomBackground.transform;

            zoomStandardDescContainer = bg.Find("CardDescription")?.gameObject;
            if (zoomStandardDescContainer != null)
                zoomDescriptionText = zoomStandardDescContainer.transform.Find("ZoomDescriptionText")?.GetComponent<Text>();

            zoomReqContainer = bg.Find("ZoomRequirement")?.gameObject;
            zoomActionDescContainer = bg.Find("ZoomActionDescription")?.gameObject;
        }

        NameText = gameObject.transform.Find("CardCanvas").Find("Background").Find("CardName").Find("NameText").GetComponent<Text>();
        Image = gameObject.transform.Find("CardCanvas").Find("Background").Find("Image").GetComponent<Image>();
        DescriptionText = gameObject.transform.Find("CardCanvas").Find("Background").Find("CardDescription").Find("DescriptionText").GetComponent<Text>();
        ATKtext = gameObject.transform.Find("CardCanvas").Find("Background").Find("ATK").Find("ATKtext").GetComponent<Text>();
        DEFtext = gameObject.transform.Find("CardCanvas").Find("Background").Find("DEF").Find("DEFtext").GetComponent<Text>();
        StarsText = gameObject.transform.Find("CardCanvas").Find("Background").Find("Stars").Find("StarsText").GetComponent<Text>();
        Background = gameObject.transform.Find("CardCanvas").Find("Background").GetComponent<Image>();
        CardCanvas = gameObject.transform.Find("CardCanvas").GetComponent<Image>();

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();
    }

    public void OnHoverEnter()
    {
        if (hasAuthority || Card.GetComponent<ThisCard>().faceup == true)
        {
            zoomCardBack.transform.localScale = new Vector3(0, 0, 0);
            zoomATK.transform.localScale = new Vector3(1, 1, 1);
            zoomDEF.transform.localScale = new Vector3(1, 1, 1);
            zoomStars.transform.localScale = new Vector3(1, 1, 1);
            zoomCardNameText.text = NameText.text;
            zoomImage.sprite = Image.sprite;

            if (zoomStandardDescContainer != null) zoomStandardDescContainer.SetActive(true);
            if (zoomReqContainer != null) zoomReqContainer.SetActive(false);
            if (zoomActionDescContainer != null) zoomActionDescContainer.SetActive(false);

            if (zoomDescriptionText != null) zoomDescriptionText.text = DescriptionText.text;

            zoomATKtext.text = ATKtext.text;
            zoomDEFtext.text = DEFtext.text;
            zoomStarstext.text = StarsText.text;
            zoomBackground.color = Background.color;
            zoomText.text = Card.GetComponent<ThisCard>().descriptionText.text;
            zoomCanvas.color = CardCanvas.color;

            // Mirrors the small card's Type/Attribute/Property bar. The text is
            // copied from the card itself rather than rebuilt, so granted
            // properties stay in sync with what the card face already shows.
            if (zoomTypeLine != null)
            {
                zoomTypeLine.transform.localScale = new Vector3(1, 1, 1);

                Text sourceLine = Card.GetComponent<ThisCard>().typeLineText;
                if (zoomTypeLineText != null && sourceLine != null)
                {
                    zoomTypeLineText.text = sourceLine.text;

                    if (zoomTypeLinePlate != null)
                    {
                        float needed = zoomTypeLineText.preferredHeight + 6f;
                        if (needed < 30f) needed = 30f;
                        zoomTypeLinePlate.sizeDelta = new Vector2(zoomTypeLinePlate.sizeDelta.x, needed);
                    }
                }
            }
        }
        else if (!hasAuthority)
        {
            zoomCardBack.transform.localScale = new Vector3(1, 1, 1);
            if (zoomTypeLine != null) zoomTypeLine.transform.localScale = new Vector3(0, 0, 0);
            zoomText.text = "Opponent's Card.";
            Debug.Log("NO AUTHORITY!");
        }
    }

    public void OnHoverExit()
    {
    }
}