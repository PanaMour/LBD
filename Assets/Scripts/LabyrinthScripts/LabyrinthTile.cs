using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabyrinthTile : MonoBehaviour
{
    public GameObject gridtile;
    public GameObject gridGenerator;
    public Color red => Color.red;
    public Color white => Color.white;
    public bool flash = false;

    // Start is called before the first frame update
    void Start()
    {
        gridGenerator = transform.parent.gameObject;
    }

    // Update is called once per frame
    public void Update()
    {
        if (flash == true)
            gridtile.GetComponent<Image>().color = LerpRed();
        else
            gridtile.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }

    public void StartMoving()
    {
        gridGenerator.GetComponent<GridBehavior>().FindDistanceTrue(gridtile.GetComponent<GridStat>().x, gridtile.GetComponent<GridStat>().y);
    }
    public void GlowBlock()
    {
        flash = true;
    }

    public void StopGlowBlock()
    {
        flash = false;
    }

    public Color LerpRed()
    {
        return Color.Lerp(white, red, Mathf.Sin(Time.time*7));
    }
}
