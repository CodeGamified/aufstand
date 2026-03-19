// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using Aufstand.Game;
using Aufstand.Game.Tactical;
using Aufstand.Scripting;

namespace Aufstand.AI
{
    /// <summary>
    /// AI tactician — runs bytecode for the enemy faction during battles.
    /// Each difficulty tier uses a different Python script.
    /// </summary>
    public class AufstandAITactician : MonoBehaviour
    {
        private AufstandTacticalProgram _program;

        public AufstandTacticalProgram Program => _program;

        public void Initialize(TacticalBattle battle, CommanderDifficulty difficulty)
        {
            _program = gameObject.AddComponent<AufstandTacticalProgram>();
            _program.Initialize(GetAICode(difficulty));
            _program.BindBattle(battle);
        }

        public static string GetAICode(CommanderDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CommanderDifficulty.Rekrut:    return REKRUT_TACTICAL;
                case CommanderDifficulty.Feldwebel: return FELDWEBEL_TACTICAL;
                case CommanderDifficulty.Oberst:    return OBERST_TACTICAL;
                case CommanderDifficulty.General:   return GENERAL_TACTICAL;
                default: return REKRUT_TACTICAL;
            }
        }

        private static readonly string REKRUT_TACTICAL = @"# Rekrut — hold position, shoot nearby
while True:
    squads = get_squad_count()
    i = 0
    while i < squads:
        set_doctrine(i, 0)
        i = i + 1
    wait
";

        private static readonly string FELDWEBEL_TACTICAL = @"# Feldwebel — defend and counter
while True:
    squads = get_squad_count()
    enemies = get_enemy_count()
    i = 0
    while i < squads:
        unit = get_squad_type(i)
        hp = get_squad_hp(i)
        if hp < 25:
            retreat_squad(i)
        elif unit == 1:
            set_doctrine(i, 2)
            if enemies > 0:
                set_target(i, 0)
        elif unit == 5:
            set_doctrine(i, 1)
            cpx = get_capture_point_x()
            cpy = get_capture_point_y()
            attack_move(i, cpx, cpy)
        else:
            set_doctrine(i, 2)
        i = i + 1
    wait
";

        private static readonly string OBERST_TACTICAL = @"# Oberst — MG suppress, rifles flank, armor push
while True:
    squads = get_squad_count()
    enemies = get_enemy_count()
    i = 0
    while i < squads:
        unit = get_squad_type(i)
        hp = get_squad_hp(i)
        sup = get_squad_suppressed(i)

        if hp < 20:
            retreat_squad(i)
        elif sup > 0.7:
            set_doctrine(i, 2)
        elif unit == 1:
            set_doctrine(i, 2)
            if enemies > 0:
                set_target(i, 0)
        elif unit == 0:
            set_doctrine(i, 3)
            cpx = get_capture_point_x()
            cpy = get_capture_point_y()
            attack_move(i, cpx, cpy)
        elif unit == 5:
            set_doctrine(i, 1)
            if enemies > 0:
                ex = get_enemy_x(0)
                ey = get_enemy_y(0)
                attack_move(i, ex, ey)
        elif unit == 4:
            dig_in(i)
        else:
            set_doctrine(i, 2)
        i = i + 1

    if get_artillery_ready():
        if enemies > 0:
            ex = get_enemy_x(0)
            ey = get_enemy_y(0)
            call_artillery(ex, ey)
    wait
";

        private static readonly string GENERAL_TACTICAL = @"# General — coordinated combined arms
while True:
    squads = get_squad_count()
    enemies = get_enemy_count()
    cap = get_capture_progress()

    # Phase: assign roles
    i = 0
    while i < squads:
        unit = get_squad_type(i)
        hp = get_squad_hp(i)
        sup = get_squad_suppressed(i)
        morale = get_squad_morale(i)

        if hp < 15:
            retreat_squad(i)
            i = i + 1
            wait

        if morale < 0.3:
            retreat_squad(i)
            i = i + 1
            wait

        # MG: suppress enemy from cover
        if unit == 1:
            set_doctrine(i, 2)
            cvx = get_cover_x(i)
            cvy = get_cover_y(i)
            move_to(i, cvx, cvy)
            if enemies > 0:
                set_target(i, 0)

        # Mortar: indirect fire on enemy cluster
        elif unit == 2:
            set_doctrine(i, 2)
            if enemies > 0:
                set_target(i, 0)

        # Sniper: pick off at range
        elif unit == 3:
            set_doctrine(i, 2)
            if enemies > 0:
                set_target(i, 0)

        # Engineer: dig in, repair armor
        elif unit == 4:
            dig_in(i)
            set_doctrine(i, 2)

        # Armor: spearhead push
        elif unit == 5:
            set_doctrine(i, 1)
            cpx = get_capture_point_x()
            cpy = get_capture_point_y()
            attack_move(i, cpx, cpy)

        # Rifle: flank or capture
        elif unit == 0:
            if cap < 0:
                set_doctrine(i, 3)
                cpx = get_capture_point_x()
                cpy = get_capture_point_y()
                attack_move(i, cpx, cpy)
            else:
                set_doctrine(i, 1)
                if enemies > 0:
                    ex = get_enemy_x(0)
                    ey = get_enemy_y(0)
                    attack_move(i, ex, ey)

        i = i + 1

    # Smoke first, then shell
    if get_artillery_ready():
        if enemies > 1:
            ex = get_enemy_x(0)
            ey = get_enemy_y(0)
            call_smoke(ex, ey)
        elif enemies > 0:
            ex = get_enemy_x(0)
            ey = get_enemy_y(0)
            call_artillery(ex, ey)

    wait
";
    }
}
