using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class OpponentLP : NetworkBehaviour
{
    public static float maxLP;
    public static float staticLP = 4000;
    public float LP;
    public Image Health;
    public Text LpText;

    void Start()
    {
        maxLP = 4000;
        staticLP = 4000;
        Health = transform.GetComponent<Image>();
        LpText = GameObject.Find("OpponentLPtext").GetComponent<Text>();
    }

    void Update()
    {
        LP = staticLP;
        Health.fillAmount = LP / maxLP;

        if (LP >= maxLP)
        {
            //LP = maxLP;
        }

        LpText.text = LP + " LP";
    }
}
