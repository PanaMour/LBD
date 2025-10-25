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
    public Text zoomATKtext;
    public Text zoomDEFtext;
    public Text zoomStarstext;
    public GameObject zoomStars;
    public GameObject zoomATK;
    public GameObject zoomDEF;
    public Image zoomBackground;
    public Image zoomCanvas;
    public Image zoomCardBack;

    public void Awake()
    {
        Canvas = GameObject.Find("Main Canvas");
        zoomCardNameText = GameObject.Find("ZoomNameText").GetComponent<Text>();
        zoomImage = GameObject.Find("ZoomImage").GetComponent<Image>();
        zoomDescriptionText = GameObject.Find("ZoomDescriptionText").GetComponent<Text>();
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
            zoomDescriptionText.text = DescriptionText.text;
            zoomATKtext.text = ATKtext.text;
            zoomDEFtext.text = DEFtext.text;
            zoomStarstext.text = StarsText.text;
            zoomBackground.color = Background.color;
            zoomText.text = Card.GetComponent<ThisCard>().descriptionText.text;
            zoomCanvas.color = CardCanvas.color;
            //PlayerManager.CmdZoomCard(name);
            /*zoomCard = Instantiate(gameObject, new Vector2(Input.mousePosition.x, Input.mousePosition.y + 250), Quaternion.identity);
            zoomCard.transform.SetParent(Canvas.transform, true);
            zoomCard.layer = LayerMask.NameToLayer("Zoom");

            RectTransform rect = zoomCard.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 344);*/
        }
        else if (!hasAuthority)
        {
            zoomCardBack.transform.localScale = new Vector3(1, 1, 1);
            zoomText.text = "Opponent's Card.";
            Debug.Log("NO AUTHORITY!");
        }
    }

    public void OnHoverExit()
    {
        //PlayerManager.CmdDestroyZoomCard();
        //Destroy(zoomCard);
    }
}
