using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// "YOUR TURN" / "OPPONENT'S TURN" banner that sweeps across the middle of the
// screen whenever the turn changes, so it's obvious whose move it is.
public class TurnBanner : MonoBehaviour
{
    const float SlideTime = 0.3f;
    const float HoldTime = 0.8f;
    const float StripHeight = 150f;
    static readonly Color MineColor = new Color(1f, 0.78f, 0.2f);
    static readonly Color TheirsColor = new Color(0.55f, 0.8f, 1f);

    static TurnBanner current;

    Image strip;
    Text label;
    RectTransform labelRect;

    public static void Show(bool myTurn)
    {
        GameObject main = GameObject.Find("Main Canvas");
        Canvas canvas = main != null ? main.GetComponent<Canvas>() : null;
        if (canvas == null) return;

        if (current != null) Destroy(current.gameObject); // a newer turn replaces an older banner

        GameObject root = new GameObject("TurnBanner", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = new Vector2(0f, 0.5f);
        rootRect.anchorMax = new Vector2(1f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(0f, StripHeight);
        rootRect.anchoredPosition = Vector2.zero;

        TurnBanner banner = root.AddComponent<TurnBanner>();
        current = banner;

        // dark strip behind the words
        banner.strip = root.AddComponent<Image>();
        banner.strip.color = new Color(0f, 0f, 0f, 0f);
        banner.strip.raycastTarget = false; // never blocks clicks

        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(root.transform, false);
        banner.labelRect = (RectTransform)textObj.transform;
        banner.labelRect.anchorMin = banner.labelRect.anchorMax = banner.labelRect.pivot = new Vector2(0.5f, 0.5f);
        banner.labelRect.sizeDelta = new Vector2(1200f, StripHeight);

        banner.label = textObj.AddComponent<Text>();
        banner.label.text = myTurn ? "YOUR TURN" : "OPPONENT'S TURN";
        banner.label.font = AnyUIFont();
        banner.label.fontSize = myTurn ? 86 : 64;
        banner.label.fontStyle = FontStyle.Bold;
        banner.label.alignment = TextAnchor.MiddleCenter;
        banner.label.horizontalOverflow = HorizontalWrapMode.Overflow;
        banner.label.verticalOverflow = VerticalWrapMode.Overflow;
        banner.label.color = myTurn ? MineColor : TheirsColor;
        banner.label.raycastTarget = false;
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(3f, -3f);

        SoundFX.Play(myTurn ? "turn_yours" : "turn_theirs", 0.8f, 0f);
        banner.StartCoroutine(banner.Run());
    }

    IEnumerator Run()
    {
        float width = ((RectTransform)transform).rect.width;
        float offscreen = width * 0.5f + 700f;

        // slide in from the left while the strip fades in
        for (float t = 0f; t < SlideTime; t += Time.unscaledDeltaTime)
        {
            float k = t / SlideTime;
            float ease = 1f - (1f - k) * (1f - k);
            labelRect.anchoredPosition = new Vector2(Mathf.Lerp(-offscreen, 0f, ease), 0f);
            strip.color = new Color(0f, 0f, 0f, 0.55f * k);
            yield return null;
        }
        labelRect.anchoredPosition = Vector2.zero;

        // hold, drifting slightly
        for (float t = 0f; t < HoldTime; t += Time.unscaledDeltaTime)
        {
            labelRect.anchoredPosition = new Vector2(Mathf.Lerp(0f, 30f, t / HoldTime), 0f);
            yield return null;
        }

        // slide out to the right while the strip fades away
        for (float t = 0f; t < SlideTime; t += Time.unscaledDeltaTime)
        {
            float k = t / SlideTime;
            labelRect.anchoredPosition = new Vector2(Mathf.Lerp(30f, offscreen, k * k), 0f);
            strip.color = new Color(0f, 0f, 0f, 0.55f * (1f - k));
            yield return null;
        }

        if (current == this) current = null;
        Destroy(gameObject);
    }

    static Font AnyUIFont()
    {
        GameObject lp = GameObject.Find("PlayerLPtext");
        Text t = lp != null ? lp.GetComponent<Text>() : null;
        if (t != null && t.font != null) return t.font;
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
