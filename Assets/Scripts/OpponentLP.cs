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

        // Against the CPU, its LP icon shows its face: Robo-Guardian.
        if (CpuPlayer.Requested) ShowPortrait(Resources.Load<Sprite>("cpu_portrait"));
    }

    // A round picture inside the icon's inner disc (under the LP label).
    void ShowPortrait(Sprite portrait)
    {
        Transform icon = transform.Find("PlayerIcon");
        if (portrait == null || icon == null || icon.Find("Portrait") != null) return;

        GameObject obj = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)obj.transform;
        rt.SetParent(icon, false);
        rt.sizeDelta = new Vector2(96f, 96f);
        rt.anchoredPosition = Vector2.zero;
        Image img = obj.GetComponent<Image>();
        img.sprite = portrait;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    // Smooth count, "-800"/"+200" popup and damage shake (see LPFeedback).
    LPFeedback feedback;

    void Update()
    {
        LP = staticLP;
        if (feedback != null) feedback.Tick(LP, maxLP);
    }
}
