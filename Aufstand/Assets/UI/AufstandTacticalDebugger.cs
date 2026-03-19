// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using CodeGamified.TUI;
using Aufstand.Scripting;

namespace Aufstand.UI
{
    /// <summary>
    /// Tactical code debugger — wires an AufstandTacticalProgram
    /// into the engine's CodeDebuggerWindow.
    /// </summary>
    public class AufstandTacticalDebugger : CodeDebuggerWindow
    {
        protected override void Awake()
        {
            base.Awake();
            windowTitle = "FIELD";
        }

        public void Bind(AufstandTacticalProgram program)
        {
            SetDataSource(new AufstandDebuggerData(program, "FIELD"));
        }
    }
}
