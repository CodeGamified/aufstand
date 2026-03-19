// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using CodeGamified.Engine;
using CodeGamified.Time;
using Aufstand.Game;
using Aufstand.Game.Tactical;
using UnityEngine;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Tactical I/O handler — bridges CUSTOM opcodes 40-76 to squad combat,
    /// battlefield awareness, squad commands, artillery, and data bus.
    /// </summary>
    public class AufstandTacticalIOHandler : IGameIOHandler
    {
        private readonly Faction _faction;
        private readonly TacticalBattle _battle;

        public AufstandTacticalIOHandler(Faction faction, TacticalBattle battle)
        {
            _faction = faction;
            _battle = battle;
        }

        public bool PreExecute(Instruction inst, MachineState state) => true;

        public void ExecuteIO(Instruction inst, MachineState state)
        {
            int op = (int)inst.Op - (int)OpCode.CUSTOM_0;

            switch ((AufstandOpCode)op)
            {
                // ── Squad Sensors ──
                case AufstandOpCode.GET_SQUAD_COUNT:
                    state.SetRegister(0, _battle.GetSquadCount(_faction));
                    break;
                case AufstandOpCode.GET_SQUAD_X:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? sq.PosX : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_Y:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? sq.PosY : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_HP:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? sq.HP : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_TYPE:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? (int)sq.Type : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_AMMO:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? sq.Ammo : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_SUPPRESSED:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? sq.Suppression : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_IN_COVER:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? (int)sq.CurrentCover : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_MORALE:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? sq.Morale : 0f);
                    break;
                }
                case AufstandOpCode.GET_SQUAD_STATUS:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, sq != null ? (int)sq.Status : 0f);
                    break;
                }

                // ── Battlefield Awareness ──
                case AufstandOpCode.GET_ENEMY_COUNT:
                    state.SetRegister(0, _battle.GetEnemyVisibleCount(_faction));
                    break;
                case AufstandOpCode.GET_ENEMY_X:
                {
                    var enemy = _battle.GetVisibleEnemy(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, enemy != null ? enemy.PosX : 0f);
                    break;
                }
                case AufstandOpCode.GET_ENEMY_Y:
                {
                    var enemy = _battle.GetVisibleEnemy(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, enemy != null ? enemy.PosY : 0f);
                    break;
                }
                case AufstandOpCode.GET_ENEMY_TYPE:
                {
                    var enemy = _battle.GetVisibleEnemy(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, enemy != null ? (int)enemy.Type : 0f);
                    break;
                }
                case AufstandOpCode.GET_ENEMY_HEALTH:
                {
                    var enemy = _battle.GetVisibleEnemy(_faction, (int)state.Registers[0]);
                    state.SetRegister(0, enemy != null ? enemy.HP : 0f);
                    break;
                }
                case AufstandOpCode.GET_COVER_X:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    if (sq != null)
                    {
                        var cover = _battle.Cover.FindNearest(sq.PosX, sq.PosY);
                        state.SetRegister(0, cover.X);
                    }
                    break;
                }
                case AufstandOpCode.GET_COVER_Y:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    if (sq != null)
                    {
                        var cover = _battle.Cover.FindNearest(sq.PosX, sq.PosY);
                        state.SetRegister(0, cover.Y);
                    }
                    break;
                }
                case AufstandOpCode.GET_COVER_RATING:
                {
                    var sq = _battle.GetSquad(_faction, (int)state.Registers[0]);
                    if (sq != null)
                    {
                        var cover = _battle.Cover.FindNearest(sq.PosX, sq.PosY);
                        state.SetRegister(0, (int)cover.Rating);
                    }
                    break;
                }
                case AufstandOpCode.GET_CAPTURE_X:
                    state.SetRegister(0, _battle.CapturePointX);
                    break;
                case AufstandOpCode.GET_CAPTURE_Y:
                    state.SetRegister(0, _battle.CapturePointY);
                    break;
                case AufstandOpCode.GET_CAPTURE_PROGRESS:
                    state.SetRegister(0, _battle.CaptureProgress);
                    break;

                // ── Squad Commands ──
                case AufstandOpCode.MOVE_TO:
                {
                    int sqId = (int)state.Registers[0];
                    float x = state.Registers[1];
                    float y = state.Registers[2];
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        sq.TargetX = x;
                        sq.TargetY = y;
                        sq.MoveRequested = true;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.ATTACK_MOVE:
                {
                    int sqId = (int)state.Registers[0];
                    float x = state.Registers[1];
                    float y = state.Registers[2];
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        sq.TargetX = x;
                        sq.TargetY = y;
                        sq.MoveRequested = true;
                        sq.CurrentDoctrine = Doctrine.Aggressive;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.SET_DOCTRINE:
                {
                    int sqId = (int)state.Registers[0];
                    int doc = Mathf.Clamp((int)state.Registers[1], 0, 4);
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        sq.CurrentDoctrine = (Doctrine)doc;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.SET_TARGET:
                {
                    int sqId = (int)state.Registers[0];
                    int enemyIdx = (int)state.Registers[1];
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        sq.TargetEnemyIdx = enemyIdx;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.GARRISON:
                {
                    int sqId = (int)state.Registers[0];
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        // Move to nearest building
                        var cover = _battle.Cover.FindNearestMin(sq.PosX, sq.PosY, CoverRating.Building);
                        if (cover.Rating == CoverRating.Building)
                        {
                            sq.TargetX = cover.X;
                            sq.TargetY = cover.Y;
                            sq.MoveRequested = true;
                            sq.IsGarrisoned = true;
                            state.SetRegister(0, 1f);
                        }
                        else state.SetRegister(0, 0f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.DIG_IN:
                {
                    int sqId = (int)state.Registers[0];
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive && sq.Type == UnitType.Engineer)
                    {
                        sq.CurrentCover = CoverRating.Heavy;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.USE_ABILITY:
                {
                    int sqId = (int)state.Registers[0];
                    int ability = (int)state.Registers[1];
                    // Ability handling simplified — mainly smoke and grenade
                    state.SetRegister(0, 1f);
                    break;
                }
                case AufstandOpCode.RETREAT_SQUAD:
                {
                    int sqId = (int)state.Registers[0];
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        sq.CurrentDoctrine = Doctrine.Retreat;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.SET_SPACING:
                {
                    int sqId = (int)state.Registers[0];
                    float dist = Mathf.Clamp(state.Registers[1], 1f, 10f);
                    var sq = _battle.GetSquad(_faction, sqId);
                    if (sq != null && sq.IsAlive)
                    {
                        sq.Spacing = dist;
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }

                // ── Artillery & Support ──
                case AufstandOpCode.CALL_ARTILLERY:
                {
                    float x = state.Registers[0];
                    float y = state.Registers[1];
                    bool ok = _battle.CallArtillery(_faction, x, y);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.CALL_SMOKE:
                {
                    float x = state.Registers[0];
                    float y = state.Registers[1];
                    bool ok = _battle.CallSmoke(_faction, x, y);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.CALL_REINFORCE:
                    // Simplified: no-op for now
                    state.SetRegister(0, 0f);
                    break;
                case AufstandOpCode.GET_ARTILLERY_READY:
                    state.SetRegister(0, _battle.IsArtilleryReady(_faction) ? 1f : 0f);
                    break;
                case AufstandOpCode.GET_SUPPORT_POINTS:
                    state.SetRegister(0, _battle.GetSupportPoints(_faction));
                    break;

                // ── Data Bus ──
                case AufstandOpCode.SEND_BUS:
                {
                    int ch = (int)state.Registers[0];
                    float val = state.Registers[1];
                    _battle.SendBus(ch, val);
                    break;
                }
                case AufstandOpCode.RECV_BUS:
                {
                    int ch = (int)state.Registers[0];
                    state.SetRegister(0, _battle.RecvBus(ch));
                    break;
                }
            }
        }

        public float GetTimeScale() =>
            SimulationTime.Instance?.timeScale ?? 1f;

        public double GetSimulationTime() =>
            SimulationTime.Instance?.simulationTime ?? 0.0;
    }
}
