// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;
using CodeGamified.Engine.Runtime;
using CodeGamified.Time;
using Aufstand.Game;
using Aufstand.Game.Tactical;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Tactical program — runs bytecode during battles (20 ops/sec sim-time).
    /// Controls squad-level behavior: movement, doctrine, targeting, artillery.
    /// </summary>
    public class AufstandTacticalProgram : ProgramBehaviour
    {
        private Faction _faction = Faction.Player;
        private TacticalBattle _battle;
        private float _opAccumulator;

        public const float OPS_PER_SECOND = 20f;

        private const string DEFAULT_CODE = @"# Aufstand — Field Commander
# Direct your squads: set doctrines, coordinate attacks, call support

while True:
    squads = get_squad_count()
    enemies = get_enemy_count()
    cap = get_capture_progress()

    i = 0
    while i < squads:
        unit = get_squad_type(i)
        hp = get_squad_hp(i)
        suppressed = get_squad_suppressed(i)
        morale = get_squad_morale(i)

        # Retreat critically wounded squads
        if hp < 20:
            retreat_squad(i)
            i = i + 1
            wait

        # Pinned squads stay put
        if suppressed > 0.7:
            set_doctrine(i, 2)
            i = i + 1
            wait

        # MGs suppress — set defensive
        if unit == 1:
            set_doctrine(i, 2)
            if enemies > 0:
                ex = get_enemy_x(0)
                ey = get_enemy_y(0)
                set_target(i, 0)

        # Rifles flank
        elif unit == 0:
            set_doctrine(i, 3)
            cpx = get_capture_point_x()
            cpy = get_capture_point_y()
            attack_move(i, cpx, cpy)

        # Mortars stay back, indirect fire
        elif unit == 2:
            set_doctrine(i, 2)
            if enemies > 0:
                set_target(i, 0)

        # Snipers pick off from distance
        elif unit == 3:
            set_doctrine(i, 2)
            if enemies > 0:
                set_target(i, 0)

        # Engineers dig in
        elif unit == 4:
            dig_in(i)
            set_doctrine(i, 2)

        # Armor push forward
        elif unit == 5:
            set_doctrine(i, 1)
            cpx = get_capture_point_x()
            cpy = get_capture_point_y()
            attack_move(i, cpx, cpy)

        i = i + 1

    # Call artillery on enemy cluster
    if get_artillery_ready():
        if enemies > 0:
            ex = get_enemy_x(0)
            ey = get_enemy_y(0)
            call_artillery(ex, ey)

    wait
";

        public System.Action OnCodeChanged;

        /// <summary>Bind to an active tactical battle (called when battle triggers).</summary>
        public void BindBattle(TacticalBattle battle)
        {
            _battle = battle;
            if (_sourceCode != null)
                LoadAndRun(_sourceCode);
        }

        public void Initialize(string initialCode = null)
        {
            _sourceCode = initialCode ?? DEFAULT_CODE;
            _programName = "AufstandTactical";
            _autoRun = false;
        }

        protected override void Start()
        {
            if (_sourceCode == null)
                _sourceCode = DEFAULT_CODE;
            _programName = "AufstandTactical";
        }

        public override bool LoadAndRun(string source)
        {
            if (_battle == null) return false;

            _sourceCode = source;
            _executor = new CodeExecutor();
            var handler = new AufstandTacticalIOHandler(_faction, _battle);
            _executor.SetIOHandler(handler);

            _program = PythonCompiler.Compile(source, _programName,
                new AufstandCompilerExtension());

            if (!_program.IsValid)
            {
                Debug.LogWarning("[AufstandTacticalProgram] Compile errors:");
                foreach (var err in _program.Errors)
                    Debug.LogWarning($"  {err}");
                return false;
            }

            _executor.LoadProgram(_program);
            _isPaused = false;
            _opAccumulator = 0f;
            return true;
        }

        protected override void Update()
        {
            if (_executor == null || _program == null || _isPaused) return;
            if (_battle == null || !_battle.IsActive) return;

            float timeScale = SimulationTime.Instance?.timeScale ?? 1f;
            if (SimulationTime.Instance != null && SimulationTime.Instance.isPaused) return;

            float simDelta = Time.deltaTime * timeScale;
            _opAccumulator += simDelta * OPS_PER_SECOND;

            int opsToRun = (int)_opAccumulator;
            _opAccumulator -= opsToRun;

            for (int i = 0; i < opsToRun; i++)
            {
                if (_executor.State.IsHalted)
                {
                    _executor.State.PC = 0;
                    _executor.State.IsHalted = false;
                }
                _executor.ExecuteOne();
            }

            if (opsToRun > 0)
                ProcessEvents();
        }

        protected override IGameIOHandler CreateIOHandler()
        {
            return _battle != null
                ? new AufstandTacticalIOHandler(_faction, _battle)
                : null;
        }

        protected override CompiledProgram CompileSource(string source, string name)
        {
            return PythonCompiler.Compile(source, name, new AufstandCompilerExtension());
        }

        protected override void ProcessEvents()
        {
            while (_executor.State.OutputEvents.Count > 0)
                _executor.State.OutputEvents.Dequeue();
        }

        public void UploadCode(string newSource)
        {
            _sourceCode = newSource ?? DEFAULT_CODE;
            if (_battle != null)
                LoadAndRun(_sourceCode);
            OnCodeChanged?.Invoke();
        }
    }
}
