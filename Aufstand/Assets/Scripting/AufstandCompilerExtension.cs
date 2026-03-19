// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Compiler extension for Aufstand — registers all strategic + tactical builtins.
    /// </summary>
    public class AufstandCompilerExtension : ICompilerExtension
    {
        public void RegisterBuiltins(CompilerContext ctx) { }

        public bool TryCompileCall(string functionName, List<AstNodes.ExprNode> args,
                                   CompilerContext ctx, int sourceLine)
        {
            switch (functionName)
            {
                // ══════════════════════════════════════════════════════════
                // STRATEGIC — Territory & Map
                // ══════════════════════════════════════════════════════════
                case "get_territory_count":
                    EmitCustom(ctx, AufstandOpCode.GET_TERRITORY_COUNT, sourceLine);
                    return true;
                case "get_my_territories":
                    EmitCustom(ctx, AufstandOpCode.GET_MY_TERRITORIES, sourceLine);
                    return true;
                case "get_enemy_territories":
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_TERRITORIES, sourceLine);
                    return true;
                case "get_territory_owner":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_TERRITORY_OWNER, sourceLine);
                    return true;
                case "get_territory_fuel":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_TERRITORY_FUEL, sourceLine);
                    return true;
                case "get_territory_munitions":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_TERRITORY_MUNITIONS, sourceLine);
                    return true;
                case "get_territory_manpower":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_TERRITORY_MANPOWER, sourceLine);
                    return true;
                case "get_adjacent":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ADJACENT, sourceLine);
                    return true;
                case "get_adjacent_id":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.GET_ADJACENT_ID, sourceLine);
                    return true;
                case "is_frontline":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.IS_FRONTLINE, sourceLine);
                    return true;
                case "get_territory_strength":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_TERRITORY_STRENGTH, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // STRATEGIC — Resources
                // ══════════════════════════════════════════════════════════
                case "get_fuel":
                    EmitCustom(ctx, AufstandOpCode.GET_FUEL, sourceLine);
                    return true;
                case "get_munitions":
                    EmitCustom(ctx, AufstandOpCode.GET_MUNITIONS, sourceLine);
                    return true;
                case "get_manpower":
                    EmitCustom(ctx, AufstandOpCode.GET_MANPOWER, sourceLine);
                    return true;
                case "get_fuel_income":
                    EmitCustom(ctx, AufstandOpCode.GET_FUEL_INCOME, sourceLine);
                    return true;
                case "get_munitions_income":
                    EmitCustom(ctx, AufstandOpCode.GET_MUNITIONS_INCOME, sourceLine);
                    return true;
                case "get_manpower_income":
                    EmitCustom(ctx, AufstandOpCode.GET_MANPOWER_INCOME, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // STRATEGIC — Supply
                // ══════════════════════════════════════════════════════════
                case "is_supplied":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.IS_SUPPLIED, sourceLine);
                    return true;
                case "get_supply_dist":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SUPPLY_DIST, sourceLine);
                    return true;
                case "get_supply_efficiency":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SUPPLY_EFFICIENCY, sourceLine);
                    return true;
                case "set_supply_priority":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.SET_SUPPLY_PRIORITY, sourceLine);
                    return true;
                case "get_supply_route_threatened":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ROUTE_THREATENED, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // STRATEGIC — Army Management
                // ══════════════════════════════════════════════════════════
                case "recruit":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.RECRUIT, sourceLine);
                    return true;
                case "transfer":
                    CompileThreeArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.TRANSFER, sourceLine);
                    return true;
                case "attack":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.ATTACK, sourceLine);
                    return true;
                case "reinforce":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.REINFORCE, sourceLine);
                    return true;
                case "fortify":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.FORTIFY, sourceLine);
                    return true;
                case "set_rally_point":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.SET_RALLY_POINT, sourceLine);
                    return true;
                case "retreat":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.RETREAT_TERRITORY, sourceLine);
                    return true;
                case "get_army_size":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ARMY_SIZE, sourceLine);
                    return true;
                case "get_army_composition":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.GET_ARMY_COMPOSITION, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // STRATEGIC — Intelligence
                // ══════════════════════════════════════════════════════════
                case "get_enemy_strength":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_STRENGTH, sourceLine);
                    return true;
                case "get_front_pressure":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_FRONT_PRESSURE, sourceLine);
                    return true;
                case "get_recon":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_RECON, sourceLine);
                    return true;
                case "order_recon":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.ORDER_RECON, sourceLine);
                    return true;

                // Turn info
                case "get_current_turn":
                    EmitCustom(ctx, AufstandOpCode.GET_CURRENT_TURN, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // TACTICAL — Squad Sensors
                // ══════════════════════════════════════════════════════════
                case "get_squad_count":
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_COUNT, sourceLine);
                    return true;
                case "get_squad_x":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_X, sourceLine);
                    return true;
                case "get_squad_y":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_Y, sourceLine);
                    return true;
                case "get_squad_hp":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_HP, sourceLine);
                    return true;
                case "get_squad_type":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_TYPE, sourceLine);
                    return true;
                case "get_squad_ammo":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_AMMO, sourceLine);
                    return true;
                case "get_squad_suppressed":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_SUPPRESSED, sourceLine);
                    return true;
                case "get_squad_in_cover":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_IN_COVER, sourceLine);
                    return true;
                case "get_squad_morale":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_MORALE, sourceLine);
                    return true;
                case "get_squad_status":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_SQUAD_STATUS, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // TACTICAL — Battlefield Awareness
                // ══════════════════════════════════════════════════════════
                case "get_enemy_count":
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_COUNT, sourceLine);
                    return true;
                case "get_enemy_x":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_X, sourceLine);
                    return true;
                case "get_enemy_y":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_Y, sourceLine);
                    return true;
                case "get_enemy_type":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_TYPE, sourceLine);
                    return true;
                case "get_enemy_health":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_ENEMY_HEALTH, sourceLine);
                    return true;
                case "get_cover_x":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_COVER_X, sourceLine);
                    return true;
                case "get_cover_y":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_COVER_Y, sourceLine);
                    return true;
                case "get_cover_rating":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GET_COVER_RATING, sourceLine);
                    return true;
                case "get_capture_point_x":
                    EmitCustom(ctx, AufstandOpCode.GET_CAPTURE_X, sourceLine);
                    return true;
                case "get_capture_point_y":
                    EmitCustom(ctx, AufstandOpCode.GET_CAPTURE_Y, sourceLine);
                    return true;
                case "get_capture_progress":
                    EmitCustom(ctx, AufstandOpCode.GET_CAPTURE_PROGRESS, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // TACTICAL — Squad Commands
                // ══════════════════════════════════════════════════════════
                case "move_to":
                    CompileThreeArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.MOVE_TO, sourceLine);
                    return true;
                case "attack_move":
                    CompileThreeArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.ATTACK_MOVE, sourceLine);
                    return true;
                case "set_doctrine":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.SET_DOCTRINE, sourceLine);
                    return true;
                case "set_target":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.SET_TARGET, sourceLine);
                    return true;
                case "garrison":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.GARRISON, sourceLine);
                    return true;
                case "dig_in":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.DIG_IN, sourceLine);
                    return true;
                case "use_ability":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.USE_ABILITY, sourceLine);
                    return true;
                case "retreat_squad":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.RETREAT_SQUAD, sourceLine);
                    return true;
                case "set_spacing":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.SET_SPACING, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // TACTICAL — Artillery & Support
                // ══════════════════════════════════════════════════════════
                case "call_artillery":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.CALL_ARTILLERY, sourceLine);
                    return true;
                case "call_smoke":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.CALL_SMOKE, sourceLine);
                    return true;
                case "call_reinforce":
                    EmitCustom(ctx, AufstandOpCode.CALL_REINFORCE, sourceLine);
                    return true;
                case "get_artillery_ready":
                    EmitCustom(ctx, AufstandOpCode.GET_ARTILLERY_READY, sourceLine);
                    return true;
                case "get_support_points":
                    EmitCustom(ctx, AufstandOpCode.GET_SUPPORT_POINTS, sourceLine);
                    return true;

                // ══════════════════════════════════════════════════════════
                // Data Bus
                // ══════════════════════════════════════════════════════════
                case "send":
                    CompileTwoArgs(args, ctx, sourceLine);
                    EmitCustom(ctx, AufstandOpCode.SEND_BUS, sourceLine);
                    return true;
                case "recv":
                    CompileArg(args, 0, ctx);
                    EmitCustom(ctx, AufstandOpCode.RECV_BUS, sourceLine);
                    return true;

                default:
                    return false;
            }
        }

        public bool TryCompileMethodCall(string objectName, string methodName,
                                         List<AstNodes.ExprNode> args,
                                         CompilerContext ctx, int sourceLine)
        {
            return false;
        }

        public bool TryCompileObjectDecl(string typeName, string varName,
                                         List<AstNodes.ExprNode> constructorArgs,
                                         CompilerContext ctx, int sourceLine)
        {
            return false;
        }

        // ── Helpers ──

        private static void EmitCustom(CompilerContext ctx, AufstandOpCode op, int sourceLine)
        {
            ctx.Emit(OpCode.CUSTOM_0 + (int)op, 0, 0, 0, sourceLine,
                     op.ToString().ToLower());
        }

        private static void CompileArg(List<AstNodes.ExprNode> args, int index, CompilerContext ctx)
        {
            if (args != null && args.Count > index)
                args[index].Compile(ctx);
        }

        private static void CompileTwoArgs(List<AstNodes.ExprNode> args,
                                            CompilerContext ctx, int sourceLine)
        {
            if (args != null && args.Count >= 2)
            {
                args[0].Compile(ctx);
                ctx.Emit(OpCode.PUSH, 0, 0, 0, sourceLine, "push R0 (arg0)");
                args[1].Compile(ctx);
                ctx.Emit(OpCode.MOV, 1, 0, 0, sourceLine, "R1 ← R0 (arg1)");
                ctx.Emit(OpCode.POP, 0, 0, 0, sourceLine, "pop R0 (arg0)");
            }
        }

        private static void CompileThreeArgs(List<AstNodes.ExprNode> args,
                                              CompilerContext ctx, int sourceLine)
        {
            if (args != null && args.Count >= 3)
            {
                args[0].Compile(ctx);
                ctx.Emit(OpCode.PUSH, 0, 0, 0, sourceLine, "push R0 (arg0)");
                args[1].Compile(ctx);
                ctx.Emit(OpCode.PUSH, 0, 0, 0, sourceLine, "push R0 (arg1)");
                args[2].Compile(ctx);
                ctx.Emit(OpCode.MOV, 2, 0, 0, sourceLine, "R2 ← R0 (arg2)");
                ctx.Emit(OpCode.POP, 1, 0, 0, sourceLine, "pop R1 (arg1)");
                ctx.Emit(OpCode.POP, 0, 0, 0, sourceLine, "pop R0 (arg0)");
            }
        }
    }
}
