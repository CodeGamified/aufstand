// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using CodeGamified.Engine;
using CodeGamified.Time;
using Aufstand.Game;
using Aufstand.Game.Strategic;
using UnityEngine;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Strategic I/O handler — bridges CUSTOM opcodes 0-39 to hex map,
    /// resources, supply lines, army management, and intelligence.
    /// </summary>
    public class AufstandStrategicIOHandler : IGameIOHandler
    {
        private readonly Faction _faction;
        private readonly HexMap _map;
        private readonly ResourceManager _resources;
        private readonly StrategicMatchManager _match;
        private readonly StrategicFogOfWar _fog;

        public AufstandStrategicIOHandler(Faction faction, HexMap map,
            ResourceManager resources, StrategicMatchManager match, StrategicFogOfWar fog)
        {
            _faction = faction;
            _map = map;
            _resources = resources;
            _match = match;
            _fog = fog;
        }

        public bool PreExecute(Instruction inst, MachineState state) => true;

        public void ExecuteIO(Instruction inst, MachineState state)
        {
            int op = (int)inst.Op - (int)OpCode.CUSTOM_0;

            switch ((AufstandOpCode)op)
            {
                // ── Territory & Map ──
                case AufstandOpCode.GET_TERRITORY_COUNT:
                    state.SetRegister(0, _map.TerritoryCount);
                    break;
                case AufstandOpCode.GET_MY_TERRITORIES:
                    state.SetRegister(0, _map.CountOwned(_faction));
                    break;
                case AufstandOpCode.GET_ENEMY_TERRITORIES:
                {
                    var enemy = _faction == Faction.Player ? Faction.Enemy : Faction.Player;
                    state.SetRegister(0, _map.CountOwned(enemy));
                    break;
                }
                case AufstandOpCode.GET_TERRITORY_OWNER:
                {
                    int id = (int)state.Registers[0];
                    var t = _map.GetTerritory(id);
                    // Remap: 0=neutral, 1=mine, 2=enemy
                    int owner = 0;
                    if (t.Owner == _faction) owner = 1;
                    else if (t.Owner != Faction.Neutral) owner = 2;
                    state.SetRegister(0, owner);
                    break;
                }
                case AufstandOpCode.GET_TERRITORY_FUEL:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).FuelOutput);
                    break;
                }
                case AufstandOpCode.GET_TERRITORY_MUNITIONS:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).MunitionsOutput);
                    break;
                }
                case AufstandOpCode.GET_TERRITORY_MANPOWER:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).ManpowerOutput);
                    break;
                }
                case AufstandOpCode.GET_ADJACENT:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetAdjacentCount(id));
                    break;
                }
                case AufstandOpCode.GET_ADJACENT_ID:
                {
                    int id = (int)state.Registers[0];
                    int idx = (int)state.Registers[1];
                    state.SetRegister(0, _map.GetAdjacentId(id, idx));
                    break;
                }
                case AufstandOpCode.IS_FRONTLINE:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.IsFrontline(id) ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.GET_TERRITORY_STRENGTH:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).GarrisonStrength);
                    break;
                }

                // ── Resources ──
                case AufstandOpCode.GET_FUEL:
                    state.SetRegister(0, _resources.GetFuel(_faction));
                    break;
                case AufstandOpCode.GET_MUNITIONS:
                    state.SetRegister(0, _resources.GetMunitions(_faction));
                    break;
                case AufstandOpCode.GET_MANPOWER:
                    state.SetRegister(0, _resources.GetManpower(_faction));
                    break;
                case AufstandOpCode.GET_FUEL_INCOME:
                    state.SetRegister(0, _resources.GetFuelIncome(_faction));
                    break;
                case AufstandOpCode.GET_MUNITIONS_INCOME:
                    state.SetRegister(0, _resources.GetMunitionsIncome(_faction));
                    break;
                case AufstandOpCode.GET_MANPOWER_INCOME:
                    state.SetRegister(0, _resources.GetManpowerIncome(_faction));
                    break;

                // ── Supply ──
                case AufstandOpCode.IS_SUPPLIED:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).IsSupplied ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.GET_SUPPLY_DIST:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).SupplyDistance);
                    break;
                }
                case AufstandOpCode.GET_SUPPLY_EFFICIENCY:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).SupplyEfficiency);
                    break;
                }
                case AufstandOpCode.SET_SUPPLY_PRIORITY:
                {
                    int id = (int)state.Registers[0];
                    int p = Mathf.Clamp((int)state.Registers[1], 0, 2);
                    var t = _map.GetTerritory(id);
                    if (t.Owner == _faction)
                    {
                        t.SupplyPriority = (SupplyPriority)p;
                        _map.SetTerritory(id, t);
                        state.SetRegister(0, 1f);
                    }
                    else state.SetRegister(0, 0f);
                    break;
                }
                case AufstandOpCode.GET_ROUTE_THREATENED:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, SupplyLine.IsRouteThreatened(_map, id) ? 1f : 0f);
                    break;
                }

                // ── Army Management ──
                case AufstandOpCode.RECRUIT:
                {
                    int unitType = Mathf.Clamp((int)state.Registers[0], 0, 5);
                    int territory = (int)state.Registers[1];
                    bool ok = _match.TryRecruit(_faction, (UnitType)unitType, territory);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.TRANSFER:
                {
                    int from = (int)state.Registers[0];
                    int to = (int)state.Registers[1];
                    int count = (int)state.Registers[2];
                    bool ok = _match.QueueTransfer(_faction, from, to, count);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.ATTACK:
                {
                    int from = (int)state.Registers[0];
                    int to = (int)state.Registers[1];
                    bool ok = _match.QueueAttack(_faction, from, to);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.REINFORCE:
                {
                    int territory = (int)state.Registers[0];
                    bool ok = _match.TryReinforce(_faction, territory);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.FORTIFY:
                {
                    int territory = (int)state.Registers[0];
                    bool ok = _match.TryFortify(_faction, territory);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.SET_RALLY_POINT:
                {
                    int territory = (int)state.Registers[0];
                    _match.SetRallyPoint(_faction, territory);
                    state.SetRegister(0, 1f);
                    break;
                }
                case AufstandOpCode.RETREAT_TERRITORY:
                {
                    int territory = (int)state.Registers[0];
                    bool ok = _match.TryRetreat(_faction, territory);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }
                case AufstandOpCode.GET_ARMY_SIZE:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _map.GetTerritory(id).GarrisonStrength);
                    break;
                }
                case AufstandOpCode.GET_ARMY_COMPOSITION:
                {
                    int id = (int)state.Registers[0];
                    int unitType = Mathf.Clamp((int)state.Registers[1], 0, 5);
                    state.SetRegister(0, _map.GetTerritory(id).GetUnitCount((UnitType)unitType));
                    break;
                }

                // ── Intelligence ──
                case AufstandOpCode.GET_ENEMY_STRENGTH:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _fog.GetEstimatedStrength(_faction, id));
                    break;
                }
                case AufstandOpCode.GET_FRONT_PRESSURE:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _fog.GetFrontPressure(id));
                    break;
                }
                case AufstandOpCode.GET_RECON:
                {
                    int id = (int)state.Registers[0];
                    state.SetRegister(0, _fog.GetIntelAge(_faction, id));
                    break;
                }
                case AufstandOpCode.ORDER_RECON:
                {
                    int id = (int)state.Registers[0];
                    bool ok = _match.TryRecon(_faction, id);
                    state.SetRegister(0, ok ? 1f : 0f);
                    break;
                }

                // ── Turn Info ──
                case AufstandOpCode.GET_CURRENT_TURN:
                    state.SetRegister(0, _match.CurrentTurn);
                    break;
            }
        }

        public float GetTimeScale() =>
            SimulationTime.Instance?.timeScale ?? 1f;

        public double GetSimulationTime() =>
            SimulationTime.Instance?.simulationTime ?? 0.0;
    }
}
