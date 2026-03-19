// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using CodeGamified.Editor;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Editor extension — provides function metadata for tap-to-code editor.
    /// Covers both strategic and tactical builtins.
    /// </summary>
    public class AufstandEditorExtension : IEditorExtension
    {
        public List<EditorTypeInfo> GetAvailableTypes() => new List<EditorTypeInfo>();

        public List<EditorFuncInfo> GetAvailableFunctions()
        {
            return new List<EditorFuncInfo>
            {
                // ── Strategic: Territory ──
                new EditorFuncInfo { Name = "get_territory_count", Hint = "total territories on map", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_my_territories",  Hint = "territories you control", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_enemy_territories", Hint = "territories enemy controls", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_territory_owner",  Hint = "owner (0=neutral,1=you,2=enemy)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_territory_fuel",   Hint = "fuel output of territory", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_territory_munitions", Hint = "munitions output", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_territory_manpower", Hint = "manpower output", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_adjacent",         Hint = "adjacent territory count", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_adjacent_id",      Hint = "ID of idx-th adjacent", ArgCount = 2 },
                new EditorFuncInfo { Name = "is_frontline",         Hint = "borders enemy? (0/1)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_territory_strength", Hint = "garrison strength", ArgCount = 1 },

                // ── Strategic: Resources ──
                new EditorFuncInfo { Name = "get_fuel",            Hint = "current fuel stockpile", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_munitions",       Hint = "current munitions", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_manpower",        Hint = "current manpower", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_fuel_income",     Hint = "fuel per turn", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_munitions_income", Hint = "munitions per turn", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_manpower_income",  Hint = "manpower per turn", ArgCount = 0 },

                // ── Strategic: Supply ──
                new EditorFuncInfo { Name = "is_supplied",         Hint = "has supply line? (0/1)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_supply_dist",     Hint = "hops to HQ", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_supply_efficiency", Hint = "supply efficiency 0-1", ArgCount = 1 },
                new EditorFuncInfo { Name = "set_supply_priority", Hint = "set supply priority (0-2)", ArgCount = 2 },
                new EditorFuncInfo { Name = "get_supply_route_threatened", Hint = "route at risk? (0/1)", ArgCount = 1 },

                // ── Strategic: Army ──
                new EditorFuncInfo { Name = "recruit",             Hint = "recruit unit at territory", ArgCount = 2 },
                new EditorFuncInfo { Name = "transfer",            Hint = "move troops between territories", ArgCount = 3 },
                new EditorFuncInfo { Name = "attack",              Hint = "launch attack from→to", ArgCount = 2 },
                new EditorFuncInfo { Name = "reinforce",           Hint = "add rifle infantry", ArgCount = 1 },
                new EditorFuncInfo { Name = "fortify",             Hint = "build defenses", ArgCount = 1 },
                new EditorFuncInfo { Name = "set_rally_point",     Hint = "set recruit destination", ArgCount = 1 },
                new EditorFuncInfo { Name = "retreat",             Hint = "withdraw from territory", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_army_size",       Hint = "garrison count", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_army_composition", Hint = "count of unit type", ArgCount = 2 },

                // ── Strategic: Intel ──
                new EditorFuncInfo { Name = "get_enemy_strength",  Hint = "estimated enemy garrison", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_front_pressure",  Hint = "threat level 0-1", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_recon",           Hint = "intel age in turns", ArgCount = 1 },
                new EditorFuncInfo { Name = "order_recon",         Hint = "send recon (costs fuel)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_current_turn",    Hint = "current turn number", ArgCount = 0 },

                // ── Tactical: Squads ──
                new EditorFuncInfo { Name = "get_squad_count",     Hint = "your squad count", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_squad_x",         Hint = "squad X position", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_y",         Hint = "squad Y position", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_hp",        Hint = "squad health", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_type",      Hint = "unit type (0-5)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_ammo",      Hint = "ammo remaining", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_suppressed", Hint = "suppression 0-1", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_in_cover",  Hint = "cover rating (0-3)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_morale",    Hint = "morale 0-1", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_squad_status",    Hint = "status (0-4)", ArgCount = 1 },

                // ── Tactical: Awareness ──
                new EditorFuncInfo { Name = "get_enemy_count",     Hint = "visible enemy squads", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_enemy_x",         Hint = "enemy X position", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_enemy_y",         Hint = "enemy Y position", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_enemy_type",      Hint = "enemy unit type", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_enemy_health",    Hint = "enemy health estimate", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_cover_x",         Hint = "nearest cover X", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_cover_y",         Hint = "nearest cover Y", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_cover_rating",    Hint = "cover quality (0-3)", ArgCount = 1 },
                new EditorFuncInfo { Name = "get_capture_point_x", Hint = "capture point X", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_capture_point_y", Hint = "capture point Y", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_capture_progress", Hint = "capture -1 to 1", ArgCount = 0 },

                // ── Tactical: Commands ──
                new EditorFuncInfo { Name = "move_to",             Hint = "move squad to (x,y)", ArgCount = 3 },
                new EditorFuncInfo { Name = "attack_move",         Hint = "advance engaging", ArgCount = 3 },
                new EditorFuncInfo { Name = "set_doctrine",        Hint = "set squad AI (0-4)", ArgCount = 2 },
                new EditorFuncInfo { Name = "set_target",          Hint = "focus fire on enemy", ArgCount = 2 },
                new EditorFuncInfo { Name = "garrison",            Hint = "enter nearest building", ArgCount = 1 },
                new EditorFuncInfo { Name = "dig_in",              Hint = "build sandbags (eng only)", ArgCount = 1 },
                new EditorFuncInfo { Name = "use_ability",         Hint = "activate special", ArgCount = 2 },
                new EditorFuncInfo { Name = "retreat_squad",       Hint = "force retreat", ArgCount = 1 },
                new EditorFuncInfo { Name = "set_spacing",         Hint = "formation spread", ArgCount = 2 },

                // ── Tactical: Support ──
                new EditorFuncInfo { Name = "call_artillery",      Hint = "barrage at (x,y)", ArgCount = 2 },
                new EditorFuncInfo { Name = "call_smoke",          Hint = "smoke at (x,y)", ArgCount = 2 },
                new EditorFuncInfo { Name = "call_reinforce",      Hint = "request fresh squad", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_artillery_ready", Hint = "artillery available? (0/1)", ArgCount = 0 },
                new EditorFuncInfo { Name = "get_support_points",  Hint = "support budget", ArgCount = 0 },

                // ── Data Bus ──
                new EditorFuncInfo { Name = "send",                Hint = "write to bus channel (0-15)", ArgCount = 2 },
                new EditorFuncInfo { Name = "recv",                Hint = "read from bus channel", ArgCount = 1 },
            };
        }

        public List<EditorMethodInfo> GetMethodsForType(string typeName) => new List<EditorMethodInfo>();

        public List<string> GetVariableNameSuggestions()
        {
            return new List<string>
            {
                "i", "j", "tid", "nid", "total", "turn",
                "fuel", "muni", "manp", "strength", "pressure",
                "squads", "enemies", "hp", "suppressed", "morale",
                "unit", "cap", "ex", "ey", "cpx", "cpy"
            };
        }

        public List<string> GetStringLiteralSuggestions() => new List<string>();
    }
}
