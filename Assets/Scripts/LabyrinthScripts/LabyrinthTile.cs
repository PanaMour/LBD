using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LabyrinthTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject gridtile;
    public GameObject gridGenerator;
    public Color red => Color.red;
    public Color green => Color.green;
    public Color white => Color.white;
    public bool isHighlighted = false;

    void Start()
    {
        gridGenerator = transform.parent.gameObject;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        gridtile.GetComponent<Image>().color = red;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gridGenerator.GetComponent<GridBehavior>().FindDistanceTrue(
        gridtile.GetComponent<GridStat>().x,
        gridtile.GetComponent<GridStat>().y
    );
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isHighlighted)
        {
            gridtile.GetComponent<Image>().color = green;
        }
        else
        {
            gridtile.GetComponent<Image>().color = white;
        }
    }

    public void StartMoving()
    {
        if (isHighlighted)
        {
            gridGenerator.GetComponent<GridBehavior>().FindDistanceTrue(
                gridtile.GetComponent<GridStat>().x,
                gridtile.GetComponent<GridStat>().y
            );
        }
    }
    public void GlowBlock()
    {
        isHighlighted = true;
        gridtile.GetComponent<Image>().color = green;
    }

    public void StopGlowBlock()
    {
        isHighlighted = false;
        gridtile.GetComponent<Image>().color = white;
    }

    public Color LerpRed()
    {
        return Color.Lerp(white, red, Mathf.Sin(Time.time*7));
    }
}
