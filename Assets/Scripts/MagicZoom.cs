using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class MagicZoom : NetworkBehaviour
{
    public GameObject Canvas;
    public PlayerManager PlayerManager;
    public GameObject Magic;
    public Text NameText;
    public Image Image;
    public Text DescriptionText;
    public Text ATKtext;
    public Text DEFtext;
    public Text StarsText;
    public Image Background;
    public Image MagicCanvas;

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

        NameText = gameObject.transform.Find("MagicCanvas").Find("MagicBackground").Find("MagicName").Find("MagicNameText").GetComponent<Text>();
        Image = gameObject.transform.Find("MagicCanvas").Find("MagicBackground").Find("MagicImage").GetComponent<Image>();
        DescriptionText = gameObject.transform.Find("MagicCanvas").Find("MagicBackground").Find("MagicDescription").Find("MagicDescriptionText").GetComponent<Text>();
        Background = gameObject.transform.Find("MagicCanvas").Find("MagicBackground").GetComponent<Image>();
        MagicCanvas = gameObject.transform.Find("MagicCanvas").GetComponent<Image>();

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();

    }

    public void OnHoverEnter()
    {
        if (hasAuthority || Magic.GetComponent<ThisMagic>().faceup == true)
        {
            zoomCardBack.transform.localScale = new Vector3(0, 0, 0);
            zoomATK.transform.localScale = new Vector3(0, 0, 0);
            zoomDEF.transform.localScale = new Vector3(0, 0, 0);
            zoomStars.transform.localScale = new Vector3(0, 0, 0);
            zoomCardNameText.text = NameText.text;
            zoomImage.sprite = Image.sprite;
            zoomDescriptionText.text = DescriptionText.text;
            zoomBackground.color = Background.color;
            zoomCanvas.color = MagicCanvas.color;
            zoomText.text = Magic.GetComponent<ThisMagic>().magicdescriptionText.text;
            //PlayerManager.CmdZoomCard(name);
            /*zoomCard = Instantiate(gameObject, new Vector2(Input.mousePosition.x, Input.mousePosition.y + 250), Quaternion.identity);
            zoomCard.transform.SetParent(Canvas.transform, true);
            zoomCard.layer = LayerMask.NameToLayer("Zoom");

            RectTransform rect = zoomCard.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 344);*/
        }
        else
        {
            zoomCardBack.transform.localScale = new Vector3(1, 1, 1);
            zoomText.text = "Opponent's Card.";
        }
    }

    public void OnHoverExit()
    {
        //PlayerManager.CmdDestroyZoomCard();
        //Destroy(zoomCard);
    }
}
