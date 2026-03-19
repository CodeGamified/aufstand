// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;

namespace Aufstand.Core
{
    /// <summary>
    /// Aufstand-specific simulation time — supports high time scales for
    /// fast-forwarding campaigns. Strategic turns = 1 game day.
    /// </summary>
    public class AufstandSimulationTime : CodeGamified.Time.SimulationTime
    {
        protected override float MaxTimeScale => 500f;

        protected override void OnInitialize()
        {
            timeScalePresets = new float[]
                { 0f, 0.5f, 1f, 2f, 5f, 10f, 50f, 100f, 500f };
            currentPresetIndex = 2; // Start at 1x
        }

        public override string GetFormattedTime()
        {
            int days = (int)(simulationTime / 86400.0) + 1;
            int hours = (int)((simulationTime % 86400.0) / 3600.0);
            return $"DAY {days} {hours:D2}:00";
        }
    }
}
