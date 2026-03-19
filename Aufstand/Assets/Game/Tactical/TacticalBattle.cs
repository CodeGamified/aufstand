// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using System.Collections.Generic;
using UnityEngine;
using CodeGamified.Time;

namespace Aufstand.Game.Tactical
{
    /// <summary>
    /// Tactical battle instance — spawns squads, manages combat, determines winner.
    /// Created on-demand when a strategic attack() triggers.
    /// Arena is 80×80 units, with procedural cover based on defender's fortification level.
    /// </summary>
    public class TacticalBattle : MonoBehaviour
    {
        public const float ARENA_SIZE = 80f;
        public const float CAPTURE_RADIUS = 5f;

        private readonly List<Squad> _attackerSquads = new List<Squad>();
        private readonly List<Squad> _defenderSquads = new List<Squad>();
        private CoverSystem _cover;

        // Capture point
        public float CapturePointX { get; private set; }
        public float CapturePointY { get; private set; }
        public float CaptureProgress { get; private set; }  // -1..1 (neg=defender, pos=attacker)
        private const float CAPTURE_SPEED = 0.02f; // per second per capturing squad

        // Artillery
        private readonly List<ArtilleryStrike> _artilleryStrikes = new List<ArtilleryStrike>();
        private float _attackerArtilleryCooldown;
        private float _defenderArtilleryCooldown;
        private int _attackerSupportPoints = 3;
        private int _defenderSupportPoints = 3;

        // Data bus (16 channels shared between all scripts)
        private readonly float[] _dataBus = new float[16];

        // State
        public bool IsActive { get; private set; }
        public float CenterX => CapturePointX;
        public float CenterY => CapturePointY;
        public int AttackerTerritory { get; private set; }
        public int DefenderTerritory { get; private set; }

        public IReadOnlyList<Squad> AttackerSquads => _attackerSquads;
        public IReadOnlyList<Squad> DefenderSquads => _defenderSquads;
        public CoverSystem Cover => _cover;
        public float[] DataBus => _dataBus;

        // Events
        public System.Action<Faction, int, int> OnBattleEnded; // winner, attackerTerr, defenderTerr

        public struct ArtilleryStrike
        {
            public float X, Y, Radius, Delay, Duration;
            public bool IsSmoke;
        }

        public IReadOnlyList<ArtilleryStrike> ActiveStrikes => _artilleryStrikes;

        public void Initialize(int[] attackerArmy, int[] defenderArmy,
                               int fortLevel, int attackerTerritory, int defenderTerritory)
        {
            AttackerTerritory = attackerTerritory;
            DefenderTerritory = defenderTerritory;
            IsActive = true;

            // Capture point at center
            CapturePointX = 0f;
            CapturePointY = 0f;
            CaptureProgress = -0.5f; // Defender starts with advantage

            // Create cover
            var coverGo = new GameObject("CoverSystem");
            coverGo.transform.SetParent(transform, false);
            _cover = coverGo.AddComponent<CoverSystem>();
            _cover.Initialize(ARENA_SIZE, fortLevel);

            // Spawn attacker squads (bottom of arena)
            SpawnSquads(_attackerSquads, attackerArmy, Faction.Player,
                -ARENA_SIZE * 0.35f, ARENA_SIZE * 0.35f, -ARENA_SIZE * 0.4f);

            // Spawn defender squads (top of arena, in cover)
            SpawnSquads(_defenderSquads, defenderArmy, Faction.Enemy,
                -ARENA_SIZE * 0.35f, ARENA_SIZE * 0.35f, ARENA_SIZE * 0.3f);

            // Reset data bus
            for (int i = 0; i < 16; i++) _dataBus[i] = 0f;
        }

        private void SpawnSquads(List<Squad> list, int[] composition, Faction faction,
                                 float xMin, float xMax, float yCenter)
        {
            int id = 0;
            for (int typeIdx = 0; typeIdx < composition.Length && typeIdx < 6; typeIdx++)
            {
                for (int j = 0; j < composition[typeIdx]; j++)
                {
                    float x = Random.Range(xMin, xMax);
                    float y = yCenter + Random.Range(-5f, 5f);

                    var go = new GameObject($"{faction}_Squad_{id}");
                    go.transform.SetParent(transform, false);
                    var squad = go.AddComponent<Squad>();
                    squad.Initialize(id, faction, (UnitType)typeIdx, x, y);
                    squad.OnDestroyed += HandleSquadDestroyed;

                    list.Add(squad);
                    id++;
                }
            }
        }

        private void Update()
        {
            if (!IsActive) return;
            if (SimulationTime.Instance == null || SimulationTime.Instance.isPaused) return;

            float dt = Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);

            // Artillery cooldowns
            if (_attackerArtilleryCooldown > 0f)
                _attackerArtilleryCooldown -= dt;
            if (_defenderArtilleryCooldown > 0f)
                _defenderArtilleryCooldown -= dt;

            // Process artillery strikes
            ProcessArtillery(dt);

            // Update cover for all squads
            UpdateSquadCover();

            // Auto-combat based on doctrine (semi-autonomous behavior)
            ProcessAutonomousBehavior(dt);

            // Process capture point
            UpdateCapture(dt);

            // Check battle end
            CheckBattleEnd();
        }

        private void ProcessArtillery(float dt)
        {
            for (int i = _artilleryStrikes.Count - 1; i >= 0; i--)
            {
                var strike = _artilleryStrikes[i];
                strike.Delay -= dt;
                if (strike.Delay > 0f)
                {
                    _artilleryStrikes[i] = strike;
                    continue;
                }

                strike.Duration -= dt;
                _artilleryStrikes[i] = strike;

                if (strike.Duration <= 0f)
                {
                    _artilleryStrikes.RemoveAt(i);
                    continue;
                }

                float rSq = strike.Radius * strike.Radius;
                var allSquads = new List<Squad>();
                allSquads.AddRange(_attackerSquads);
                allSquads.AddRange(_defenderSquads);

                foreach (var squad in allSquads)
                {
                    if (!squad.IsAlive) continue;
                    float dx = squad.PosX - strike.X;
                    float dy = squad.PosY - strike.Y;
                    if (dx * dx + dy * dy <= rSq)
                    {
                        if (strike.IsSmoke)
                            squad.ApplySuppression(0.1f * dt);
                        else
                        {
                            squad.TakeDamage(15f * dt, false, 0.3f * dt);
                        }
                    }
                }
            }
        }

        private void UpdateSquadCover()
        {
            var allSquads = new List<Squad>();
            allSquads.AddRange(_attackerSquads);
            allSquads.AddRange(_defenderSquads);

            foreach (var squad in allSquads)
            {
                if (!squad.IsAlive) continue;
                squad.CurrentCover = _cover.GetCoverAt(squad.PosX, squad.PosY);
            }
        }

        private void ProcessAutonomousBehavior(float dt)
        {
            // Attackers
            foreach (var squad in _attackerSquads)
                ProcessSquadAI(squad, _defenderSquads, dt);
            // Defenders
            foreach (var squad in _defenderSquads)
                ProcessSquadAI(squad, _attackerSquads, dt);
        }

        private void ProcessSquadAI(Squad squad, List<Squad> enemies, float dt)
        {
            if (!squad.IsAlive) return;
            if (squad.Status == SquadStatus.Pinned) return;
            if (squad.Status == SquadStatus.Retreating) return;

            // Find nearest visible enemy
            Squad nearestEnemy = null;
            float nearestDist = float.MaxValue;
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                float dist = squad.DistanceTo(enemy.PosX, enemy.PosY);
                if (dist > squad.GetRange() * 1.5f) continue;
                if (!_cover.HasLineOfSight(squad.PosX, squad.PosY, enemy.PosX, enemy.PosY))
                    continue;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestEnemy = enemy;
                }
            }

            // Doctrine-based behavior
            switch (squad.CurrentDoctrine)
            {
                case Doctrine.Aggressive:
                    if (nearestEnemy != null && nearestDist <= squad.GetRange())
                    {
                        if (squad.TryFire())
                        {
                            float dps = squad.GetDPS() * dt;
                            bool flanking = IsFlanking(squad, nearestEnemy);
                            nearestEnemy.TakeDamage(dps, flanking,
                                UnitStats.Get(squad.Type).SuppressionRate);
                        }
                    }
                    else if (nearestEnemy != null)
                    {
                        // Move toward enemy
                        if (!squad.MoveRequested)
                        {
                            squad.TargetX = nearestEnemy.PosX;
                            squad.TargetY = nearestEnemy.PosY;
                            squad.MoveRequested = true;
                        }
                    }
                    break;

                case Doctrine.Defensive:
                    // Stay in position, fire at enemies in range
                    if (nearestEnemy != null && nearestDist <= squad.GetRange())
                    {
                        if (squad.TryFire())
                        {
                            float dps = squad.GetDPS() * dt;
                            nearestEnemy.TakeDamage(dps, false,
                                UnitStats.Get(squad.Type).SuppressionRate);
                        }
                    }
                    // Seek nearby cover if not in cover
                    else if (squad.CurrentCover == CoverRating.None && !squad.MoveRequested)
                    {
                        var c = _cover.FindNearest(squad.PosX, squad.PosY);
                        if (c.Rating > CoverRating.None)
                        {
                            squad.TargetX = c.X;
                            squad.TargetY = c.Y;
                            squad.MoveRequested = true;
                        }
                    }
                    break;

                case Doctrine.Flank:
                    if (nearestEnemy != null)
                    {
                        // Move perpendicular to enemy
                        float dx = nearestEnemy.PosX - squad.PosX;
                        float dy = nearestEnemy.PosY - squad.PosY;
                        float perpX = -dy;
                        float perpY = dx;
                        float len = Mathf.Sqrt(perpX * perpX + perpY * perpY);
                        if (len > 0.01f)
                        {
                            squad.TargetX = nearestEnemy.PosX + (perpX / len) * 15f;
                            squad.TargetY = nearestEnemy.PosY + (perpY / len) * 15f;
                            squad.MoveRequested = true;
                        }

                        if (nearestDist <= squad.GetRange() && squad.TryFire())
                        {
                            float dps = squad.GetDPS() * dt;
                            bool flanking = IsFlanking(squad, nearestEnemy);
                            nearestEnemy.TakeDamage(dps, flanking,
                                UnitStats.Get(squad.Type).SuppressionRate);
                        }
                    }
                    break;

                case Doctrine.Hold:
                    // Don't move, fire at enemies in range
                    if (nearestEnemy != null && nearestDist <= squad.GetRange())
                    {
                        if (squad.TryFire())
                        {
                            float dps = squad.GetDPS() * dt;
                            nearestEnemy.TakeDamage(dps, false,
                                UnitStats.Get(squad.Type).SuppressionRate);
                        }
                    }
                    break;

                case Doctrine.Retreat:
                    // Move away from enemies toward spawn edge
                    float retreatY = squad.Faction == Faction.Player
                        ? -ARENA_SIZE * 0.5f : ARENA_SIZE * 0.5f;
                    squad.TargetX = squad.PosX;
                    squad.TargetY = retreatY;
                    squad.MoveRequested = true;
                    squad.Status = SquadStatus.Retreating;
                    break;
            }
        }

        /// <summary>Check if attacker is flanking (attacking from side/rear).</summary>
        private bool IsFlanking(Squad attacker, Squad defender)
        {
            // Simple flank check: if attacker is >90° off from defender's facing direction
            // For now, approximate: if attacker is behind the defender's movement vector
            if (!defender.MoveRequested) return false;

            float moveX = defender.TargetX - defender.PosX;
            float moveY = defender.TargetY - defender.PosY;
            float len = Mathf.Sqrt(moveX * moveX + moveY * moveY);
            if (len < 0.1f) return false;

            float atkX = attacker.PosX - defender.PosX;
            float atkY = attacker.PosY - defender.PosY;
            float dot = (moveX * atkX + moveY * atkY) / len;

            return dot < 0f; // Attacker is behind defender
        }

        private void UpdateCapture(float dt)
        {
            int attackersNear = 0;
            int defendersNear = 0;
            float rSq = CAPTURE_RADIUS * CAPTURE_RADIUS;

            foreach (var s in _attackerSquads)
            {
                if (!s.IsAlive) continue;
                if (!UnitStats.Get(s.Type).CanCapture) continue;
                float dx = s.PosX - CapturePointX;
                float dy = s.PosY - CapturePointY;
                if (dx * dx + dy * dy <= rSq) attackersNear++;
            }

            foreach (var s in _defenderSquads)
            {
                if (!s.IsAlive) continue;
                if (!UnitStats.Get(s.Type).CanCapture) continue;
                float dx = s.PosX - CapturePointX;
                float dy = s.PosY - CapturePointY;
                if (dx * dx + dy * dy <= rSq) defendersNear++;
            }

            if (attackersNear > defendersNear)
                CaptureProgress = Mathf.Min(1f, CaptureProgress + CAPTURE_SPEED * attackersNear * dt);
            else if (defendersNear > attackersNear)
                CaptureProgress = Mathf.Max(-1f, CaptureProgress - CAPTURE_SPEED * defendersNear * dt);
        }

        // =================================================================
        // COMMANDS (called by tactical scripts)
        // =================================================================

        public Squad GetAttackerSquad(int id)
        {
            if (id < 0 || id >= _attackerSquads.Count) return null;
            return _attackerSquads[id];
        }

        public Squad GetDefenderSquad(int id)
        {
            if (id < 0 || id >= _defenderSquads.Count) return null;
            return _defenderSquads[id];
        }

        public int GetSquadCount(Faction faction) =>
            faction == Faction.Player ? _attackerSquads.Count : _defenderSquads.Count;

        public Squad GetSquad(Faction faction, int id) =>
            faction == Faction.Player ? GetAttackerSquad(id) : GetDefenderSquad(id);

        public int GetAliveCount(Faction faction)
        {
            int c = 0;
            var list = faction == Faction.Player ? _attackerSquads : _defenderSquads;
            foreach (var s in list)
                if (s.IsAlive) c++;
            return c;
        }

        public int GetEnemyVisibleCount(Faction faction)
        {
            var myList = faction == Faction.Player ? _attackerSquads : _defenderSquads;
            var enemyList = faction == Faction.Player ? _defenderSquads : _attackerSquads;

            int visible = 0;
            foreach (var enemy in enemyList)
            {
                if (!enemy.IsAlive) continue;
                foreach (var mine in myList)
                {
                    if (!mine.IsAlive) continue;
                    if (mine.DistanceTo(enemy.PosX, enemy.PosY) <= mine.GetRange() * 1.5f
                        && _cover.HasLineOfSight(mine.PosX, mine.PosY, enemy.PosX, enemy.PosY))
                    {
                        visible++;
                        break;
                    }
                }
            }
            return visible;
        }

        public Squad GetVisibleEnemy(Faction faction, int index)
        {
            var myList = faction == Faction.Player ? _attackerSquads : _defenderSquads;
            var enemyList = faction == Faction.Player ? _defenderSquads : _attackerSquads;

            int found = 0;
            foreach (var enemy in enemyList)
            {
                if (!enemy.IsAlive) continue;
                bool visible = false;
                foreach (var mine in myList)
                {
                    if (!mine.IsAlive) continue;
                    if (mine.DistanceTo(enemy.PosX, enemy.PosY) <= mine.GetRange() * 1.5f
                        && _cover.HasLineOfSight(mine.PosX, mine.PosY, enemy.PosX, enemy.PosY))
                    {
                        visible = true;
                        break;
                    }
                }
                if (visible)
                {
                    if (found == index) return enemy;
                    found++;
                }
            }
            return null;
        }

        /// <summary>Call artillery barrage at position (costs munitions, 8s delay).</summary>
        public bool CallArtillery(Faction faction, float x, float y)
        {
            ref float cooldown = ref _attackerArtilleryCooldown;
            ref int points = ref _attackerSupportPoints;
            if (faction == Faction.Enemy)
            {
                cooldown = ref _defenderArtilleryCooldown;
                points = ref _defenderSupportPoints;
            }

            if (cooldown > 0f || points <= 0) return false;
            cooldown = 30f; // 30s cooldown
            points--;

            _artilleryStrikes.Add(new ArtilleryStrike
            {
                X = x, Y = y, Radius = 10f, Delay = 8f, Duration = 5f, IsSmoke = false
            });
            return true;
        }

        /// <summary>Call smoke barrage (blocks LOS, 5s delay).</summary>
        public bool CallSmoke(Faction faction, float x, float y)
        {
            ref float cooldown = ref _attackerArtilleryCooldown;
            ref int points = ref _attackerSupportPoints;
            if (faction == Faction.Enemy)
            {
                cooldown = ref _defenderArtilleryCooldown;
                points = ref _defenderSupportPoints;
            }

            if (cooldown > 0f || points <= 0) return false;
            cooldown = 20f;
            points--;

            _artilleryStrikes.Add(new ArtilleryStrike
            {
                X = x, Y = y, Radius = 8f, Delay = 5f, Duration = 15f, IsSmoke = true
            });
            return true;
        }

        public bool IsArtilleryReady(Faction faction) =>
            faction == Faction.Player ? _attackerArtilleryCooldown <= 0f : _defenderArtilleryCooldown <= 0f;

        public int GetSupportPoints(Faction faction) =>
            faction == Faction.Player ? _attackerSupportPoints : _defenderSupportPoints;

        /// <summary>Data bus write.</summary>
        public void SendBus(int channel, float value)
        {
            if (channel >= 0 && channel < 16)
                _dataBus[channel] = value;
        }

        /// <summary>Data bus read.</summary>
        public float RecvBus(int channel)
        {
            return (channel >= 0 && channel < 16) ? _dataBus[channel] : 0f;
        }

        // =================================================================
        // END CONDITIONS
        // =================================================================

        private void HandleSquadDestroyed(Squad squad)
        {
            Debug.Log($"[AUFSTAND] Squad {squad.SquadId} ({squad.Type}) destroyed");
        }

        private void CheckBattleEnd()
        {
            int atkAlive = GetAliveCount(Faction.Player);
            int defAlive = GetAliveCount(Faction.Enemy);

            // Attacker wins: capture point full or all defenders eliminated
            if (CaptureProgress >= 1f || defAlive == 0)
            {
                IsActive = false;
                OnBattleEnded?.Invoke(Faction.Player, AttackerTerritory, DefenderTerritory);
                return;
            }

            // Defender wins: all attackers eliminated or capture point fully defender-controlled
            if (atkAlive == 0 || CaptureProgress <= -1f)
            {
                IsActive = false;
                OnBattleEnded?.Invoke(Faction.Enemy, AttackerTerritory, DefenderTerritory);
            }
        }
    }
}
