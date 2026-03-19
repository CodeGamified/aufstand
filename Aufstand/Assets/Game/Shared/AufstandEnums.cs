// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
namespace Aufstand.Game
{
    public enum Faction
    {
        Neutral = 0,
        Player  = 1,
        Enemy   = 2
    }

    public enum UnitType
    {
        Rifle    = 0,
        MG       = 1,
        Mortar   = 2,
        Sniper   = 3,
        Engineer = 4,
        Armor    = 5
    }

    public enum Doctrine
    {
        Hold       = 0,
        Aggressive = 1,
        Defensive  = 2,
        Flank      = 3,
        Retreat    = 4
    }

    public enum SquadStatus
    {
        Idle      = 0,
        Moving    = 1,
        Engaging  = 2,
        Retreating = 3,
        Pinned    = 4
    }

    public enum CoverRating
    {
        None     = 0,   // 0% reduction
        Light    = 1,   // 25% reduction (fences, craters)
        Heavy    = 2,   // 50% reduction (walls, sandbags)
        Building = 3    // 75% reduction (inside building)
    }

    public enum AbilityType
    {
        Smoke   = 0,
        Grenade = 1,
        Mine    = 2,
        Repair  = 3
    }

    public enum SupplyPriority
    {
        Low    = 0,
        Normal = 1,
        High   = 2
    }
}
