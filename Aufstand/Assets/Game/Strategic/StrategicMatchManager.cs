// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;
using CodeGamified.Time;

namespace Aufstand.Game.Strategic
{
    /// <summary>
    /// Strategic match manager — turn resolution, army movement, recruitment,
    /// fortification, attack triggering, and victory detection.
    /// 1 turn = 1 in-game day. Strategic scripts run each turn.
    /// </summary>
    public class StrategicMatchManager : MonoBehaviour
    {
        private HexMap _map;
        private ResourceManager _resources;
        private StrategicFogOfWar _fog;
        private bool _autoRestart;
        private float _restartDelay;

        public int CurrentTurn { get; private set; }
        public bool MatchInProgress { get; private set; }
        public bool BattleInProgress { get; private set; }

        // Turn timer — 1 turn = 1 sim-time day (86400s at 1x speed, compressed)
        private float _turnAccumulator;
        public const float SECONDS_PER_TURN = 10f; // 10 sim-seconds per strategic turn

        // Events
        public System.Action<int> OnTurnAdvanced;
        public System.Action<int, Faction> OnTerritoryConquered;
        public System.Action<Faction> OnVictory;
        public System.Action<int, int> OnBattleTriggered;  // attacker territory, defender territory

        // Pending orders (accumulated during script execution, resolved at turn end)
        private readonly List<TransferOrder> _pendingTransfers = new List<TransferOrder>();
        private readonly List<AttackOrder> _pendingAttacks = new List<AttackOrder>();

        private struct TransferOrder
        {
            public Faction Faction;
            public int From, To, Count;
        }

        private struct AttackOrder
        {
            public Faction Faction;
            public int From, To;
        }

        public void Initialize(HexMap map, ResourceManager resources, StrategicFogOfWar fog,
                               bool autoRestart, float restartDelay)
        {
            _map = map;
            _resources = resources;
            _fog = fog;
            _autoRestart = autoRestart;
            _restartDelay = restartDelay;
            CurrentTurn = 1;
            MatchInProgress = true;
        }

        private void Update()
        {
            if (!MatchInProgress || BattleInProgress) return;
            if (SimulationTime.Instance == null || SimulationTime.Instance.isPaused) return;

            float dt = Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);
            _turnAccumulator += dt;

            if (_turnAccumulator >= SECONDS_PER_TURN)
            {
                _turnAccumulator -= SECONDS_PER_TURN;
                AdvanceTurn();
            }
        }

        private void AdvanceTurn()
        {
            CurrentTurn++;

            // 1. Apply resource income
            _resources.ApplyIncome();

            // 2. Recalculate supply lines
            SupplyLine.Recalculate(_map, Faction.Player);
            SupplyLine.Recalculate(_map, Faction.Enemy);

            // 3. Age fog of war intel
            _fog.AgeTurn();

            // 4. Resolve pending orders
            ResolveTransfers();
            ResolveAttacks();

            // 5. Unsupplied garrisons suffer attrition
            ApplySupplyAttrition();

            // 6. Check victory
            CheckVictory();

            OnTurnAdvanced?.Invoke(CurrentTurn);
        }

        // =================================================================
        // ORDERS (called by scripts via IOHandler)
        // =================================================================

        /// <summary>Queue a troop transfer between adjacent owned territories.</summary>
        public bool QueueTransfer(Faction faction, int from, int to, int count)
        {
            if (!_map.AreAdjacent(from, to)) return false;
            var fromT = _map.GetTerritory(from);
            var toT = _map.GetTerritory(to);
            if (fromT.Owner != faction) return false;
            if (toT.Owner != faction) return false;
            if (fromT.GarrisonStrength < count) return false;

            _pendingTransfers.Add(new TransferOrder
            {
                Faction = faction, From = from, To = to, Count = count
            });
            return true;
        }

        /// <summary>Queue an attack from owned territory to adjacent enemy/neutral territory.</summary>
        public bool QueueAttack(Faction faction, int from, int to)
        {
            if (!_map.AreAdjacent(from, to)) return false;
            var fromT = _map.GetTerritory(from);
            var toT = _map.GetTerritory(to);
            if (fromT.Owner != faction) return false;
            if (toT.Owner == faction) return false;
            if (fromT.GarrisonStrength <= 1) return false; // must leave at least 1

            _pendingAttacks.Add(new AttackOrder { Faction = faction, From = from, To = to });
            return true;
        }

        /// <summary>Recruit a unit at a territory. Costs manpower (+ fuel for vehicles).</summary>
        public bool TryRecruit(Faction faction, UnitType type, int territory)
        {
            var t = _map.GetTerritory(territory);
            if (t.Owner != faction) return false;

            var stats = UnitStats.Get(type);
            if (!_resources.TrySpend(faction, stats.FuelCost, 0, stats.ManpowerCost))
                return false;

            t.AddUnits(type, 1);
            _map.SetTerritory(territory, t);
            return true;
        }

        /// <summary>Reinforce a territory with generic infantry (costs manpower).</summary>
        public bool TryReinforce(Faction faction, int territory)
        {
            return TryRecruit(faction, UnitType.Rifle, territory);
        }

        /// <summary>Fortify a territory (costs fuel + munitions).</summary>
        public bool TryFortify(Faction faction, int territory)
        {
            var t = _map.GetTerritory(territory);
            if (t.Owner != faction) return false;
            if (t.FortificationLevel >= 3) return false;

            int fuelCost = 10 + t.FortificationLevel * 5;
            int muniCost = 10 + t.FortificationLevel * 5;
            if (!_resources.TrySpend(faction, fuelCost, muniCost, 0))
                return false;

            t.FortificationLevel++;
            _map.SetTerritory(territory, t);
            return true;
        }

        /// <summary>Set rally point for a faction.</summary>
        public void SetRallyPoint(Faction faction, int territory)
        {
            for (int i = 0; i < _map.TerritoryCount; i++)
            {
                var t = _map.GetTerritory(i);
                if (t.Owner == faction)
                {
                    t.IsRallyPoint = (i == territory);
                    _map.SetTerritory(i, t);
                }
            }
        }

        /// <summary>Retreat forces from a territory to nearest safe territory.</summary>
        public bool TryRetreat(Faction faction, int territory)
        {
            var t = _map.GetTerritory(territory);
            if (t.Owner != faction || t.GarrisonStrength == 0) return false;

            // Find nearest non-frontline owned territory
            int bestDest = -1;
            float bestDist = float.MaxValue;
            int adjCount = _map.GetAdjacentCount(territory);
            for (int i = 0; i < adjCount; i++)
            {
                int adj = _map.GetAdjacentId(territory, i);
                var adjT = _map.GetTerritory(adj);
                if (adjT.Owner == faction && !_map.IsFrontline(adj))
                {
                    float dx = adjT.CenterX - t.CenterX;
                    float dz = adjT.CenterZ - t.CenterZ;
                    float dist = dx * dx + dz * dz;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestDest = adj;
                    }
                }
            }

            // Fallback: any adjacent owned territory
            if (bestDest < 0)
            {
                for (int i = 0; i < adjCount; i++)
                {
                    int adj = _map.GetAdjacentId(territory, i);
                    if (_map.GetTerritory(adj).Owner == faction)
                    {
                        bestDest = adj;
                        break;
                    }
                }
            }

            if (bestDest < 0) return false;

            // Move all units
            var dest = _map.GetTerritory(bestDest);
            dest.RifleCount += t.RifleCount;
            dest.MGCount += t.MGCount;
            dest.MortarCount += t.MortarCount;
            dest.SniperCount += t.SniperCount;
            dest.EngineerCount += t.EngineerCount;
            dest.ArmorCount += t.ArmorCount;
            dest.GarrisonStrength = dest.TotalUnits;

            t.RifleCount = t.MGCount = t.MortarCount = t.SniperCount = t.EngineerCount = t.ArmorCount = 0;
            t.GarrisonStrength = 0;

            _map.SetTerritory(territory, t);
            _map.SetTerritory(bestDest, dest);
            return true;
        }

        /// <summary>Order recon on a territory (costs fuel, refreshes intel).</summary>
        public bool TryRecon(Faction faction, int territory)
        {
            if (!_resources.TrySpend(faction, 5, 0, 0)) return false;
            _fog.RefreshIntel(faction, territory);
            return true;
        }

        /// <summary>Get army composition for a territory (used when entering tactical layer).</summary>
        public int[] GetArmyComposition(int territory)
        {
            var t = _map.GetTerritory(territory);
            return new int[]
            {
                t.RifleCount, t.MGCount, t.MortarCount,
                t.SniperCount, t.EngineerCount, t.ArmorCount
            };
        }

        // =================================================================
        // RESOLVE
        // =================================================================

        private void ResolveTransfers()
        {
            foreach (var order in _pendingTransfers)
            {
                var from = _map.GetTerritory(order.From);
                var to = _map.GetTerritory(order.To);

                int actual = Mathf.Min(order.Count, from.GarrisonStrength - 1);
                if (actual <= 0) continue;

                // Transfer proportionally from each unit type
                float ratio = (float)actual / from.GarrisonStrength;
                int rif = Mathf.RoundToInt(from.RifleCount * ratio);
                int mg = Mathf.RoundToInt(from.MGCount * ratio);
                int mor = Mathf.RoundToInt(from.MortarCount * ratio);
                int snp = Mathf.RoundToInt(from.SniperCount * ratio);
                int eng = Mathf.RoundToInt(from.EngineerCount * ratio);
                int arm = Mathf.RoundToInt(from.ArmorCount * ratio);

                from.RemoveUnits(UnitType.Rifle, rif);
                from.RemoveUnits(UnitType.MG, mg);
                from.RemoveUnits(UnitType.Mortar, mor);
                from.RemoveUnits(UnitType.Sniper, snp);
                from.RemoveUnits(UnitType.Engineer, eng);
                from.RemoveUnits(UnitType.Armor, arm);

                to.AddUnits(UnitType.Rifle, rif);
                to.AddUnits(UnitType.MG, mg);
                to.AddUnits(UnitType.Mortar, mor);
                to.AddUnits(UnitType.Sniper, snp);
                to.AddUnits(UnitType.Engineer, eng);
                to.AddUnits(UnitType.Armor, arm);

                _map.SetTerritory(order.From, from);
                _map.SetTerritory(order.To, to);
            }
            _pendingTransfers.Clear();
        }

        private void ResolveAttacks()
        {
            foreach (var order in _pendingAttacks)
            {
                var from = _map.GetTerritory(order.From);
                if (from.GarrisonStrength <= 1) continue;

                // Trigger tactical battle
                BattleInProgress = true;
                OnBattleTriggered?.Invoke(order.From, order.To);
                return; // One battle at a time
            }
            _pendingAttacks.Clear();
        }

        /// <summary>Called by bootstrap when tactical battle ends.</summary>
        public void ResolveBattle(Faction winner, int attackerTerritory, int defenderTerritory)
        {
            BattleInProgress = false;
            _pendingAttacks.Clear();

            if (winner == _map.GetTerritory(attackerTerritory).Owner)
            {
                // Attacker won — take territory
                var def = _map.GetTerritory(defenderTerritory);
                var atk = _map.GetTerritory(attackerTerritory);

                def.Owner = atk.Owner;
                def.FortificationLevel = 0;

                // Transfer half the attacker's forces
                int halfRif = atk.RifleCount / 2;
                int halfMG = atk.MGCount / 2;
                int halfMor = atk.MortarCount / 2;
                int halfSnp = atk.SniperCount / 2;
                int halfEng = atk.EngineerCount / 2;
                int halfArm = atk.ArmorCount / 2;

                atk.RemoveUnits(UnitType.Rifle, halfRif);
                atk.RemoveUnits(UnitType.MG, halfMG);
                atk.RemoveUnits(UnitType.Mortar, halfMor);
                atk.RemoveUnits(UnitType.Sniper, halfSnp);
                atk.RemoveUnits(UnitType.Engineer, halfEng);
                atk.RemoveUnits(UnitType.Armor, halfArm);

                def.RifleCount = halfRif;
                def.MGCount = halfMG;
                def.MortarCount = halfMor;
                def.SniperCount = halfSnp;
                def.EngineerCount = halfEng;
                def.ArmorCount = halfArm;
                def.GarrisonStrength = def.TotalUnits;

                _map.SetTerritory(attackerTerritory, atk);
                _map.SetTerritory(defenderTerritory, def);
                OnTerritoryConquered?.Invoke(defenderTerritory, atk.Owner);
            }
            else
            {
                // Defender won — attacker loses forces (already handled in tactical)
                var atk = _map.GetTerritory(attackerTerritory);
                atk.GarrisonStrength = Mathf.Max(1, atk.GarrisonStrength / 2);
                _map.SetTerritory(attackerTerritory, atk);
            }
        }

        private void ApplySupplyAttrition()
        {
            for (int i = 0; i < _map.TerritoryCount; i++)
            {
                var t = _map.GetTerritory(i);
                if (t.Owner == Faction.Neutral) continue;
                if (t.IsSupplied) continue;

                // Unsupplied: lose 10% garrison per turn (minimum 1)
                int loss = Mathf.Max(1, t.GarrisonStrength / 10);
                t.GarrisonStrength = Mathf.Max(0, t.GarrisonStrength - loss);
                t.RifleCount = Mathf.Max(0, t.RifleCount - loss);
                t.GarrisonStrength = t.TotalUnits;

                if (t.GarrisonStrength == 0)
                    t.Owner = Faction.Neutral;

                _map.SetTerritory(i, t);
            }
        }

        private void CheckVictory()
        {
            int playerTerr = _map.CountOwned(Faction.Player);
            int enemyTerr = _map.CountOwned(Faction.Enemy);

            // Victory: own >60% of territories or enemy has 0
            if (enemyTerr == 0 || playerTerr > _map.TerritoryCount * 0.6f)
            {
                MatchInProgress = false;
                OnVictory?.Invoke(Faction.Player);
            }
            else if (playerTerr == 0 || enemyTerr > _map.TerritoryCount * 0.6f)
            {
                MatchInProgress = false;
                OnVictory?.Invoke(Faction.Enemy);
            }
        }
    }
}
