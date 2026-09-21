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
    public GameObject zoomStandardDescContainer;
    public GameObject zoomReqContainer;
    public GameObject zoomActionDescContainer;

    public Text zoomATKtext;
    public Text zoomDEFtext;
    public Text zoomStarstext;
    public GameObject zoomStars;
    public GameObject zoomATK;
    public GameObject zoomDEF;
    public GameObject zoomTypeLine;
    public Text zoomTypeLineText;
    public RectTransform zoomTypeLinePlate;
    public Image zoomTypeLinePlateImage;
    public Image zoomBackground;
    public Image zoomCanvas;
    public Image zoomCardBack;

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
        zoomTypeLine = GameObject.Find("ZoomTypeLine");
        if (zoomTypeLine != null)
        {
            zoomTypeLinePlate = zoomTypeLine.GetComponent<RectTransform>();
            zoomTypeLinePlateImage = zoomTypeLine.GetComponent<Image>();
            Transform inner = zoomTypeLine.transform.Find("ZoomTypeLineText");
            if (inner != null) zoomTypeLineText = inner.GetComponent<Text>();
        }
        zoomBackground = GameObject.Find("ZoomBackground").GetComponent<Image>();
        zoomCanvas = GameObject.Find("ZoomCardCanvas").GetComponent<Image>();
        zoomCardBack = GameObject.Find("ZoomCardBack").GetComponent<Image>();

        if (zoomBackground != null)
        {
            Transform bg = zoomBackground.transform;

            zoomStandardDescContainer = bg.Find("CardDescription")?.gameObject;
            if (zoomStandardDescContainer != null)
                zoomDescriptionText = zoomStandardDescContainer.transform.Find("ZoomDescriptionText")?.GetComponent<Text>();

            zoomReqContainer = bg.Find("ZoomRequirement")?.gameObject;
            zoomActionDescContainer = bg.Find("ZoomActionDescription")?.gameObject;
        }

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
            // Magic cards show their own kind here (Magic / Armor Magic /
            // Labyrinth Magic) in place of a monster's Type/Attribute line.
            if (zoomTypeLine != null)
            {
                zoomTypeLine.transform.localScale = new Vector3(1, 1, 1);
                if (zoomTypeLineText != null)
                {
                    zoomTypeLineText.text = Magic.GetComponent<ThisMagic>().BuildMagicTypeLine();
                    if (zoomTypeLinePlateImage != null)
                        zoomTypeLinePlateImage.color = Magic.GetComponent<ThisMagic>().MagicTypeColor();
                    if (zoomTypeLinePlate != null)
                        zoomTypeLinePlate.sizeDelta = new Vector2(zoomTypeLinePlate.sizeDelta.x, 30f);
                }
            }
            zoomCardNameText.text = NameText.text;
            zoomImage.sprite = Image.sprite;

            if (zoomStandardDescContainer != null) zoomStandardDescContainer.SetActive(true);
            if (zoomReqContainer != null) zoomReqContainer.SetActive(false);
            if (zoomActionDescContainer != null) zoomActionDescContainer.SetActive(false);

            if (zoomDescriptionText != null) zoomDescriptionText.text = DescriptionText.text;

            // sprite/type must be copied too, not just color -- otherwise a
            // stale sprite+type left behind by whichever card kind was hovered
            // previously renders wrong here and can expose the white
            // ZoomCardImage layer underneath.
            zoomBackground.sprite = Background.sprite;
            zoomBackground.color = Background.color;
            zoomBackground.type = Background.type;
            zoomBackground.pixelsPerUnitMultiplier = Background.pixelsPerUnitMultiplier;

            zoomCanvas.sprite = MagicCanvas.sprite;
            zoomCanvas.color = MagicCanvas.color;
            zoomCanvas.type = MagicCanvas.type;
            zoomCanvas.pixelsPerUnitMultiplier = MagicCanvas.pixelsPerUnitMultiplier;

            zoomText.text = Magic.GetComponent<ThisMagic>().magicdescriptionText.text;
        }
        else
        {
            zoomCardBack.transform.localScale = new Vector3(1, 1, 1);
            zoomText.text = "Opponent's Card.";
        }
    }

    public void OnHoverExit()
    {
    }
}