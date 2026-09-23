using System.Collections;
using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Visual;
using UnityEngine;

namespace SomnusNet.Units
{
    public class DreamBlob : MonoBehaviour
    {
        const int ShooterHealth = 120;
        const float MinFireInterval = 0.35f;
        const float SpeedUpgradeFactor = 0.85f;
        const float TooSlowFollowUpDelay = 0.22f;
        const float DivineAssistInterval = 5.5f;
        const float DivineAssistSpawnHeight = 4.5f;
        const float MagmaDefenseInterval = 4.2f;
        const int MagmaDefenseDamage = 14;
        const int DamageUpgradeAmount = 5;
        const int CreatorRampDamagePerHit = 3;

        static readonly Color DreamGlowColor = new(0.2f, 0.55f, 0.28f, 0.35f);
        static readonly Color CreatorShotColor = new(0.45f, 0.82f, 0.48f, 0.95f);
        static readonly Color FourOhFourBodyColor = new(0.55f, 0.78f, 0.95f);
        static readonly Color FourOhFourGlowColor = new(0.65f, 0.85f, 1f, 0.35f);
        static readonly Color FourOhFourShotColor = new(0.72f, 0.9f, 1f, 0.95f);
        static readonly Color BlazeGlowColor = new(1f, 0.45f, 0.22f, 0.38f);
        static readonly Color BlazeShotColor = new(1f, 0.55f, 0.2f, 0.95f);
        static readonly Color DealerShotColor = new(0.78f, 0.62f, 1f, 0.95f);
        static readonly Color CheerfulGlowColor = new(0.42f, 0.92f, 0.48f, 0.38f);
        static readonly Color ArchivistGlowColor = new(0.95f, 0.82f, 0.45f, 0.38f);
        static readonly Color ArchivistShotColor = new(0.98f, 0.88f, 0.55f, 0.95f);
        static readonly Color CountdownGlowColor = new(1f, 0.48f, 0.18f, 0.38f);
        static readonly Color CountdownShotColor = new(1f, 0.62f, 0.22f, 0.95f);
        static readonly Color LanternGlowColor = new(0.58f, 0.78f, 1f, 0.38f);

        struct TimedDamageBoost
        {
            public int amount;
            public float expiresAt;
        }

        public const int SpeedUpgradeCost = 40;
        public const int DamageUpgradeCost = 70;

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Transform glowChild;

        BlobDefinition _def;
        int _rangeSize = GridManager.BlobRangeSize;
        int _health;
        float _actionTimer;
        float _divineAssistTimer;
        float _magmaDefenseTimer;
        float _fireIntervalMultiplier = 1f;
        int _bonusDamage;
        Glitch _rampTarget;
        int _rampHits;
        float _idleTimer;
        Sprite _currentBasicSprite;
        int _currentIdleFrameIndex = -1;
        readonly List<TimedDamageBoost> _slimeFriendBoosts = new();
        readonly List<TimedDamageBoost> _resumeBoosts = new();
        float _slimeHugLuckExpiresAt;
        float _loopedSightExpiresAt;
        float _lanternPulseBonusExpiresAt;
        int _lanternPulseBonusAmount;
        float _lanternPulseStrifeExpiresAt;
        int _lanternPulseStrifeBonus;
        bool _strifeExtendedNextPulse;
        int _archivistVolleyCount;
        float _placedAtUnscaledTime;
        string _storyDesignation;
        bool _canPlaceOnWater;

        public BlobKind Kind => _def != null ? BlobKinds.Normalize(_def.kind) : BlobKind.Gatekeeper;
        public bool CanPlaceOnWater => _canPlaceOnWater;
        public int Column { get; private set; }
        public int Lane { get; private set; }
        public float PlacedAtUnscaledTime => _placedAtUnscaledTime;
        public bool IsAlive => _health > 0;
        public string DisplayName => !string.IsNullOrEmpty(_storyDesignation)
            ? _storyDesignation
            : _def != null ? _def.displayName : "Blob";
        public string TypeDisplay => _def != null ? _def.TypeDisplay : "None";

        public bool HasType(string label) => _def != null && _def.HasType(label);
        public bool HasUpgradeChoice => HasSpeedUpgrade || HasDamageUpgrade;
        public bool IsHarmonyType => HasType(HarmonyTypeRules.HarmonyLabel);

        public bool HasSpeedUpgrade { get; private set; }
        public bool HasDamageUpgrade { get; private set; }

        public bool HasStarBomb => Kind == BlobKind.Gatekeeper && HasSpeedUpgrade;
        public bool HasCrescentTrap => Kind == BlobKind.Gatekeeper && HasDamageUpgrade;
        public bool HasTooSlow => Kind == BlobKind.Creator && HasSpeedUpgrade;
        public bool HasPuppeteer => Kind == BlobKind.Creator && HasDamageUpgrade;
        public bool HasSpores => Kind == BlobKind.FourOhFour && HasSpeedUpgrade;
        public bool HasDivineAssist => Kind == BlobKind.FourOhFour && HasDamageUpgrade;
        public bool HasMagmaDefense => Kind == BlobKind.Blaze && HasSpeedUpgrade;
        public bool HasPandas => Kind == BlobKind.Blaze && HasDamageUpgrade;
        public bool HasSecond => Kind == BlobKind.Dealer && HasSpeedUpgrade;
        public bool HasHeartEater => Kind == BlobKind.Dealer && HasDamageUpgrade;
        public bool HasViscous => Kind == BlobKind.Cheerful && HasSpeedUpgrade;
        public bool HasHugs => Kind == BlobKind.Cheerful && HasDamageUpgrade;
        public bool HasRewind => Kind == BlobKind.Archivist && HasSpeedUpgrade;
        public bool HasResume => Kind == BlobKind.Archivist && HasDamageUpgrade;
        public bool HasDanger => Kind == BlobKind.Countdown && HasSpeedUpgrade;
        public bool HasHigher => Kind == BlobKind.Countdown && HasDamageUpgrade;
        public bool HasMind => Kind == BlobKind.Lantern && HasSpeedUpgrade;
        public bool HasHeart => Kind == BlobKind.Lantern && HasDamageUpgrade;

        public bool HasLoopedSight
        {
            get
            {
                if (Kind == BlobKind.Lantern)
                    return true;

                if (!BlobTypeFeatures.Enabled)
                    return false;

                if (LoopedSightRules.HasNaturalLoopedSight(this))
                    return true;

                return Time.time < _loopedSightExpiresAt;
            }
        }

        public int LanternPulseBonusDamage
        {
            get
            {
                if (Time.time >= _lanternPulseBonusExpiresAt)
                    return 0;
                return _lanternPulseBonusAmount;
            }
        }

        public int LanternPulseStrifeBonus
        {
            get
            {
                if (Time.time >= _lanternPulseStrifeExpiresAt)
                    return 0;
                return _lanternPulseStrifeBonus;
            }
        }

        public int EffectiveStrifeLevel => StrifeLevel + LanternPulseStrifeBonus;

        public int CheerfulFriendBoostDamage
        {
            get
            {
                PruneExpiredCheerfulBoosts();
                var total = 0;
                foreach (var stack in _slimeFriendBoosts)
                    total += stack.amount;
                return total;
            }
        }

        public int ResumeBoostDamage
        {
            get
            {
                PruneExpiredResumeBoosts();
                var total = 0;
                foreach (var stack in _resumeBoosts)
                    total += stack.amount;
                return total;
            }
        }

        public int CheerfulHugLuckBonus
        {
            get
            {
                if (Time.time >= _slimeHugLuckExpiresAt)
                    return 0;
                return CheerfulFriendBoostRules.HugsLuckLevelsPerHit;
            }
        }

        public DreamBlob CheerfulBestFriend =>
            Kind == BlobKind.Cheerful ? CheerfulBestFriendSynergy.SelectBestFriend(this) : null;

        public int RangeSize => _rangeSize;

        public string PrimaryUpgradeLabel => Kind switch
        {
            BlobKind.Gatekeeper => "Star Bomb",
            BlobKind.Creator => "Too Slow",
            BlobKind.FourOhFour => "Spores",
            BlobKind.Blaze => "Magma Defense",
            BlobKind.Dealer => "Silver",
            BlobKind.Cheerful => "Viscous",
            BlobKind.Archivist => "Rewind",
            BlobKind.Countdown => "Danger",
            BlobKind.Lantern => "Mind",
            _ => "Attack Speed"
        };

        public string SecondaryUpgradeLabel => Kind switch
        {
            BlobKind.Gatekeeper => "Crescent Trap",
            BlobKind.Creator => "Puppeteer",
            BlobKind.FourOhFour => "Divine Assist",
            BlobKind.Blaze => "Pandas",
            BlobKind.Dealer => "Heart Eater",
            BlobKind.Cheerful => "Hugs",
            BlobKind.Archivist => "Resume",
            BlobKind.Countdown => "Higher",
            BlobKind.Lantern => "Heart",
            _ => "Attack Damage"
        };

        public bool CanBuySpeedUpgrade =>
            !HasUpgradeChoice
            && PonderEconomy.Instance != null
            && PonderEconomy.Instance.CanAfford(SpeedUpgradeCost);

        public bool CanBuyDamageUpgrade =>
            !HasUpgradeChoice
            && PonderEconomy.Instance != null
            && PonderEconomy.Instance.CanAfford(DamageUpgradeCost);
        public bool IsSpeedPathLocked => HasDamageUpgrade;
        public bool IsDamagePathLocked => HasSpeedUpgrade;

        public int CreatorRampHits => Kind == BlobKind.Creator ? _rampHits : 0;

        public int HarmonyBandMateCount
        {
            get
            {
                if (!HarmonyTypeRules.FeatureEnabled || !IsHarmonyType || GridManager.Instance == null)
                    return 0;
                return Mathf.Max(0, GridManager.Instance.GetHarmonyClusterSize(this) - 1);
            }
        }

        public float AttackSpeedBuffMultiplier
        {
            get
            {
                if (Kind == BlobKind.Gatekeeper || !HasSpeedUpgrade) return 1f;
                return 1f / SpeedUpgradeFactor;
            }
        }

        public float AttackDamageBuffMultiplier
        {
            get
            {
                if (Kind == BlobKind.Gatekeeper || !HasDamageUpgrade || _def == null || _def.projectileDamage <= 0)
                    return 1f;
                return (_def.projectileDamage + _bonusDamage) / (float)_def.projectileDamage;
            }
        }

        public float SpeedTypeSynergyMultiplier
        {
            get
            {
                if (!BlobTypeFeatures.Enabled || !HasType(SpeedTypeRules.Label) || GridManager.Instance == null)
                    return 1f;
                var unique = GridManager.Instance.CountUniqueKindsWithType(SpeedTypeRules.Label);
                if (unique <= 0) return 1f;
                var mult = 1f + unique * SpeedTypeRules.AttackSpeedPerUniqueOnField;
                return DreamTeamSynergy.ScaleBondMultiplier(mult, this);
            }
        }

        public float DreamTeamDreamSpeedMultiplier =>
            DreamTeamSynergy.GetAttackSpeedMultiplier(this);

        public int DreamTeamSynergyBonusDamage =>
            DreamTeamSynergy.ScaleBondBonus(DreamTeamSynergy.GetBonusDamage(this), this);

        public float DreamTeamSynergyDamageMultiplier =>
            DreamTeamSynergy.GetBonusDamageMultiplier(this);

        public int BrokenRingsBonusDamage =>
            BrokenRingsSynergy.GetBonusDamage(this);

        public int LuckLevel => SlimySupportSynergy.GetLuckLevel(this) + CheerfulHugLuckBonus;

        public int StrifeLevel => BenchTrioSynergy.GetStrifeLevel(this);

        public int SlimySupportBonusDamage => SlimySupportSynergy.GetBonusDamage(this);

        public int EntertainerBonusDamage => EntertainerSynergy.GetBonusDamage(this);

        public float BrokenRingsSpeedMultiplier
        {
            get
            {
                var interval = BrokenRingsSynergy.GetAttackSpeedIntervalMultiplier(this);
                if (interval <= 0f || interval >= 1f)
                    return 1f;
                return 1f / interval;
            }
        }

        public int DefinitionBaseDamage => _def != null ? _def.projectileDamage : 0;
        public int UpgradeBonusDamage => _bonusDamage;
        public int PlacementCost => _def != null ? _def.ponderCost : 0;

        public float HarmonyBuffMultiplier
        {
            get
            {
                if (!HarmonyTypeRules.FeatureEnabled || !IsHarmonyType || _def == null) return 1f;
                var bonus = HarmonyBonusDamage;
                if (bonus <= 0) return 1f;
                var baseDamage = Mathf.Max(1, _def.projectileDamage + _bonusDamage);
                return (baseDamage + bonus) / (float)baseDamage;
            }
        }

        public float DreamFocusBuffMultiplier
        {
            get
            {
                if (Kind != BlobKind.Creator || _def == null || CreatorRampBonusDamage <= 0) return 1f;
                var baseDamage = Mathf.Max(1, _def.projectileDamage + _bonusDamage);
                return (baseDamage + CreatorRampBonusDamage) / (float)baseDamage;
            }
        }

        public int HarmonyBonusDamage =>
            HarmonyTypeRules.FeatureEnabled && IsHarmonyType
                ? DreamTeamSynergy.ScaleBondBonus(
                    HarmonyBandMateCount * HarmonyTypeRules.DamagePerNearbyBlob, this)
                : 0;

        public int CreatorRampBonusDamage =>
            Kind == BlobKind.Creator ? _rampHits * CreatorRampDamagePerHit : 0;

        public float PermanentAttackSpeedMultiplier
        {
            get
            {
                if (_def == null)
                    return 1f;

                var baseInterval = Mathf.Max(MinFireInterval, _def.fireInterval);
                var current = EffectiveFireInterval;
                if (current <= 0f)
                    return 1f;

                return baseInterval / current;
            }
        }

        float EffectiveFireInterval
        {
            get
            {
                var interval = ComputeFireIntervalExcludingSecond();
                if (HasSecond)
                    interval *= DealerSecondSynergy.GetSecondIntervalMultiplier(this, interval);
                return interval;
            }
        }

        public float ComputeFireIntervalExcludingSecond()
        {
            if (_def == null)
                return MinFireInterval;

            var interval = _def.fireInterval * _fireIntervalMultiplier;
            if (BlobTypeFeatures.Enabled && HasType(SpeedTypeRules.Label))
                interval /= SpeedTypeSynergyMultiplier;
            if (BlobTypeFeatures.Enabled && HasType(DreamTeamTypeRules.Label))
                interval *= DreamTeamSynergy.GetAttackSpeedIntervalMultiplier(this);
            if (BlobTypeFeatures.Enabled && HasType(BrokenRingsTypeRules.Label))
                interval *= BrokenRingsSynergy.GetAttackSpeedIntervalMultiplier(this);
            if (HasResume)
                interval *= ArchivistRules.ResumeIntervalMultiplier;
            if (HasRewind)
                interval *= ArchivistRules.RewindFireIntervalMultiplier;
            interval *= BenchTrioSynergy.GetLanternFieldIntervalMultiplier(this);
            return Mathf.Max(MinFireInterval, interval);
        }

        int BaseProjectileDamage =>
            _def.projectileDamage + _bonusDamage + HarmonyBonusDamage + DreamTeamSynergyBonusDamage +
            BrokenRingsBonusDamage + SlimySupportBonusDamage + EntertainerBonusDamage + CheerfulFriendBoostDamage +
            ResumeBoostDamage + LanternPulseBonusDamage;

        public void Initialize(BlobDefinition def)
        {
            _def = def;
            _canPlaceOnWater = def.canPlaceOnWater;
            var kind = BlobKinds.Normalize(def.kind);
            _rangeSize = def.rangeSize > 0 ? def.rangeSize : GridManager.BlobRangeSize;
            _health = def.maxHealth > 0 ? def.maxHealth : ShooterHealth;
            _rampTarget = null;
            _rampHits = 0;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && kind == BlobKind.Gatekeeper)
            {
                spriteRenderer.color = Color.white;
                _idleTimer = Random.Range(0f, 4f);
                _currentBasicSprite = null;
                _currentIdleFrameIndex = -1;
                UpdateGatekeeperBlobVisual();
            }
            else if (spriteRenderer != null && kind == BlobKind.Creator)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                _idleTimer = Random.Range(0f, 4f);
                _currentBasicSprite = null;
                _currentIdleFrameIndex = -1;
                UpdateDreamBlobVisual();
            }
            else
            {
                if (kind == BlobKind.FourOhFour)
                    ApplyFourOhFourVisual(def.sprite);
                else if (kind == BlobKind.Blaze)
                    ApplyBlazeVisual(def.sprite);
                else if (kind == BlobKind.Dealer)
                    ApplyDealerVisual(def.sprite);
                else if (kind == BlobKind.Cheerful)
                    ApplyCheerfulVisual(def.sprite);
                else if (kind == BlobKind.Archivist)
                    ApplyArchivistVisual(def.sprite);
                else if (kind == BlobKind.Countdown)
                    ApplyCountdownVisual(def.sprite);
                else if (kind == BlobKind.Lantern)
                    ApplyLanternVisual(def.sprite);
                else if (spriteRenderer != null && def.sprite != null)
                    spriteRenderer.sprite = def.sprite;
            }

            if (glowChild != null && kind == BlobKind.Gatekeeper)
            {
                var glowSr = glowChild.GetComponent<SpriteRenderer>();
                if (glowSr != null)
                    glowSr.color = new Color(1f, 1f, 1f, 0.35f);
            }
            else if (glowChild != null && kind == BlobKind.Creator)
            {
                var glowSr = glowChild.GetComponent<SpriteRenderer>();
                if (glowSr != null)
                    glowSr.color = DreamGlowColor;
            }

            ApplyVisualScale();
            _actionTimer = Random.Range(0f, 0.35f);
            _divineAssistTimer = HasDivineAssist ? DivineAssistInterval * 0.5f : 0f;
            _magmaDefenseTimer = HasMagmaDefense ? MagmaDefenseInterval * 0.5f : 0f;
        }

        void ApplyCountdownVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : CountdownBlobSprites.ShopSprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = CountdownGlowColor;
        }

        void ApplyLanternVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : LanternBlobSprites.ShopSprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = LanternGlowColor;
        }

        void ApplyArchivistVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : ArchivistBlobSprites.BodySprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = ArchivistGlowColor;
        }

        void ApplyCheerfulVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : CheerfulBlobSprites.BodySprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = CheerfulGlowColor;
        }

        void ApplyDealerVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : DealerBlobSprites.BodySprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = new Color(0.72f, 0.55f, 0.98f, 0.35f);
        }

        void ApplyBlazeVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : BlazeBlobSprites.BodySprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = BlazeGlowColor;
        }

        void ApplyFourOhFourVisual(Sprite assetSprite)
        {
            var sprite = assetSprite != null ? assetSprite : FourOhFourBlobSprites.BodySprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            if (glowChild == null) return;
            var glowSr = glowChild.GetComponent<SpriteRenderer>();
            if (glowSr == null) return;
            glowSr.sprite = sprite;
            glowSr.color = FourOhFourGlowColor;
        }

        void ApplyVisualScale()
        {
            if (GridManager.Instance == null) return;
            transform.localScale = Vector3.one * GridManager.Instance.BlobVisualScale;
        }

        public void BindGrid(int col, int lane)
        {
            Column = col;
            Lane = lane;
            _placedAtUnscaledTime = Time.unscaledTime;
            if (GetComponent<BlobRangeVisualizer>() == null)
                gameObject.AddComponent<BlobRangeVisualizer>();
        }

        public void SetStoryDesignation(string designation) => _storyDesignation = designation;

        public void ClearStoryDesignation() => _storyDesignation = null;

        public void ApplyResumeBoost(int amount, float durationSeconds)
        {
            if (!IsAlive || durationSeconds <= 0f || amount <= 0)
                return;

            PruneExpiredResumeBoosts();
            _resumeBoosts.Add(new TimedDamageBoost
            {
                amount = amount,
                expiresAt = Time.time + durationSeconds
            });
        }

        public void ApplyCheerfulFriendBoost(int amount, float durationSeconds, int luckLevels = 0)
        {
            if (!IsAlive || durationSeconds <= 0f)
                return;

            if (amount > 0)
            {
                PruneExpiredCheerfulBoosts();
                _slimeFriendBoosts.Add(new TimedDamageBoost
                {
                    amount = amount,
                    expiresAt = Time.time + durationSeconds
                });
            }

            if (luckLevels <= 0)
                return;

            _slimeHugLuckExpiresAt = Time.time + durationSeconds;
        }

        public void ApplyLoopedSight(float durationSeconds)
        {
            if (!IsAlive || durationSeconds <= 0f)
                return;

            _loopedSightExpiresAt = Mathf.Max(_loopedSightExpiresAt, Time.time + durationSeconds);
        }

        public void ApplyLanternPulseDamageBoost(int amount, float durationSeconds)
        {
            if (!IsAlive || durationSeconds <= 0f || amount <= 0)
                return;

            _lanternPulseBonusExpiresAt = Mathf.Max(_lanternPulseBonusExpiresAt, Time.time + durationSeconds);
            _lanternPulseBonusAmount = amount;
        }

        public void ApplyLanternPulseStrifeBoost(int levels, float durationSeconds)
        {
            if (!IsAlive || durationSeconds <= 0f || levels <= 0)
                return;

            _lanternPulseStrifeExpiresAt = Mathf.Max(_lanternPulseStrifeExpiresAt, Time.time + durationSeconds);
            _lanternPulseStrifeBonus = levels;
        }

        void PruneExpiredFriendBoosts()
        {
            var now = Time.time;
            for (var i = _slimeFriendBoosts.Count - 1; i >= 0; i--)
            {
                if (_slimeFriendBoosts[i].expiresAt <= now)
                    _slimeFriendBoosts.RemoveAt(i);
            }
        }

        void PruneExpiredResumeBoosts()
        {
            var now = Time.time;
            for (var i = _resumeBoosts.Count - 1; i >= 0; i--)
            {
                if (_resumeBoosts[i].expiresAt <= now)
                    _resumeBoosts.RemoveAt(i);
            }
        }

        void PruneExpiredCheerfulBoosts()
        {
            PruneExpiredFriendBoosts();
        }

        public bool TryUpgradeAttackSpeed()
        {
            if (!CanBuySpeedUpgrade) return false;
            if (!PonderEconomy.Instance.TrySpend(SpeedUpgradeCost)) return false;
            if (Kind != BlobKind.Gatekeeper && Kind != BlobKind.Creator && Kind != BlobKind.FourOhFour && Kind != BlobKind.Blaze
                && Kind != BlobKind.Dealer && Kind != BlobKind.Cheerful && Kind != BlobKind.Archivist
                && Kind != BlobKind.Countdown)
                _fireIntervalMultiplier *= SpeedUpgradeFactor;
            HasSpeedUpgrade = true;
            if (Kind == BlobKind.Blaze)
                _magmaDefenseTimer = MagmaDefenseInterval * 0.5f;
            return true;
        }

        public bool TryUpgradeAttackDamage()
        {
            if (!CanBuyDamageUpgrade) return false;
            if (!PonderEconomy.Instance.TrySpend(DamageUpgradeCost)) return false;
            if (Kind != BlobKind.Gatekeeper && Kind != BlobKind.Creator && Kind != BlobKind.FourOhFour && Kind != BlobKind.Blaze
                && Kind != BlobKind.Dealer && Kind != BlobKind.Cheerful && Kind != BlobKind.Archivist
                && Kind != BlobKind.Countdown)
                _bonusDamage += DamageUpgradeAmount;
            HasDamageUpgrade = true;
            if (Kind == BlobKind.FourOhFour)
                _divineAssistTimer = DivineAssistInterval * 0.5f;
            return true;
        }

        public void DeleteWithRefund()
        {
            if (!IsAlive) return;

            StargazerRegistry.NotifyDeleted(this);

            var refund = PlacementCost;
            GridManager.Instance?.ClearCell(this);
            _health = 0;
            if (refund > 0 && PonderEconomy.Instance != null)
                PonderEconomy.Instance.Add(refund);
            Destroy(gameObject);
        }

        void Update()
        {
            if (!IsAlive) return;

            if (Kind == BlobKind.Cheerful)
            {
                UpdateCheerfulBehavior();
                return;
            }

            if (Kind == BlobKind.Lantern)
            {
                UpdateLanternBehavior();
                return;
            }

            if (Kind == BlobKind.Countdown)
            {
                UpdateCountdownBehavior();
                return;
            }

            var glitchInRange = FindGlitchInRange();

            if (Kind == BlobKind.Gatekeeper)
                UpdateGatekeeperBlobVisual();
            else if (Kind == BlobKind.Creator)
                UpdateDreamBlobVisual();

            if (HasDivineAssist && glitchInRange != null)
            {
                _divineAssistTimer -= Time.deltaTime;
                if (_divineAssistTimer <= 0f)
                {
                    FireDivineAssist(glitchInRange);
                    _divineAssistTimer = DivineAssistInterval;
                }
            }

            if (HasMagmaDefense)
            {
                _magmaDefenseTimer -= Time.deltaTime;
                if (_magmaDefenseTimer <= 0f)
                {
                    TriggerMagmaDefense();
                    _magmaDefenseTimer = MagmaDefenseInterval;
                }
            }

            _actionTimer -= Time.deltaTime;
            if (_actionTimer > 0f) return;

            if (glitchInRange == null)
            {
                _actionTimer = 0.12f;
                return;
            }

            if (Kind == BlobKind.Creator && _rampTarget != glitchInRange)
            {
                _rampTarget = glitchInRange;
                _rampHits = 0;
            }

            var volleyRampHits = _rampHits;

            if (Kind == BlobKind.Archivist && HasResume &&
                _archivistVolleyCount >= ArchivistRules.ResumeShotsBeforeBuff)
            {
                var resumeTarget = ArchivistResumeSynergy.SelectTarget(this);
                if (resumeTarget != null)
                {
                    FireResumeShot(resumeTarget);
                    _archivistVolleyCount = 0;
                    _actionTimer = EffectiveFireInterval;
                    return;
                }
            }

            FireShot(glitchInRange, volleyRampHits);
            if (Kind == BlobKind.Archivist && HasResume)
                _archivistVolleyCount++;

            TryLuckBonusShot(glitchInRange, volleyRampHits);

            if (HasTooSlow)
                StartCoroutine(FireTooSlowFollowUp(glitchInRange, volleyRampHits));

            if (Kind == BlobKind.Creator)
                _rampHits++;

            _actionTimer = EffectiveFireInterval;
        }

        void UpdateLanternBehavior()
        {
            _actionTimer -= Time.deltaTime;
            if (_actionTimer > 0f)
                return;

            PulseLoopedSight();
            TryStrifeExtendedNextPulse();
            TryLuckBonusPulse();
            _actionTimer = EffectiveFireInterval;
        }

        void PulseLoopedSight(bool consumeStrifeBonus = true)
        {
            var grid = GridManager.Instance;
            if (grid == null)
                return;

            var duration = LanternRules.LoopedSightDurationSeconds;
            if (consumeStrifeBonus && _strifeExtendedNextPulse)
            {
                duration += LanternRules.StrifeDurationBonusSeconds;
                _strifeExtendedNextPulse = false;
            }

            grid.GetBlobRangeBounds(Column, Lane, out var minCol, out var maxCol, out var minRow, out var maxRow,
                LanternRules.PulseRangeSize);

            for (var c = minCol; c <= maxCol; c++)
            for (var r = minRow; r <= maxRow; r++)
            {
                if (!grid.IsInside(c, r))
                    continue;

                var blob = grid.Get(c, r);
                if (blob == null || !blob.IsAlive || !LanternRules.CanReceivePulseBuff(this, blob))
                    continue;

                blob.ApplyLoopedSight(duration);
                var pulseDamage = LanternRules.GetPulseDamageForBlob(this, blob);
                if (pulseDamage > 0)
                    blob.ApplyLanternPulseDamageBoost(pulseDamage, duration);
                if (HasMind && blob != this)
                    blob.ApplyLanternPulseStrifeBoost(LanternRules.MindStrifeBonusLevels, duration);
            }

            LanternPulseEffect.Play(this, LanternRules.PulseRangeSize);
            PulseFeedback();
        }

        void TryStrifeExtendedNextPulse()
        {
            var level = EffectiveStrifeLevel;
            if (level <= 0)
                return;

            var chance = level * StrifeRules.ExtraAttackChancePerLevel;
            if (Random.value >= chance)
                return;

            _strifeExtendedNextPulse = true;
            StrifeSparkleEffect.Play(this);
        }

        void TryLuckBonusPulse()
        {
            var level = LuckLevel;
            if (level <= 0)
                return;

            var chance = level * LuckRules.ExtraAttackChancePerLevel;
            if (Random.value >= chance)
                return;

            PulseLoopedSight(consumeStrifeBonus: false);
            LuckSparkleEffect.Play(this);
        }

        void UpdateCountdownBehavior()
        {
            _actionTimer -= Time.deltaTime;
            if (_actionTimer > 0f)
                return;

            var glitchInRange = FindGlitchInRange();
            var placed = TryPlaceLandmine();
            if (!placed && glitchInRange == null)
            {
                _actionTimer = 0.12f;
                return;
            }

            if (glitchInRange != null)
                TryStrifeBonusShot(glitchInRange);

            TryLuckBonusShot(glitchInRange, 0);

            if (placed)
                PulseFeedback();

            _actionTimer = EffectiveFireInterval;
        }

        bool TryPlaceLandmine()
        {
            if (_def == null || GridManager.Instance == null)
                return false;

            var damage = BaseProjectileDamage;
            return CountdownLandmineField.TryPlaceMine(this, Column, Lane, _rangeSize, damage);
        }

        void TryStrifeBonusShot(Glitch glitch)
        {
            var level = EffectiveStrifeLevel;
            if (level <= 0)
                return;

            var chance = level * StrifeRules.ExtraAttackChancePerLevel;
            if (Random.value >= chance)
                return;

            if (Kind == BlobKind.Cheerful)
            {
                var friend = CheerfulBestFriendSynergy.SelectBestFriend(this);
                if (friend == null || !friend.IsAlive)
                    return;

                var buffAmount = CheerfulFriendBoostRules.ComputeBuffAmount(this);
                friend.ApplyCheerfulFriendBoost(buffAmount, CheerfulFriendBoostRules.BuffDurationSeconds);
                StrifeSparkleEffect.Play(this);
                return;
            }

            if (Kind == BlobKind.Countdown &&
                glitch != null && LoopedModifierRules.CanBlobTarget(glitch, this))
            {
                FireStrifeShot(glitch);
                StrifeSparkleEffect.Play(this);
            }
        }

        void FireStrifeShot(Glitch glitch)
        {
            if (glitch == null || !LoopedModifierRules.CanBlobTarget(glitch, this) || _def == null)
                return;

            var damage = ComputeProjectileDamage(glitch);
            var projGo = new GameObject("StrifeShot");
            projGo.transform.position = transform.position;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;
            projGo.transform.localScale = Vector3.one * ProjectileVisualScale * 0.9f;
            sr.sprite = CreatePulseSprite();
            sr.color = new Color(1f, 0.22f, 0.1f, 0.98f);

            var proj = projGo.AddComponent<Projectile>();
            proj.Launch(_def.projectileSpeed * 1.1f, damage, glitch, 0f, 1f, ProjectileBehavior.StrifeShot, this);
        }

        void UpdateCheerfulBehavior()
        {
            PruneExpiredCheerfulBoosts();

            _actionTimer -= Time.deltaTime;
            if (_actionTimer > 0f)
                return;

            var friend = CheerfulBestFriendSynergy.SelectBestFriend(this);
            if (friend == null)
            {
                _actionTimer = 0.25f;
                return;
            }

            FireFriendBoostShot(friend);
            TryLuckBonusShot(null, 0);
            TryStrifeBonusShot(null);
            _actionTimer = EffectiveFireInterval;
        }

        void FireFriendBoostShot(DreamBlob friend)
        {
            if (friend == null || !friend.IsAlive || _def == null)
                return;

            var buffAmount = CheerfulFriendBoostRules.ComputeBuffAmount(this);
            var projGo = new GameObject("CheerfulCheer");
            projGo.transform.position = transform.position;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;
            projGo.transform.localScale = Vector3.one * ProjectileVisualScale * 0.85f;

            var proj = projGo.AddComponent<Projectile>();
            proj.LaunchAtFriend(_def.projectileSpeed, buffAmount, CheerfulFriendBoostRules.BuffDurationSeconds, friend,
                this);
            PulseFeedback();
        }

        void TryLuckBonusShot(Glitch glitch, int volleyRampHits)
        {
            var level = LuckLevel;
            if (level <= 0)
                return;

            var chance = level * LuckRules.ExtraAttackChancePerLevel;
            if (Random.value >= chance)
                return;

            if (Kind == BlobKind.Countdown)
            {
                if (TryPlaceLandmine())
                    LuckSparkleEffect.Play(this);
                return;
            }

            if (Kind == BlobKind.Cheerful)
            {
                var friend = CheerfulBestFriendSynergy.SelectBestFriend(this);
                if (friend != null && friend.IsAlive)
                {
                    FireFriendBoostShot(friend);
                    LuckSparkleEffect.Play(this);
                }
                return;
            }

            if (glitch != null && LoopedModifierRules.CanBlobTarget(glitch, this))
            {
                FireShot(glitch, volleyRampHits);
                LuckSparkleEffect.Play(this);
            }
        }

        IEnumerator FireTooSlowFollowUp(Glitch glitch, int volleyRampHits)
        {
            var lastPos = glitch != null ? glitch.transform.position : transform.position;
            yield return new WaitForSeconds(TooSlowFollowUpDelay);
            if (!IsAlive) yield break;

            if (glitch != null && LoopedModifierRules.CanBlobTarget(glitch, this))
                FireShot(glitch, volleyRampHits);
            else
                FireTooSlowMissShot(lastPos);
        }

        void FireTooSlowMissShot(Vector3 destination)
        {
            var projGo = new GameObject("ThoughtPulseMiss");
            projGo.transform.position = transform.position;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;
            projGo.transform.localScale = Vector3.one * ProjectileVisualScale * (24f / 32f);
            StartCoroutine(AnimateMissShot(projGo, destination));
        }

        IEnumerator AnimateMissShot(GameObject projGo, Vector3 destination)
        {
            const float speed = 9f;
            var sr = projGo != null ? projGo.GetComponent<SpriteRenderer>() : null;
            var timer = 0f;

            while (projGo != null && Vector3.Distance(projGo.transform.position, destination) > 0.15f)
            {
                timer += Time.deltaTime;
                if (sr != null)
                {
                    var frame = TooSlowShotSprites.GetFrame(timer);
                    if (frame != null)
                    {
                        sr.sprite = frame;
                        sr.color = Color.white;
                    }
                }

                projGo.transform.position = Vector3.MoveTowards(
                    projGo.transform.position, destination, speed * Time.deltaTime);
                FaceProjectileToward(projGo.transform, projGo.transform.position, destination);
                yield return null;
            }

            if (projGo != null)
                Destroy(projGo);
        }

        static void FaceProjectileToward(Transform projectile, Vector3 from, Vector3 to)
        {
            var dir = to - from;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            projectile.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        void FireShot(Glitch glitch, int? dreamRampHitsOverride = null)
        {
            if (!LanternRules.AttacksGlitches(this))
                return;

            var behavior = ResolveProjectileBehavior();
            var color = ResolveProjectileColor(behavior, Kind);

            var damage = ComputeProjectileDamage(glitch, dreamRampHitsOverride);

            var projGo = new GameObject("ThoughtPulse");
            projGo.transform.position = transform.position;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;

            var useGatekeeperAttack = Kind == BlobKind.Gatekeeper && behavior == ProjectileBehavior.Standard;
            var useTooSlow = HasTooSlow && Kind == BlobKind.Creator && behavior == ProjectileBehavior.Standard;
            var usePuppeteer = HasPuppeteer && Kind == BlobKind.Creator && behavior == ProjectileBehavior.Standard;
            var useCreatorShot = Kind == BlobKind.Creator && behavior == ProjectileBehavior.Standard
                && !useTooSlow && !usePuppeteer;

            if (!useGatekeeperAttack && !useTooSlow && !usePuppeteer && !useCreatorShot)
            {
                projGo.transform.localScale = Vector3.one * ProjectileVisualScale;
                ApplyProjectileVisual(sr, Kind, behavior, color);
            }
            else if (useTooSlow || usePuppeteer || useCreatorShot)
            {
                projGo.transform.localScale = Vector3.one * ProjectileVisualScale * (24f / 32f);
            }

            var proj = projGo.AddComponent<Projectile>();
            var speed = _def.projectileSpeed;
            if (HasRewind)
                speed *= ArchivistRules.RewindProjectileSpeedMultiplier;

            var attacker = Kind == BlobKind.Creator || Kind == BlobKind.Blaze || Kind == BlobKind.FourOhFour
                || Kind == BlobKind.Dealer || Kind == BlobKind.Archivist || Kind == BlobKind.Countdown
                ? this
                : null;
            proj.Launch(speed, damage, glitch, _def.slowDuration, _def.slowMultiplier, behavior, attacker,
                useGatekeeperAttackVisual: useGatekeeperAttack, useTooSlowVisual: useTooSlow, usePuppeteerVisual: usePuppeteer,
                useCreatorShotVisual: useCreatorShot);
            PulseFeedback();
        }

        void FireResumeShot(DreamBlob target)
        {
            if (target == null || !target.IsAlive || _def == null)
                return;

            var buffAmount = ArchivistResumeRules.ComputeBuffAmount(this);
            var projGo = new GameObject("ArchivistResume");
            projGo.transform.position = transform.position;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;
            projGo.transform.localScale = Vector3.one * ProjectileVisualScale * 0.85f;
            sr.sprite = CreatePulseSprite();
            sr.color = ArchivistShotColor;

            var proj = projGo.AddComponent<Projectile>();
            proj.LaunchAtFriend(_def.projectileSpeed * 0.9f, buffAmount, ArchivistResumeRules.BuffDurationSeconds,
                target, this, allowViscousBounce: false);
            PulseFeedback();
        }

        public int ComputeDamageAgainst(Glitch glitch) => ComputeProjectileDamage(glitch);

        int ComputeProjectileDamage(Glitch glitch, int? dreamRampHitsOverride = null)
        {
            if (!LanternRules.AttacksGlitches(this))
                return 0;

            var damage = BaseProjectileDamage;
            if (Kind == BlobKind.Creator)
                damage += (dreamRampHitsOverride ?? _rampHits) * CreatorRampDamagePerHit;

            if (glitch != null && GridManager.Instance != null)
            {
                GridManager.Instance.WorldToNearestCell(glitch.transform.position, out var gCol, out var gRow);
                var absentBonus = AbsentHistorySynergy.GetBonusDamage(this, gCol, gRow);
                damage += DreamTeamSynergy.ScaleBondBonus(absentBonus, this);
                damage += DreamTeamSynergy.GetCloseRangeBonusDamage(this, gCol, gRow);
                damage += BrokenRingsSynergy.GetCloseRangeBonusDamage(this, gCol, gRow);
                damage += BurdenedCrownSynergy.GetCloseRangeBonusDamage(this, gCol, gRow);
            }

            damage += BlazePandasSynergy.GetNeighborDamageBonus(this);

            return damage;
        }

        void TriggerMagmaDefense()
        {
            var grid = GridManager.Instance;
            if (grid == null) return;

            var damage = Mathf.Max(1, MagmaDefenseDamage + _bonusDamage / 2);
            grid.ForEachGlitchInBlobBand(Column, Lane, _rangeSize, glitch =>
            {
                if (LoopedModifierRules.CanAreaEffectDamage(glitch, this))
                    glitch.TakeDamage(damage, this);
            });

            SpawnMagmaRingFlash();
        }

        void SpawnMagmaRingFlash()
        {
            var grid = GridManager.Instance;
            if (grid == null) return;

            var root = new GameObject("MagmaDefenseRing");
            root.transform.position = transform.position;
            grid.GetBlobRangeBounds(Column, Lane, out var minCol, out var maxCol, out var minRow, out var maxRow,
                _rangeSize);

            for (var c = minCol; c <= maxCol; c++)
            for (var r = minRow; r <= maxRow; r++)
            {
                if (!grid.IsInside(c, r)) continue;

                var cellGo = new GameObject($"MagmaCell_{c}_{r}");
                cellGo.transform.SetParent(root.transform);
                cellGo.transform.position = grid.CellToWorld(c, r);
                var sr = cellGo.AddComponent<SpriteRenderer>();
                sr.sprite = CreatePulseSprite();
                var onPath = grid.IsPathCell(c, r);
                sr.color = onPath
                    ? new Color(1f, 0.52f, 0.15f, 0.72f)
                    : new Color(1f, 0.42f, 0.12f, 0.55f);
                sr.sortingOrder = onPath ? 9 : 11;
                cellGo.transform.localScale = Vector3.one * (grid.cellSize * 0.85f);
            }

            Destroy(root, 0.45f);
        }

        void FireDivineAssist(Glitch glitch)
        {
            if (glitch == null || !LoopedModifierRules.CanBlobTarget(glitch, this))
                return;

            var damage = ComputeProjectileDamage(glitch);
            var spawnPos = glitch.transform.position + Vector3.up * DivineAssistSpawnHeight;

            var projGo = new GameObject("DivineAssist");
            projGo.transform.position = spawnPos;
            var sr = projGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            projGo.transform.localScale = Vector3.one * ProjectileVisualScale;
            sr.sprite = FourOhFourShotSprites.StandardSprite;
            sr.color = Color.white;

            var proj = projGo.AddComponent<Projectile>();
            proj.Launch(_def.projectileSpeed * 1.35f, damage, glitch, 0f, 1f, ProjectileBehavior.Standard, this);
        }

        ProjectileBehavior ResolveProjectileBehavior()
        {
            if (Kind == BlobKind.Gatekeeper)
            {
                if (HasStarBomb) return ProjectileBehavior.StarBomb;
                if (HasCrescentTrap) return ProjectileBehavior.CrescentTrap;
            }

            if (Kind == BlobKind.FourOhFour && HasSpores)
                return ProjectileBehavior.Spores;

            if (Kind == BlobKind.Dealer)
                return ProjectileBehavior.DealerSplit;

            return ProjectileBehavior.Standard;
        }

        static Color ResolveProjectileColor(ProjectileBehavior behavior, BlobKind kind)
        {
            if (behavior == ProjectileBehavior.StarBomb)
                return new Color(1f, 0.55f, 0.12f, 0.95f);
            if (behavior == ProjectileBehavior.CrescentTrap)
                return new Color(0.55f, 0.82f, 1f, 0.9f);
            if (kind == BlobKind.FourOhFour)
                return FourOhFourShotColor;
            if (kind == BlobKind.Blaze)
                return BlazeShotColor;
            if (kind == BlobKind.Dealer)
                return DealerShotColor;
            if (kind == BlobKind.Archivist)
                return ArchivistShotColor;
            if (kind == BlobKind.Countdown)
                return CountdownShotColor;
            if (kind == BlobKind.Creator)
                return CreatorShotColor;
            return new Color(1f, 0.92f, 0.15f, 0.95f);
        }

        static void ApplyProjectileVisual(SpriteRenderer sr, BlobKind kind, ProjectileBehavior behavior, Color tint)
        {
            var shotSprite = kind switch
            {
                BlobKind.Creator => CreatorShotSprites.Sprite,
                BlobKind.FourOhFour => FourOhFourShotSprites.GetSprite(behavior),
                _ => null
            };

            if (shotSprite != null)
            {
                sr.sprite = shotSprite;
                sr.color = Color.white;
                return;
            }

            sr.sprite = CreatePulseSprite();
            sr.color = tint;
        }

        public static Color GetProjectileTint(BlobKind kind, ProjectileBehavior behavior) =>
            ResolveProjectileColor(behavior, kind);

        public const float ProjectileVisualScale = 0.72f;

        Glitch FindGlitchInRange()
        {
            var grid = GridManager.Instance;
            if (grid == null) return null;

            var corePos = (Vector2)grid.CoreWorldPosition;
            Glitch best = null;
            var bestDistToCore = float.MaxValue;

            foreach (var glitch in GlitchRegistry.All)
            {
                if (glitch == null || !LoopedModifierRules.CanBlobTarget(glitch, this)) continue;

                grid.WorldToNearestCell(glitch.transform.position, out var gCol, out var gRow);
                if (!grid.IsInBlobRange(Column, Lane, gCol, gRow, _rangeSize)) continue;

                var distToCore = Vector2.Distance(corePos, glitch.transform.position);
                if (distToCore < bestDistToCore)
                {
                    bestDistToCore = distToCore;
                    best = glitch;
                }
            }

            return best;
        }

        public void TakeBite(int damage)
        {
            if (!IsAlive) return;
            _health -= damage;
            transform.localScale = Vector3.one * (0.85f + Mathf.PingPong(Time.time * 8f, 0.15f));
            if (_health <= 0)
                Die();
        }

        void Die()
        {
            GridManager.Instance.ClearCell(this);
            Destroy(gameObject, 0.1f);
        }

        void PulseFeedback()
        {
            if (glowChild != null)
                glowChild.localScale = Vector3.one * 1.15f;
        }

        void UpdateGatekeeperBlobVisual()
        {
            if (spriteRenderer == null)
                return;

            _idleTimer += Time.deltaTime;
            var frameIndex = GatekeeperBlobSprites.GetIdleFrameIndex(_idleTimer);
            if (frameIndex == _currentIdleFrameIndex)
                return;

            _currentIdleFrameIndex = frameIndex;
            ApplyGatekeeperSprite(GatekeeperBlobSprites.GetIdleFrame(_idleTimer));
        }

        void UpdateDreamBlobVisual()
        {
            if (spriteRenderer == null)
                return;

            _idleTimer += Time.deltaTime;
            var frameIndex = CreatorBlobSprites.GetIdleFrameIndex(_idleTimer);
            if (frameIndex == _currentIdleFrameIndex)
                return;

            _currentIdleFrameIndex = frameIndex;
            ApplyDreamSprite(CreatorBlobSprites.GetIdleFrame(_idleTimer));
        }

        void ApplyDreamSprite(Sprite sprite)
        {
            ApplyGatekeeperSprite(sprite);
        }

        void ApplyGatekeeperSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                if (_def != null && _def.sprite != null)
                    sprite = _def.sprite;
                else
                    return;
            }

            if (sprite == _currentBasicSprite)
                return;

            _currentBasicSprite = sprite;
            spriteRenderer.sprite = sprite;
        }

        public static Sprite FallbackProjectileSprite => CreatePulseSprite();

        static Sprite _pulseSprite;
        static Sprite CreatePulseSprite()
        {
            if (_pulseSprite != null) return _pulseSprite;
            const int s = 24;
            var tex = new Texture2D(s, s);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), new Vector2(s / 2f, s / 2f));
                tex.SetPixel(x, y, d < s * 0.30f ? new Color(1f, 1f, 1f, 0.95f) : Color.clear);
            }
            tex.Apply();
            _pulseSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 24f);
            return _pulseSprite;
        }
    }
}
