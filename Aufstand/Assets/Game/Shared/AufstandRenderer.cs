// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;
using Aufstand.Game;
using Aufstand.Game.Strategic;

namespace Aufstand.Game
{
    /// <summary>
    /// Renderer — builds 3D visuals for the hex map and tactical battles.
    /// Hex tiles are colored by owner, garrison dots show army size.
    /// </summary>
    public class AufstandRenderer : MonoBehaviour
    {
        private HexMap _map;
        private StrategicMatchManager _match;

        private readonly List<GameObject> _hexVisuals = new List<GameObject>();
        private readonly List<GameObject> _garrisonDots = new List<GameObject>();

        private Material _playerMat;
        private Material _enemyMat;
        private Material _neutralMat;
        private Material _hqMat;

        public void Initialize(HexMap map, StrategicMatchManager match)
        {
            _map = map;
            _match = match;

            CreateMaterials();
            BuildHexVisuals();
        }

        private void CreateMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Sprites/Default");

            _playerMat = new Material(shader);
            _playerMat.color = new Color(0.15f, 0.45f, 0.9f);  // Blue
            _playerMat.EnableKeyword("_EMISSION");
            _playerMat.SetColor("_EmissionColor", new Color(0.15f, 0.45f, 0.9f) * 0.3f);

            _enemyMat = new Material(shader);
            _enemyMat.color = new Color(0.9f, 0.2f, 0.15f);    // Red
            _enemyMat.EnableKeyword("_EMISSION");
            _enemyMat.SetColor("_EmissionColor", new Color(0.9f, 0.2f, 0.15f) * 0.3f);

            _neutralMat = new Material(shader);
            _neutralMat.color = new Color(0.35f, 0.35f, 0.3f);  // Grey

            _hqMat = new Material(shader);
            _hqMat.color = new Color(1f, 0.85f, 0.2f);          // Gold
            _hqMat.EnableKeyword("_EMISSION");
            _hqMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 0.5f);
        }

        private void BuildHexVisuals()
        {
            for (int i = 0; i < _map.TerritoryCount; i++)
            {
                var t = _map.GetTerritory(i);

                // Hex tile as flat cylinder
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = $"Hex_{i}";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(t.CenterX, 0.05f, t.CenterZ);
                go.transform.localScale = new Vector3(
                    HexMap.HEX_RADIUS * 1.7f, 0.1f, HexMap.HEX_RADIUS * 1.7f);

                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = GetMaterial(t);

                _hexVisuals.Add(go);

                // Garrison dot (small sphere, size indicates garrison)
                var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.name = $"Garrison_{i}";
                dot.transform.SetParent(go.transform, false);
                dot.transform.localPosition = new Vector3(0f, 1f, 0f);
                float dotScale = Mathf.Clamp(t.GarrisonStrength * 0.05f, 0.1f, 0.5f);
                dot.transform.localScale = new Vector3(dotScale, dotScale, dotScale);

                var dcol = dot.GetComponent<Collider>();
                if (dcol != null) Destroy(dcol);

                var drend = dot.GetComponent<Renderer>();
                if (drend != null)
                    drend.material = GetMaterial(t);

                _garrisonDots.Add(dot);
            }
        }

        private Material GetMaterial(Territory t)
        {
            if (t.IsHQ) return _hqMat;
            switch (t.Owner)
            {
                case Faction.Player: return _playerMat;
                case Faction.Enemy:  return _enemyMat;
                default:             return _neutralMat;
            }
        }

        private void Update()
        {
            // Update visuals each frame to reflect state changes
            for (int i = 0; i < _map.TerritoryCount && i < _hexVisuals.Count; i++)
            {
                var t = _map.GetTerritory(i);
                var rend = _hexVisuals[i].GetComponent<Renderer>();
                if (rend != null)
                    rend.material = GetMaterial(t);

                // Update garrison dot size
                if (i < _garrisonDots.Count)
                {
                    float dotScale = Mathf.Clamp(t.GarrisonStrength * 0.05f, 0.1f, 0.5f);
                    _garrisonDots[i].transform.localScale = new Vector3(dotScale, dotScale, dotScale);

                    var drend = _garrisonDots[i].GetComponent<Renderer>();
                    if (drend != null)
                        drend.material = GetMaterial(t);
                }
            }
        }
    }
}
