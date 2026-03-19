// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;
using CodeGamified.Engine.Runtime;
using CodeGamified.Time;
using Aufstand.Game;
using Aufstand.Game.Strategic;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Strategic program — runs bytecode once per turn (10 ops/sec sim-time).
    /// Manages theatre-level decisions: army allocation, attacks, supply, intel.
    /// </summary>
    public class AufstandStrategicProgram : ProgramBehaviour
    {
        private Faction _faction;
        private HexMap _map;
        private ResourceManager _resources;
        private StrategicMatchManager _match;
        private StrategicFogOfWar _fog;
        private float _opAccumulator;

        public const float OPS_PER_SECOND = 10f;

        private const string DEFAULT_CODE = @"# Aufstand — Strategic Commander
# Manage your theatre: allocate armies, attack weak points, maintain supply

while True:
    turn = get_current_turn()
    fuel = get_fuel()
    muni = get_munitions()
    manp = get_manpower()
    my_terr = get_my_territories()
    total = get_territory_count()

    # Scan frontline territories
    i = 0
    while i < total:
        owner = get_territory_owner(i)
        if owner == 1:
            if is_frontline(i):
                strength = get_territory_strength(i)
                pressure = get_front_pressure(i)

                # Reinforce threatened sectors
                if pressure > 0.6:
                    if strength < 5:
                        if manp > 30:
                            reinforce(i)
                            manp = get_manpower()

                # Fortify key positions
                if strength > 6:
                    if fuel > 20:
                        fortify(i)

                # Attack weak neighbors
                adj = get_adjacent(i)
                j = 0
                while j < adj:
                    nid = get_adjacent_id(i, j)
                    n_owner = get_territory_owner(nid)
                    if n_owner == 2:
                        e_str = get_enemy_strength(nid)
                        if strength > e_str + 3:
                            attack(i, nid)
                    j = j + 1

            # Check supply
            if is_supplied(i) == 0:
                set_supply_priority(i, 2)

        i = i + 1

    # Recruit at rally point if flush
    if manp > 100:
        i = 0
        while i < total:
            if get_territory_owner(i) == 1:
                if get_territory_strength(i) < 3:
                    recruit(0, i)
            i = i + 1

    # Recon stale targets
    if fuel > 15:
        i = 0
        while i < total:
            if get_territory_owner(i) == 2:
                age = get_recon(i)
                if age > 3:
                    order_recon(i)
                    fuel = get_fuel()
            i = i + 1

    wait
";

        public System.Action OnCodeChanged;

        public void Initialize(Faction faction, HexMap map, ResourceManager resources,
                               StrategicMatchManager match, StrategicFogOfWar fog,
                               string initialCode = null)
        {
            _faction = faction;
            _map = map;
            _resources = resources;
            _match = match;
            _fog = fog;
            _sourceCode = initialCode ?? DEFAULT_CODE;
            _programName = "AufstandStrategic";
            _autoRun = false;

            LoadAndRun(_sourceCode);
        }

        protected override void Start()
        {
            // Don't call base — Initialize handles setup
        }

        public override bool LoadAndRun(string source)
        {
            _sourceCode = source;
            _executor = new CodeExecutor();
            var handler = new AufstandStrategicIOHandler(
                _faction, _map, _resources, _match, _fog);
            _executor.SetIOHandler(handler);

            _program = PythonCompiler.Compile(source, _programName,
                new AufstandCompilerExtension());

            if (!_program.IsValid)
            {
                Debug.LogWarning("[AufstandStrategicProgram] Compile errors:");
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
            if (_match == null || !_match.MatchInProgress) return;
            if (_match.BattleInProgress) return; // Pause strategic during battle

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
            return new AufstandStrategicIOHandler(_faction, _map, _resources, _match, _fog);
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
            LoadAndRun(newSource ?? DEFAULT_CODE);
            OnCodeChanged?.Invoke();
        }
    }
}
