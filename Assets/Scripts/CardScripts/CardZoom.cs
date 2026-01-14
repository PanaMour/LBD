using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CardZoom : NetworkBehaviour
{
    [Header("3D Animation Settings")]
    public float zoomAmount = 0.8f;   // How far toward the camera the card moves
    public float liftAmount = 0.4f;   // How far "up" the card moves

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isZoomed = false;

    [Header("UI References")]
    private GameObject zoomPanel;
    private Text zName, zDesc, zATK, zDEF;
    private Image zImage;
    private GameObject zStatsContainer;

    public void Awake()
    {
        // 1. Find the Main Zoom Panel (The parent object we created in the Canvas)
        zoomPanel = GameObject.Find("ZoomPanel");

        if (zoomPanel != null)
        {
            // 2. Cache the UI components inside the panel for speed
            // Assumes these names match your ZoomPanel hierarchy
            zName = zoomPanel.transform.Find("ZoomNameText")?.GetComponent<Text>();
            zDesc = zoomPanel.transform.Find("ZoomDescriptionText")?.GetComponent<Text>();
            zImage = zoomPanel.transform.Find("ZoomCardImage")?.GetComponent<Image>();
            zStatsContainer = zoomPanel.transform.Find("ZoomStats")?.gameObject;

            if (zStatsContainer != null)
            {
                zATK = zStatsContainer.transform.Find("ZoomATKText")?.GetComponent<Text>();
                zDEF = zStatsContainer.transform.Find("ZoomDEFText")?.GetComponent<Text>();
            }

            // Start with the panel hidden
            zoomPanel.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {
        // Only zoom if we have authority or the card is face up on the table
        bool isFaceUp = false;
        if (GetComponent<ThisCard>() != null) isFaceUp = GetComponent<ThisCard>().faceup;
        if (GetComponent<ThisMagic>() != null) isFaceUp = GetComponent<ThisMagic>().faceup;

        if (hasAuthority || isFaceUp)
        {
            // --- 3D POP-UP LOGIC ---
            if (!isZoomed)
            {
                originalPosition = transform.localPosition;
                originalRotation = transform.localRotation;

                // Move closer to camera and slightly up
                transform.localPosition += new Vector3(0, liftAmount, -zoomAmount);
                transform.localRotation = Quaternion.Euler(0, 0, 0); // Straighten card
                isZoomed = true;
            }

            // --- 2D UI PANEL LOGIC ---
            if (zoomPanel != null)
            {
                zoomPanel.SetActive(true);
                UpdateZoomUI();
            }
        }
    }

    public void OnHoverExit()
    {
        // Reset 3D position
        if (isZoomed)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            isZoomed = false;
        }

        // Hide 2D Panel
        if (zoomPanel != null)
        {
            zoomPanel.SetActive(false);
        }
    }

    private void UpdateZoomUI()
    {
        // Handle Monster Card Data
        ThisCard monster = GetComponent<ThisCard>();
        if (monster != null)
        {
            if (zName) zName.text = monster.cardName;
            if (zDesc) zDesc.text = monster.descriptionText.text;
            if (zImage) zImage.sprite = monster.thisSprite;

            if (zStatsContainer)
            {
                zStatsContainer.SetActive(true); // Show ATK/DEF for monsters
                if (zATK) zATK.text = "ATK: " + monster.atk;
                if (zDEF) zDEF.text = "DEF: " + monster.def;
            }
            return;
        }

        // Handle Magic Card Data
        ThisMagic magic = GetComponent<ThisMagic>();
        if (magic != null)
        {
            if (zName) zName.text = magic.magicName;
            if (zDesc) zDesc.text = magic.magicdescriptionText.text;
            if (zImage) zImage.sprite = magic.thisSprite;

            if (zStatsContainer)
            {
                zStatsContainer.SetActive(false); // Hide ATK/DEF for magic
            }
        }
    }
}