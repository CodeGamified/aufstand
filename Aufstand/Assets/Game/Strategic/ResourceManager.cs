// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;

namespace Aufstand.Game.Strategic
{
    /// <summary>
    /// Resource manager — tracks fuel, munitions, manpower for each faction.
    /// Income is calculated per turn from owned territories.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        private HexMap _map;

        // Player resources
        public int PlayerFuel { get; private set; }
        public int PlayerMunitions { get; private set; }
        public int PlayerManpower { get; private set; }

        // Enemy resources
        public int EnemyFuel { get; private set; }
        public int EnemyMunitions { get; private set; }
        public int EnemyManpower { get; private set; }

        // Income (cached from last calculation)
        public int PlayerFuelIncome { get; private set; }
        public int PlayerMunitionsIncome { get; private set; }
        public int PlayerManpowerIncome { get; private set; }
        public int EnemyFuelIncome { get; private set; }
        public int EnemyMunitionsIncome { get; private set; }
        public int EnemyManpowerIncome { get; private set; }

        public void Initialize(HexMap map, int startFuel, int startMunitions, int startManpower)
        {
            _map = map;
            PlayerFuel = startFuel;
            PlayerMunitions = startMunitions;
            PlayerManpower = startManpower;
            EnemyFuel = startFuel;
            EnemyMunitions = startMunitions;
            EnemyManpower = startManpower;
            RecalculateIncome();
        }

        public int GetFuel(Faction f) => f == Faction.Player ? PlayerFuel : EnemyFuel;
        public int GetMunitions(Faction f) => f == Faction.Player ? PlayerMunitions : EnemyMunitions;
        public int GetManpower(Faction f) => f == Faction.Player ? PlayerManpower : EnemyManpower;
        public int GetFuelIncome(Faction f) => f == Faction.Player ? PlayerFuelIncome : EnemyFuelIncome;
        public int GetMunitionsIncome(Faction f) => f == Faction.Player ? PlayerMunitionsIncome : EnemyMunitionsIncome;
        public int GetManpowerIncome(Faction f) => f == Faction.Player ? PlayerManpowerIncome : EnemyManpowerIncome;

        /// <summary>Recalculate income from owned + supplied territories.</summary>
        public void RecalculateIncome()
        {
            PlayerFuelIncome = PlayerMunitionsIncome = PlayerManpowerIncome = 0;
            EnemyFuelIncome = EnemyMunitionsIncome = EnemyManpowerIncome = 0;

            for (int i = 0; i < _map.TerritoryCount; i++)
            {
                var t = _map.GetTerritory(i);
                if (t.Owner == Faction.Neutral) continue;

                // Territory must be supplied to produce full output
                float factor = t.IsSupplied ? t.SupplyEfficiency : 0.25f;

                int fuel = Mathf.RoundToInt(t.FuelOutput * factor);
                int muni = Mathf.RoundToInt(t.MunitionsOutput * factor);
                int manp = Mathf.RoundToInt(t.ManpowerOutput * factor);

                if (t.Owner == Faction.Player)
                {
                    PlayerFuelIncome += fuel;
                    PlayerMunitionsIncome += muni;
                    PlayerManpowerIncome += manp;
                }
                else
                {
                    EnemyFuelIncome += fuel;
                    EnemyMunitionsIncome += muni;
                    EnemyManpowerIncome += manp;
                }
            }
        }

        /// <summary>Apply income at turn start.</summary>
        public void ApplyIncome()
        {
            RecalculateIncome();
            PlayerFuel += PlayerFuelIncome;
            PlayerMunitions += PlayerMunitionsIncome;
            PlayerManpower += PlayerManpowerIncome;
            EnemyFuel += EnemyFuelIncome;
            EnemyMunitions += EnemyMunitionsIncome;
            EnemyManpower += EnemyManpowerIncome;
        }

        /// <summary>Spend resources. Returns false if insufficient.</summary>
        public bool TrySpend(Faction faction, int fuel, int munitions, int manpower)
        {
            if (faction == Faction.Player)
            {
                if (PlayerFuel < fuel || PlayerMunitions < munitions || PlayerManpower < manpower)
                    return false;
                PlayerFuel -= fuel;
                PlayerMunitions -= munitions;
                PlayerManpower -= manpower;
            }
            else
            {
                if (EnemyFuel < fuel || EnemyMunitions < munitions || EnemyManpower < manpower)
                    return false;
                EnemyFuel -= fuel;
                EnemyMunitions -= munitions;
                EnemyManpower -= manpower;
            }
            return true;
        }
    }
}
