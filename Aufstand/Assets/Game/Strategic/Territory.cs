// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
namespace Aufstand.Game.Strategic
{
    /// <summary>
    /// State for a single hex territory on the strategic map.
    /// Struct for cache-friendly storage in the HexMap array.
    /// </summary>
    public struct Territory
    {
        public int Id;
        public float CenterX;
        public float CenterZ;

        // Ownership
        public Faction Owner;
        public bool IsHQ;

        // Resources (per turn output)
        public int FuelOutput;
        public int MunitionsOutput;
        public int ManpowerOutput;

        // Military
        public int GarrisonStrength;       // Total unit count
        public int FortificationLevel;     // 0=none, 1=sandbags, 2=bunkers, 3=minefields+bunkers

        // Army breakdown (units garrisoned by type)
        public int RifleCount;
        public int MGCount;
        public int MortarCount;
        public int SniperCount;
        public int EngineerCount;
        public int ArmorCount;

        // Supply
        public SupplyPriority SupplyPriority;
        public bool IsSupplied;
        public int SupplyDistance;           // Hops to HQ (-1 = cut off)
        public float SupplyEfficiency;       // 0.0-1.0

        // Rally point
        public bool IsRallyPoint;

        /// <summary>Total unit count from type breakdown.</summary>
        public int TotalUnits =>
            RifleCount + MGCount + MortarCount + SniperCount + EngineerCount + ArmorCount;

        /// <summary>Get count of specific unit type.</summary>
        public int GetUnitCount(UnitType type)
        {
            switch (type)
            {
                case UnitType.Rifle:    return RifleCount;
                case UnitType.MG:       return MGCount;
                case UnitType.Mortar:   return MortarCount;
                case UnitType.Sniper:   return SniperCount;
                case UnitType.Engineer: return EngineerCount;
                case UnitType.Armor:    return ArmorCount;
                default: return 0;
            }
        }

        /// <summary>Add units of a given type.</summary>
        public void AddUnits(UnitType type, int count)
        {
            switch (type)
            {
                case UnitType.Rifle:    RifleCount += count; break;
                case UnitType.MG:       MGCount += count; break;
                case UnitType.Mortar:   MortarCount += count; break;
                case UnitType.Sniper:   SniperCount += count; break;
                case UnitType.Engineer: EngineerCount += count; break;
                case UnitType.Armor:    ArmorCount += count; break;
            }
            GarrisonStrength = TotalUnits;
        }

        /// <summary>Remove units of a given type (clamped to 0).</summary>
        public void RemoveUnits(UnitType type, int count)
        {
            switch (type)
            {
                case UnitType.Rifle:    RifleCount = System.Math.Max(0, RifleCount - count); break;
                case UnitType.MG:       MGCount = System.Math.Max(0, MGCount - count); break;
                case UnitType.Mortar:   MortarCount = System.Math.Max(0, MortarCount - count); break;
                case UnitType.Sniper:   SniperCount = System.Math.Max(0, SniperCount - count); break;
                case UnitType.Engineer: EngineerCount = System.Math.Max(0, EngineerCount - count); break;
                case UnitType.Armor:    ArmorCount = System.Math.Max(0, ArmorCount - count); break;
            }
            GarrisonStrength = TotalUnits;
        }
    }
}
