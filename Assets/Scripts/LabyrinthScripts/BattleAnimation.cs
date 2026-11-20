using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Plays a battle on this screen: both monsters grow for the fight, the
// attacker lunges at the defender, the defender shakes on impact, each
// monster's number ("ATK 1800", "DEF 1100") hovers above it (the loser's
// greys out), and whoever lost shrinks away. The battle itself is already settled
// by the host; this only shows it. It works on stand-in copies of the two
// models, so it can finish even after the losing monster has been removed.
public class BattleAnimation : MonoBehaviour
{
    public const int DefenderDies = 0;
    public const int AttackerDies = 1;
    public const int BothDie = 2;
    public const int NobodyDies = 3; // e.g. attacking a stronger Defense position monster
    public const int Negated = 4;    // Intercept

    const float LungeTime = 0.3f;
    const float ReturnTime = 0.25f;
    const float VanishTime = 0.5f;
    const float HoldTime = 0.4f;
    const float LabelLift = 0.35f;   // world units above the taller (grown) model
    const float Grow = 1.7f;         // fighters are drawn bigger during the fight

    static readonly Color AttackColor = new Color(1f, 0.55f, 0.15f);
    static readonly Color EnemyAttackColor = new Color(1f, 0.3f, 0.25f);
    static readonly Color DefenseColor = new Color(0.4f, 0.75f, 1f);
    static readonly Color LoserColor = new Color(0.6f, 0.6f, 0.6f);

    // Life-point popups wait while a battle hasn't reached its impact yet, so
    // the "-800" lands together with the hit (see LPFeedback).
    public static int ActiveBeforeImpact;

    class Fighter
    {
        public LabyrinthObject real;
        public GameObject hiddenModel;
        public Transform ghost;
        public Vector3 home;
        public Vector3 baseScale;
        public float height;
        public bool loses;
    }

    // One number above each monster, pushed apart along the line between
    // them on screen so it's always clear which number belongs to whom.
    Text attackerLabel;
    Text defenderLabel;
    Vector3 attackerTop;
    Vector3 defenderTop;

    public static void Play(LabyrinthObject attacker, LabyrinthObject defender,
        int attackerValue, int defenderValue, bool defenderInDefense, int result)
    {
        if (attacker == null || defender == null) return;
        BattleAnimation anim = new GameObject("BattleAnimation").AddComponent<BattleAnimation>();
        anim.StartCoroutine(anim.Run(attacker, defender, attackerValue, defenderValue, defenderInDefense, result));
    }

    IEnumerator Run(LabyrinthObject attackerObj, LabyrinthObject defenderObj,
        int attackerValue, int defenderValue, bool defenderInDefense, int result)
    {
        Fighter attacker = MakeFighter(attackerObj);
        Fighter defender = MakeFighter(defenderObj);
        attacker.loses = result == AttackerDies || result == BothDie;
        defender.loses = result == DefenderDies || result == BothDie;

        string atkPart = "ATK " + attackerValue;
        string defPart = (defenderInDefense ? "DEF " : "ATK ") + defenderValue;
        Color defColor = defenderInDefense ? DefenseColor : EnemyAttackColor;
        attackerLabel = MakeLabel(Colored(atkPart, AttackColor));
        defenderLabel = MakeLabel(Colored(defPart, defColor));
        attackerTop = attacker.home + Vector3.up * (attacker.height * Grow + LabelLift);
        defenderTop = defender.home + Vector3.up * (defender.height * Grow + LabelLift);

        Vector3 toDefender = defender.home - attacker.home;
        ActiveBeforeImpact++;
        bool impactDone = false;
        try
        {
            // 1. Both grow, and the attacker lunges most of the way in.
            for (float t = 0f; t < LungeTime; t += Time.deltaTime)
            {
                float k = t / LungeTime;
                SetScale(attacker, Mathf.Lerp(1f, Grow, k));
                SetScale(defender, Mathf.Lerp(1f, Grow, k));
                Place(attacker, attacker.home + toDefender * 0.6f * k * k);
                Place(defender, defender.home);
                PlaceLabels();
                yield return null;
            }
            ActiveBeforeImpact--;
            impactDone = true;

            // 2. Impact: the loser's number greys out, the defender shakes and
            // flares, and the attacker pulls back.
            if (result == Negated)
                SetText(defenderLabel, "<color=#9FD8FF>Negated!</color>");
            else
            {
                SetText(attackerLabel, Colored(atkPart, attacker.loses ? LoserColor : AttackColor));
                SetText(defenderLabel, Colored(defPart, defender.loses ? LoserColor : defColor));
            }
            for (float t = 0f; t < ReturnTime; t += Time.deltaTime)
            {
                float k = t / ReturnTime;
                Place(attacker, attacker.home + toDefender * 0.6f * (1f - k));
                float shake = 0.08f * (1f - k);
                Place(defender, defender.home + new Vector3(Random.Range(-shake, shake), 0f, Random.Range(-shake, shake)));
                SetScale(defender, Grow * (1f + 0.15f * (1f - k)));
                float pop = 1f + 0.3f * (1f - k);
                if (attackerLabel != null) attackerLabel.transform.localScale = Vector3.one * pop;
                if (defenderLabel != null) defenderLabel.transform.localScale = Vector3.one * pop;
                PlaceLabels();
                yield return null;
            }
            Place(attacker, attacker.home);
            Place(defender, defender.home);
            SetScale(defender, Grow);
            if (attackerLabel != null) attackerLabel.transform.localScale = Vector3.one;
            if (defenderLabel != null) defenderLabel.transform.localScale = Vector3.one;

            // 3. Losers shrink and sink away.
            for (float t = 0f; t < VanishTime; t += Time.deltaTime)
            {
                float k = t / VanishTime;
                foreach (Fighter f in new[] { attacker, defender })
                {
                    if (!f.loses) continue;
                    SetScale(f, Grow * (1f - k));
                    Place(f, f.home - Vector3.up * f.height * 0.5f * k);
                }
                PlaceLabels();
                yield return null;
            }

            // 4. Survivors settle back to normal size while the label fades.
            for (float t = 0f; t < HoldTime; t += Time.deltaTime)
            {
                float k = t / HoldTime;
                foreach (Fighter f in new[] { attacker, defender })
                    if (!f.loses) SetScale(f, Mathf.Lerp(Grow, 1f, k));
                Fade(attackerLabel, 1f - k);
                Fade(defenderLabel, 1f - k);
                PlaceLabels();
                yield return null;
            }
        }
        finally
        {
            if (!impactDone) ActiveBeforeImpact--;
            Cleanup(attacker);
            Cleanup(defender);
            if (attackerLabel != null) Destroy(attackerLabel.gameObject);
            if (defenderLabel != null) Destroy(defenderLabel.gameObject);
        }
        Destroy(gameObject);
    }

    // Hide the real model and work on a copy of it in its exact place.
    static Fighter MakeFighter(LabyrinthObject monster)
    {
        Fighter f = new Fighter { real = monster, home = monster.transform.position, height = 0.5f };
        GameObject model = monster.current3DModel;
        if (model == null) return f;

        GameObject copy = Instantiate(model, model.transform.position, model.transform.rotation);
        copy.name = model.name + " (battle)";
        copy.transform.localScale = model.transform.lossyScale;
        foreach (Collider c in copy.GetComponentsInChildren<Collider>(true)) Destroy(c);
        foreach (MonoBehaviour b in copy.GetComponentsInChildren<MonoBehaviour>(true)) b.enabled = false;

        f.ghost = copy.transform;
        f.home = copy.transform.position;
        f.baseScale = copy.transform.localScale;

        Renderer[] renderers = copy.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            f.height = Mathf.Max(0.2f, b.max.y - f.home.y);
        }

        model.SetActive(false);
        f.hiddenModel = model;
        return f;
    }

    static void Place(Fighter f, Vector3 position)
    {
        if (f.ghost != null) f.ghost.position = position;
    }

    static void SetScale(Fighter f, float factor)
    {
        if (f.ghost != null) f.ghost.localScale = f.baseScale * Mathf.Max(0f, factor);
    }

    static void Cleanup(Fighter f)
    {
        if (f.ghost != null) Destroy(f.ghost.gameObject);
        // Survivors get their real model back (losers' monsters are removed by the host).
        if (!f.loses && f.real != null && f.hiddenModel != null) f.hiddenModel.SetActive(true);
    }

    // ---- the clash label (UI text that follows a point in the 3D world) ----

    static Canvas canvas;

    static string Colored(string text, Color color)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
    }

    // Each label sits above its own monster; the two are pushed apart along
    // the on-screen line between the monsters (sideways for a side-by-side
    // pair, up/down for a pair one above the other) so they never overlap.
    void PlaceLabels()
    {
        Camera cam = Camera.main;
        if (cam == null || canvas == null) return;
        Vector2 a = ToCanvas(cam, attackerTop);
        Vector2 d = ToCanvas(cam, defenderTop);
        Vector2 mid = (a + d) * 0.5f;
        Vector2 dir = a - d;
        dir = dir.sqrMagnitude < 1f ? Vector2.right : dir.normalized;

        // Each number goes straight over its own monster; only if the two
        // would overlap are they pushed apart along the line between the
        // monsters, by just enough to clear each other.
        float widthA = attackerLabel != null ? attackerLabel.preferredWidth : 0f;
        float widthD = defenderLabel != null ? defenderLabel.preferredWidth : 0f;
        float needX = (widthA + widthD) * 0.5f + 36f;
        float needY = 52f;
        float need = Mathf.Min(
            Mathf.Abs(dir.x) > 0.01f ? needX / Mathf.Abs(dir.x) : float.MaxValue,
            Mathf.Abs(dir.y) > 0.01f ? needY / Mathf.Abs(dir.y) : float.MaxValue);
        Vector2 aPos = a, dPos = d;
        if ((a - d).magnitude < need)
        {
            aPos = mid + dir * need * 0.5f;
            dPos = mid - dir * need * 0.5f;
        }
        if (attackerLabel != null) attackerLabel.rectTransform.localPosition = aPos;
        if (defenderLabel != null) defenderLabel.rectTransform.localPosition = dPos;
    }

    static Vector2 ToCanvas(Camera cam, Vector3 worldPoint)
    {
        Vector3 screen = cam.WorldToScreenPoint(worldPoint);
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screen, uiCam, out Vector2 local);
        return local;
    }

    static void SetText(Text label, string text)
    {
        if (label != null) label.text = text;
    }

    static void Fade(Text label, float alpha)
    {
        if (label == null) return;
        Color c = label.color; c.a = alpha; label.color = c;
        Outline o = label.GetComponent<Outline>();
        if (o != null) { Color oc = o.effectColor; oc.a = 0.8f * alpha; o.effectColor = oc; }
    }

    static Text MakeLabel(string text)
    {
        if (canvas == null)
        {
            GameObject main = GameObject.Find("Main Canvas");
            canvas = main != null ? main.GetComponent<Canvas>() : null;
            if (canvas == null) return null;
        }

        GameObject go = new GameObject("BattleNumber", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetAsLastSibling();
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(260f, 60f);

        Text label = go.AddComponent<Text>();
        label.text = text;
        label.font = AnyUIFont();
        label.fontSize = 34;
        label.fontStyle = FontStyle.Bold;
        label.supportRichText = true;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.color = Color.white; // the numbers carry their own colours
        label.raycastTarget = false;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);
        return label;
    }

    static Font AnyUIFont()
    {
        GameObject lp = GameObject.Find("PlayerLPtext");
        Text t = lp != null ? lp.GetComponent<Text>() : null;
        if (t != null && t.font != null) return t.font;
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
