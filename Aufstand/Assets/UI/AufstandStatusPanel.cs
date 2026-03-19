// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using CodeGamified.TUI;
using CodeGamified.Time;
using Aufstand.Game;
using Aufstand.Game.Strategic;
using Aufstand.Scripting;

namespace Aufstand.UI
{
    /// <summary>
    /// Unified status panel — 7 columns:
    ///   PLAYER │ SETTINGS │ MAP │ AUFSTAND │ CONTROLS │ AUDIO │ COMMANDER
    /// Shows turn info, resources, territory count, front status.
    /// </summary>
    public class AufstandStatusPanel : TerminalWindow
    {
        private StrategicMatchManager _match;
        private AufstandStrategicProgram _strategicProgram;
        private AufstandTacticalProgram _tacticalProgram;

        private const int COL_COUNT = 7;
        private int[] _colPositions;
        private const int MaxStatusRows = 10;

        protected override void Awake()
        {
            base.Awake();
            windowTitle = "AUFSTAND";
            totalRows = MaxStatusRows;
        }

        public void Bind(StrategicMatchManager match,
                         AufstandStrategicProgram strategic,
                         AufstandTacticalProgram tactical)
        {
            _match = match;
            _strategicProgram = strategic;
            _tacticalProgram = tactical;
        }

        protected override void OnLayoutReady()
        {
            ComputeColumnPositions();
            foreach (var row in rows)
                row.SetNPanelMode(true, _colPositions);
        }

        private void ComputeColumnPositions()
        {
            float[] ratios = { 0f, 0.11f, 0.22f, 0.33f, 0.67f, 0.78f, 0.89f };
            _colPositions = new int[COL_COUNT];
            _colPositions[0] = 0;
            for (int i = 1; i < COL_COUNT; i++)
                _colPositions[i] = Mathf.RoundToInt(totalChars * ratios[i]);
        }

        protected override void Update()
        {
            base.Update();
            if (!rowsReady || _colPositions == null) return;

            RenderStatus();
        }

        private void RenderStatus()
        {
            int turn = _match?.CurrentTurn ?? 0;
            bool inBattle = _match?.BattleInProgress ?? false;
            string timeStr = SimulationTime.Instance?.GetFormattedTime() ?? "DAY 1";
            float timeScale = SimulationTime.Instance?.timeScale ?? 1f;

            string battleStatus = inBattle
                ? TUIColors.Fg(TUIColors.Red, "⚔ BATTLE IN PROGRESS")
                : TUIColors.Fg(TUIColors.BrightGreen, "STRATEGIC PHASE");

            // Row 0: headers
            SetRowPanels(0, new[]
            {
                TUIColors.Fg(TUIColors.BrightCyan, "PLAYER"),
                TUIColors.Fg(TUIColors.BrightCyan, "SETTINGS"),
                TUIColors.Fg(TUIColors.BrightCyan, "MAP"),
                TUIColors.Fg(TUIColors.BrightGreen, "═══ AUFSTAND ═══"),
                TUIColors.Fg(TUIColors.BrightCyan, "CONTROLS"),
                TUIColors.Fg(TUIColors.BrightCyan, "AUDIO"),
                TUIColors.Fg(TUIColors.BrightCyan, "COMMANDER")
            });

            // Row 1: detail line 1
            SetRowPanels(1, new[]
            {
                $"Turn: {turn}",
                "",
                $"Yours: {GetMyTerritories()}",
                $"{timeStr}  {timeScale:F0}x",
                "+/- Time Scale",
                "",
                $"Strat: {(_strategicProgram != null ? "ACTIVE" : "---")}"
            });

            // Row 2: detail line 2
            SetRowPanels(2, new[]
            {
                $"Status: {(inBattle ? "BATTLE" : "STRATEGIC")}",
                "",
                $"Enemy: {GetEnemyTerritories()}",
                battleStatus,
                "Space: Pause",
                "",
                $"Tact:  {(_tacticalProgram != null ? "ACTIVE" : "---")}"
            });
        }

        private int GetMyTerritories()
        {
            return 0; // Will be filled from match data
        }

        private int GetEnemyTerritories()
        {
            return 0;
        }

        private void SetRowPanels(int rowIdx, string[] texts)
        {
            if (rowIdx >= rows.Count) return;
            rows[rowIdx].SetNPanelTexts(texts);
        }
    }
}
