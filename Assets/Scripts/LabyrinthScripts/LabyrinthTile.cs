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
    public bool isAttackTarget = false;
    static readonly Color AttackHoverColor = new Color(1f, 0.55f, 0f);
    static readonly Color FatalColor = new Color(0.5f, 0f, 0.5f);

    // Fatal Square: the tile stays purple (instead of white) whenever it
    // isn't highlighted, until the trap is used up or runs out.
    public bool fatalMarked = false;
    Color RestColor => fatalMarked ? FatalColor : Color.white;

    void Start()
    {
        if (gridGenerator == null)
            gridGenerator = GameObject.Find("GridGenerator");

        if (gridtile == null)
            gridtile = this.gameObject;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Color hoverColor = isAttackTarget ? AttackHoverColor : isHighlighted ? Color.cyan : Color.red;

        GetComponent<Renderer>().material.color = hoverColor;

        Transform quad = transform.Find("Quad");
        if (quad != null)
        {
            quad.GetComponent<Renderer>().material.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color resetColor = isAttackTarget ? Color.red : isHighlighted ? Color.green : RestColor;

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
        isAttackTarget = false;
        SetColor(RestColor);
    }

    public void SetFatalMark(bool marked)
    {
        fatalMarked = marked;
        if (!isHighlighted && !isAttackTarget) SetColor(RestColor);
    }

    // Back to the resting color unless the tile is currently highlighted.
    public void ShowRestColor()
    {
        if (!isHighlighted && !isAttackTarget) SetColor(RestColor);
    }

    void SetColor(Color c)
    {
        GetComponent<Renderer>().material.color = c;
        Transform quad = transform.Find("Quad");
        if (quad != null) quad.GetComponent<Renderer>().material.color = c;
    }

    public Color LerpRed()
    {
        return Color.Lerp(white, red, Mathf.Sin(Time.time*7));
    }
    // Attack target: stays red until cleared (hover shows orange, and
    // leaving the tile returns it to red instead of white).
    public void RedGlowBlock()
    {
        isAttackTarget = true;
        GetComponent<Renderer>().material.color = Color.red;

        Transform quad = transform.Find("Quad");
        if (quad != null) quad.GetComponent<Renderer>().material.color = Color.red;
    }
}
