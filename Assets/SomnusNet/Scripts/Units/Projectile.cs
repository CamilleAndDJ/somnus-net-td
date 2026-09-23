using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Visual;
using UnityEngine;

namespace SomnusNet.Units
{
    public enum ProjectileBehavior
    {
        Standard,
        StarBomb,
        CrescentTrap,
        Spores,
        DealerSplit,
        SplitShard,
        FriendBoost,
        StrifeShot
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class Projectile : MonoBehaviour
    {
        const float HitDistance = 0.18f;
        const float StarBombFuseSeconds = 2f;
        const float StarBombFlightFps = 24f;
        const float StarBombAttachedFps = 6f;
        // Match in-flight pulse: 24px @ 24 PPU with ProjectileVisualScale, adjusted for 32px frames.
        const float StarBombVisualScale = DreamBlob.ProjectileVisualScale * (24f / 32f);
        const float StarBombSplashRadius = 1.15f;
        const float StarBombSplashDamageMultiplier = 0.5f;
        const float CrescentTrapSlowDuration = 3f;
        const float CrescentTrapSlowMultiplier = 0.55f;
        const float CrescentTrapLingerSeconds = 3f;
        const float CrescentTrapFps = 3f;
        const float CrescentTrapFlightFps = 24f;
        const float SporesSlowDuration = 5f;
        const float SporesSlowMultiplier = 0.5f;
        const float SporesSplashRadius = 1.35f;
        const int SporesMaxExtraTargets = 3;
        const int DealerSplitCount = 4;
        const float DealerSplitDamageMultiplier = 0.45f;
        const float SplitShardMaxTravel = 2f;
        const float DealerSplitFanHalfSpread = 42f;
        const float GatekeeperAttackFps = 12f;
        const int StarBombAttachedSortOrder = 13;
        const int CrescentTrapSortOrder = 14;

        int _damage;
        float _speed;
        float _slowDuration;
        float _slowMultiplier;
        Glitch _target;
        DreamBlob _attacker;
        ProjectileBehavior _behavior;
        bool _stuck;
        bool _useGatekeeperAttackVisual;
        bool _useTooSlowVisual;
        bool _usePuppeteerVisual;
        bool _useCreatorShotVisual;
        float _stickTimer;
        float _flightTimer;
        Vector2 _direction;
        float _traveled;
        float _maxTravel;
        Glitch _hitGlitch;
        DreamBlob _friendTarget;
        int _friendBuffAmount;
        float _friendBuffDuration;
        bool _allowViscousBounce;

        public void Launch(float speed, int damage, Glitch initialTarget,
            float slowDuration = 0f, float slowMultiplier = 1f,
            ProjectileBehavior behavior = ProjectileBehavior.Standard,
            DreamBlob attacker = null, bool useGatekeeperAttackVisual = false, bool useTooSlowVisual = false,
            bool usePuppeteerVisual = false, bool useCreatorShotVisual = false)
        {
            _speed = speed;
            _damage = damage;
            _target = initialTarget;
            _slowDuration = slowDuration;
            _slowMultiplier = slowMultiplier;
            _behavior = behavior;
            _attacker = attacker;
            _useGatekeeperAttackVisual = useGatekeeperAttackVisual;
            _useTooSlowVisual = useTooSlowVisual;
            _usePuppeteerVisual = usePuppeteerVisual;
            _useCreatorShotVisual = useCreatorShotVisual;
            _stuck = false;
            _stickTimer = 0f;
            _flightTimer = 0f;

            if (_useGatekeeperAttackVisual)
                ApplyGatekeeperAttackVisual();
            else if (_useTooSlowVisual)
                ApplyTooSlowVisual();
            else if (_usePuppeteerVisual)
                ApplyPuppeteerVisual();
            else if (_useCreatorShotVisual)
                ApplyCreatorShotVisual();
            else if (_behavior == ProjectileBehavior.StarBomb)
                ApplyStarBombFlightVisual();
            else if (_behavior == ProjectileBehavior.CrescentTrap)
                ApplyCrescentTrapFlightVisual();
            else
                EnsureStandardFlightVisual();

            if (_target != null)
                FaceTarget(transform.position, _target.transform.position);
        }

        public void LaunchDirection(float speed, int damage, Vector2 direction, DreamBlob attacker,
            Glitch excludeGlitch = null)
        {
            _speed = speed;
            _damage = damage;
            _target = null;
            _attacker = attacker;
            _behavior = ProjectileBehavior.SplitShard;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _traveled = 0f;
            _maxTravel = SplitShardMaxTravel;
            _hitGlitch = excludeGlitch;
            _stuck = false;
            _slowDuration = 0f;
            _slowMultiplier = 1f;
            _useGatekeeperAttackVisual = false;
            _useTooSlowVisual = false;
            _usePuppeteerVisual = false;
            _useCreatorShotVisual = false;

            EnsureStandardFlightVisual();
            FaceDirection(_direction);
        }

        public void LaunchAtFriend(float speed, int buffAmount, float buffDuration, DreamBlob friend, DreamBlob attacker,
            bool allowViscousBounce = true)
        {
            _speed = speed;
            _damage = 0;
            _friendTarget = friend;
            _friendBuffAmount = buffAmount;
            _friendBuffDuration = buffDuration;
            _attacker = attacker;
            _allowViscousBounce = allowViscousBounce;
            _behavior = ProjectileBehavior.FriendBoost;
            _target = null;
            _stuck = false;
            _slowDuration = 0f;
            _slowMultiplier = 1f;
            _useGatekeeperAttackVisual = false;
            _useTooSlowVisual = false;
            _usePuppeteerVisual = false;
            _useCreatorShotVisual = false;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (attacker != null && attacker.Kind == BlobKind.Archivist)
                {
                    sr.sprite = GatekeeperAttackSprites.GetFrame(0f, 12f);
                    sr.color = new Color(0.98f, 0.88f, 0.55f, 0.95f);
                }
                else
                {
                    sr.sprite = CheerfulHeartSprites.ProjectileSprite;
                    sr.color = new Color(0.38f, 1f, 0.48f, 1f);
                }
            }

            if (_friendTarget != null)
                FaceTarget(transform.position, _friendTarget.transform.position);
        }

        void FaceDirection(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        void EnsureStandardFlightVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite != null)
                return;

            var tint = _attacker != null
                ? DreamBlob.GetProjectileTint(_attacker.Kind, _behavior)
                : _behavior == ProjectileBehavior.Spores
                    ? new Color(0.82f, 0.95f, 1f, 0.95f)
                    : new Color(1f, 0.92f, 0.15f, 0.95f);
            ApplySheetOrFallback(null, 12, tint);
        }

        void Update()
        {
            if (_behavior == ProjectileBehavior.FriendBoost)
            {
                UpdateFriendBoost();
                return;
            }

            if (_behavior == ProjectileBehavior.SplitShard)
            {
                UpdateSplitShard();
                return;
            }

            if (_stuck)
            {
                UpdateStuck();
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            if (!_target.IsTargetable && !_target.IsPuppet)
            {
                Destroy(gameObject);
                return;
            }

            if (!_target.IsPuppet && !CanDamageGlitch(_target))
            {
                Destroy(gameObject);
                return;
            }

            var pos = transform.position;
            var targetPos = _target.transform.position;
            pos = Vector3.MoveTowards(pos, targetPos, _speed * Time.deltaTime);
            transform.position = pos;
            FaceTarget(pos, targetPos);

            if (_useGatekeeperAttackVisual)
            {
                _flightTimer += Time.deltaTime;
                ApplyGatekeeperAttackVisual();
            }
            else if (_useTooSlowVisual)
            {
                _flightTimer += Time.deltaTime;
                ApplyTooSlowVisual();
            }
            else if (_usePuppeteerVisual)
            {
                _flightTimer += Time.deltaTime;
                ApplyPuppeteerVisual();
            }
            else if (_useCreatorShotVisual)
            {
                _flightTimer += Time.deltaTime;
                ApplyCreatorShotVisual();
            }
            else if (_behavior == ProjectileBehavior.StarBomb)
            {
                _flightTimer += Time.deltaTime;
                ApplyStarBombFlightVisual();
            }
            else if (_behavior == ProjectileBehavior.CrescentTrap)
            {
                _flightTimer += Time.deltaTime;
                ApplyCrescentTrapFlightVisual();
            }

            if (Vector3.Distance(pos, targetPos) >= HitDistance) return;

            if (_behavior == ProjectileBehavior.Standard)
                HitStandard();
            else if (_behavior == ProjectileBehavior.StrifeShot)
                HitStrifeShot();
            else if (_behavior == ProjectileBehavior.Spores)
                HitSpores();
            else if (_behavior == ProjectileBehavior.DealerSplit)
                HitDealerSplit();
            else
                StickToTarget();
        }

        void UpdateFriendBoost()
        {
            if (_friendTarget == null || !_friendTarget.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            var pos = transform.position;
            var targetPos = _friendTarget.transform.position;
            pos = Vector3.MoveTowards(pos, targetPos, _speed * Time.deltaTime);
            transform.position = pos;
            FaceTarget(pos, targetPos);

            if (Vector3.Distance(pos, targetPos) >= HitDistance)
                return;

            if (_attacker != null && _attacker.Kind == BlobKind.Archivist)
                _friendTarget.ApplyResumeBoost(_friendBuffAmount, _friendBuffDuration);
            else
            {
                var luckLevels = _attacker != null && _attacker.HasHugs
                    ? CheerfulFriendBoostRules.HugsLuckLevelsPerHit
                    : 0;
                _friendTarget.ApplyCheerfulFriendBoost(_friendBuffAmount, _friendBuffDuration, luckLevels);
            }

            if (_allowViscousBounce && _attacker != null && _attacker.HasViscous &&
                Random.value < CheerfulFriendBoostRules.ViscousBounceChance)
            {
                TrySpawnViscousBounce();
            }

            Destroy(gameObject);
        }

        void TrySpawnViscousBounce()
        {
            if (_attacker == null || _friendTarget == null)
                return;

            var bounceTarget = CheerfulBestFriendSynergy.SelectViscousBounceTarget(_friendTarget, _attacker);
            if (bounceTarget == null)
                return;

            var projGo = new GameObject("CheerfulCheerBounce");
            projGo.transform.position = transform.position;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;
            projGo.transform.localScale = transform.localScale;

            var proj = projGo.AddComponent<Projectile>();
            proj.LaunchAtFriend(_speed, _friendBuffAmount, _friendBuffDuration, bounceTarget, _attacker,
                allowViscousBounce: false);
        }

        void UpdateSplitShard()
        {
            var step = _speed * Time.deltaTime;
            _traveled += step;
            transform.position += (Vector3)(_direction * step);

            if (_traveled >= _maxTravel)
            {
                Destroy(gameObject);
                return;
            }

            foreach (var glitch in GlitchRegistry.All)
            {
                if (glitch == null || glitch == _hitGlitch || !CanAreaEffectDamageGlitch(glitch))
                    continue;
                if (Vector3.Distance(transform.position, glitch.transform.position) > HitDistance)
                    continue;

                glitch.TakeDamage(_damage, _attacker);
                Destroy(gameObject);
                return;
            }
        }

        void HitDealerSplit()
        {
            var hitPos = _target != null ? _target.transform.position : transform.position;
            if (_target != null && CanDamageGlitch(_target))
                _target.TakeDamage(_damage, _attacker);

            var approach = hitPos - transform.position;
            if (approach.sqrMagnitude < 0.0001f && _attacker != null)
                approach = hitPos - _attacker.transform.position;
            if (approach.sqrMagnitude < 0.0001f)
                approach = Vector3.right;

            var splitDamage = Mathf.Max(1, Mathf.RoundToInt(_damage * DealerSplitDamageMultiplier));
            SpawnDealerSplitShots(hitPos, approach, splitDamage);
            Destroy(gameObject);
        }

        void SpawnDealerSplitShots(Vector3 origin, Vector3 approachDir, int splitDamage)
        {
            var approach = new Vector2(approachDir.x, approachDir.y);
            if (approach.sqrMagnitude < 0.0001f)
                approach = Vector2.right;
            else
                approach.Normalize();

            var baseAngle = Mathf.Atan2(approach.y, approach.x) * Mathf.Rad2Deg;
            for (var i = 0; i < DealerSplitCount; i++)
            {
                var t = DealerSplitCount <= 1 ? 0f : i / (float)(DealerSplitCount - 1) * 2f - 1f;
                var angleRad = (baseAngle + t * DealerSplitFanHalfSpread) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                var projGo = new GameObject("DealerSplitShard");
                projGo.transform.position = origin;
                var sr = projGo.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 11;
                projGo.transform.localScale = Vector3.one * DreamBlob.ProjectileVisualScale;

                var proj = projGo.AddComponent<Projectile>();
                proj.LaunchDirection(_speed, splitDamage, dir, _attacker, _target);
            }
        }

        void HitSpores()
        {
            ApplySporesHit(_target);

            var center = _target != null ? (Vector2)_target.transform.position : (Vector2)transform.position;
            var hits = 0;

            foreach (var glitch in GlitchRegistry.All)
            {
                if (hits >= SporesMaxExtraTargets) break;
                if (glitch == null || glitch == _target || !CanDamageGlitch(glitch)) continue;
                if (Vector2.Distance(center, glitch.transform.position) > SporesSplashRadius) continue;
                ApplySporesHit(glitch);
                hits++;
            }

            Destroy(gameObject);
        }

        void ApplySporesHit(Glitch glitch)
        {
            if (!CanDamageGlitch(glitch)) return;
            glitch.TakeDamage(_damage, _attacker);
            glitch.ApplySlow(SporesSlowDuration, SporesSlowMultiplier);
        }

        void HitStandard()
        {
            if (!CanDamageGlitch(_target))
            {
                Destroy(gameObject);
                return;
            }

            _target.TakeDamage(_damage, _attacker);
            if (_slowDuration > 0f)
                _target.ApplySlow(_slowDuration, _slowMultiplier);
            ApplyArchivistHitEffects(_target);
            Destroy(gameObject);
        }

        void HitStrifeShot()
        {
            if (!CanDamageGlitch(_target))
            {
                Destroy(gameObject);
                return;
            }

            _target.TakeDamage(_damage, _attacker);
            _target.ApplyFragility(StrifeRules.FragilityDurationSeconds);
            ApplyArchivistHitEffects(_target);
            Destroy(gameObject);
        }

        void ApplyArchivistHitEffects(Glitch glitch)
        {
            if (glitch == null || _attacker == null || _attacker.Kind != BlobKind.Archivist)
                return;

            if (Random.value < ArchivistRules.FreezeChance)
                glitch.ApplyStop(ArchivistRules.FreezeDuration);

            if (_attacker.HasRewind)
                glitch.PushBackward(ArchivistRules.PushBackwardDistance);
        }

        void StickToTarget()
        {
            _stuck = true;
            transform.position = _target.transform.position;
            transform.rotation = Quaternion.identity;

            if (_behavior == ProjectileBehavior.StarBomb)
            {
                _stickTimer = StarBombFuseSeconds;
                ApplyStarBombAttachedVisual();
                return;
            }

            _target.ApplySlow(CrescentTrapSlowDuration, CrescentTrapSlowMultiplier);
            _stickTimer = CrescentTrapLingerSeconds;
            ApplyCrescentTrapVisual();
        }

        void UpdateStuck()
        {
            if (_target == null || !CanDamageGlitch(_target))
            {
                Destroy(gameObject);
                return;
            }

            transform.position = _target.transform.position;

            if (_behavior == ProjectileBehavior.StarBomb)
                ApplyStarBombAttachedVisual();
            else if (_behavior == ProjectileBehavior.CrescentTrap)
                ApplyCrescentTrapVisual();

            _stickTimer -= Time.deltaTime;
            if (_stickTimer > 0f) return;

            if (_behavior == ProjectileBehavior.StarBomb)
                DetonateStarBomb();

            Destroy(gameObject);
        }

        void DetonateStarBomb()
        {
            if (_target != null && CanDamageGlitch(_target))
                _target.TakeDamage(_damage, _attacker);

            var center = transform.position;
            var splashDamage = Mathf.Max(1, Mathf.RoundToInt(_damage * StarBombSplashDamageMultiplier));

            foreach (var glitch in GlitchRegistry.All)
            {
                if (glitch == null || glitch == _target || !CanDamageGlitch(glitch)) continue;
                if (Vector2.Distance(center, glitch.transform.position) > StarBombSplashRadius) continue;
                glitch.TakeDamage(splashDamage, _attacker);
            }
        }

        bool CanDamageGlitch(Glitch glitch) => LoopedModifierRules.CanBlobTarget(glitch, _attacker);

        bool CanAreaEffectDamageGlitch(Glitch glitch) =>
            LoopedModifierRules.CanAreaEffectDamage(glitch, _attacker);

        void ApplyGatekeeperAttackVisual()
        {
            ApplySheetOrFallback(
                GatekeeperAttackSprites.GetFrame(_flightTimer, GatekeeperAttackFps),
                12,
                new Color(1f, 0.92f, 0.15f, 0.95f));
        }

        void ApplyTooSlowVisual()
        {
            ApplySheetOrFallback(
                TooSlowShotSprites.GetFrame(_flightTimer),
                12,
                DreamBlob.GetProjectileTint(BlobKind.Creator, ProjectileBehavior.Standard));
        }

        void ApplyPuppeteerVisual()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            var frame = PuppeteerSprites.GetFrame(_flightTimer);
            if (frame != null)
            {
                sr.sprite = frame;
                sr.color = Color.white;
                sr.sortingOrder = 12;
                transform.localScale = Vector3.one * DreamBlob.ProjectileVisualScale * (24f / 32f);
                return;
            }

            sr.sprite = DreamBlob.FallbackProjectileSprite;
            sr.color = DreamBlob.GetProjectileTint(BlobKind.Creator, ProjectileBehavior.Standard);
            sr.sortingOrder = 12;
            transform.localScale = Vector3.one * DreamBlob.ProjectileVisualScale;
        }

        void ApplyCreatorShotVisual()
        {
            ApplySheetOrFallback(
                CreatorShotSprites.GetFrame(_flightTimer),
                12,
                DreamBlob.GetProjectileTint(BlobKind.Creator, ProjectileBehavior.Standard));
        }

        void ApplyStarBombFlightVisual()
        {
            ApplySheetOrFallback(
                StarBombFlightSprites.GetFrame(_flightTimer, StarBombFlightFps),
                12,
                new Color(1f, 0.55f, 0.12f, 0.95f));
        }

        void ApplyCrescentTrapFlightVisual()
        {
            ApplySheetOrFallback(
                CrescentTrapFlightSprites.GetFrame(_flightTimer, CrescentTrapFlightFps),
                12,
                new Color(0.55f, 0.82f, 1f, 0.9f));
        }

        void ApplyStarBombAttachedVisual()
        {
            ApplySheetOrFallback(
                StarBombAttachedSprites.GetFrame(StarBombFuseSeconds - _stickTimer, StarBombAttachedFps),
                StarBombAttachedSortOrder,
                new Color(1f, 0.55f, 0.12f, 0.95f));
        }

        void ApplyCrescentTrapVisual()
        {
            var elapsed = CrescentTrapLingerSeconds - _stickTimer;
            ApplySheetOrFallback(
                CrescentTrapSprites.GetFrame(elapsed, CrescentTrapFps),
                CrescentTrapSortOrder,
                new Color(0.55f, 0.82f, 1f, 0.9f));
        }

        void ApplySheetOrFallback(Sprite frame, int sortingOrder, Color fallbackColor)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            if (frame != null)
            {
                sr.sprite = frame;
                sr.color = Color.white;
                sr.sortingOrder = sortingOrder;
                transform.localScale = Vector3.one * StarBombVisualScale;
                return;
            }

            sr.sprite = DreamBlob.FallbackProjectileSprite;
            sr.color = fallbackColor;
            sr.sortingOrder = sortingOrder;
            transform.localScale = Vector3.one * DreamBlob.ProjectileVisualScale;
        }

        void FaceTarget(Vector3 from, Vector3 to)
        {
            var dir = to - from;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            // Sprites are authored facing up (+Y).
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
