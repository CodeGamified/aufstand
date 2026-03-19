// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeGamified.TUI;
using CodeGamified.Settings;
using Aufstand.Game.Strategic;
using Aufstand.Scripting;

namespace Aufstand.UI
{
    /// <summary>
    /// TUI Manager for Aufstand — dual code debugger panels + status bar.
    ///
    /// Layout:
    ///   ┌──────────────────────────┐                ┌──────────────────────────┐
    ///   │ COMMAND TERMINAL         │   GAME VIEW    │ FIELD TERMINAL           │
    ///   │ SOURCE ┆ MACHINE ┆ STATE│   (34% open)   │ STATE ┆ MACHINE ┆ SOURCE │
    ///   ├──────────────────────────┴────────────────┴──────────────────────────┤
    ///   │ PLAYER ┆ SETTINGS ┆ MAP ┆ AUFSTAND ┆ CONTROLS ┆ AUDIO ┆ COMMANDER  │
    ///   └─────────────────────────────────────────────────────────────────────┘
    /// </summary>
    public class AufstandTUIManager : MonoBehaviour, ISettingsListener
    {
        private StrategicMatchManager _match;
        private AufstandStrategicProgram _strategicProgram;
        private AufstandTacticalProgram _tacticalProgram;

        private Canvas _canvas;
        private RectTransform _canvasRect;

        private AufstandStrategicDebugger _strategicDebugger;
        private AufstandTacticalDebugger _tacticalDebugger;
        private AufstandStatusPanel _statusPanel;

        private RectTransform _stratDebuggerRect;
        private RectTransform _tactDebuggerRect;
        private RectTransform _statusPanelRect;

        private TMP_FontAsset _font;
        private float _fontSize;

        private RectTransform[] _allPanelRects;

        public void Initialize(StrategicMatchManager match,
                               AufstandStrategicProgram strategicProgram,
                               AufstandTacticalProgram tacticalProgram)
        {
            _match = match;
            _strategicProgram = strategicProgram;
            _tacticalProgram = tacticalProgram;
            _fontSize = SettingsBridge.FontSize;

            BuildCanvas();
            BuildPanels();
        }

        private void OnEnable()  => SettingsBridge.Register(this);
        private void OnDisable() => SettingsBridge.Unregister(this);

        public void OnSettingsChanged(SettingsSnapshot settings, SettingsCategory changed)
        {
            if (changed != SettingsCategory.Display) return;
            if (Mathf.Approximately(settings.FontSize, _fontSize)) return;
            _fontSize = settings.FontSize;
            RebuildPanels();
        }

        private void RebuildPanels()
        {
            if (_allPanelRects != null)
                foreach (var rt in _allPanelRects)
                    if (rt != null) Destroy(rt.gameObject);

            _strategicDebugger = null;
            _tacticalDebugger = null;
            _statusPanel = null;
            BuildPanels();
        }

        // ═══════════════════════════════════════════════════════════════
        // CANVAS
        // ═══════════════════════════════════════════════════════════════

        private void BuildCanvas()
        {
            var canvasGO = new GameObject("AufstandTUI_Canvas");
            canvasGO.transform.SetParent(transform, false);

            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;
            _canvas.sortingOrder = 100;
            _canvas.planeDistance = 1f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            _canvasRect = canvasGO.GetComponent<RectTransform>();

            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // PANELS
        // ═══════════════════════════════════════════════════════════════

        private void BuildPanels()
        {
            const float statusH = 0.25f;

            // ── Strategic debugger (left panel: 0-33%) ──
            _stratDebuggerRect = CreatePanel("Strategic_Debugger",
                new Vector2(0f, statusH), new Vector2(0.33f, 1f));
            _strategicDebugger = _stratDebuggerRect.gameObject
                .AddComponent<AufstandStrategicDebugger>();
            AddPanelBackground(_stratDebuggerRect);
            _strategicDebugger.InitializeProgrammatic(GetFont(), _fontSize,
                _stratDebuggerRect.GetComponent<Image>());
            _strategicDebugger.SetTitle("COMMAND TERMINAL");
            if (_strategicProgram != null)
                _strategicDebugger.Bind(_strategicProgram);

            // ── Tactical debugger (right panel: 67-100%) ──
            _tactDebuggerRect = CreatePanel("Tactical_Debugger",
                new Vector2(0.67f, statusH), new Vector2(1f, 1f));
            _tacticalDebugger = _tactDebuggerRect.gameObject
                .AddComponent<AufstandTacticalDebugger>();
            AddPanelBackground(_tactDebuggerRect);
            _tacticalDebugger.InitializeProgrammatic(GetFont(), _fontSize,
                _tactDebuggerRect.GetComponent<Image>());
            _tacticalDebugger.SetTitle("FIELD TERMINAL");
            _tacticalDebugger.SetMirrorPanels(true);
            if (_tacticalProgram != null)
                _tacticalDebugger.Bind(_tacticalProgram);

            // ── Status Panel (bottom: 0-25%) ──
            _statusPanelRect = CreatePanel("StatusPanel",
                new Vector2(0f, 0f), new Vector2(1f, statusH));
            _statusPanel = _statusPanelRect.gameObject.AddComponent<AufstandStatusPanel>();
            AddPanelBackground(_statusPanelRect);
            _statusPanel.InitializeProgrammatic(GetFont(), _fontSize - 1f,
                _statusPanelRect.GetComponent<Image>());
            _statusPanel.Bind(_match, _strategicProgram, _tacticalProgram);

            _allPanelRects = new[]
            {
                _stratDebuggerRect, _tactDebuggerRect, _statusPanelRect
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════

        private RectTransform CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvasRect, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private void AddPanelBackground(RectTransform rt)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.04f, 0.06f, 0.04f, 0.92f);
        }

        private TMP_FontAsset GetFont()
        {
            if (_font == null)
            {
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (_font == null)
                    _font = TMP_Settings.defaultFontAsset;
            }
            return _font;
        }
    }
}
