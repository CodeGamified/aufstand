// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using CodeGamified.Time;

namespace Aufstand.Game.Tactical
{
    /// <summary>
    /// A squad on the tactical battlefield. Has position, HP, ammo,
    /// morale, suppression, and semi-autonomous behavior driven by doctrine.
    /// </summary>
    public class Squad : MonoBehaviour
    {
        // Identity
        public int SquadId { get; private set; }
        public Faction Faction { get; private set; }
        public UnitType Type { get; private set; }
        public bool IsAlive { get; private set; } = true;

        // Position (XZ plane)
        public float PosX;
        public float PosY;  // Z in world space

        // Combat state
        public float HP;
        public float MaxHP;
        public float Ammo;
        public float MaxAmmo;
        public float Morale;        // 0.0-1.0 — low morale = auto-retreat
        public float Suppression;   // 0.0-1.0 — >0.7 = pinned
        public SquadStatus Status;
        public Doctrine CurrentDoctrine;

        // Cover
        public CoverRating CurrentCover;
        public bool IsGarrisoned;    // Inside a building

        // Movement command
        public float TargetX;
        public float TargetY;
        public bool MoveRequested;

        // Targeting
        public int TargetEnemyIdx = -1;

        // Spacing (formation spread)
        public float Spacing = 3f;

        // Internal
        private UnitStats.Stats _stats;
        private float _fireCooldown;
        private float _abilityTimer;

        // Events
        public System.Action<Squad> OnDestroyed;
        public System.Action<Squad> OnRetreated;

        public void Initialize(int id, Faction faction, UnitType type, float x, float y)
        {
            SquadId = id;
            Faction = faction;
            Type = type;
            IsAlive = true;
            PosX = x;
            PosY = y;
            _stats = UnitStats.Get(type);
            HP = _stats.BaseHP;
            MaxHP = _stats.BaseHP;
            Ammo = 100f;
            MaxAmmo = 100f;
            Morale = 1f;
            Suppression = 0f;
            Status = SquadStatus.Idle;
            CurrentDoctrine = Doctrine.Hold;
            CurrentCover = CoverRating.None;
            Spacing = 3f;
            SyncTransform();
        }

        private void Update()
        {
            if (!IsAlive) return;
            if (SimulationTime.Instance == null || SimulationTime.Instance.isPaused) return;

            float dt = Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);

            // Cooldowns
            if (_fireCooldown > 0f)
                _fireCooldown = Mathf.Max(0f, _fireCooldown - dt);

            // Suppression decay
            Suppression = Mathf.Max(0f, Suppression - 0.05f * dt);
            if (Suppression > 0.7f)
                Status = SquadStatus.Pinned;
            else if (Status == SquadStatus.Pinned)
                Status = SquadStatus.Idle;

            // Morale recovery (slow)
            if (Suppression < 0.3f && HP > MaxHP * 0.5f)
                Morale = Mathf.Min(1f, Morale + 0.02f * dt);

            // Auto-retreat on low morale
            if (Morale < 0.2f && Status != SquadStatus.Retreating)
            {
                Status = SquadStatus.Retreating;
                OnRetreated?.Invoke(this);
            }

            // Movement
            if (MoveRequested && Status != SquadStatus.Pinned)
            {
                float speed = _stats.Speed;
                // Suppression slows movement
                speed *= (1f - Suppression * 0.5f);
                // Low morale slows movement
                speed *= (0.5f + Morale * 0.5f);

                float dx = TargetX - PosX;
                float dy = TargetY - PosY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > 0.5f)
                {
                    float step = speed * dt;
                    if (step > dist) step = dist;
                    PosX += (dx / dist) * step;
                    PosY += (dy / dist) * step;
                    Status = SquadStatus.Moving;
                }
                else
                {
                    MoveRequested = false;
                    if (Status == SquadStatus.Moving)
                        Status = SquadStatus.Idle;
                }

                SyncTransform();
            }
        }

        /// <summary>Try to fire at a target position/squad.</summary>
        public bool TryFire()
        {
            if (!IsAlive || _fireCooldown > 0f) return false;
            if (Ammo <= 0f) return false;
            if (Status == SquadStatus.Pinned) return false;

            float ratePerSecond = _stats.DPS / 10f; // approximate
            _fireCooldown = 1f / Mathf.Max(0.1f, ratePerSecond);
            Ammo = Mathf.Max(0f, Ammo - 1f);
            Status = SquadStatus.Engaging;
            return true;
        }

        /// <summary>Take damage from an attack. Flanking ignores cover and deals 1.5x.</summary>
        public void TakeDamage(float damage, bool flanking, float suppressionAmount)
        {
            if (!IsAlive) return;

            // Cover reduction
            float reduction = GetCoverReduction();
            if (flanking) reduction = 0f;

            float actualDamage = damage * (1f - reduction);
            if (flanking) actualDamage *= 1.5f;

            HP -= actualDamage;
            Suppression = Mathf.Min(1f, Suppression + suppressionAmount);
            Morale = Mathf.Max(0f, Morale - actualDamage * 0.005f);

            if (HP <= 0f)
            {
                HP = 0f;
                IsAlive = false;
                Status = SquadStatus.Idle;
                OnDestroyed?.Invoke(this);
            }
        }

        /// <summary>Apply suppression without damage (e.g. nearby explosion).</summary>
        public void ApplySuppression(float amount)
        {
            if (!IsAlive) return;
            Suppression = Mathf.Min(1f, Suppression + amount);
            Morale = Mathf.Max(0f, Morale - amount * 0.1f);
        }

        public float GetCoverReduction()
        {
            switch (CurrentCover)
            {
                case CoverRating.Light:    return 0.25f;
                case CoverRating.Heavy:    return 0.50f;
                case CoverRating.Building: return 0.75f;
                default:                   return 0f;
            }
        }

        public float GetRange() => _stats.Range;
        public float GetDPS() => _stats.DPS * (0.5f + Morale * 0.5f) * (1f - Suppression * 0.5f);

        public float DistanceTo(float x, float y)
        {
            float dx = PosX - x;
            float dy = PosY - y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public void SyncTransform()
        {
            transform.position = new Vector3(PosX, 0.25f, PosY);
        }

        public void ClearMoveCommand() => MoveRequested = false;
    }
}
