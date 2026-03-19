// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;

namespace Aufstand.Game.Strategic
{
    /// <summary>
    /// Hex-based theatre map. Generates territories with adjacency,
    /// resource output, and strategic positions. Territories are laid
    /// out in a hex grid on XZ plane.
    /// </summary>
    public class HexMap : MonoBehaviour
    {
        public int TerritoryCount { get; private set; }
        public float MapSize { get; private set; }

        private Territory[] _territories;
        private List<int>[] _adjacency;

        // Hex geometry constants
        public const float HEX_RADIUS = 8f;
        private static readonly float HEX_WIDTH = HEX_RADIUS * 2f;
        private static readonly float HEX_HEIGHT = Mathf.Sqrt(3f) * HEX_RADIUS;

        public void Initialize(int count, float mapSize)
        {
            TerritoryCount = count;
            MapSize = mapSize;
            _territories = new Territory[count];
            _adjacency = new List<int>[count];

            GenerateHexGrid();
            AssignResources();
            AssignInitialOwners();
            BuildGroundPlane();
        }

        public Territory GetTerritory(int id)
        {
            if (id < 0 || id >= TerritoryCount) return default;
            return _territories[id];
        }

        public void SetTerritory(int id, Territory t)
        {
            if (id >= 0 && id < TerritoryCount)
                _territories[id] = t;
        }

        public int GetAdjacentCount(int id)
        {
            if (id < 0 || id >= TerritoryCount) return 0;
            return _adjacency[id]?.Count ?? 0;
        }

        public int GetAdjacentId(int id, int index)
        {
            if (id < 0 || id >= TerritoryCount) return -1;
            var adj = _adjacency[id];
            if (adj == null || index < 0 || index >= adj.Count) return -1;
            return adj[index];
        }

        public bool AreAdjacent(int a, int b)
        {
            if (a < 0 || a >= TerritoryCount) return false;
            return _adjacency[a] != null && _adjacency[a].Contains(b);
        }

        /// <summary>Count territories owned by a faction.</summary>
        public int CountOwned(Faction faction)
        {
            int c = 0;
            for (int i = 0; i < TerritoryCount; i++)
                if (_territories[i].Owner == faction) c++;
            return c;
        }

        /// <summary>Is this territory on the front line (borders enemy)?</summary>
        public bool IsFrontline(int id)
        {
            if (id < 0 || id >= TerritoryCount) return false;
            var owner = _territories[id].Owner;
            if (owner == Faction.Neutral) return false;

            foreach (int adj in _adjacency[id])
            {
                var adjOwner = _territories[adj].Owner;
                if (adjOwner != owner && adjOwner != Faction.Neutral)
                    return true;
            }
            return false;
        }

        /// <summary>Find HQ territory for a faction (highest-value territory farthest from enemy).</summary>
        public int FindHQ(Faction faction)
        {
            int best = -1;
            for (int i = 0; i < TerritoryCount; i++)
            {
                if (_territories[i].Owner == faction)
                {
                    if (best < 0 || _territories[i].IsHQ)
                        best = i;
                }
            }
            return best;
        }

        // =================================================================
        // GENERATION
        // =================================================================

        private void GenerateHexGrid()
        {
            float half = MapSize / 2f;

            // Determine grid dimensions to fit count
            int cols = Mathf.CeilToInt(Mathf.Sqrt(TerritoryCount * 1.2f));
            int rows = Mathf.CeilToInt((float)TerritoryCount / cols);

            int id = 0;
            for (int row = 0; row < rows && id < TerritoryCount; row++)
            {
                for (int col = 0; col < cols && id < TerritoryCount; col++)
                {
                    float x = col * HEX_WIDTH * 0.75f - half + HEX_RADIUS;
                    float z = row * HEX_HEIGHT - half * 0.8f + HEX_HEIGHT * 0.5f;
                    if (col % 2 == 1) z += HEX_HEIGHT * 0.5f;

                    _territories[id] = new Territory
                    {
                        Id = id,
                        CenterX = x,
                        CenterZ = z,
                        Owner = Faction.Neutral,
                        GarrisonStrength = 0,
                        FortificationLevel = 0
                    };

                    _adjacency[id] = new List<int>();
                    id++;
                }
            }

            // Build adjacency by distance
            float maxAdjacentDist = HEX_WIDTH * 1.1f;
            for (int i = 0; i < TerritoryCount; i++)
            {
                for (int j = i + 1; j < TerritoryCount; j++)
                {
                    float dx = _territories[i].CenterX - _territories[j].CenterX;
                    float dz = _territories[i].CenterZ - _territories[j].CenterZ;
                    float dist = Mathf.Sqrt(dx * dx + dz * dz);
                    if (dist < maxAdjacentDist)
                    {
                        _adjacency[i].Add(j);
                        _adjacency[j].Add(i);
                    }
                }
            }
        }

        private void AssignResources()
        {
            for (int i = 0; i < TerritoryCount; i++)
            {
                var t = _territories[i];
                t.FuelOutput = Random.Range(2, 12);
                t.MunitionsOutput = Random.Range(2, 10);
                t.ManpowerOutput = Random.Range(5, 20);
                _territories[i] = t;
            }
        }

        private void AssignInitialOwners()
        {
            // Player owns bottom-left cluster, enemy owns top-right cluster, rest neutral
            int playerCount = TerritoryCount / 4;
            int enemyCount = TerritoryCount / 4;

            // Sort by distance from bottom-left
            int[] sorted = new int[TerritoryCount];
            for (int i = 0; i < TerritoryCount; i++) sorted[i] = i;

            System.Array.Sort(sorted, (a, b) =>
            {
                float da = _territories[a].CenterX + _territories[a].CenterZ;
                float db = _territories[b].CenterX + _territories[b].CenterZ;
                return da.CompareTo(db);
            });

            for (int i = 0; i < playerCount; i++)
            {
                var t = _territories[sorted[i]];
                t.Owner = Faction.Player;
                t.GarrisonStrength = Random.Range(3, 8);
                if (i == 0) t.IsHQ = true;
                _territories[sorted[i]] = t;
            }

            for (int i = 0; i < enemyCount; i++)
            {
                int idx = sorted[TerritoryCount - 1 - i];
                var t = _territories[idx];
                t.Owner = Faction.Enemy;
                t.GarrisonStrength = Random.Range(3, 8);
                if (i == 0) t.IsHQ = true;
                _territories[idx] = t;
            }

            // Neutral territories get small garrisons
            for (int i = 0; i < TerritoryCount; i++)
            {
                if (_territories[i].Owner == Faction.Neutral)
                {
                    var t = _territories[i];
                    t.GarrisonStrength = Random.Range(0, 3);
                    _territories[i] = t;
                }
            }
        }

        private void BuildGroundPlane()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ground.name = "Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ground.transform.localScale = new Vector3(MapSize * 1.2f, MapSize * 1.2f, 1f);

            var col = ground.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var rend = ground.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Sprites/Default"));
                mat.color = new Color(0.08f, 0.10f, 0.06f);
                rend.material = mat;
            }
        }
    }
}
