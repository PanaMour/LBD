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
        if (gridGenerator == null)
            gridGenerator = GameObject.Find("GridGenerator");

        if (gridtile == null)
            gridtile = this.gameObject;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Color hoverColor = isHighlighted ? Color.cyan : Color.red;

        GetComponent<Renderer>().material.color = hoverColor;

        Transform quad = transform.Find("Quad");
        if (quad != null)
        {
            quad.GetComponent<Renderer>().material.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color resetColor = isHighlighted ? Color.green : Color.white;

        GetComponent<Renderer>().material.color = resetColor;

        Transform quad = transform.Find("Quad");
        if (quad != null)
        {
            quad.GetComponent<Renderer>().material.color = resetColor;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        GridStat stats = gridtile.GetComponent<GridStat>();
        gridGenerator.GetComponent<GridBehavior>().OnTileClicked(stats.x, stats.y);
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

        GetComponent<Renderer>().material.color = Color.green;

        Transform quad = transform.Find("Quad");
        if (quad != null)
        {
            quad.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    public void StopGlowBlock()
    {
        isHighlighted = false;

        GetComponent<Renderer>().material.color = Color.white;

        Transform quad = transform.Find("Quad");
        if (quad != null)
        {
            quad.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public Color LerpRed()
    {
        return Color.Lerp(white, red, Mathf.Sin(Time.time*7));
    }
    public void RedGlowBlock()
    {
        GetComponent<Renderer>().material.color = Color.red;
    }
}
