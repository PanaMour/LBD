using UnityEngine;
using UnityEngine.UI;

// Shared life-point display for PlayerLP and OpponentLP. The number and the
// circle's fill run smoothly to the real value instead of jumping, every
// change pops up as "-800" (red) or "+200" (green) next to the circle, and
// the circle shakes when damage is taken. Purely visual: the real value is
// still the static LP each script reads.
public class LPFeedback
{
    const float MinSpeed = 600f;    // LP per second, at least
    const float CatchUpTime = 0.6f; // big changes still finish in about this long
    const float ShakeTime = 0.35f;
    const float ShakeStrength = 10f;

    readonly RectTransform circle;
    readonly Image bar;
    readonly Text text;
    readonly Vector3 restPosition;

    float shown;
    float last;
    float shakeLeft;

    public LPFeedback(RectTransform circle, Image bar, Text text, float startLP)
    {
        this.circle = circle;
        this.bar = bar;
        this.text = text;
        restPosition = circle.localPosition;
        shown = startLP;
        last = startLP;
        BattleAnimation.ActiveBeforeImpact = 0; // new match: no battle can be in flight
    }

    // A change that arrives while a battle animation hasn't hit yet waits
    // for the impact (at most this long, in case an animation was cut short).
    const float MaxHoldForBattle = 0.6f;
    float heldFor;

    public void Tick(float actual, float maxLP)
    {
        bool holdForBattle = actual != last && BattleAnimation.ActiveBeforeImpact > 0 && heldFor < MaxHoldForBattle;
        if (holdForBattle) heldFor += Time.deltaTime;

        if (actual != last && !holdForBattle)
        {
            heldFor = 0f;
            if (last > 0f)
            {
                int delta = Mathf.RoundToInt(actual - last);
                LPPopup.Spawn(circle, text, delta);
                if (delta < 0) shakeLeft = ShakeTime;
            }
            else
            {
                shown = actual; // the reset after a finished match: no popup, no count-up
            }
            last = actual;
        }

        // "last" is the value already announced; the display counts toward it.
        float speed = Mathf.Max(MinSpeed, Mathf.Abs(last - shown) / CatchUpTime);
        shown = Mathf.MoveTowards(shown, last, speed * Time.deltaTime);

        if (bar != null) bar.fillAmount = Mathf.Clamp01(shown / maxLP);
        if (text != null) text.text = Mathf.Max(0f, Mathf.Round(shown)) + " LP";

        if (shakeLeft > 0f)
        {
            shakeLeft -= Time.deltaTime;
            float strength = ShakeStrength * Mathf.Clamp01(shakeLeft / ShakeTime);
            circle.localPosition = restPosition + (Vector3)(Random.insideUnitCircle * strength);
        }
        else if (circle.localPosition != restPosition)
        {
            circle.localPosition = restPosition;
        }
    }
}
