using UnityEngine;

// Water overlay on one labyrinth square: rises in after a delay (so a flood
// visibly spreads outward from the Card Base), ripples while it lasts, and
// sinks away when drained.
public class WaterTile : MonoBehaviour
{
    const float RiseTime = 0.45f;
    const float DrainTime = 0.8f;
    const float MaxAlpha = 0.62f;
    static readonly Color WaterColor = new Color(0.25f, 0.6f, 1f, 1f);

    Material mat;
    float startTime;
    float drainStartTime = -1f;
    float drainFromAlpha;
    float phase;

    public void Begin(Material shared, float delay)
    {
        Renderer r = GetComponent<Renderer>();
        r.sharedMaterial = shared;
        mat = r.material;
        startTime = Time.time + delay;
        phase = Random.value * Mathf.PI * 2f;
        transform.localScale = Vector3.zero;
        SetAlpha(0f);
    }

    public void Drain()
    {
        if (drainStartTime >= 0f) return;
        drainStartTime = Time.time;
        drainFromAlpha = mat != null ? mat.color.a : MaxAlpha;
    }

    void Update()
    {
        if (mat == null) return;

        float now = Time.time;
        mat.mainTextureOffset = new Vector2(now * 0.06f, now * 0.035f);

        if (drainStartTime >= 0f)
        {
            float d = Mathf.Clamp01((now - drainStartTime) / DrainTime);
            SetAlpha(drainFromAlpha * (1f - d));
            if (d >= 1f) Destroy(gameObject);
            return;
        }

        float t = Mathf.Clamp01((now - startTime) / RiseTime);
        if (t <= 0f) return;

        float eased = 1f - (1f - t) * (1f - t);
        transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1f, eased);
        float shimmer = 0.06f * Mathf.Sin(now * 1.7f + phase);
        SetAlpha(eased * (MaxAlpha + shimmer));
    }

    void SetAlpha(float a)
    {
        Color c = WaterColor;
        c.a = a;
        mat.color = c;
    }

    void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}
