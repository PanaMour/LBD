using UnityEngine;

// CPU mode: a giant Robo-Guardian sitting across the table from the player,
// as the face of the computer opponent. Only its upper body shows above the
// far edge (the table itself hides the rest). Purely visual and only on the
// host's screen (the only screen in a CPU game).
//
// It idles with a slow bob and sway, recoils when the CPU loses LP, hops when
// it takes LP from the player, and at the end of the game topples over (CPU
// lost) or jumps for joy (CPU won).
public class CpuAvatar : MonoBehaviour
{
    // Tuned by eye from the default camera view.
    const float Height = 15f;            // world units, head to toe
    const float ShownAboveTable = 0.48f; // share of its height above the table top
    const float GapBehindTable = 0.35f;  // between the table's far edge and its chest
    const int OwnLayer = 31;             // lit by its own light, nothing else is

    Transform model;
    Vector3 basePos;
    Quaternion baseRot;
    float lastCpuLP = -1f, lastPlayerLP = -1f;
    float recoil, hop;      // 1 -> 0 as each reaction plays out
    float fallen;           // 0 -> 1 as it topples over
    bool gameEnded;

    public static void Create()
    {
        if (FindObjectOfType<CpuAvatar>() != null) return;
        GameObject prefab = Resources.Load<GameObject>("robo-guardian");
        GameObject table = GameObject.Find("TableWood");
        if (prefab == null || table == null || table.GetComponent<Renderer>() == null) return;
        new GameObject("CPU Avatar").AddComponent<CpuAvatar>().Build(prefab, table.GetComponent<Renderer>().bounds);

        // Its hand of cards moves to the top-left so it doesn't hide the robot.
        GameObject hand = GameObject.Find("EnemyArea");
        if (hand != null) hand.transform.localPosition = new Vector3(-0.42f, hand.transform.localPosition.y, hand.transform.localPosition.z);
    }

    void Build(GameObject prefab, Bounds table)
    {
        model = Instantiate(prefab, transform).transform;
        foreach (Collider c in model.GetComponentsInChildren<Collider>(true)) c.enabled = false; // not clickable
        foreach (Transform t in model.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = OwnLayer;

        // Facing the player (the table's near end is towards -z).
        baseRot = Quaternion.Euler(0f, 180f, 0f);
        transform.rotation = baseRot;

        // Scale to the wanted height, whatever the model's own size.
        Bounds b = WorldBounds(model);
        if (b.size.y > 0.0001f) model.localScale *= Height / b.size.y;
        b = WorldBounds(model);

        // Behind the far edge, centred, sunk so only the upper body shows.
        float feetY = table.max.y - ShownAboveTable * Height;
        basePos = new Vector3(table.center.x, feetY, table.max.z + b.extents.z + GapBehindTable);
        transform.position = basePos;
        Bounds placed = WorldBounds(model); // centre it on the base point, feet at feetY
        model.position += new Vector3(basePos.x - placed.center.x, feetY - placed.min.y, basePos.z - placed.center.z);

        // The scene's light is focused on the board; it gets a gentle light of its own.
        Light light = new GameObject("CPU Avatar Light").AddComponent<Light>();
        light.transform.SetParent(transform, false);
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.shadows = LightShadows.None;
        light.cullingMask = 1 << OwnLayer;
        light.transform.rotation = Quaternion.Euler(30f, -20f, 0f); // from the player's side
    }

    float sink;

    // How far to lower it so the top of its head sits just under the top of
    // the screen (0 if it already fits).
    float HeadroomSink()
    {
        Camera cam = Camera.main;
        if (cam == null) return 0f;
        Ray top = cam.ViewportPointToRay(new Vector3(0.5f, 0.97f, 0f));
        if (Mathf.Abs(top.direction.z) < 0.0001f) return 0f;
        float d = (basePos.z - top.origin.z) / top.direction.z;
        if (d <= 0f) return 0f;
        float screenTopY = top.origin.y + top.direction.y * d;
        return Mathf.Max(0f, basePos.y + Height - screenTopY);
    }

    static Bounds WorldBounds(Transform t)
    {
        Renderer[] rs = t.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    void Update()
    {
        if (model == null) return;

        // React to life point changes.
        float cpuLP = OpponentLP.staticLP, playerLP = PlayerLP.staticLP;
        if (lastCpuLP >= 0f && cpuLP < lastCpuLP) recoil = 1f;
        if (lastPlayerLP >= 0f && playerLP < lastPlayerLP) hop = 1f;
        lastCpuLP = cpuLP; lastPlayerLP = playerLP;
        if (!gameEnded && GameManager.IsGameOver) gameEnded = true;

        float t = Time.time;
        float dt = Time.deltaTime;
        recoil = Mathf.Max(0f, recoil - dt * 1.6f);
        hop = Mathf.Max(0f, hop - dt * 1.4f);

        // Keep its head on screen: when the camera shows less above the table
        // (the zoomed-in board view), it ducks down to fit.
        sink = Mathf.Lerp(sink, HeadroomSink(), dt * 5f);

        Vector3 pos = basePos + Vector3.down * sink;
        Quaternion rot = baseRot;
        Vector3 forward = baseRot * Vector3.forward; // towards the player

        // idle: slow bob and sway
        pos += Vector3.up * Mathf.Sin(t * 1.6f) * 0.12f;
        rot *= Quaternion.Euler(0f, Mathf.Sin(t * 0.7f) * 4f, Mathf.Sin(t * 1.1f) * 1.5f);

        // hit: knocked back and shaking
        if (recoil > 0f)
        {
            float k = recoil;
            pos -= forward * Mathf.Sin(k * Mathf.PI) * 1.2f;
            rot *= Quaternion.Euler(-10f * Mathf.Sin(k * Mathf.PI), Mathf.Sin(t * 60f) * 5f * k, 0f);
        }

        // scored: a little hop
        if (hop > 0f) pos += Vector3.up * Mathf.Sin(hop * Mathf.PI) * 1.2f;

        if (gameEnded)
        {
            if (cpuLP <= 0f) // lost: slumps backwards and sinks behind the table
            {
                fallen = Mathf.Min(1f, fallen + dt * 0.6f);
                float f = 1f - (1f - fallen) * (1f - fallen); // eases out
                rot = baseRot * Quaternion.Euler(-40f * f, 0f, 0f);
                pos = basePos + Vector3.down * Height * 0.45f * f;
            }
            else // won: keeps jumping for joy
            {
                pos = basePos + Vector3.up * Mathf.Abs(Mathf.Sin(t * 4f)) * 1.5f;
            }
        }

        transform.SetPositionAndRotation(pos, rot);
    }
}
