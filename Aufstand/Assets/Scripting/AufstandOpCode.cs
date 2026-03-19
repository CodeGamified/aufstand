// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
namespace Aufstand.Scripting
{
    /// <summary>
    /// Aufstand-specific opcodes mapped to CUSTOM_0..CUSTOM_N.
    /// Split into strategic (map/resources/army) and tactical (squad/combat/artillery).
    /// Strategic: opcodes 0-39. Tactical: opcodes 40-79.
    /// </summary>
    public enum AufstandOpCode
    {
        // =====================================================================
        // STRATEGIC OPCODES (0-39) — run once per turn
        // =====================================================================

        // ── Territory & Map ──
        GET_TERRITORY_COUNT     = 0,   // Total territories on map
        GET_MY_TERRITORIES      = 1,   // Count controlled by player
        GET_ENEMY_TERRITORIES   = 2,   // Count controlled by enemy
        GET_TERRITORY_OWNER     = 3,   // Owner of territory R0 (0=neutral, 1=you, 2=enemy)
        GET_TERRITORY_FUEL      = 4,   // Fuel output of territory R0
        GET_TERRITORY_MUNITIONS = 5,   // Munitions output of territory R0
        GET_TERRITORY_MANPOWER  = 6,   // Manpower output of territory R0
        GET_ADJACENT            = 7,   // Count of adjacent territories to R0
        GET_ADJACENT_ID         = 8,   // ID of R1-th adjacent territory to R0
        IS_FRONTLINE            = 9,   // 1 if territory R0 borders enemy
        GET_TERRITORY_STRENGTH  = 10,  // Garrison strength in territory R0

        // ── Resources ──
        GET_FUEL                = 11,  // Current fuel stockpile
        GET_MUNITIONS           = 12,  // Current munitions stockpile
        GET_MANPOWER            = 13,  // Current manpower stockpile
        GET_FUEL_INCOME         = 14,  // Fuel per turn
        GET_MUNITIONS_INCOME    = 15,  // Munitions per turn
        GET_MANPOWER_INCOME     = 16,  // Manpower per turn

        // ── Supply ──
        IS_SUPPLIED             = 17,  // 1 if territory R0 has supply line to HQ
        GET_SUPPLY_DIST         = 18,  // Supply chain length to HQ
        GET_SUPPLY_EFFICIENCY   = 19,  // Supply efficiency 0.0-1.0
        SET_SUPPLY_PRIORITY     = 20,  // Set supply priority for territory R0 (R1=priority)
        GET_ROUTE_THREATENED    = 21,  // 1 if supply route threatened

        // ── Army Management ──
        RECRUIT                 = 22,  // Recruit unit type R0 at territory R1
        TRANSFER                = 23,  // Transfer R2 troops from territory R0 to R1
        ATTACK                  = 24,  // Launch attack from territory R0 to R1
        REINFORCE               = 25,  // Reinforce territory R0 with rifle infantry
        FORTIFY                 = 26,  // Build defenses at territory R0
        SET_RALLY_POINT         = 27,  // Set rally point to territory R0
        RETREAT_TERRITORY       = 28,  // Retreat forces from territory R0
        GET_ARMY_SIZE           = 29,  // Get garrison size at territory R0
        GET_ARMY_COMPOSITION    = 30,  // Get count of unit type R1 at territory R0

        // ── Intelligence ──
        GET_ENEMY_STRENGTH      = 31,  // Estimated enemy garrison at territory R0
        GET_FRONT_PRESSURE      = 32,  // Enemy threat level at territory R0 (0.0-1.0)
        GET_RECON               = 33,  // Intel age in turns for territory R0
        ORDER_RECON             = 34,  // Send recon to territory R0 (costs fuel)

        // ── Turn Info ──
        GET_CURRENT_TURN        = 35,  // Current turn number

        // Reserved strategic: 36-39

        // =====================================================================
        // TACTICAL OPCODES (40-79) — run during battles at 20 ops/sec
        // =====================================================================

        // ── Squad Sensors ──
        GET_SQUAD_COUNT         = 40,  // Number of squads you command
        GET_SQUAD_X             = 41,  // Squad R0 position X
        GET_SQUAD_Y             = 42,  // Squad R0 position Y
        GET_SQUAD_HP            = 43,  // Squad R0 health
        GET_SQUAD_TYPE          = 44,  // Squad R0 unit type
        GET_SQUAD_AMMO          = 45,  // Squad R0 ammo remaining
        GET_SQUAD_SUPPRESSED    = 46,  // Squad R0 suppression 0.0-1.0
        GET_SQUAD_IN_COVER      = 47,  // Squad R0 in cover? (0-3 cover rating)
        GET_SQUAD_MORALE        = 48,  // Squad R0 morale 0.0-1.0
        GET_SQUAD_STATUS        = 49,  // Squad R0 status (0=idle,1=moving,2=engaging,3=retreating,4=pinned)

        // ── Battlefield Awareness ──
        GET_ENEMY_COUNT         = 50,  // Visible enemy squad count
        GET_ENEMY_X             = 51,  // Enemy R0 position X
        GET_ENEMY_Y             = 52,  // Enemy R0 position Y
        GET_ENEMY_TYPE          = 53,  // Enemy R0 unit type
        GET_ENEMY_HEALTH        = 54,  // Enemy R0 approx health
        GET_COVER_X             = 55,  // Nearest cover position X
        GET_COVER_Y             = 56,  // Nearest cover position Y
        GET_COVER_RATING        = 57,  // Nearest cover quality (0-3)
        GET_CAPTURE_X           = 58,  // Capture point X
        GET_CAPTURE_Y           = 59,  // Capture point Y
        GET_CAPTURE_PROGRESS    = 60,  // Capture progress -1.0 to 1.0

        // ── Squad Commands ──
        MOVE_TO                 = 61,  // Move squad R0 to (R1, R2)
        ATTACK_MOVE             = 62,  // Attack-move squad R0 to (R1, R2)
        SET_DOCTRINE            = 63,  // Set doctrine for squad R0 to R1
        SET_TARGET              = 64,  // Focus fire: squad R0 targets enemy R1
        GARRISON                = 65,  // Squad R0 enters nearest building
        DIG_IN                  = 66,  // Squad R0 creates sandbag cover (engineer only)
        USE_ABILITY             = 67,  // Squad R0 uses ability R1 (0=smoke,1=grenade,2=mine,3=repair)
        RETREAT_SQUAD           = 68,  // Force retreat: squad R0
        SET_SPACING             = 69,  // Set formation spread for squad R0 to R1

        // ── Artillery & Support ──
        CALL_ARTILLERY          = 70,  // Artillery barrage at (R0, R1) — 8s delay
        CALL_SMOKE              = 71,  // Smoke barrage at (R0, R1) — 5s delay
        CALL_REINFORCE          = 72,  // Request fresh squad from reserves
        GET_ARTILLERY_READY     = 73,  // 1 if artillery cooldown complete
        GET_SUPPORT_POINTS      = 74,  // Available support call budget

        // ── Data Bus ──
        SEND_BUS                = 75,  // Write R1 to channel R0
        RECV_BUS                = 76,  // Read channel R0 → R0
    }
}
