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
//   1. draw (the opening five on its first turn), then play its draw, damage
//      and healing spells
//   2. summon its strongest monster it can (tributing a weaker one for a 5+ star),
//      then use its spells on the board (removal, equips, ...) and set traps
//   3. for each monster: attack the enemy base directly if it can, otherwise
//      a monster it beats; failing that, move toward the enemy base (preferring
//      squares next to monsters it can beat) and try again from there
//   4. end its turn
// There is a short pause between actions so the animations can be followed.
// When attacked, it decides for itself whether to spring one of its set traps
// (ChooseTrap).
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
        CpuAvatar.Create(); // its giant Robo-Guardian behind the board
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

        if (!GameManager.IsGameOver) yield return PlayOpeningSpells();

        // 2. Summon, then spells on the board and traps.
        summoned = false;
        if (!GameManager.IsGameOver) yield return SummonRoutine();
        if (summoned) yield return new WaitForSeconds(ThinkTime + 0.6f);
        if (!GameManager.IsGameOver) yield return PlayBoardSpells();
        if (!GameManager.IsGameOver) yield return SetTraps();

        // 3. Each monster acts: any that can hit the enemy base right now
        // first (it may be the winning blow), then strongest first.
        GridBehavior grid = FindObjectOfType<GridBehavior>();
        List<LabyrinthObject> mine = MyMonsters();
        mine.Sort((a, b) =>
        {
            bool da = grid != null && grid.CanDirectAttack(a), db = grid != null && grid.CanDirectAttack(b);
            if (da != db) return da ? -1 : 1;
            return Atk(b).CompareTo(Atk(a));
        });
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

    // A Labyrinth monster from a treasure chest: Special Summoned to a free
    // Card Base square if it has an open zone, otherwise added to the hand.
    public void PlaceTreasureMonster(ThisCard card)
    {
        GridBehavior gb = FindObjectOfType<GridBehavior>();
        int onField = MyMonsters().Count;
        GridStat square = gb != null ? FreeBaseSquare(gb) : null;
        if (card != null && onField < 4 && square != null)
        {
            card.attackmode = true;
            if (pm.ServerTrySummon(card, square.name))
            {
                pm.ServerPlayCard(card.gameObject, onField);
                return;
            }
        }
        if (card != null) pm.ServerShowInHand(card.gameObject);
    }

    // ---- a monster's turn ----

    IEnumerator ActWith(LabyrinthObject m)
    {
        GridBehavior gb = FindObjectOfType<GridBehavior>();
        if (gb == null) yield break;

        // In Defense (e.g. after its own Last Stand Barrier): back to Attack
        // when it's not a wall-type monster, or when it can hit the base now.
        if (!m.attackMode)
        {
            if (Atk(m) < Def(m) && !gb.CanDirectAttack(m)) yield break;
            Debug.Log("[CPU] Switches " + (CardOf(m) != null ? CardOf(m).cardName : m.name) + " to Attack position.");
            pm.ServerChangeBattlePosition(m.card, true);
            yield return new WaitForSeconds(0.8f);
            if (m == null || !m.attackMode) yield break; // refused (already changed position this turn)
        }

        // From where it stands (weakening a target first if a spell lets it win).
        yield return CombatSpell(m, gb);
        if (m == null || GameManager.IsGameOver) yield break;
        if (TryAttackFromHere(m, gb)) { yield return new WaitForSeconds(AfterAttackTime); yield break; }

        // Move, then try again from the new square.
        if (m.hasMovedThisTurn || m.isImmobile) yield break;
        GridStat best = BestSquare(m, gb);
        if (best == null) yield break;

        yield return new WaitForSeconds(ThinkTime * 0.5f);
        m.ServerTryMove(best.name);
        yield return new WaitForSeconds(AfterMoveTime);
        if (m == null || GameManager.IsGameOver) yield break;

        yield return CombatSpell(m, gb);
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

    // ---- spells ----

    const int MooyanCurry = 1, PotOfGreed = 2, SeMastigwnw = 3, TargetPractice = 4, Enrage = 5, Succumb = 6,
        DivineSword = 7, Blastigator = 8, Exhaust = 9, LabyrinthDice = 16, MechanicalLegs = 18,
        FleetfootBlessing = 19, WeightedShackles = 20, LabyrinthLootbox = 22;

    // Draw, damage and healing: played straight after the draw.
    IEnumerator PlayOpeningSpells()
    {
        int[] order = { PotOfGreed, LabyrinthLootbox, SeMastigwnw, MooyanCurry };
        foreach (int id in order)
        {
            foreach (ThisMagic spell in SpellsInHand(id))
            {
                if (GameManager.IsGameOver || FreeActionSlot() < 0) yield break;
                Magic data = PlayerManager.MagicData(id);
                if (data == null) continue;

                Debug.Log("[CPU] Plays " + data.cardName + ".");
                pm.ServerPlayCard(spell.gameObject, FreeActionSlot());
                yield return new WaitForSeconds(1.0f);
                for (int i = 0; i < data.drawXcards; i++)
                {
                    pm.ServerDrawCard();
                    yield return new WaitForSeconds(1.2f);
                }
                if (data.damageDealtBySpell > 0) pm.ServerChangeLP(0, data.damageDealtBySpell);
                if (data.damageHealedBySpell > 0) pm.ServerChangeLP(-data.damageHealedBySpell, 0);
                yield return new WaitForSeconds(0.6f);
                pm.ServerFinishSpell(spell.gameObject);
                yield return new WaitForSeconds(0.8f);
            }
        }
    }

    // Removal, position changes and equips, once its monsters are out.
    IEnumerator PlayBoardSpells()
    {
        GridBehavior gb = FindObjectOfType<GridBehavior>();
        if (gb == null) yield break;

        foreach (ThisMagic spell in SpellsInHand(-1))
        {
            if (GameManager.IsGameOver || FreeActionSlot() < 0) yield break;
            LabyrinthObject target = BoardSpellTarget(spell.thisId, gb);
            if (target == null) continue;
            yield return CastOn(spell, target);
        }
    }

    // Who a board spell should go on, or null to keep it for now.
    LabyrinthObject BoardSpellTarget(int id, GridBehavior gb)
    {
        List<LabyrinthObject> enemies = EnemyMonsters();
        List<LabyrinthObject> mine = MyMonsters();
        switch (id)
        {
            case TargetPractice: // destroy the biggest threat
                return Best(enemies, e => Threat(e, gb));

            case Succumb: // a strong attacker of theirs goes to Defense
                return Best(enemies.FindAll(e => e.attackMode && Atk(e) > Def(e) && Atk(e) >= 1200), e => Threat(e, gb));

            case WeightedShackles: // slow down their strongest walker
                return Best(enemies.FindAll(e => !e.isImmobile && Atk(e) >= 1200), e => Threat(e, gb));

            case LabyrinthDice: // throw a monster that's closing in on its base somewhere random
                return Best(enemies.FindAll(e => RowsFromMyBase(e, gb) <= 4), e => Threat(e, gb));

            case DivineSword:
            case Blastigator:
                return Best(mine.FindAll(m => Atk(m) > 0 && !IsEquipped(m)), m => Atk(m));

            case FleetfootBlessing:
                return Best(mine.FindAll(m => Atk(m) > 0 && !m.isImmobile && !IsEquipped(m)), m => Atk(m));

            case MechanicalLegs:
                return Best(mine.FindAll(m => Atk(m) > 0 && m.isImmobile && !IsEquipped(m)), m => Atk(m));
        }
        if (ThisMagic.TryGetRequiredEquipType(id, out Type type)) // typed armour (+400/+400)
            return Best(mine.FindAll(m => !IsEquipped(m) && CardOf(m) != null && CardOf(m).currentTypes.Contains(type)), m => Atk(m));
        return null; // Exhaust and Enrage wait for a fight; the rest it doesn't use
    }

    // Before attacking with m: if no fight in reach is winnable but one would
    // be after Exhaust (-400 ATK) or Enrage (Defense -> Attack), use it.
    IEnumerator CombatSpell(LabyrinthObject m, GridBehavior gb)
    {
        if (m.hasAttackedThisTurn || FreeActionSlot() < 0) yield break;
        List<LabyrinthObject> inReach = EnemyMonsters().FindAll(e => gb.CanAttackMonster(m, e));
        if (inReach.Count == 0 || inReach.Exists(e => Beats(m, e)) || gb.CanDirectAttack(m)) yield break;

        List<ThisMagic> exhaust = SpellsInHand(Exhaust);
        if (exhaust.Count > 0)
        {
            LabyrinthObject e = Best(inReach.FindAll(x => x.attackMode && Atk(m) > Atk(x) - 400), x => Atk(x));
            if (e != null) { yield return CastOn(exhaust[0], e); yield break; }
        }
        List<ThisMagic> enrage = SpellsInHand(Enrage);
        if (enrage.Count > 0)
        {
            LabyrinthObject e = Best(inReach.FindAll(x => !x.attackMode && Atk(m) > Atk(x)), x => Def(x));
            if (e != null) yield return CastOn(enrage[0], e);
        }
    }

    IEnumerator CastOn(ThisMagic spell, LabyrinthObject target)
    {
        Debug.Log("[CPU] Plays " + spell.magicName + " on " + (CardOf(target) != null ? CardOf(target).cardName : target.name) + ".");
        pm.ServerPlayCard(spell.gameObject, FreeActionSlot());
        yield return new WaitForSeconds(1.0f);
        if (target != null) pm.ServerExecuteMagicEffect(spell.gameObject, target.gameObject);
        yield return new WaitForSeconds(1.2f);
    }

    // ---- traps ----

    // Sets its traps face down, keeping one slot free for spells.
    // (Honey Snare is skipped: nothing in the game triggers it yet.)
    IEnumerator SetTraps()
    {
        foreach (ThisAction trap in InHand<ThisAction>())
        {
            if (GameManager.IsGameOver || FreeActionSlotCount() <= 1) yield break;
            if (trap.thisId == 3) continue;
            pm.ServerPlayCard(trap.gameObject, FreeActionSlot());
            yield return new WaitForSeconds(0.8f);
        }
    }

    // Called by the attack code when one of its monsters is attacked: which of
    // the set traps this attack triggers (if any) is worth springing.
    public ThisAction ChooseTrap(List<ThisAction> options, LabyrinthObject attacker, LabyrinthObject defender, bool defenderWouldDie)
    {
        if (options == null || options.Count == 0) return null;
        int a = Atk(attacker), atk = Atk(defender), def = Def(defender);
        bool loses = defender.attackMode ? a >= atk : a > def;

        int graveyardMonsters = 0;
        foreach (ThisCard c in FindObjectsOfType<ThisCard>())
            if (Mine(c) && c.beInGraveyard) graveyardMonsters++;
        int weakened = a - 100 * graveyardMonsters;

        ThisAction Find(int id) => options.Find(t => t.thisId == id);

        if (loses && Find(5) != null) return Find(5); // Intercept: the attack never happens
        if (loses && defender.attackMode && a <= def + atk && Find(4) != null) return Find(4); // Last Stand Barrier saves it
        if (loses && Find(1) != null && ChooseDiscard() != null
            && (defender.attackMode ? weakened < atk : weakened <= def)) return Find(1); // Echo of Silence turns the fight
        if (defenderWouldDie && Find(2) != null) return Find(2); // Phantom Binding: the winner can't move any more
        return null;
    }

    // Echo of Silence's cost: its least useful card in hand.
    public GameObject ChooseDiscard()
    {
        ThisCard weakest = Best(InHand<ThisCard>(), c => -c.actualATK);
        if (weakest != null) return weakest.gameObject;
        foreach (ThisMagic m in InHand<ThisMagic>()) return m.gameObject;
        foreach (ThisAction t in InHand<ThisAction>()) return t.gameObject;
        return null;
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

    List<ThisCard> MonstersInHand() => InHand<ThisCard>().FindAll(c => !c.beInGraveyard && !PlayerManager.IsCardOnField(c.gameObject));

    // Its cards in hand: on the host's screen, the cards in the opponent's hand.
    List<T> InHand<T>() where T : NetworkBehaviour
    {
        var list = new List<T>();
        GameObject enemyArea = GameObject.Find("EnemyArea");
        if (enemyArea == null) return list;
        foreach (Transform child in enemyArea.transform)
        {
            T c = child.GetComponent<T>();
            if (c != null && Mine(c)) list.Add(c);
        }
        return list;
    }

    // Spells in hand with this id (-1: all of them).
    List<ThisMagic> SpellsInHand(int id) => InHand<ThisMagic>().FindAll(m => id < 0 || m.thisId == id);

    // Its spell & trap zones, as the host's screen shows them.
    int FreeActionSlot()
    {
        for (int i = 0; i < pm.EnemyActionSockets.Count; i++)
            if (SlotFree(pm.EnemyActionSockets[i])) return i;
        return -1;
    }

    int FreeActionSlotCount()
    {
        int n = 0;
        foreach (GameObject s in pm.EnemyActionSockets) if (SlotFree(s)) n++;
        return n;
    }

    static bool SlotFree(GameObject slot)
    {
        if (slot == null) return false;
        foreach (Transform child in slot.transform)
            if (child.GetComponent<ThisMagic>() != null || child.GetComponent<ThisAction>() != null) return false;
        return true;
    }

    static bool IsEquipped(LabyrinthObject m) { ThisCard c = CardOf(m); return c != null && c.equippedTo != null; }

    int RowsFromMyBase(LabyrinthObject m, GridBehavior gb)
    {
        GridStat s = m.GetComponentInParent<GridStat>();
        return s != null ? (gb.rows - 1) - s.y : 99;
    }

    // How dangerous one of the player's monsters is: strong, and close to the CPU's base.
    float Threat(LabyrinthObject e, GridBehavior gb) => Atk(e) + (15 - Mathf.Min(RowsFromMyBase(e, gb), 15)) * 60f;

    static T Best<T>(List<T> items, System.Func<T, float> score) where T : class
    {
        T best = null;
        float bestScore = float.MinValue;
        foreach (T item in items)
        {
            float sc = score(item);
            if (best == null || sc > bestScore) { best = item; bestScore = sc; }
        }
        return best;
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
    static int Def(LabyrinthObject m) { ThisCard c = CardOf(m); return c != null ? c.actualDEF : 0; }

    // Would this monster win (destroy the target without being destroyed)?
    static bool Beats(LabyrinthObject attacker, LabyrinthObject target)
    {
        ThisCard a = CardOf(attacker), t = CardOf(target);
        if (a == null || t == null) return false;
        return target.attackMode ? a.actualATK > t.actualATK : a.actualATK > t.actualDEF;
    }
}
