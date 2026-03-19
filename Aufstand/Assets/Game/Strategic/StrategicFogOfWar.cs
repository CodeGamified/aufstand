// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;

namespace Aufstand.Game.Strategic
{
    /// <summary>
    /// Strategic fog of war — enemy territory strength is estimated.
    /// Recon missions refresh intel. Stale intel drifts from reality.
    /// </summary>
    public class StrategicFogOfWar : MonoBehaviour
    {
        private HexMap _map;

        // Intel age per territory (turns since last recon, per faction)
        private int[] _playerIntelAge;
        private int[] _enemyIntelAge;

        // Estimated strength (what the faction believes)
        private float[] _playerEstimates;
        private float[] _enemyEstimates;

        public void Initialize(HexMap map)
        {
            _map = map;
            int count = map.TerritoryCount;
            _playerIntelAge = new int[count];
            _enemyIntelAge = new int[count];
            _playerEstimates = new float[count];
            _enemyEstimates = new float[count];

            // Start with rough estimates
            for (int i = 0; i < count; i++)
            {
                var t = map.GetTerritory(i);
                _playerIntelAge[i] = 5;
                _enemyIntelAge[i] = 5;
                _playerEstimates[i] = t.GarrisonStrength + Random.Range(-2f, 2f);
                _enemyEstimates[i] = t.GarrisonStrength + Random.Range(-2f, 2f);
            }

            // Own territories have perfect intel
            for (int i = 0; i < count; i++)
            {
                var t = map.GetTerritory(i);
                if (t.Owner == Faction.Player)
                {
                    _playerIntelAge[i] = 0;
                    _playerEstimates[i] = t.GarrisonStrength;
                }
                if (t.Owner == Faction.Enemy)
                {
                    _enemyIntelAge[i] = 0;
                    _enemyEstimates[i] = t.GarrisonStrength;
                }
            }
        }

        /// <summary>Get estimated enemy strength from a faction's perspective.</summary>
        public float GetEstimatedStrength(Faction viewer, int territoryId)
        {
            if (territoryId < 0 || territoryId >= _map.TerritoryCount) return 0f;

            var t = _map.GetTerritory(territoryId);
            // Own territories are always known exactly
            if (t.Owner == viewer) return t.GarrisonStrength;

            return viewer == Faction.Player
                ? _playerEstimates[territoryId]
                : _enemyEstimates[territoryId];
        }

        /// <summary>Get intel age (turns since recon) for a territory.</summary>
        public int GetIntelAge(Faction viewer, int territoryId)
        {
            if (territoryId < 0 || territoryId >= _map.TerritoryCount) return 99;

            var t = _map.GetTerritory(territoryId);
            if (t.Owner == viewer) return 0;

            return viewer == Faction.Player
                ? _playerIntelAge[territoryId]
                : _enemyIntelAge[territoryId];
        }

        /// <summary>Refresh intel on a territory (recon mission).</summary>
        public void RefreshIntel(Faction viewer, int territoryId)
        {
            if (territoryId < 0 || territoryId >= _map.TerritoryCount) return;
            var t = _map.GetTerritory(territoryId);

            if (viewer == Faction.Player)
            {
                _playerIntelAge[territoryId] = 0;
                _playerEstimates[territoryId] = t.GarrisonStrength;
            }
            else
            {
                _enemyIntelAge[territoryId] = 0;
                _enemyEstimates[territoryId] = t.GarrisonStrength;
            }
        }

        /// <summary>Age all intel by one turn. Estimates drift from reality.</summary>
        public void AgeTurn()
        {
            for (int i = 0; i < _map.TerritoryCount; i++)
            {
                var t = _map.GetTerritory(i);

                // Don't age own territories
                if (t.Owner != Faction.Player)
                {
                    _playerIntelAge[i]++;
                    // Drift estimate toward reality but add noise
                    float drift = Random.Range(-1f, 1f) * _playerIntelAge[i] * 0.3f;
                    _playerEstimates[i] = Mathf.Max(0f, _playerEstimates[i] + drift);
                }
                else
                {
                    _playerIntelAge[i] = 0;
                    _playerEstimates[i] = t.GarrisonStrength;
                }

                if (t.Owner != Faction.Enemy)
                {
                    _enemyIntelAge[i]++;
                    float drift = Random.Range(-1f, 1f) * _enemyIntelAge[i] * 0.3f;
                    _enemyEstimates[i] = Mathf.Max(0f, _enemyEstimates[i] + drift);
                }
                else
                {
                    _enemyIntelAge[i] = 0;
                    _enemyEstimates[i] = t.GarrisonStrength;
                }

                // Frontline neighbors give partial intel
                if (t.Owner == Faction.Player && _map.IsFrontline(i))
                {
                    int adjCount = _map.GetAdjacentCount(i);
                    for (int j = 0; j < adjCount; j++)
                    {
                        int adj = _map.GetAdjacentId(i, j);
                        if (_map.GetTerritory(adj).Owner == Faction.Enemy && _playerIntelAge[adj] > 2)
                        {
                            _playerIntelAge[adj] = 2;
                            _playerEstimates[adj] = _map.GetTerritory(adj).GarrisonStrength
                                + Random.Range(-1f, 1f);
                        }
                    }
                }
            }
        }

        /// <summary>Front pressure: how threatened a frontline territory is (0-1).</summary>
        public float GetFrontPressure(int territoryId)
        {
            if (territoryId < 0 || territoryId >= _map.TerritoryCount) return 0f;
            var t = _map.GetTerritory(territoryId);

            float enemyStrength = 0f;
            int adjCount = _map.GetAdjacentCount(territoryId);
            for (int i = 0; i < adjCount; i++)
            {
                int adj = _map.GetAdjacentId(territoryId, i);
                var adjT = _map.GetTerritory(adj);
                if (adjT.Owner != t.Owner && adjT.Owner != Faction.Neutral)
                    enemyStrength += adjT.GarrisonStrength;
            }

            float myStrength = Mathf.Max(1f, t.GarrisonStrength);
            return Mathf.Clamp01(enemyStrength / (myStrength + enemyStrength));
        }
    }
}
