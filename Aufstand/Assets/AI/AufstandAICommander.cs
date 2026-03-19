// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using Aufstand.Game;
using Aufstand.Game.Strategic;
using Aufstand.Scripting;

namespace Aufstand.AI
{
    /// <summary>
    /// AI strategic commander — runs bytecode for the enemy faction.
    /// Each difficulty tier is a different Python script.
    /// Same engine as the player; no special C# logic.
    /// </summary>
    public class AufstandAICommander : MonoBehaviour
    {
        private Faction _faction;
        private HexMap _map;
        private ResourceManager _resources;
        private StrategicMatchManager _match;
        private StrategicFogOfWar _fog;
        private AufstandStrategicProgram _program;

        public CommanderDifficulty Difficulty { get; private set; }
        public AufstandStrategicProgram Program => _program;

        public void Initialize(Faction faction, HexMap map, ResourceManager resources,
                               StrategicMatchManager match, StrategicFogOfWar fog,
                               CommanderDifficulty difficulty)
        {
            _faction = faction;
            _map = map;
            _resources = resources;
            _match = match;
            _fog = fog;
            Difficulty = difficulty;

            _program = gameObject.AddComponent<AufstandStrategicProgram>();
            string code = GetAICode(difficulty);
            _program.Initialize(faction, map, resources, match, fog, code);
        }

        public static string GetAICode(CommanderDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CommanderDifficulty.Rekrut:    return REKRUT;
                case CommanderDifficulty.Feldwebel: return FELDWEBEL;
                case CommanderDifficulty.Oberst:    return OBERST;
                case CommanderDifficulty.General:   return GENERAL;
                default: return REKRUT;
            }
        }

        // ── Rekrut: passive, slow, minimal initiative ──
        private static readonly string REKRUT = @"# ═══════════════════════════════════
# REKRUT — Passive Defender
# Reinforce weak frontlines, rarely attacks
# ═══════════════════════════════════
while True:
    total = get_territory_count()
    manp = get_manpower()

    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i):
                str = get_territory_strength(i)
                if str < 3:
                    if manp > 40:
                        reinforce(i)
                        manp = get_manpower()
        i = i + 1

    wait
";

        // ── Feldwebel: balanced, defends and counterattacks ──
        private static readonly string FELDWEBEL = @"# ═══════════════════════════════════
# FELDWEBEL — Balanced Commander
# Reinforce frontlines, attack weak neighbors
# ═══════════════════════════════════
while True:
    total = get_territory_count()
    fuel = get_fuel()
    manp = get_manpower()

    best_target = -1
    best_from = -1
    best_score = 9999

    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            strength = get_territory_strength(i)

            if is_frontline(i):
                # Reinforce thin lines
                if strength < 4:
                    if manp > 30:
                        reinforce(i)
                        manp = get_manpower()

                # Check neighbors for weak points
                adj = get_adjacent(i)
                j = 0
                while j < adj:
                    nid = get_adjacent_id(i, j)
                    if get_territory_owner(nid) != 1:
                        e_str = get_enemy_strength(nid)
                        if e_str < best_score:
                            if strength > e_str + 2:
                                best_score = e_str
                                best_target = nid
                                best_from = i
                    j = j + 1
        i = i + 1

    # Attack weakest neighbor
    if best_target > -1:
        attack(best_from, best_target)

    # Fortify strong positions
    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i):
                if get_territory_strength(i) > 5:
                    if fuel > 20:
                        fortify(i)
                        fuel = get_fuel()
        i = i + 1

    wait
";

        // ── Oberst: aggressive flanker, recon-heavy ──
        private static readonly string OBERST = @"# ═══════════════════════════════════
# OBERST — Aggressive Flanker
# Recon heavily, exploit weak points, cut supply
# ═══════════════════════════════════
while True:
    total = get_territory_count()
    fuel = get_fuel()
    muni = get_munitions()
    manp = get_manpower()

    # Recon enemy territories
    i = 0
    while i < total:
        if get_territory_owner(i) == 2:
            age = get_recon(i)
            if age > 2:
                if fuel > 10:
                    order_recon(i)
                    fuel = get_fuel()
        i = i + 1

    # Find weakest enemy territory on our border
    best_target = -1
    best_from = -1
    best_score = 9999

    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i):
                str = get_territory_strength(i)
                adj = get_adjacent(i)
                j = 0
                while j < adj:
                    nid = get_adjacent_id(i, j)
                    owner = get_territory_owner(nid)
                    if owner != 1:
                        e_str = get_enemy_strength(nid)
                        # Prefer cutting supply routes
                        score = e_str
                        if get_supply_route_threatened(nid):
                            score = score - 3
                        if score < best_score:
                            if str > e_str:
                                best_score = score
                                best_target = nid
                                best_from = i
                    j = j + 1
        i = i + 1

    if best_target > -1:
        attack(best_from, best_target)

    # Mass recruit armor where safe
    if manp > 80:
        if fuel > 40:
            i = 0
            while i < total:
                if get_territory_owner(i) == 1:
                    if is_frontline(i) == 0:
                        recruit(5, i)
                        manp = get_manpower()
                i = i + 1

    # Reinforce hotspots
    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i):
                press = get_front_pressure(i)
                if press > 0.5:
                    if manp > 25:
                        reinforce(i)
                        manp = get_manpower()
        i = i + 1

    wait
";

        // ── General: optimal combined-arms strategy ──
        private static readonly string GENERAL = @"# ═══════════════════════════════════
# GENERAL — Supreme Commander
# Full spectrum: recon, supply management,
# combined arms, feints, exploitation
# ═══════════════════════════════════
while True:
    total = get_territory_count()
    turn = get_current_turn()
    fuel = get_fuel()
    muni = get_munitions()
    manp = get_manpower()
    my_t = get_my_territories()

    # Phase 1: Recon everything within budget
    i = 0
    while i < total:
        if get_territory_owner(i) != 1:
            if get_recon(i) > 1:
                if fuel > 8:
                    order_recon(i)
                    fuel = get_fuel()
        i = i + 1

    # Phase 2: Supply management
    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_supplied(i) == 0:
                set_supply_priority(i, 2)
            eff = get_supply_efficiency(i)
            if eff < 0.5:
                set_supply_priority(i, 2)
        i = i + 1

    # Phase 3: Build army composition
    if manp > 50:
        # Recruit varied units at safe territories
        i = 0
        while i < total:
            if get_territory_owner(i) == 1:
                if is_frontline(i) == 0:
                    str = get_territory_strength(i)
                    if str < 2:
                        # Cycle unit types based on turn
                        utype = turn % 6
                        recruit(utype, i)
                        manp = get_manpower()
            i = i + 1

    # Phase 4: Concentrate force — transfer to frontline
    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i) == 0:
                str = get_territory_strength(i)
                if str > 3:
                    adj = get_adjacent(i)
                    j = 0
                    while j < adj:
                        nid = get_adjacent_id(i, j)
                        if get_territory_owner(nid) == 1:
                            if is_frontline(nid):
                                transfer(i, nid, str - 1)
                                j = adj
                        j = j + 1
        i = i + 1

    # Phase 5: Attack — overwhelm weakest point
    best_ratio = 0
    best_from = -1
    best_to = -1

    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i):
                my_str = get_territory_strength(i)
                adj = get_adjacent(i)
                j = 0
                while j < adj:
                    nid = get_adjacent_id(i, j)
                    if get_territory_owner(nid) != 1:
                        e_str = get_enemy_strength(nid)
                        if e_str < 1:
                            e_str = 1
                        ratio = my_str / e_str
                        if ratio > best_ratio:
                            best_ratio = ratio
                            best_from = i
                            best_to = nid
                    j = j + 1
        i = i + 1

    if best_ratio > 1.5:
        attack(best_from, best_to)

    # Phase 6: Fortify deep positions
    i = 0
    while i < total:
        if get_territory_owner(i) == 1:
            if is_frontline(i):
                if get_territory_strength(i) > 6:
                    if fuel > 15:
                        fortify(i)
                        fuel = get_fuel()
        i = i + 1

    wait
";
    }
}
