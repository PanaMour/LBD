using UnityEngine;
using UnityEngine.UI;

// One floating "-800" / "+200": pops in, drifts toward the middle of the
// screen, fades out and removes itself.
public class LPPopup : MonoBehaviour
{
    const float Life = 1.3f;
    const float Drift = 70f;
    static readonly Color DamageColor = new Color(1f, 0.25f, 0.2f);
    static readonly Color HealColor = new Color(0.35f, 1f, 0.45f);

    Text label;
    Outline outline;
    Vector3 start;
    Vector3 driftDirection;
    float age;

    public static void Spawn(RectTransform circle, Text style, int delta)
    {
        if (delta == 0 || circle == null) return;
        Canvas canvas = circle.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        RectTransform root = (RectTransform)canvas.rootCanvas.transform;

        // Where the circle sits on the canvas decides which way is "inwards".
        Vector3 at = root.InverseTransformPoint(circle.position);
        Vector3 inwards = new Vector3(at.x > 0f ? -1f : 1f, at.y > 0f ? -1f : 1f, 0f);
        float size = Mathf.Max(circle.rect.width, 80f);

        GameObject go = new GameObject("LPPopup", typeof(RectTransform));
        go.transform.SetParent(root, false);
        go.transform.SetAsLastSibling();
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(260f, 70f);

        LPPopup popup = go.AddComponent<LPPopup>();
        popup.start = at + new Vector3(inwards.x * size * 1.1f, inwards.y * size * 0.25f, 0f);
        popup.driftDirection = new Vector3(0f, inwards.y, 0f);
        rt.localPosition = popup.start;

        popup.label = go.AddComponent<Text>();
        popup.label.text = (delta > 0 ? "+" : "") + delta;
        popup.label.font = style != null && style.font != null ? style.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        popup.label.fontSize = 48;
        popup.label.fontStyle = FontStyle.Bold;
        popup.label.alignment = TextAnchor.MiddleCenter;
        popup.label.horizontalOverflow = HorizontalWrapMode.Overflow;
        popup.label.verticalOverflow = VerticalWrapMode.Overflow;
        popup.label.color = delta > 0 ? HealColor : DamageColor;
        popup.label.raycastTarget = false; // never blocks clicks

        popup.outline = go.AddComponent<Outline>();
        popup.outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        popup.outline.effectDistance = new Vector2(2f, -2f);
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = age / Life;
        if (t >= 1f) { Destroy(gameObject); return; }

        // quick pop (1.4x -> 1x), steady drift, fade over the last half
        float pop = age < 0.15f ? Mathf.Lerp(1.4f, 1f, age / 0.15f) : 1f;
        transform.localScale = Vector3.one * pop;
        transform.localPosition = start + driftDirection * Drift * t;

        float alpha = t < 0.5f ? 1f : 1f - (t - 0.5f) / 0.5f;
        Color c = label.color; c.a = alpha; label.color = c;
        Color o = outline.effectColor; o.a = 0.8f * alpha; outline.effectColor = o;
    }
}
