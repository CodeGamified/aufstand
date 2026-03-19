# Aufstand — Code Your War

**Grand strategy meets tactical combat — all through code.**

A WW2 programming game that plays on two layers simultaneously. At the **strategic layer**, you allocate armies, manage supply lines, and seize territories across a theatre map — Risk-style. At the **tactical layer**, your squads fight Company of Heroes–style skirmishes where units operate with semi-autonomy based on the doctrines you script. Your code handles everything: logistics, reinforcement priorities, flanking maneuvers, retreat conditions, artillery timing. No click-micro. No hotkeys. Just a terminal and a war.

Built with Unity 6 on the [CodeGamified](https://codegamified.github.io/) engine.

---

## Concept

You command a WW2 faction across a hex-based theatre map divided into territories. Each territory has resources (fuel, munitions, manpower) and strategic value. You write **strategic scripts** that decide where to attack, what to reinforce, and how to route supply. When two opposing forces meet, a **tactical battle** plays out — and you write **tactical scripts** that give your squads standing orders, engagement rules, and fallback behaviors.

Scripts execute at **10 ops/sec** (strategic) and **20 ops/sec** (tactical, sim-time). Crank to 100x and watch entire campaigns unfold in minutes.

Units don't wait for orders every frame. They're semi-autonomous: your tactical code sets **doctrines** (aggressive, defensive, flanking, hold-position) and units interpret them with built-in behavior trees. Better code means smarter interpretation. Dumb code means squads walking into crossfires.

---

## Two Layers

```
┌──────────────────────────────────────────────────────────────────┐
│  STRATEGIC LAYER  (Risk / Axis & Allies)                         │
│                                                                  │
│  Hex map · Territory control · Army allocation · Supply routes   │
│  Resource management · Reinforcement queues · Front-line logic   │
│                                                                  │
│  Your code decides: WHERE to fight, WHAT to send, HOW to supply │
├──────────────────────────────────────────────────────────────────┤
│  TACTICAL LAYER  (Company of Heroes)                             │
│                                                                  │
│  Squad-level combat · Cover system · Suppression · Flanking      │
│  Vehicle armor · Line of sight · Semi-autonomous unit behavior   │
│                                                                  │
│  Your code decides: HOW units fight, WHEN to retreat, WHERE to   │
│  set up MGs, which squads flank, when to call artillery          │
└──────────────────────────────────────────────────────────────────┘
```

---

## Strategic Script API

### Territory & Map

| Function | Returns |
|---|---|
| `get_territory_count()` | Total territories on map |
| `get_my_territories()` | Number you control |
| `get_enemy_territories()` | Number enemy controls |
| `get_territory_owner(id)` | Owner of territory (0=neutral, 1=you, 2=enemy) |
| `get_territory_fuel(id)` | Fuel output per turn |
| `get_territory_munitions(id)` | Munitions output per turn |
| `get_territory_manpower(id)` | Manpower output per turn |
| `get_adjacent(id)` | Number of adjacent territories |
| `get_adjacent_id(id, idx)` | ID of the idx-th adjacent territory |
| `is_frontline(id)` | 1 if territory borders enemy territory |
| `get_territory_strength(id)` | Garrison strength in territory |

### Resources

| Function | Returns |
|---|---|
| `get_fuel()` | Current fuel stockpile |
| `get_munitions()` | Current munitions stockpile |
| `get_manpower()` | Current manpower stockpile |
| `get_fuel_income()` | Fuel per turn |
| `get_munitions_income()` | Munitions per turn |
| `get_manpower_income()` | Manpower per turn |

### Supply Lines

| Function | Returns |
|---|---|
| `is_supplied(id)` | 1 if territory has unbroken supply line to HQ |
| `get_supply_dist(id)` | Supply chain length (hops to HQ) |
| `get_supply_efficiency(id)` | Supply efficiency 0.0–1.0 (degrades with distance/damage) |
| `set_supply_priority(id, p)` | Prioritize supply routing (0=low, 1=normal, 2=high) |
| `get_supply_route_threatened(id)` | 1 if supply route passes through contested territory |

### Army Management

| Function | Description |
|---|---|
| `recruit(unit_type, territory)` | Recruit unit at territory (costs manpower) |
| `transfer(territory_from, territory_to, count)` | Move troops between adjacent territories |
| `attack(territory_from, territory_to)` | Launch attack → triggers tactical battle |
| `reinforce(territory)` | Send reserves to territory (costs manpower) |
| `fortify(territory)` | Build defenses (costs fuel + munitions) |
| `set_rally_point(territory)` | New recruits auto-deploy here |
| `retreat(territory)` | Withdraw forces to nearest friendly territory |
| `get_army_size(territory)` | Number of units garrisoned |
| `get_army_composition(territory, type)` | Count of specific unit type in territory |

### Intelligence

| Function | Returns |
|---|---|
| `get_enemy_strength(id)` | Estimated enemy garrison (fog of war — accuracy varies) |
| `get_front_pressure(id)` | Enemy threat level on frontline territory (0.0–1.0) |
| `get_recon(id)` | Intel age in turns (0=fresh, higher=stale) |
| `order_recon(id)` | Send recon to territory (costs fuel, refreshes intel) |

---

## Tactical Script API

When an `attack()` triggers a battle, the tactical layer loads. Your tactical scripts control squad-level behavior.

### Squad Sensors

| Function | Returns |
|---|---|
| `get_squad_count()` | Number of squads you command |
| `get_squad_x(id)` / `get_squad_y(id)` | Squad position |
| `get_squad_hp(id)` | Squad health (aggregate) |
| `get_squad_type(id)` | Unit type (0=rifle, 1=mg, 2=mortar, 3=sniper, 4=engineer, 5=armor) |
| `get_squad_ammo(id)` | Ammo remaining |
| `get_squad_suppressed(id)` | Suppression level 0.0–1.0 (>0.7 = pinned) |
| `get_squad_in_cover(id)` | 1 if in cover |
| `get_squad_morale(id)` | Morale 0.0–1.0 (low morale = slower, less accurate) |
| `get_squad_status(id)` | Status (0=idle, 1=moving, 2=engaging, 3=retreating, 4=pinned) |

### Battlefield Awareness

| Function | Returns |
|---|---|
| `get_enemy_count()` | Number of visible enemy squads |
| `get_enemy_x(idx)` / `get_enemy_y(idx)` | Enemy squad position |
| `get_enemy_type(idx)` | Enemy unit type |
| `get_enemy_health(idx)` | Estimated enemy squad health |
| `get_cover_x(idx)` / `get_cover_y(idx)` | Nearest cover position |
| `get_cover_rating(idx)` | Cover quality (0=none, 1=light, 2=heavy, 3=building) |
| `get_capture_point_x()` / `get_capture_point_y()` | Territory control point location |
| `get_capture_progress()` | Capture progress −1.0 to 1.0 (negative=enemy, positive=you) |

### Squad Commands

| Function | Description |
|---|---|
| `move_to(squad, x, y)` | Move squad to position |
| `attack_move(squad, x, y)` | Advance and engage contacts en route |
| `set_doctrine(squad, doctrine)` | Set squad AI behavior (0=hold, 1=aggressive, 2=defensive, 3=flank, 4=retreat) |
| `set_target(squad, enemy_idx)` | Focus fire on specific enemy |
| `garrison(squad)` | Enter nearest building |
| `dig_in(squad)` | Create sandbag cover at position (engineers only, costs time) |
| `use_ability(squad, ability)` | Activate special (smoke=0, grenade=1, mine=2, repair=3) |
| `retreat_squad(squad)` | Force retreat to spawn |
| `set_spacing(squad, dist)` | Set formation spread (tight=1, normal=3, wide=6) |

### Artillery & Support

| Function | Description |
|---|---|
| `call_artillery(x, y)` | Call artillery barrage on position (costs munitions, 8s delay) |
| `call_smoke(x, y)` | Smoke barrage — blocks line of sight (costs munitions, 5s delay) |
| `call_reinforce()` | Request fresh squad from reserves (costs manpower, 15s delay) |
| `get_artillery_ready()` | 1 if artillery cooldown complete |
| `get_support_points()` | Available support call budget |

### Data Bus

Squads share a 16-channel float bus for inter-script coordination:

| Function | Description |
|---|---|
| `send(channel, value)` | Write to shared bus (channels 0–15) |
| `recv(channel)` | Read from shared bus → R0 |

---

## Unit Types

| Type | Role | Strengths | Weaknesses |
|---|---|---|---|
| **Rifle** | General infantry | Versatile, cheap, captures points | Low DPS |
| **MG** | Suppression | Pins squads, area denial | Slow setup, vulnerable while moving |
| **Mortar** | Indirect fire | Hits behind cover, smoke rounds | Minimum range, fragile |
| **Sniper** | Recon + pick-off | Long range, reveals fog | Low HP, useless up close |
| **Engineer** | Utility | Builds cover, lays mines, repairs | Weak in direct combat |
| **Armor** | Heavy assault | High HP, high damage, mobile | Expensive, vulnerable to flanking AT |

---

## Example: Strategic Script

```python
# Identify weakest frontline, reinforce it; attack weakest enemy border
best_target = -1
best_score = 9999

t = get_my_territories()
i = 0
while i < t:
    tid = get_adjacent_id(i, 0)
    if is_frontline(tid):
        strength = get_territory_strength(tid)
        enemy_str = get_enemy_strength(tid)
        if enemy_str < best_score:
            best_score = enemy_str
            best_target = tid
        if strength < 3:
            reinforce(tid)
    i = i + 1

if best_target > -1:
    if get_manpower() > 50:
        attack(best_target, best_target)
```

## Example: Tactical Script

```python
# MG suppresses, rifles flank, retreat if pinned
squads = get_squad_count()
i = 0
while i < squads:
    unit = get_squad_type(i)
    suppressed = get_squad_suppressed(i)
    hp = get_squad_hp(i)

    if hp < 20:
        retreat_squad(i)
    elif suppressed > 0.7:
        set_doctrine(i, 4)
    elif unit == 1:
        set_doctrine(i, 2)
        ex = get_enemy_x(0)
        ey = get_enemy_y(0)
        set_target(i, 0)
    elif unit == 0:
        set_doctrine(i, 3)
        attack_move(i, get_capture_point_x(), get_capture_point_y())
    i = i + 1

if get_artillery_ready():
    ex = get_enemy_x(0)
    ey = get_enemy_y(0)
    call_artillery(ex, ey)
```

---

## Game Mechanics

### Strategic Layer
- **Hex Map**: 30–60 territories grouped into regions with variable resource output
- **Supply Lines**: Territories must trace an unbroken path to your HQ to receive supply. Cut supply lines → garrisons starve (−morale, −reinforcement, −ammo regen)
- **Fog of War**: Enemy territory strength is estimated. Recon missions improve intel freshness. Stale intel can be wildly wrong
- **Resources**: Fuel (moves armor, runs supply trucks), Munitions (artillery, abilities, ammo), Manpower (recruitment, reinforcement)
- **Fortification**: Territories can be fortified with bunkers and minefields — defenders get cover bonuses and attacker penalties
- **Turn Resolution**: Strategic scripts run each turn (1 turn = 1 in-game day). Attacks trigger tactical battles resolved in real-time sim

### Tactical Layer
- **Cover System**: Light cover (fences, craters) reduces incoming damage 25%. Heavy cover (walls, sandbags) 50%. Buildings 75%
- **Suppression**: MGs and sustained fire build suppression. Suppressed squads move slower, shoot less accurately, eventually pin (can't act until suppression drops)
- **Flanking**: Attacking from the side or rear ignores cover bonuses and deals 1.5x damage
- **Morale**: Squads lose morale from casualties, suppression, being unsupplied. Low morale = auto-retreat. High morale = faster, more accurate
- **Line of Sight**: True LOS checks. Buildings block vision. Fog and smoke block vision. Elevated positions extend sight range
- **Semi-Autonomous AI**: Squads given `set_doctrine()` will make moment-to-moment micro-decisions (seek cover, return fire, reposition) within that doctrine. You set the strategy; they execute the tactics
- **Victory**: Capture the majority of control points or eliminate all enemy forces

---

## Architecture

```
Aufstand/Assets/
├── AI/                  AI commanders (strategic + tactical, same bytecode engine)
├── Core/                AufstandBootstrap — scene wiring & initialization
├── Game/
│   ├── Strategic/       HexMap, Territory, SupplyLine, FogOfWar, ResourceManager
│   ├── Tactical/        Squad, Cover, Suppression, Projectile, Artillery, CapturePoint
│   └── Shared/          UnitType, Faction, ArmyComposition, BattleResolver
├── Procedural/          Hex tiles, terrain, buildings, unit meshes, fortifications
├── Scenes/              Strategic map + tactical battle scenes
├── Scripting/
│   ├── AufstandStrategicProgram.cs
│   ├── AufstandTacticalProgram.cs
│   ├── AufstandCompilerExtension.cs
│   ├── AufstandStrategicIOHandler.cs
│   ├── AufstandTacticalIOHandler.cs
│   └── AufstandOpCode.cs
├── UI/                  Strategic TUI (map, resources, intel) + Tactical TUI (squad status, battle log)
└── Engine/              Shared CodeGamified engine submodule
    ├── CodeGamified.Audio/
    ├── CodeGamified.Bootstrap/
    ├── CodeGamified.Camera/
    ├── CodeGamified.Editor/
    ├── CodeGamified.Engine/
    ├── CodeGamified.Persistence/
    ├── CodeGamified.Procedural/
    ├── CodeGamified.Quality/
    ├── CodeGamified.Settings/
    ├── CodeGamified.Time/
    └── CodeGamified.TUI/
```

---

## Terminal Panels

### Strategic View

```
┌─ THEATRE MAP ──────────────────────┬─ INTEL ────────────────┐
│                                    │ SECTOR 7: ~40 inf      │
│   [■] [■] [□] [▨] [▨]            │ SECTOR 12: ??? (stale) │
│   [■] [■] [□] [▨] [▨]            │ FRONT PRESSURE: HIGH   │
│   [■] [★] [□] [□] [▩]            │                        │
│                                    │ RECON: 2 active        │
│   ■=yours □=neutral ▨=enemy ★=HQ  │                        │
├─ RESOURCES ────────────────────────┼─ SUPPLY ───────────────┤
│ FUEL: 120 (+15/turn)              │ ROUTE 1: HQ→S3→S7 OK  │
│ MUNI: 85  (+10/turn)              │ ROUTE 2: HQ→S4→S9 CUT │
│ MANP: 200 (+25/turn)              │ EFFICIENCY: 72%        │
└────────────────────────────────────┴────────────────────────┘
```

### Tactical View

```
┌─ BATTLEFIELD ──────────────────────┬─ SQUAD STATUS ─────────┐
│                                    │ SQ0 RIFLE  ██████░░ OK │
│   [R]···→  ╔══╗   ←···[E]        │ SQ1 MG     █████░░░ OK │
│            ║██║                    │ SQ2 RIFLE  ███░░░░░ LOW│
│   [R]→     ╚══╝     [E]          │ SQ3 MORTAR ████████ OK │
│      [M]·····→  💥                │ SQ4 ARMOR  ██████░░ OK │
│                                    │                        │
├─ BATTLE LOG ───────────────────────┤ ARTILLERY: READY       │
│ SQ1 suppressing ENEMY_0           │ SUPPORT: 3 pts         │
│ SQ0 flanking via north            │ CAPTURE: [====>   ] 62%│
│ ARTILLERY impact in 3s...         │                        │
└────────────────────────────────────┴────────────────────────┘
```

---

## What You Learn

| Concept | How Aufstand Teaches It |
|---|---|
| **Graph algorithms** | Supply routes are shortest-path problems through hex graphs |
| **Resource optimization** | Fuel/munitions/manpower allocation under constraints |
| **State machines** | Squad doctrines are finite state machines you configure via code |
| **Conditional logic** | Engagement rules: *if flanked, retreat; if suppressed, smoke; if outnumbered, dig in* |
| **Multi-agent coordination** | Squads share a data bus — your code orchestrates group tactics |
| **Heuristic search** | Finding the weakest point to attack, the safest route to supply |
| **Priority systems** | Triage: what to reinforce, what to abandon, what to sacrifice |

---

## Getting Started

1. Open the `Aufstand/` folder in Unity 6 (6000.0.36f1 or compatible)
2. Open `Assets/Scenes/Engine.unity`
3. Press Play
4. Write strategic code in the **Command Terminal** to manage your theatre
5. When battles trigger, write tactical code in the **Field Terminal** to direct your squads
6. Crank time warp and watch your campaign play out

---

## License

MIT — Copyright CodeGamified 2025-2026