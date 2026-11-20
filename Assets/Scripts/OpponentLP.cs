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
        feedback = new LPFeedback((RectTransform)transform, Health, LpText, staticLP);
    }

    // Smooth count, "-800"/"+200" popup and damage shake (see LPFeedback).
    LPFeedback feedback;

    void Update()
    {
        LP = staticLP;
        if (feedback != null) feedback.Tick(LP, maxLP);
    }
}
