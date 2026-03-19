// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;
using CodeGamified.Engine;
using CodeGamified.Engine.Runtime;
using CodeGamified.TUI;
using Aufstand.Scripting;

namespace Aufstand.UI
{
    /// <summary>
    /// Adapts an AufstandStrategicProgram or AufstandTacticalProgram
    /// into the engine's IDebuggerDataSource contract.
    /// </summary>
    public class AufstandDebuggerData : IDebuggerDataSource
    {
        private readonly ProgramBehaviour _program;
        private readonly string _label;

        public AufstandDebuggerData(ProgramBehaviour program, string label = null)
        {
            _program = program;
            _label = label;
        }

        public string ProgramName => _label ?? _program?.ProgramName ?? "Aufstand";
        public string[] SourceLines => _program?.Program?.SourceLines;

        public bool HasLiveProgram =>
            _program != null && _program.Executor != null && _program.Program != null
            && _program.Program.Instructions != null && _program.Program.Instructions.Length > 0;

        public int PC
        {
            get
            {
                var s = _program?.State;
                if (s == null) return 0;
                return s.LastExecutedPC >= 0 ? s.LastExecutedPC : s.PC;
            }
        }

        public long CycleCount => _program?.State?.CycleCount ?? 0;

        public string StatusString
        {
            get
            {
                if (_program == null || _program.Executor == null)
                    return TUIColors.Dimmed("NO PROGRAM");
                var state = _program.State;
                if (state == null) return TUIColors.Dimmed("NO STATE");
                int instCount = _program.Program?.Instructions?.Length ?? 0;
                return TUIColors.Fg(TUIColors.BrightGreen, $"TICK {instCount} inst");
            }
        }

        public List<string> BuildSourceLines(int pc, int scrollOffset, int maxRows)
        {
            var lines = new List<string>();
            var src = SourceLines;
            if (src == null) return lines;

            int startLine = Mathf.Max(0, scrollOffset);
            for (int i = startLine; i < src.Length && lines.Count < maxRows; i++)
            {
                bool isActive = false;
                if (HasLiveProgram && pc < _program.Program.Instructions.Length)
                {
                    int srcLine = _program.Program.Instructions[pc].SourceLine - 1;
                    isActive = (i == srcLine);
                }

                string prefix = isActive
                    ? TUIColors.Fg(TUIColors.BrightGreen, $" {TUIGlyphs.ArrowR} ")
                    : "   ";
                lines.Add($"{prefix}{i + 1,3}  {src[i]}");
            }
            return lines;
        }

        public List<string> BuildMachineLines(int pc, int maxRows)
        {
            var lines = new List<string>();
            if (!HasLiveProgram) return lines;

            var instructions = _program.Program.Instructions;
            int start = Mathf.Max(0, pc - maxRows / 2);

            for (int i = start; i < instructions.Length && lines.Count < maxRows; i++)
            {
                var inst = instructions[i];
                bool isActive = (i == pc);
                string prefix = isActive
                    ? TUIColors.Fg(TUIColors.BrightYellow, ">")
                    : " ";
                string opName = inst.Op.ToString();
                if (opName.StartsWith("CUSTOM_"))
                {
                    int customIdx = (int)inst.Op - (int)OpCode.CUSTOM_0;
                    if (System.Enum.IsDefined(typeof(AufstandOpCode), customIdx))
                        opName = ((AufstandOpCode)customIdx).ToString();
                }
                lines.Add($"{prefix}{i:D4}: {opName}");
            }
            return lines;
        }

        public List<string> BuildStateLines()
        {
            var lines = new List<string>();
            if (!HasLiveProgram) return lines;

            var state = _program.State;
            lines.Add($"PC: {state.PC}");
            lines.Add($"Cycles: {state.CycleCount}");

            int regCount = 8;
            for (int i = 0; i < regCount; i++)
                lines.Add($"R{i}: {state.Registers[i]:F2}");

            return lines;
        }
    }
}
