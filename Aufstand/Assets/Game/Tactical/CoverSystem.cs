// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;

namespace Aufstand.Game.Tactical
{
    /// <summary>
    /// Cover system for the tactical battlefield. Manages cover objects
    /// (walls, sandbags, craters, buildings) and queries for nearest cover.
    /// </summary>
    public class CoverSystem : MonoBehaviour
    {
        public struct CoverPoint
        {
            public float X, Y;
            public CoverRating Rating;
            public float Radius;  // Effective radius for this cover
        }

        public struct Building
        {
            public float MinX, MaxX, MinY, MaxY;
        }

        private readonly List<CoverPoint> _covers = new List<CoverPoint>();
        private readonly List<Building> _buildings = new List<Building>();

        public IReadOnlyList<CoverPoint> Covers => _covers;
        public IReadOnlyList<Building> Buildings => _buildings;

        public void Initialize(float arenaSize, int fortificationLevel)
        {
            _covers.Clear();
            _buildings.Clear();

            float half = arenaSize / 2f;

            // Generate buildings (2-4 depending on map)
            int buildingCount = Random.Range(2, 5);
            for (int i = 0; i < buildingCount; i++)
            {
                float w = Random.Range(4f, 8f);
                float h = Random.Range(4f, 8f);
                float cx = Random.Range(-half * 0.6f, half * 0.6f);
                float cy = Random.Range(-half * 0.6f, half * 0.6f);

                _buildings.Add(new Building
                {
                    MinX = cx - w / 2f, MaxX = cx + w / 2f,
                    MinY = cy - h / 2f, MaxY = cy + h / 2f
                });

                // Building corners provide Building-level cover
                _covers.Add(new CoverPoint { X = cx, Y = cy, Rating = CoverRating.Building, Radius = w/2f });
            }

            // Walls / sandbags (heavy cover)
            int wallCount = Random.Range(6, 12) + fortificationLevel * 3;
            for (int i = 0; i < wallCount; i++)
            {
                float x = Random.Range(-half * 0.8f, half * 0.8f);
                float y = Random.Range(-half * 0.8f, half * 0.8f);
                _covers.Add(new CoverPoint { X = x, Y = y, Rating = CoverRating.Heavy, Radius = 1.5f });
            }

            // Craters / fences (light cover)
            int craterCount = Random.Range(10, 20);
            for (int i = 0; i < craterCount; i++)
            {
                float x = Random.Range(-half * 0.9f, half * 0.9f);
                float y = Random.Range(-half * 0.9f, half * 0.9f);
                _covers.Add(new CoverPoint { X = x, Y = y, Rating = CoverRating.Light, Radius = 1f });
            }
        }

        /// <summary>Get the best cover rating at a position.</summary>
        public CoverRating GetCoverAt(float x, float y)
        {
            CoverRating best = CoverRating.None;

            // Buildings
            foreach (var bld in _buildings)
            {
                if (x >= bld.MinX && x <= bld.MaxX && y >= bld.MinY && y <= bld.MaxY)
                    return CoverRating.Building;
            }

            // Cover points
            foreach (var cp in _covers)
            {
                float dx = x - cp.X;
                float dy = y - cp.Y;
                if (dx * dx + dy * dy <= cp.Radius * cp.Radius)
                {
                    if (cp.Rating > best)
                        best = cp.Rating;
                }
            }
            return best;
        }

        /// <summary>Find nearest cover point to a position.</summary>
        public CoverPoint FindNearest(float x, float y)
        {
            CoverPoint nearest = default;
            float bestDist = float.MaxValue;

            foreach (var cp in _covers)
            {
                float dx = cp.X - x;
                float dy = cp.Y - y;
                float dist = dx * dx + dy * dy;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = cp;
                }
            }
            return nearest;
        }

        /// <summary>Find nearest cover with at least this rating.</summary>
        public CoverPoint FindNearestMin(float x, float y, CoverRating minRating)
        {
            CoverPoint nearest = default;
            float bestDist = float.MaxValue;

            foreach (var cp in _covers)
            {
                if (cp.Rating < minRating) continue;
                float dx = cp.X - x;
                float dy = cp.Y - y;
                float dist = dx * dx + dy * dy;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = cp;
                }
            }
            return nearest;
        }

        /// <summary>True line-of-sight check between two points (blocked by buildings).</summary>
        public bool HasLineOfSight(float fromX, float fromY, float toX, float toY)
        {
            foreach (var bld in _buildings)
            {
                if (SegmentIntersectsAABB(fromX, fromY, toX, toY,
                    bld.MinX, bld.MinY, bld.MaxX, bld.MaxY))
                    return false;
            }
            return true;
        }

        private static bool SegmentIntersectsAABB(float ax, float ay, float bx, float by,
                                                   float minX, float minY, float maxX, float maxY)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float tMin = 0f, tMax = 1f;

            float[] p = { -dx, dx, -dy, dy };
            float[] q = { ax - minX, maxX - ax, ay - minY, maxY - ay };

            for (int i = 0; i < 4; i++)
            {
                if (Mathf.Abs(p[i]) < 1e-8f)
                {
                    if (q[i] < 0f) return false;
                }
                else
                {
                    float t = q[i] / p[i];
                    if (p[i] < 0f) tMin = Mathf.Max(tMin, t);
                    else tMax = Mathf.Min(tMax, t);
                    if (tMin > tMax) return false;
                }
            }
            return true;
        }
    }
}
