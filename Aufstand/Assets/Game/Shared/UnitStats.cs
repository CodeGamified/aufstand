// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
namespace Aufstand.Game
{
    /// <summary>
    /// Static unit stats table — cost, combat values, capabilities per UnitType.
    /// </summary>
    public static class UnitStats
    {
        public struct Stats
        {
            public UnitType Type;
            public string Name;
            public int ManpowerCost;
            public int FuelCost;
            public float BaseHP;
            public float DPS;
            public float Range;
            public float Speed;           // units/sec in tactical
            public float SuppressionRate;  // how fast this unit suppresses targets
            public bool CanCapture;
            public bool CanBuildCover;
            public bool CanLayMines;
            public bool CanRepair;
        }

        public static readonly Stats[] Table = new Stats[]
        {
            // Rifle — cheap, versatile, captures points
            new Stats
            {
                Type = UnitType.Rifle, Name = "Rifle",
                ManpowerCost = 20, FuelCost = 0,
                BaseHP = 100f, DPS = 8f, Range = 25f, Speed = 5f,
                SuppressionRate = 0.05f,
                CanCapture = true
            },
            // MG — suppression, area denial, slow to reposition
            new Stats
            {
                Type = UnitType.MG, Name = "MG",
                ManpowerCost = 30, FuelCost = 0,
                BaseHP = 80f, DPS = 12f, Range = 35f, Speed = 3f,
                SuppressionRate = 0.25f,
                CanCapture = true
            },
            // Mortar — indirect fire, smoke rounds, minimum range
            new Stats
            {
                Type = UnitType.Mortar, Name = "Mortar",
                ManpowerCost = 35, FuelCost = 5,
                BaseHP = 60f, DPS = 15f, Range = 50f, Speed = 3f,
                SuppressionRate = 0.15f,
                CanCapture = false
            },
            // Sniper — long range, recon, pick-off
            new Stats
            {
                Type = UnitType.Sniper, Name = "Sniper",
                ManpowerCost = 40, FuelCost = 0,
                BaseHP = 40f, DPS = 20f, Range = 60f, Speed = 4f,
                SuppressionRate = 0.02f,
                CanCapture = false
            },
            // Engineer — builds cover, lays mines, repairs vehicles
            new Stats
            {
                Type = UnitType.Engineer, Name = "Engineer",
                ManpowerCost = 25, FuelCost = 5,
                BaseHP = 70f, DPS = 5f, Range = 15f, Speed = 4f,
                SuppressionRate = 0.03f,
                CanCapture = true,
                CanBuildCover = true,
                CanLayMines = true,
                CanRepair = true
            },
            // Armor — heavy assault, expensive, vulnerable to flanking
            new Stats
            {
                Type = UnitType.Armor, Name = "Armor",
                ManpowerCost = 60, FuelCost = 30,
                BaseHP = 300f, DPS = 25f, Range = 40f, Speed = 6f,
                SuppressionRate = 0.10f,
                CanCapture = false
            },
        };

        public static Stats Get(UnitType type) => Table[(int)type];
    }
}
