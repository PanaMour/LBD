using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// The computer opponent for CPU mode. It sits on a player object that the
// host adds for a BotConnectionToClient (a connection with nobody behind it),
// so the game treats it exactly like a joining player: it moves first, owns
// its own cards and monsters, and its Card Base is the far row (15).
//
// On its turn it acts through the same Server* methods a real player's
// commands use, so every rule check applies to it too:
//   1. draw (the opening five on its first turn)
//   2. summon its strongest monster it can (tributing a weaker one for a 5+ star)
//   3. for each monster: attack the enemy base directly if it can, otherwise
//      a monster it beats; failing that, move toward the enemy base (preferring
//      squares next to monsters it can beat) and try again from there
//   4. end its turn
// There is a short pause between actions so the animations can be followed.
public class CpuPlayer : MonoBehaviour
{
    // Set by the main menu's "VS CPU" button; read when the host starts.
    public static bool Requested;

    const int BotConnectionId = 99999;
    const float ThinkTime = 0.9f;
    const float AfterAttackTime = 2.0f;
    const float AfterMoveTime = 0.8f;

    PlayerManager pm;
    bool acting;

    // ---- setup ----

    // Called from the host's PlayerManager.OnStartServer in CPU mode.
    public static IEnumerator SpawnWhenReady()
    {
        yield return new WaitForSeconds(0.5f);
        if (!NetworkServer.active) yield break;
        foreach (PlayerManager p in FindObjectsOfType<PlayerManager>())
            if (p.IsCpu) yield break; // already there

        BotConnectionToClient conn = new BotConnectionToClient(BotConnectionId);
        NetworkServer.AddConnection(conn);
        GameObject player = Instantiate(NetworkManager.singleton.playerPrefab);
        player.name = "CPU Player";
        NetworkServer.AddPlayerForConnection(conn, player);
        player.AddComponent<CpuPlayer>();
        Debug.Log("[CPU] Opponent joined.");
    }

    void Start()
    {
        pm = GetComponent<PlayerManager>();
    }

    void Update()
    {
        if (!NetworkServer.active || pm == null || acting || GameManager.IsGameOver) return;
        if (PlayerManager.ServerHostHasTurn) return; // the CPU plays the non-host side
        StartCoroutine(TakeTurn());
    }

    // ---- one turn ----

    IEnumerator TakeTurn()
    {
        acting = true;
        yield return new WaitForSeconds(1.2f); // let the turn banner play

        // 1. Draw.
        if (!pm.ServerHasDealtHand)
        {
            pm.ServerDealOpeningHand();
            yield return new WaitForSeconds(5.5f); // the opening hand arrives one card a second
        }
        else
        {
            pm.ServerDrawCard();
            yield return new WaitForSeconds(1.5f);
        }

        // 2. Summon.
        summoned = false;
        if (!GameManager.IsGameOver) yield return SummonRoutine();
        if (summoned) yield return new WaitForSeconds(ThinkTime + 0.6f);

        // 3. Each monster acts, strongest first.
        List<LabyrinthObject> mine = MyMonsters();
        mine.Sort((a, b) => Atk(b).CompareTo(Atk(a)));
        foreach (LabyrinthObject m in mine)
        {
            if (GameManager.IsGameOver) break;
            yield return WaitUntilCalm();
            if (m == null) continue; // destroyed meanwhile
            yield return ActWith(m);
        }

        // 4. End the turn (once nothing is waiting on a trap answer).
        yield return WaitUntilCalm();
        yield return new WaitForSeconds(0.6f);
        if (!GameManager.IsGameOver) pm.ServerEndTurn();
        while (!PlayerManager.ServerHostHasTurn && !GameManager.IsGameOver)
        {
            yield return new WaitForSeconds(0.5f);
            yield return WaitUntilCalm();
            if (!PlayerManager.ServerHostHasTurn) pm.ServerEndTurn(); // retry if it was refused
        }
        acting = false;
    }

    // Waits while an attack is paused on the human's trap decision.
    IEnumerator WaitUntilCalm()
    {
        while (LabyrinthObject.AnyAttackAwaitingTrap() && !GameManager.IsGameOver)
            yield return new WaitForSeconds(0.3f);
    }

    // ---- summoning ----

    bool summoned;

    IEnumerator SummonRoutine()
    {
        GridBehavior gb = FindObjectOfType<GridBehavior>();
        if (gb == null) yield break;

        List<ThisCard> hand = MonstersInHand();
        if (hand.Count == 0) yield break;
        hand.Sort((a, b) => b.actualATK.CompareTo(a.actualATK));

        List<LabyrinthObject> onField = MyMonsters();
        GridStat square = FreeBaseSquare(gb);

        foreach (ThisCard card in hand)
        {
            if (card.stars >= 5)
            {
                // Needs a tribute: the weakest of its own monsters still in its base.
                LabyrinthObject victim = WeakestInOwnBase(gb);
                if (victim == null || Atk(victim) >= card.actualATK) continue;
                GridStat victimSquare = victim.GetComponentInParent<GridStat>();
                pm.ServerDestroyOwnCard(victim.card, 0); // pays the tribute
                victim.ServerDestroyWithVanish();
                yield return new WaitForSeconds(0.7f); // the tributed monster is gone (and its vanish seen)
                summoned = Summon(card, square != null ? square : victimSquare, onField.Count - 1);
                yield break;
            }
            if (onField.Count >= 4 || square == null) continue; // no zone or square free
            summoned = Summon(card, square, onField.Count);
            yield break;
        }
    }

    bool Summon(ThisCard card, GridStat square, int zoneIndex)
    {
        if (square == null) return false;
        card.attackmode = true;
        if (!pm.ServerTrySummon(card, square.name)) return false;
        pm.ServerPlayCard(card.gameObject, Mathf.Clamp(zoneIndex, 0, 3)); // show the card in a monster zone
        return true;
    }

    // ---- a monster's turn ----

    IEnumerator ActWith(LabyrinthObject m)
    {
        if (!m.attackMode) yield break;
        GridBehavior gb = FindObjectOfType<GridBehavior>();
        if (gb == null) yield break;

        // From where it stands.
        if (TryAttackFromHere(m, gb)) { yield return new WaitForSeconds(AfterAttackTime); yield break; }

        // Move, then try again from the new square.
        if (m.hasMovedThisTurn || m.isImmobile) yield break;
        GridStat best = BestSquare(m, gb);
        if (best == null) yield break;

        yield return new WaitForSeconds(ThinkTime * 0.5f);
        m.ServerTryMove(best.name);
        yield return new WaitForSeconds(AfterMoveTime);
        if (m == null || GameManager.IsGameOver) yield break;

        if (TryAttackFromHere(m, gb)) yield return new WaitForSeconds(AfterAttackTime);
    }

    bool TryAttackFromHere(LabyrinthObject m, GridBehavior gb)
    {
        if (m.hasAttackedThisTurn) return false;

        if (gb.CanDirectAttack(m))
        {
            m.ServerTryDirectAttack();
            return true;
        }

        LabyrinthObject target = null;
        int bestValue = -1;
        foreach (LabyrinthObject enemy in EnemyMonsters())
        {
            if (!gb.CanAttackMonster(m, enemy) || !Beats(m, enemy)) continue;
            int value = Atk(enemy) + 100; // prefer knocking out the strongest monster it beats
            if (value > bestValue) { bestValue = value; target = enemy; }
        }
        if (target == null) return false;
        m.ServerTryAttack(target.gameObject);
        return true;
    }

    // Where to walk: a spot in front of an empty enemy base beats everything,
    // then a spot next to a monster it can beat; otherwise as close to the
    // enemy base (row 0) as possible, avoiding spots next to stronger enemies.
    GridStat BestSquare(LabyrinthObject m, GridBehavior gb)
    {
        GridStat here = m.GetComponentInParent<GridStat>();
        if (here == null) return null;
        bool enemyBaseEmpty = !EnemyBaseOccupied(gb, m);
        List<LabyrinthObject> enemies = EnemyMonsters();

        GridStat best = null;
        float bestScore = Score(m, here, gb, enemies, enemyBaseEmpty);
        foreach (KeyValuePair<GridStat, int> option in gb.ReachableSquares(m))
        {
            float score = Score(m, option.Key, gb, enemies, enemyBaseEmpty) + Random.Range(0f, 3f);
            if (score > bestScore) { bestScore = score; best = option.Key; }
        }
        return best; // null = stay put
    }

    float Score(LabyrinthObject m, GridStat s, GridBehavior gb, List<LabyrinthObject> enemies, bool enemyBaseEmpty)
    {
        float score = -s.y * 10f; // closer to the enemy base (row 0) is better
        int[] dx = { 0, 0, -1, 1 }, dy = { 1, -1, 0, 0 };
        for (int d = 0; d < 4; d++)
        {
            int nx = s.x + dx[d], ny = s.y + dy[d];
            if (gb.SquareAt(nx, ny) == null || !gb.OpenBetween(m, s.x, s.y, nx, ny)) continue;
            if (gb.IsEnemyBaseSquare(m, nx, ny) && enemyBaseEmpty) score += 1000f;
            foreach (LabyrinthObject e in enemies)
            {
                GridStat es = e.GetComponentInParent<GridStat>();
                if (es == null || es.x != nx || es.y != ny) continue;
                if (Beats(m, e)) score += 500f + Atk(e) * 0.1f;
                else if (e.attackMode && Atk(e) > Atk(m)) score -= 150f;
            }
        }
        return score;
    }

    // ---- reading the board ----

    bool Mine(NetworkBehaviour nb) => nb != null && nb.connectionToClient == pm.connectionToClient;

    List<LabyrinthObject> MyMonsters()
    {
        var list = new List<LabyrinthObject>();
        foreach (LabyrinthObject m in FindObjectsOfType<LabyrinthObject>())
            if (Mine(m) && m.card != null) list.Add(m);
        return list;
    }

    List<LabyrinthObject> EnemyMonsters()
    {
        var list = new List<LabyrinthObject>();
        foreach (LabyrinthObject m in FindObjectsOfType<LabyrinthObject>())
            if (!Mine(m) && m.card != null) list.Add(m);
        return list;
    }

    List<ThisCard> MonstersInHand()
    {
        var list = new List<ThisCard>();
        GameObject enemyArea = GameObject.Find("EnemyArea"); // the CPU's hand, as the host sees it
        foreach (ThisCard c in FindObjectsOfType<ThisCard>())
        {
            if (!Mine(c) || c.beInGraveyard || PlayerManager.IsCardOnField(c.gameObject)) continue;
            if (enemyArea != null && c.transform.parent != enemyArea.transform) continue;
            list.Add(c);
        }
        return list;
    }

    GridStat FreeBaseSquare(GridBehavior gb)
    {
        int row = gb.rows - 1; // the joining side's Card Base
        var free = new List<GridStat>();
        for (int x = 2; x <= 8; x++)
        {
            GridStat s = gb.SquareAt(x, row);
            if (s != null && s.GetComponentInChildren<LabyrinthObject>() == null) free.Add(s);
        }
        if (free.Count == 0) return null;
        free.Sort((a, b) => Mathf.Abs(a.x - 5).CompareTo(Mathf.Abs(b.x - 5))); // middle first
        return free[0];
    }

    LabyrinthObject WeakestInOwnBase(GridBehavior gb)
    {
        LabyrinthObject weakest = null;
        foreach (LabyrinthObject m in MyMonsters())
        {
            GridStat s = m.GetComponentInParent<GridStat>();
            if (s == null || s.y != gb.rows - 1) continue;
            if (weakest == null || Atk(m) < Atk(weakest)) weakest = m;
        }
        return weakest;
    }

    bool EnemyBaseOccupied(GridBehavior gb, LabyrinthObject me)
    {
        foreach (LabyrinthObject e in EnemyMonsters())
        {
            GridStat s = e.GetComponentInParent<GridStat>();
            if (s != null && gb.IsEnemyBaseSquare(me, s.x, s.y)) return true;
        }
        return false;
    }

    static ThisCard CardOf(LabyrinthObject m) => m != null && m.card != null ? m.card.GetComponent<ThisCard>() : null;
    static int Atk(LabyrinthObject m) { ThisCard c = CardOf(m); return c != null ? c.actualATK : 0; }

    // Would this monster win (destroy the target without being destroyed)?
    static bool Beats(LabyrinthObject attacker, LabyrinthObject target)
    {
        ThisCard a = CardOf(attacker), t = CardOf(target);
        if (a == null || t == null) return false;
        return target.attackMode ? a.actualATK > t.actualATK : a.actualATK > t.actualDEF;
    }
}
