using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Visual;
using UnityEngine;

namespace SomnusNet.Units
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Glitch : MonoBehaviour
    {
        const int PonderPerDefeat = PonderEconomyRules.KillReward;

        const float WaypointReachDistance = 0.06f;
        const float PuppetLifetime = 5f;
        const float PuppetContactRadius = 0.55f;
        const int PuppetContactDamage = 12;
        const float PuppetContactCooldown = 0.35f;

        static readonly Color PuppetBodyColor = new(0.62f, 0.95f, 0.58f);
        const float PuppetOverlayYOffset = -0.1f;
        const float PuppetOverlayScale = 1.32f;

        [SerializeField] SpriteRenderer bodyRenderer;
        [SerializeField] SpriteRenderer shellRenderer;
        [SerializeField] SpriteRenderer glowRenderer;

        GlitchDefinition _def;
        int _health;
        int _shell;
        float _slowUntil;
        float _stopUntil;
        float _moveMultiplier = 1f;
        Vector3[] _waypoints;
        int _waypointIndex;

        bool _isPuppet;
        bool _isLooped;
        float _puppetEndTime;
        float _puppetAnimStart;
        bool _puppetDriftReverse;
        int _puppetWaypointIndex;
        float _puppetContactCooldownUntil;
        float _fragilityUntil;
        int _lastMineCol = int.MinValue;
        int _lastMineRow = int.MinValue;
        SpriteRenderer _puppetOverlayRenderer;

        float _dashCooldownTimer;
        float _dashEndTime;
        bool _isDashing;

        public GlitchKind Kind => _def.kind;
        public bool IsMiniBoss => _def != null && MiniBossRules.IsMiniBoss(_def.kind);
        public bool IsRoundBoss => _def != null && CicadianRhythmRules.IsBoss(_def.kind);
        public bool IsPuppet => _isPuppet;
        public bool IsLooped => _isLooped;
        public bool IsTargetable => IsAlive && !IsPuppet;
        public bool IsFragile => !IsPuppet && Time.time < _fragilityUntil;
        public bool IsAlive => _isPuppet || _health > 0 || _shell > 0;

        void Awake()
        {
            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable() => GlitchRegistry.Register(this);

        void OnDestroy() => GlitchRegistry.Unregister(this);

        public void Initialize(GlitchDefinition def, int pathIndex = 0)
        {
            _def = def;
            _health = def.maxHealth;
            _shell = def.shellHealth;
            _isPuppet = false;
            _isLooped = false;
            ClearLoopedOverlay();

            var grid = GridManager.Instance;
            var sourceWaypoints = grid != null ? grid.GetGlitchWaypoints(pathIndex) : null;
            _waypoints = sourceWaypoints is { Count: > 0 }
                ? new Vector3[sourceWaypoints.Count]
                : System.Array.Empty<Vector3>();
            for (var i = 0; i < _waypoints.Length; i++)
                _waypoints[i] = sourceWaypoints[i];

            if (_waypoints.Length > 0)
            {
                transform.position = _waypoints[0];
                _waypointIndex = 1;
            }

            var sprite = def.sprite != null
                ? def.sprite
                : MiniBossVisuals.GetSprite(def.kind) ?? GlitchVisual.FallbackSprite;
            if (bodyRenderer != null)
            {
                bodyRenderer.sprite = sprite;
                bodyRenderer.color = Color.white;
                bodyRenderer.sortingOrder = 10;
            }

            if (glowRenderer != null)
            {
                glowRenderer.sprite = sprite;
                glowRenderer.color = new Color(0.85f, 0.6f, 1f, 0.45f);
                glowRenderer.sortingOrder = 9;
                glowRenderer.transform.localScale = Vector3.one * 1.25f;
            }

            if (shellRenderer != null)
            {
                shellRenderer.enabled = _shell > 0;
                shellRenderer.sortingOrder = 11;
                if (_shell > 0 && def.shellOverlaySprite != null)
                    shellRenderer.sprite = def.shellOverlaySprite;
            }

            ApplyVisualScale();

            if (CicadianRhythmRules.IsBoss(def.kind))
            {
                ApplyLoopedModifier();
                _dashCooldownTimer = CicadianRhythmRules.InitialDashDelay;
                _isDashing = false;
            }
        }

        public void ApplyLoopedModifier()
        {
            if (_isLooped || IsMiniBoss || IsPuppet)
                return;

            _isLooped = true;
            LoopedVisual.Apply(transform);
        }

        void ClearLoopedOverlay()
        {
            var existing = transform.Find("LoopedOverlay");
            if (existing != null)
                Destroy(existing.gameObject);
        }

        void ApplyVisualScale()
        {
            if (GridManager.Instance == null || _def == null) return;
            var scale = GridManager.Instance.GlitchVisualScale;
            if (_def.visualScaleMultiplier > 1.001f)
                scale *= _def.visualScaleMultiplier;
            else if (MiniBossRules.IsMiniBoss(_def.kind))
                scale *= MiniBossRules.DefaultVisualScaleMultiplier;
            else if (CicadianRhythmRules.IsBoss(_def.kind))
                scale *= CicadianRhythmRules.DefaultVisualScaleMultiplier;

            transform.localScale = Vector3.one * scale;
            if (glowRenderer != null)
                glowRenderer.transform.localScale = Vector3.one * 1.15f;
            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                var radiusScale = _def.visualScaleMultiplier > 1.001f
                    ? _def.visualScaleMultiplier
                    : MiniBossRules.IsMiniBoss(_def.kind)
                        ? MiniBossRules.DefaultVisualScaleMultiplier
                        : CicadianRhythmRules.IsBoss(_def.kind)
                            ? CicadianRhythmRules.DefaultVisualScaleMultiplier
                            : 1f;
                col.radius = 0.42f * radiusScale;
            }
        }

        public void ApplySlow(float duration, float speedMultiplier)
        {
            if (IsPuppet) return;
            _slowUntil = Time.time + duration;
            _moveMultiplier = Mathf.Min(_moveMultiplier, speedMultiplier);
        }

        public void ApplyStop(float duration)
        {
            if (IsPuppet || duration <= 0f) return;
            _stopUntil = Mathf.Max(_stopUntil, Time.time + duration);
        }

        public void ApplyFragility(float duration)
        {
            if (IsPuppet || duration <= 0f) return;
            _fragilityUntil = Mathf.Max(_fragilityUntil, Time.time + duration);
            if (bodyRenderer != null && !IsPuppet)
                bodyRenderer.color = new Color(1f, 0.82f, 0.62f);
        }

        public void PushBackward(float distance)
        {
            if (IsPuppet || distance <= 0f || _waypoints == null || _waypoints.Length == 0)
                return;

            var remaining = distance;
            var pos = transform.position;

            while (remaining > 0.001f)
            {
                if (_waypointIndex > 0)
                {
                    var previous = _waypoints[_waypointIndex - 1];
                    var step = Vector3.Distance(pos, previous);
                    if (step <= remaining)
                    {
                        pos = previous;
                        remaining -= step;
                        _waypointIndex--;
                        continue;
                    }

                    pos = Vector3.MoveTowards(pos, previous, remaining);
                    remaining = 0f;
                    break;
                }

                pos.x += remaining;
                remaining = 0f;
            }

            transform.position = pos;
        }

        void Update()
        {
            if (!IsAlive) return;

            if (_isPuppet)
            {
                UpdatePuppet();
                return;
            }

            if (Time.time >= _slowUntil)
                _moveMultiplier = 1f;

            if (Time.time < _stopUntil)
                return;

            if (IsRoundBoss)
                UpdateCicadianDash();

            var speedMultiplier = _moveMultiplier;
            if (_isDashing)
                speedMultiplier *= CicadianRhythmRules.DashSpeedMultiplier;

            MoveForward(_def.moveSpeed * speedMultiplier * Time.deltaTime);
            TryTriggerLandmines();

            if (transform.position.x < GridManager.Instance.CoreLossX)
                ReachCore();
        }

        void UpdateCicadianDash()
        {
            if (_isDashing)
            {
                if (Time.time >= _dashEndTime)
                    _isDashing = false;
                return;
            }

            _dashCooldownTimer -= Time.deltaTime;
            if (_dashCooldownTimer > 0f)
                return;

            _isDashing = true;
            _dashEndTime = Time.time + CicadianRhythmRules.DashDuration;
            _dashCooldownTimer = CicadianRhythmRules.DashInterval;
        }

        void TryTriggerLandmines()
        {
            var grid = GridManager.Instance;
            if (grid == null)
                return;

            grid.WorldToNearestCell(transform.position, out var col, out var row);
            if (col == _lastMineCol && row == _lastMineRow)
                return;

            _lastMineCol = col;
            _lastMineRow = row;

            if (!LoopedModifierRules.CanTriggerLandmines(this))
                return;

            CountdownLandmineField.TryTriggerAt(col, row, this);
        }

        void MoveForward(float step)
        {
            var pos = transform.position;

            if (_waypointIndex < _waypoints.Length)
            {
                var target = _waypoints[_waypointIndex];
                pos = Vector3.MoveTowards(pos, target, step);
                transform.position = pos;

                if (Vector3.Distance(pos, target) <= WaypointReachDistance)
                    _waypointIndex++;
            }
            else
            {
                pos.x -= step;
                transform.position = pos;
            }
        }

        void UpdatePuppet()
        {
            if (Time.time >= _puppetEndTime)
            {
                Destroy(gameObject);
                return;
            }

            UpdatePuppetOverlayVisual();
            MovePuppetReverse(_def.moveSpeed * Time.deltaTime);
            TryPuppetContactDamage();
        }

        void MovePuppetReverse(float step)
        {
            var pos = transform.position;

            if (_puppetDriftReverse)
            {
                pos.x += step;
                transform.position = pos;

                if (_waypoints.Length == 0 ||
                    pos.x >= _waypoints[_waypoints.Length - 1].x - WaypointReachDistance)
                    _puppetDriftReverse = false;
                return;
            }

            if (_puppetWaypointIndex >= 0)
            {
                var target = _waypoints[_puppetWaypointIndex];
                pos = Vector3.MoveTowards(pos, target, step);
                transform.position = pos;

                if (Vector3.Distance(pos, target) <= WaypointReachDistance)
                    _puppetWaypointIndex--;
                return;
            }

            pos.x += step;
            transform.position = pos;
        }

        void TryPuppetContactDamage()
        {
            if (Time.time < _puppetContactCooldownUntil) return;

            var pos = transform.position;
            foreach (var other in GlitchRegistry.All)
            {
                if (other == null || other == this || !other.IsTargetable) continue;
                if (Vector2.SqrMagnitude((Vector2)(other.transform.position - pos)) > PuppetContactRadius * PuppetContactRadius)
                    continue;

                other.TakeDamage(PuppetContactDamage);
                _puppetContactCooldownUntil = Time.time + PuppetContactCooldown;
                return;
            }
        }

        public void TakeDamage(int amount, DreamBlob killer = null)
        {
            if (!IsAlive || IsPuppet) return;

            if (IsRoundBoss)
                amount = CicadianRhythmRules.ScaleIncomingDamage(amount, killer);

            if (IsFragile)
                amount = Mathf.RoundToInt(amount * StrifeRules.DamageMultiplierWhileFragile);

            if (_def.kind == GlitchKind.Faze && Random.value < MiniBossRules.FazeDodgeChance)
            {
                FlashDodge();
                return;
            }

            if (_shell > 0)
            {
                _shell -= amount;
                if (_shell < 0)
                {
                    amount = -_shell;
                    _shell = 0;
                    if (shellRenderer != null) shellRenderer.enabled = false;
                }
                else
                {
                    Flash();
                    return;
                }
            }

            _health -= amount;
            Flash();
            if (_health <= 0)
                Die(killer);
        }

        void Flash()
        {
            if (bodyRenderer != null)
                bodyRenderer.color = IsPuppet ? PuppetBodyColor : _isDashing
                ? new Color(1f, 0.82f, 0.55f)
                : new Color(1f, 0.92f, 1f);
            CancelInvoke(nameof(ResetColor));
            CancelInvoke(nameof(ResetDodgeColor));
            Invoke(nameof(ResetColor), 0.08f);
        }

        void FlashDodge()
        {
            if (bodyRenderer != null)
                bodyRenderer.color = new Color(0.45f, 0.98f, 1f);
            if (glowRenderer != null)
                glowRenderer.color = new Color(0.7f, 1f, 1f, 0.9f);
            CancelInvoke(nameof(ResetColor));
            CancelInvoke(nameof(ResetDodgeColor));
            Invoke(nameof(ResetDodgeColor), 0.14f);
        }

        void ResetDodgeColor()
        {
            if (bodyRenderer == null) return;
            if (IsPuppet)
            {
                bodyRenderer.color = PuppetBodyColor;
                return;
            }

            bodyRenderer.color = ResolveBodyColor();
            if (glowRenderer != null)
                glowRenderer.color = new Color(0.85f, 0.6f, 1f, 0.45f);
        }

        void ResetColor()
        {
            if (bodyRenderer == null) return;
            if (IsPuppet)
            {
                bodyRenderer.color = PuppetBodyColor;
                return;
            }

            bodyRenderer.color = ResolveBodyColor();
            if (glowRenderer != null && !IsInvoking(nameof(ResetDodgeColor)))
                glowRenderer.color = new Color(0.85f, 0.6f, 1f, 0.45f);
        }

        Color ResolveBodyColor()
        {
            if (IsFragile)
                return new Color(1f, 0.82f, 0.62f);
            if (_moveMultiplier < 1f)
                return new Color(0.75f, 0.85f, 1f);
            return Color.white;
        }

        void Die(DreamBlob killer = null)
        {
            if (TryBecomePuppet(killer)) return;

            PonderEconomy.Instance.Add(PonderPerDefeat);
            GameManager.Instance.NotifyGlitchDefeated();
            Destroy(gameObject);
        }

        bool TryBecomePuppet(DreamBlob killer)
        {
            if (killer == null || !killer.IsAlive || killer.Kind != BlobKind.Creator || !killer.HasPuppeteer)
                return false;

            _isPuppet = true;
            _health = 0;
            _shell = 0;
            _slowUntil = 0f;
            _moveMultiplier = 1f;
            _puppetEndTime = Time.time + PuppetLifetime;

            PonderEconomy.Instance.Add(PonderPerDefeat);
            GameManager.Instance.NotifyGlitchDefeated();

            if (shellRenderer != null)
                shellRenderer.enabled = false;

            ApplyPuppetVisual();
            InitPuppetPath();
            return true;
        }

        void ApplyPuppetVisual()
        {
            _puppetAnimStart = Time.time;

            if (bodyRenderer != null)
                bodyRenderer.color = PuppetBodyColor;

            if (glowRenderer != null)
                glowRenderer.enabled = false;

            EnsurePuppetOverlay();
            if (_puppetOverlayRenderer != null)
            {
                _puppetOverlayRenderer.transform.localPosition = new Vector3(0f, PuppetOverlayYOffset, 0f);
                _puppetOverlayRenderer.transform.localScale = Vector3.one * PuppetOverlayScale;
                _puppetOverlayRenderer.enabled = true;
                UpdatePuppetOverlayVisual();
            }
        }

        void EnsurePuppetOverlay()
        {
            if (_puppetOverlayRenderer != null)
                return;

            var overlay = new GameObject("PuppetMindControl");
            overlay.transform.SetParent(transform, false);
            overlay.transform.localPosition = new Vector3(0f, PuppetOverlayYOffset, 0f);
            overlay.transform.localScale = Vector3.one * PuppetOverlayScale;
            _puppetOverlayRenderer = overlay.AddComponent<SpriteRenderer>();
            _puppetOverlayRenderer.sortingOrder = 14;
        }

        void UpdatePuppetOverlayVisual()
        {
            if (_puppetOverlayRenderer == null || !IsPuppet)
                return;

            var frame = PuppetMindControlSprites.GetFrame(Time.time - _puppetAnimStart);
            if (frame == null)
                return;

            _puppetOverlayRenderer.sprite = frame;
            _puppetOverlayRenderer.color = Color.white;
        }

        void InitPuppetPath()
        {
            if (_waypointIndex >= _waypoints.Length)
            {
                _puppetDriftReverse = true;
                _puppetWaypointIndex = _waypoints.Length - 1;
                return;
            }

            _puppetDriftReverse = false;
            _puppetWaypointIndex = Mathf.Max(0, _waypointIndex - 1);
        }

        void ReachCore()
        {
            GameManager.Instance.NotifyGlitchReachedCore();
            Destroy(gameObject);
        }
    }
}
