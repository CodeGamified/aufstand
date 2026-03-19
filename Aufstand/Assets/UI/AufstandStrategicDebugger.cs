// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using CodeGamified.TUI;
using Aufstand.Scripting;

namespace Aufstand.UI
{
    /// <summary>
    /// Strategic code debugger — wires an AufstandStrategicProgram
    /// into the engine's CodeDebuggerWindow.
    /// </summary>
    public class AufstandStrategicDebugger : CodeDebuggerWindow
    {
        protected override void Awake()
        {
            base.Awake();
            windowTitle = "COMMAND";
        }

        public void Bind(AufstandStrategicProgram program)
        {
            SetDataSource(new AufstandDebuggerData(program, "COMMAND"));
        }
    }
}
