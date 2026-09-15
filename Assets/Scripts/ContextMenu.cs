using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ContextMenuItem
{

    public string text;
    public Button button;
    public Action<Image> action;

    public ContextMenuItem(string text, Button button, Action<Image> action)
    {
        this.text = text;
        this.button = button;
        this.action = action;
    }
}

public class ContextMenu : MonoBehaviour
{
    public Image contentPanel;
    public Canvas canvas;

    private static ContextMenu instance;

    public static ContextMenu Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(ContextMenu)) as ContextMenu;
                if (instance == null)
                {
                    instance = new ContextMenu();
                }
            }
            return instance;
        }
    }

    // Tracks the single open menu so a second right-click replaces it
    // instead of stacking another one on top.
    private Image activeMenu;

    public void CreateContextMenu(List<ContextMenuItem> items, Vector2 screenPosition)
    {
        CloseActiveMenu();

        Image panel = Instantiate(contentPanel) as Image;

        // screenPosition comes from Camera.WorldToScreenPoint, i.e. real
        // screen pixels, which only line up 1:1 with the canvas's local
        // space when the CanvasScaler's scaleFactor is exactly 1 (Main
        // Canvas uses ScaleWithScreenSize, so that's only true at exactly
        // its 1920x1080 reference resolution). Resolve the actual world
        // point for that screen position first, then reparent onto the
        // canvas with worldPositionStays=true so Unity solves the panel's
        // own anchoredPosition for us -- this is correct regardless of the
        // panel's anchor settings (this prefab anchors at its own bottom-left
        // corner, not the canvas's center, so computing a canvas-local point
        // directly and assigning it to anchoredPosition would be off by the
        // gap between those two anchor origins).
        UnityEngine.Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform, screenPosition, cam, out Vector3 worldPoint);
        panel.transform.position = worldPoint;

        panel.transform.SetParent(canvas.transform, true);
        panel.transform.SetAsLastSibling();

        activeMenu = panel;

        foreach (var item in items)
        {
            ContextMenuItem tempReference = item;
            Button button = Instantiate(item.button) as Button;
            Text buttonText = button.GetComponentInChildren(typeof(Text)) as Text;
            buttonText.text = item.text;
            button.onClick.AddListener(delegate { tempReference.action(panel); });
            button.transform.SetParent(panel.transform, false);
        }
    }

    public void CloseActiveMenu()
    {
        if (activeMenu != null)
        {
            Destroy(activeMenu.gameObject);
            activeMenu = null;
        }
    }
}