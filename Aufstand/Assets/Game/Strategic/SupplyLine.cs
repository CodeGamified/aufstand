// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;

namespace Aufstand.Game.Strategic
{
    /// <summary>
    /// Supply line evaluation — BFS from HQ through owned territories.
    /// Territories must trace an unbroken path to HQ to receive supply.
    /// Cut supply lines → garrisons starve (−morale, −reinforcement, −ammo).
    /// </summary>
    public static class SupplyLine
    {
        /// <summary>
        /// Recalculate supply for all territories owned by this faction.
        /// Uses BFS from HQ. Distance degrades efficiency.
        /// Contested neighbors along the path reduce efficiency further.
        /// </summary>
        public static void Recalculate(HexMap map, Faction faction)
        {
            int hq = map.FindHQ(faction);
            if (hq < 0) return;

            // Reset all supply for this faction
            for (int i = 0; i < map.TerritoryCount; i++)
            {
                var t = map.GetTerritory(i);
                if (t.Owner == faction)
                {
                    t.IsSupplied = false;
                    t.SupplyDistance = -1;
                    t.SupplyEfficiency = 0f;
                    map.SetTerritory(i, t);
                }
            }

            // BFS from HQ
            var queue = new Queue<int>();
            var visited = new HashSet<int>();

            var hqT = map.GetTerritory(hq);
            hqT.IsSupplied = true;
            hqT.SupplyDistance = 0;
            hqT.SupplyEfficiency = 1f;
            map.SetTerritory(hq, hqT);

            queue.Enqueue(hq);
            visited.Add(hq);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                var currentT = map.GetTerritory(current);

                int adjCount = map.GetAdjacentCount(current);
                for (int i = 0; i < adjCount; i++)
                {
                    int neighbor = map.GetAdjacentId(current, i);
                    if (visited.Contains(neighbor)) continue;

                    var neighborT = map.GetTerritory(neighbor);
                    if (neighborT.Owner != faction) continue;

                    visited.Add(neighbor);

                    int dist = currentT.SupplyDistance + 1;

                    // Efficiency degrades with distance and threat
                    float efficiency = currentT.SupplyEfficiency * 0.9f;

                    // Check if this supply route is threatened (neighbor borders enemy)
                    if (map.IsFrontline(neighbor))
                        efficiency *= 0.7f;

                    // Priority boost
                    if (neighborT.SupplyPriority == SupplyPriority.High)
                        efficiency = Mathf.Min(1f, efficiency * 1.2f);
                    else if (neighborT.SupplyPriority == SupplyPriority.Low)
                        efficiency *= 0.8f;

                    neighborT.IsSupplied = true;
                    neighborT.SupplyDistance = dist;
                    neighborT.SupplyEfficiency = Mathf.Max(0.1f, efficiency);
                    map.SetTerritory(neighbor, neighborT);

                    queue.Enqueue(neighbor);
                }
            }
        }

        /// <summary>Is the supply route to this territory threatened?</summary>
        public static bool IsRouteThreatened(HexMap map, int territoryId)
        {
            var t = map.GetTerritory(territoryId);
            if (!t.IsSupplied) return true;
            // A route is threatened if efficiency is below 0.5
            return t.SupplyEfficiency < 0.5f;
        }
    }
}
