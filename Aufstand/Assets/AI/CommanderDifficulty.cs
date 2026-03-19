// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
namespace Aufstand.AI
{
    /// <summary>
    /// AI commander difficulty tiers — affects strategic and tactical behavior.
    /// </summary>
    public enum CommanderDifficulty
    {
        Rekrut    = 0,  // Passive, slow to reinforce, no recon
        Feldwebel = 1,  // Balanced, defends frontline, occasional attacks
        Oberst    = 2,  // Aggressive flanker, recon-heavy, exploits weak points
        General   = 3,  // Optimal play — full supply management, combined arms, feints
    }
}
