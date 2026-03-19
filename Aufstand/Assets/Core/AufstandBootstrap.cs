// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CodeGamified.Camera;
using CodeGamified.Time;
using CodeGamified.Settings;
using CodeGamified.Quality;
using CodeGamified.Bootstrap;
using Aufstand.Game;
using Aufstand.Game.Strategic;
using Aufstand.Game.Tactical;
using Aufstand.Scripting;
using Aufstand.AI;
using Aufstand.UI;

namespace Aufstand.Core
{
    /// <summary>
    /// Bootstrap for Aufstand — WW2 grand strategy + tactical combat.
    /// Two-layer game: hex-map strategy (Risk) + squad-level combat (Company of Heroes).
    /// </summary>
    public class AufstandBootstrap : GameBootstrap
    {
        protected override string LogTag => "AUFSTAND";

        // =================================================================
        // INSPECTOR
        // =================================================================

        [Header("Strategic Map")]
        public int territoryCount = 40;
        public float mapSize = 200f;

        [Header("Starting Resources")]
        public int startFuel = 100;
        public int startMunitions = 80;
        public int startManpower = 200;

        [Header("Match")]
        public bool autoRestart = true;
        public float restartDelay = 5f;

        [Header("Scripting")]
        public bool enableScripting = true;

        [Header("AI")]
        public bool enableAI = true;
        public CommanderDifficulty aiDifficulty = CommanderDifficulty.Feldwebel;

        [Header("TUI")]
        public bool enableTUI = true;

        [Header("Camera")]
        public bool configureCamera = true;

        // =================================================================
        // RUNTIME REFERENCES
        // =================================================================

        private HexMap _hexMap;
        private ResourceManager _resourceManager;
        private StrategicMatchManager _strategicMatch;
        private StrategicFogOfWar _fogOfWar;
        private AufstandRenderer _renderer;

        // Scripting
        private AufstandStrategicProgram _playerStrategicProgram;
        private AufstandTacticalProgram _playerTacticalProgram;
        private AufstandInputProvider _inputProvider;

        // AI
        private AufstandAICommander _aiCommander;

        // UI
        private AufstandTUIManager _tuiManager;

        // Camera
        private CameraAmbientMotion _cameraSway;

        // Tactical (created on-demand when battles trigger)
        private TacticalBattle _activeBattle;

        // =================================================================
        // UPDATE
        // =================================================================

        private void Update()
        {
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            // If in tactical battle, follow battle center
            if (_activeBattle != null && _activeBattle.IsActive)
            {
                Vector3 target = new Vector3(
                    _activeBattle.CenterX, 50f, _activeBattle.CenterY);
                cam.transform.position = Vector3.Lerp(
                    cam.transform.position, target,
                    Time.unscaledDeltaTime * 4f);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 25f,
                    Time.unscaledDeltaTime * 2f);
            }
            else
            {
                // Strategic overview
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, mapSize * 0.5f,
                    Time.unscaledDeltaTime * 2f);
            }
        }

        // =================================================================
        // BOOTSTRAP
        // =================================================================

        private void Start()
        {
            Log("AUFSTAND Bootstrap starting...");

            SettingsBridge.Load();
            QualityBridge.SetTier((QualityTier)SettingsBridge.QualityLevel);

            SetupSimulationTime();
            SetupCamera();
            CreateHexMap();
            CreateResourceManager();
            CreateFogOfWar();
            CreateStrategicMatch();
            CreateRenderer();
            CreateInputProvider();

            if (enableScripting) CreatePlayerPrograms();
            if (enableAI) CreateAICommander();
            if (enableTUI) CreateTUI();

            WireEvents();
            RunAfterFrames(() => Log("Boot complete."));
        }

        // =================================================================
        // SIMULATION TIME
        // =================================================================

        private void SetupSimulationTime()
        {
            EnsureSimulationTime<AufstandSimulationTime>();
        }

        // =================================================================
        // CAMERA — orthographic top-down for strategic, zoom for tactical
        // =================================================================

        private void SetupCamera()
        {
            if (!configureCamera) return;

            var cam = EnsureCamera();
            cam.orthographic = true;
            cam.orthographicSize = mapSize * 0.5f;

            cam.transform.position = new Vector3(0f, 50f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.04f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            _cameraSway = cam.gameObject.AddComponent<CameraAmbientMotion>();
            _cameraSway.lookAtTarget = Vector3.zero;

            // Post-processing
            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
                camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var volumeGO = new GameObject("PostProcessVolume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = profile.Add<Bloom>();
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.8f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.8f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.4f;
            volume.profile = profile;

            Log("Camera: ortho top-down, strategic overview");
        }

        // =================================================================
        // HEX MAP
        // =================================================================

        private void CreateHexMap()
        {
            var go = new GameObject("HexMap");
            _hexMap = go.AddComponent<HexMap>();
            _hexMap.Initialize(territoryCount, mapSize);
            Log($"Created HexMap ({territoryCount} territories)");
        }

        // =================================================================
        // RESOURCES
        // =================================================================

        private void CreateResourceManager()
        {
            var go = new GameObject("ResourceManager");
            _resourceManager = go.AddComponent<ResourceManager>();
            _resourceManager.Initialize(_hexMap, startFuel, startMunitions, startManpower);
            Log("Created ResourceManager");
        }

        // =================================================================
        // FOG OF WAR
        // =================================================================

        private void CreateFogOfWar()
        {
            var go = new GameObject("FogOfWar");
            _fogOfWar = go.AddComponent<StrategicFogOfWar>();
            _fogOfWar.Initialize(_hexMap);
            Log("Created StrategicFogOfWar");
        }

        // =================================================================
        // STRATEGIC MATCH
        // =================================================================

        private void CreateStrategicMatch()
        {
            var go = new GameObject("StrategicMatch");
            _strategicMatch = go.AddComponent<StrategicMatchManager>();
            _strategicMatch.Initialize(_hexMap, _resourceManager, _fogOfWar, autoRestart, restartDelay);
            _strategicMatch.OnBattleTriggered += HandleBattleTriggered;
            Log("Created StrategicMatchManager");
        }

        // =================================================================
        // RENDERER
        // =================================================================

        private void CreateRenderer()
        {
            var go = new GameObject("Renderer");
            _renderer = go.AddComponent<AufstandRenderer>();
            _renderer.Initialize(_hexMap, _strategicMatch);
            Log("Created AufstandRenderer");
        }

        // =================================================================
        // INPUT
        // =================================================================

        private void CreateInputProvider()
        {
            var go = new GameObject("InputProvider");
            _inputProvider = go.AddComponent<AufstandInputProvider>();
            Log("Created AufstandInputProvider");
        }

        // =================================================================
        // PLAYER SCRIPTING
        // =================================================================

        private void CreatePlayerPrograms()
        {
            // Strategic program — runs each turn
            var stratGo = new GameObject("PlayerStrategicProgram");
            _playerStrategicProgram = stratGo.AddComponent<AufstandStrategicProgram>();
            _playerStrategicProgram.Initialize(
                Faction.Player, _hexMap, _resourceManager, _strategicMatch, _fogOfWar);

            // Tactical program — runs during battles
            var tactGo = new GameObject("PlayerTacticalProgram");
            _playerTacticalProgram = tactGo.AddComponent<AufstandTacticalProgram>();

            Log("Created strategic + tactical player programs");
        }

        // =================================================================
        // AI
        // =================================================================

        private void CreateAICommander()
        {
            var go = new GameObject("AICommander");
            _aiCommander = go.AddComponent<AufstandAICommander>();
            _aiCommander.Initialize(
                Faction.Enemy, _hexMap, _resourceManager, _strategicMatch, _fogOfWar,
                aiDifficulty);
            Log($"Created AI Commander (difficulty: {aiDifficulty})");
        }

        // =================================================================
        // TUI
        // =================================================================

        private void CreateTUI()
        {
            var go = new GameObject("TUIManager");
            _tuiManager = go.AddComponent<AufstandTUIManager>();
            _tuiManager.Initialize(
                _strategicMatch, _playerStrategicProgram, _playerTacticalProgram);
            Log("Created AufstandTUIManager (left=COMMAND, right=FIELD, bottom=STATUS)");
        }

        // =================================================================
        // BATTLE TRIGGERED
        // =================================================================

        private void HandleBattleTriggered(int attackerTerritory, int defenderTerritory)
        {
            Log($"BATTLE: Territory {attackerTerritory} → {defenderTerritory}");

            var go = new GameObject("TacticalBattle");
            _activeBattle = go.AddComponent<TacticalBattle>();

            var attackerArmy = _strategicMatch.GetArmyComposition(attackerTerritory);
            var defenderArmy = _strategicMatch.GetArmyComposition(defenderTerritory);
            var defenderFort = _hexMap.GetTerritory(defenderTerritory).FortificationLevel;

            _activeBattle.Initialize(
                attackerArmy, defenderArmy, defenderFort,
                attackerTerritory, defenderTerritory);

            // Wire tactical program to active battle
            if (_playerTacticalProgram != null)
                _playerTacticalProgram.BindBattle(_activeBattle);

            _activeBattle.OnBattleEnded += HandleBattleEnded;
        }

        private void HandleBattleEnded(Faction winner, int attackerTerritory, int defenderTerritory)
        {
            Log($"Battle ended: {winner} wins ({attackerTerritory} vs {defenderTerritory})");
            _strategicMatch.ResolveBattle(winner, attackerTerritory, defenderTerritory);

            if (_activeBattle != null)
            {
                Destroy(_activeBattle.gameObject, 1f);
                _activeBattle = null;
            }
        }

        // =================================================================
        // EVENT WIRING
        // =================================================================

        private void WireEvents()
        {
            if (SimulationTime.Instance != null)
            {
                SimulationTime.Instance.OnTimeScaleChanged += s => Log($"Time scale → {s:F0}x");
                SimulationTime.Instance.OnPausedChanged += p => Log(p ? "PAUSED" : "RESUMED");
            }

            if (_strategicMatch != null)
            {
                _strategicMatch.OnTurnAdvanced += turn =>
                    Log($"TURN {turn} — Day {turn}");

                _strategicMatch.OnTerritoryConquered += (id, faction) =>
                    Log($"Territory {id} conquered by {faction}");

                _strategicMatch.OnVictory += faction =>
                    Log($"VICTORY! {faction} wins the campaign!");
            }
        }
    }
}
