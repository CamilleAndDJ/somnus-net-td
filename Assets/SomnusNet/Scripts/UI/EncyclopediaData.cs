using System;
using System.Collections.Generic;
using System.Linq;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Units;
using SomnusNet.Visual;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class EncyclopediaData
    {
        public sealed class TypeEntry
        {
            public string Label;
            public BlobTypeCategory Category;
            public Sprite Icon;
            public Color IconColor;
            public string Title;
            public string Body;
        }

        public sealed class BlobEntry
        {
            public BlobKind Kind;
            public string Name;
            public Sprite Icon;
            public string Body;
            public int SortOrder;
            public BlobUpgradeInfo Upgrades;
        }

        public sealed class BlobUpgradeInfo
        {
            public string BaseShotLabel;
            public BlobProjectilePreviewKind BasePreview;
            public BlobProjectilePreviewKind BaseEffectPreview;
            public string SpeedUpgradeName;
            public string SpeedUpgradeDescription;
            public BlobProjectilePreviewKind SpeedPreview;
            public BlobProjectilePreviewKind SpeedEffectPreview;
            public string DamageUpgradeName;
            public string DamageUpgradeDescription;
            public BlobProjectilePreviewKind DamagePreview;
            public BlobProjectilePreviewKind DamageEffectPreview;
        }

        public sealed class GlitchEntry
        {
            public GlitchKind Kind;
            public string Name;
            public Sprite Icon;
            public string Body;
            public bool IsMiniBoss;
            public bool IsRoundBoss;
        }

        static readonly BlobKind[] BlobSortOrder =
        {
            BlobKind.Gatekeeper,
            BlobKind.Creator,
            BlobKind.FourOhFour,
            BlobKind.Blaze,
            BlobKind.Dealer,
            BlobKind.Cheerful,
            BlobKind.Archivist,
            BlobKind.Countdown,
            BlobKind.Lantern
        };

        static readonly GlitchKind[] EnemyOrder =
        {
            GlitchKind.GlitchMite,
            GlitchKind.LagBeetle,
            GlitchKind.ShellGlitch,
            GlitchKind.PacketSwarm
        };

        static readonly GlitchKind[] MiniBossOrder =
        {
            GlitchKind.Overload,
            GlitchKind.Faze,
            GlitchKind.Sink
        };

        static readonly GlitchKind[] RoundBossOrder =
        {
            GlitchKind.CicadianRhythm
        };

        public static IReadOnlyList<TypeEntry> BuildTypeEntries()
        {
            var entries = new List<TypeEntry>();
            if (!BlobTypeFeatures.Enabled)
                return entries;

            entries.Add(CreateHarmonyType());
            entries.Add(CreateSpeedType());
            entries.Add(CreateAbsentHistoryType());
            entries.Add(CreateEntertainerType());
            entries.Add(CreateDreamTeamType());
            entries.Add(CreateBrokenRingsType());
            entries.Add(CreateSlimySupportType());
            entries.Add(CreateBenchTrioType());
            entries.Add(CreateBurdenedCrownType());

            return entries
                .OrderBy(e => e.Category == BlobTypeCategory.General ? 0 : 1)
                .ThenBy(e => e.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<BlobEntry> BuildBlobEntries(GameCatalog catalog)
        {
            if (catalog == null)
                return Array.Empty<BlobEntry>();

            var entries = new List<BlobEntry>();
            foreach (var def in catalog.blobs)
            {
                if (def == null) continue;
                entries.Add(new BlobEntry
                {
                    Kind = def.kind,
                    Name = def.displayName,
                    Icon = GetBlobIcon(def.kind, def),
                    Body = BuildBlobBody(def),
                    Upgrades = BuildBlobUpgradeInfo(def.kind),
                    SortOrder = BlobSortIndex(def.kind)
                });
            }

            return entries
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<GlitchEntry> BuildGlitchEntries(GameCatalog catalog)
        {
            if (catalog == null)
                return Array.Empty<GlitchEntry>();

            var map = new Dictionary<GlitchKind, GlitchDefinition>();
            foreach (var def in catalog.glitches)
            {
                if (def != null)
                    map[def.kind] = def;
            }

            var entries = new List<GlitchEntry>();
            foreach (var kind in EnemyOrder)
            {
                if (map.TryGetValue(kind, out var def))
                    entries.Add(CreateGlitchEntry(def, false));
            }

            foreach (var kind in MiniBossOrder)
            {
                if (map.TryGetValue(kind, out var def))
                    entries.Add(CreateGlitchEntry(def, isMiniBoss: true));
            }

            foreach (var kind in RoundBossOrder)
            {
                if (map.TryGetValue(kind, out var def))
                    entries.Add(CreateGlitchEntry(def, isRoundBoss: true));
            }

            foreach (var def in catalog.glitches)
            {
                if (def == null) continue;
                if (EnemyOrder.Contains(def.kind) || MiniBossOrder.Contains(def.kind)
                    || RoundBossOrder.Contains(def.kind))
                    continue;
                entries.Add(CreateGlitchEntry(def, MiniBossRules.IsMiniBoss(def.kind)));
            }

            return entries;
        }

        public static Sprite GetTypeIcon(string label)
        {
            if (label == HarmonyTypeRules.HarmonyLabel)
                return HarmonySymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            if (label == SpeedTypeRules.Label)
                return SpeedSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            if (label == DreamTeamTypeRules.Label)
                return DreamTeamSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            if (label == BrokenRingsTypeRules.Label)
                return BrokenRingsSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            if (label == SlimySupportTypeRules.Label)
                return SlimySupportSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            if (label == EntertainerTypeRules.Label)
                return EntertainerSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            if (label == BenchTrioTypeRules.Label)
                return BenchTrioSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            return TypeHudSprites.CreateCircleSprite();
        }

        public static Color GetTypeIconColor(string label) => label switch
        {
            var l when l == HarmonyTypeRules.HarmonyLabel => Color.white,
            var l when l == SpeedTypeRules.Label => Color.white,
            var l when l == DreamTeamTypeRules.Label => Color.white,
            var l when l == BrokenRingsTypeRules.Label => Color.white,
            var l when l == SlimySupportTypeRules.Label => SlimySupportSymbolSprites.IconColor,
            var l when l == EntertainerTypeRules.Label => new Color(1f, 0.88f, 0.42f),
            var l when l == AbsentHistoryTypeRules.Label => new Color(0.55f, 0.78f, 0.98f),
            var l when l == BenchTrioTypeRules.Label => new Color(1f, 0.58f, 0.22f),
            var l when l == BurdenedCrownTypeRules.Label => new Color(0.92f, 0.72f, 0.28f),
            _ => Color.white
        };

        public static Sprite GetBlobIcon(BlobKind kind, BlobDefinition def)
        {
            return kind switch
            {
                BlobKind.Gatekeeper => GatekeeperBlobSprites.ShopSprite,
                BlobKind.Creator => CreatorBlobSprites.ShopSprite,
                BlobKind.FourOhFour => FourOhFourBlobSprites.ShopSprite,
                BlobKind.Blaze => BlazeBlobSprites.ShopSprite,
                BlobKind.Dealer => DealerBlobSprites.ShopSprite,
                BlobKind.Cheerful => CheerfulBlobSprites.ShopSprite,
                BlobKind.Archivist => ArchivistBlobSprites.ShopSprite,
                BlobKind.Countdown => CountdownBlobSprites.ShopSprite,
                BlobKind.Lantern => LanternBlobSprites.ShopSprite,
                _ => def?.sprite
            };
        }

        public static Sprite GetGlitchIcon(GlitchKind kind, GlitchDefinition def)
        {
            if (MiniBossRules.IsMiniBoss(kind) || CicadianRhythmRules.IsBoss(kind))
            {
                var mini = MiniBossVisuals.GetSprite(kind);
                if (mini != null) return mini;
            }

            return def?.sprite;
        }

        static TypeEntry CreateHarmonyType() => new()
        {
            Label = HarmonyTypeRules.HarmonyLabel,
            Category = HarmonyTypeRules.Category,
            Icon = GetTypeIcon(HarmonyTypeRules.HarmonyLabel),
            IconColor = GetTypeIconColor(HarmonyTypeRules.HarmonyLabel),
            Title = TypeDescriptionText.PopupTitle(HarmonyTypeRules.HarmonyLabel, HarmonyTypeRules.Category),
            Body =
                "Same bonus for every Harmony blob in a linked band.\nNon-Harmony blobs count as mates but do not receive the buff.\nBands do not chain through non-Harmony blobs.\n\n" +
                "Band in 3×3. Nearby blobs count as mates; bands link through Harmony only.\n+4 damage per mate — Harmony blobs only."
        };

        static TypeEntry CreateSpeedType() => new()
        {
            Label = SpeedTypeRules.Label,
            Category = SpeedTypeRules.Category,
            Icon = GetTypeIcon(SpeedTypeRules.Label),
            IconColor = GetTypeIconColor(SpeedTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(SpeedTypeRules.Label, SpeedTypeRules.Category),
            Body = TypeDescriptionText.GeneralTypeBody(
                "All Speed-type blobs attack faster based on how many Speed types are on the field.\n+8% attack speed per unique Speed blob on the field.")
        };

        static TypeEntry CreateAbsentHistoryType() => new()
        {
            Label = AbsentHistoryTypeRules.Label,
            Category = AbsentHistoryTypeRules.Category,
            Icon = GetTypeIcon(AbsentHistoryTypeRules.Label),
            IconColor = GetTypeIconColor(AbsentHistoryTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(AbsentHistoryTypeRules.Label, AbsentHistoryTypeRules.Category),
            Body = TypeDescriptionText.GeneralTypeBody(
                "Absent History blobs naturally have Looped Sight — they can target Looped glitches and deal full damage to Cicadian Rhythm.\nFar-ring hits (outside 5×5, inside 7×7) gain +4 damage per unique blob on the field.")
        };

        static TypeEntry CreateDreamTeamType() => new()
        {
            Label = DreamTeamTypeRules.Label,
            Category = DreamTeamTypeRules.Category,
            Icon = GetTypeIcon(DreamTeamTypeRules.Label),
            IconColor = GetTypeIconColor(DreamTeamTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(DreamTeamTypeRules.Label, DreamTeamTypeRules.Category),
            Body = TypeDescriptionText.GroupTypeBody(DreamTeamTypeRules.MemberRoster)
        };

        static TypeEntry CreateBrokenRingsType() => new()
        {
            Label = BrokenRingsTypeRules.Label,
            Category = BrokenRingsTypeRules.Category,
            Icon = GetTypeIcon(BrokenRingsTypeRules.Label),
            IconColor = GetTypeIconColor(BrokenRingsTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(BrokenRingsTypeRules.Label, BrokenRingsTypeRules.Category),
            Body = TypeDescriptionText.GroupTypeBodyFromLines(
                Array.ConvertAll(BrokenRingsTypeRules.MemberRoster, m => m.effectLine))
        };

        static TypeEntry CreateEntertainerType() => new()
        {
            Label = EntertainerTypeRules.Label,
            Category = EntertainerTypeRules.Category,
            Icon = GetTypeIcon(EntertainerTypeRules.Label),
            IconColor = GetTypeIconColor(EntertainerTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(EntertainerTypeRules.Label, EntertainerTypeRules.Category),
            Body = TypeDescriptionText.GeneralTypeBody(
                "Adjacent blobs (including diagonals) gain +3 attack damage when any Entertainer is next to them. Multiple adjacent Entertainers do not stack on the same target. Entertainers can buff each other, but never themselves.")
        };

        static TypeEntry CreateSlimySupportType() => new()
        {
            Label = SlimySupportTypeRules.Label,
            Category = SlimySupportTypeRules.Category,
            Icon = GetTypeIcon(SlimySupportTypeRules.Label),
            IconColor = GetTypeIconColor(SlimySupportTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(SlimySupportTypeRules.Label, SlimySupportTypeRules.Category),
            Body = TypeDescriptionText.GroupTypeBodyFromLines(
                Array.ConvertAll(SlimySupportTypeRules.MemberRoster, m => m.effectLine)) +
                "\n\nLuck: each level grants a 10% chance to attack one extra time."
        };

        static TypeEntry CreateBenchTrioType() => new()
        {
            Label = BenchTrioTypeRules.Label,
            Category = BenchTrioTypeRules.Category,
            Icon = GetTypeIcon(BenchTrioTypeRules.Label),
            IconColor = GetTypeIconColor(BenchTrioTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(BenchTrioTypeRules.Label, BenchTrioTypeRules.Category),
            Body = TypeDescriptionText.GroupTypeBodyFromLines(
                Array.ConvertAll(BenchTrioTypeRules.MemberRoster, m => m.effectLine)) +
                "\n\nStrife: each level grants a 10% chance to fire a fragility shot."
        };

        static TypeEntry CreateBurdenedCrownType() => new()
        {
            Label = BurdenedCrownTypeRules.Label,
            Category = BurdenedCrownTypeRules.Category,
            Icon = GetTypeIcon(BurdenedCrownTypeRules.Label),
            IconColor = GetTypeIconColor(BurdenedCrownTypeRules.Label),
            Title = TypeDescriptionText.PopupTitle(BurdenedCrownTypeRules.Label, BurdenedCrownTypeRules.Category),
            Body = TypeDescriptionText.GeneralTypeBody(
                "+3 close-range damage per unique Burdened Crown blob on the field. Applies on hits inside the blob's inner 3×3 range.")
        };

        static GlitchEntry CreateGlitchEntry(GlitchDefinition def, bool isMiniBoss = false, bool isRoundBoss = false) => new()
        {
            Kind = def.kind,
            Name = def.displayName,
            Icon = GetGlitchIcon(def.kind, def),
            IsMiniBoss = isMiniBoss,
            IsRoundBoss = isRoundBoss,
            Body = BuildGlitchBody(def, isMiniBoss, isRoundBoss)
        };

        static string BuildBlobBody(BlobDefinition def)
        {
            var kind = BlobKinds.Normalize(def.kind);
            var lines = new List<string>
            {
                def.displayName,
                $"Types: {def.TypeDisplay}",
                def.verbDescription,
                $"Cost: {def.ponderCost} Ponders"
            };

            if (kind == BlobKind.Cheerful)
            {
                lines.Add("Does not attack glitches.");
                lines.Add("Cheers an adjacent Best Friend with stacking attack damage buffs.");
                lines.Add($"Cheer interval: {def.fireInterval:0.##}s");
                lines.Add("Best Friend priority: Dealer, then longest on field.");
                return string.Join("\n", lines);
            }

            if (kind == BlobKind.Archivist)
            {
                lines.Add($"Range: {def.rangeSize}×{def.rangeSize}");
                lines.Add($"Shot damage: {def.projectileDamage}");
                lines.Add($"Fire interval: {def.fireInterval:0.##}s");
                lines.Add($"Freeze chance: {ArchivistRules.FreezeChance:P0} for {ArchivistRules.FreezeDuration:0.#}s");
                lines.Add("Broken Rings — grants close-range damage to the roster.");
                lines.Add("Absent History — bonus damage on far-ring hits.");
                return string.Join("\n", lines);
            }

            if (kind == BlobKind.Countdown)
            {
                lines.Add($"Range: {def.rangeSize}×{def.rangeSize}");
                lines.Add("Lays landmines on path tiles in range (up to 3 per tile, prefers empty tiles).");
                lines.Add($"Mine blast: {CountdownRules.BlastRangeSize}×{CountdownRules.BlastRangeSize} (Danger: {CountdownRules.DangerBlastRangeSize}×{CountdownRules.DangerBlastRangeSize}), hits multiple glitches.");
                lines.Add($"Mine damage: {def.projectileDamage}");
                lines.Add($"Lay interval: {def.fireInterval:0.##}s");
                lines.Add($"Stack limit: {CountdownRules.MaxMinesPerTile}/tile (Danger: {CountdownRules.DangerMaxMinesPerTile}/tile).");
                lines.Add("Strife — chance to fire a fragility shot (double damage for a few seconds).");
                lines.Add("Higher — all stacked mines on a tile detonate together.");
                lines.Add("Burdened Crown — +3 close-range dmg per member on field (3×3).");
                return string.Join("\n", lines);
            }

            if (kind == BlobKind.Lantern)
            {
                lines.Add("Does not attack glitches.");
                lines.Add($"Pulse range: {LanternRules.PulseRangeSize}×{LanternRules.PulseRangeSize}");
                lines.Add($"Pulse interval: {def.fireInterval:0.##}s");
                lines.Add("Absent History — always has Looped Sight.");
                lines.Add($"Pulse grants {LoopedSightRules.StatusLabel} for {LanternRules.LoopedSightDurationSeconds:0.#}s to nearby blobs.");
                lines.Add($"Pulse — Bench Trio +{LanternRules.PulseBenchTrioBonusDamage} dmg; Heart adds +{LanternRules.HeartGeneralPulseDamage} to all blobs and +{LanternRules.HeartExtraBenchTrioDamage} more to Bench Trio.");
                lines.Add("Mind — affected blobs gain Strife +1 for the pulse duration.");
                lines.Add($"On field — Bench Trio members gain atk speed x{LanternRules.GetFieldAttackSpeedMultiplier():F2} while Lantern is alive.");
                lines.Add("Strife — chance to extend the next pulse's buff duration.");
                lines.Add("Luck — chance for an extra pulse.");
                lines.Add("Bench Trio + Absent History.");
                return string.Join("\n", lines);
            }

            lines.Add($"Range: {def.rangeSize}×{def.rangeSize}");
            lines.Add($"Shot damage: {def.projectileDamage}");
            lines.Add($"Fire interval: {def.fireInterval:0.##}s");

            if (def.shotsPerVolley > 1)
                lines.Add($"Shots per volley: {def.shotsPerVolley}");
            if (def.slowDuration > 0f)
                lines.Add($"Slow: {def.slowDuration:0.#}s at {def.slowMultiplier:P0} speed");

            return string.Join("\n", lines);
        }

        public static BlobUpgradeInfo BuildBlobUpgradeInfo(BlobKind kind) => kind switch
        {
            BlobKind.Gatekeeper => new BlobUpgradeInfo
            {
                BaseShotLabel = "Gatekeeper Shot",
                BasePreview = BlobProjectilePreviewKind.GatekeeperAttack,
                SpeedUpgradeName = "Star Bomb",
                SpeedUpgradeDescription = "Sticky bomb with a 2s fuse. Explodes for splash damage.",
                SpeedPreview = BlobProjectilePreviewKind.StarBombFlight,
                SpeedEffectPreview = BlobProjectilePreviewKind.StarBombAttached,
                DamageUpgradeName = "Crescent Trap",
                DamageUpgradeDescription = "Sticks to the target, slows it, and lingers on the lane.",
                DamagePreview = BlobProjectilePreviewKind.CrescentTrapFlight,
                DamageEffectPreview = BlobProjectilePreviewKind.CrescentTrapAttached
            },
            BlobKind.Creator => new BlobUpgradeInfo
            {
                BaseShotLabel = "Creator Shot",
                BasePreview = BlobProjectilePreviewKind.CreatorShot,
                SpeedUpgradeName = "Too Slow",
                SpeedUpgradeDescription = "Fires a follow-up shot shortly after the first.",
                SpeedPreview = BlobProjectilePreviewKind.TooSlowShot,
                DamageUpgradeName = "Puppeteer",
                DamageUpgradeDescription =
                    "Killing blows turn enemies into puppets that walk backward and damage others.",
                DamagePreview = BlobProjectilePreviewKind.PuppeteerShot,
                DamageEffectPreview = BlobProjectilePreviewKind.PuppetMindControl
            },
            BlobKind.FourOhFour => new BlobUpgradeInfo
            {
                BaseShotLabel = "404 Shot",
                BasePreview = BlobProjectilePreviewKind.FourOhFourStandard,
                SpeedUpgradeName = "Spores",
                SpeedUpgradeDescription = "Shots slow for 5s and splash to up to 3 nearby glitches.",
                SpeedPreview = BlobProjectilePreviewKind.FourOhFourStandard,
                SpeedEffectPreview = BlobProjectilePreviewKind.FourOhFourSpores,
                DamageUpgradeName = "Divine Assist",
                DamageUpgradeDescription = "Periodically calls down extra strikes on the current target.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            BlobKind.Blaze => new BlobUpgradeInfo
            {
                BaseShotLabel = "Blaze Shot",
                BasePreview = BlobProjectilePreviewKind.TintedPulse,
                SpeedUpgradeName = "Magma Defense",
                SpeedUpgradeDescription = "Periodically releases a fire ring that damages nearby glitches.",
                SpeedPreview = BlobProjectilePreviewKind.None,
                DamageUpgradeName = "Pandas",
                DamageUpgradeDescription = "Buffs neighboring blobs in range with bonus damage.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            BlobKind.Dealer => new BlobUpgradeInfo
            {
                BaseShotLabel = "Dealer Shot",
                BasePreview = BlobProjectilePreviewKind.TintedPulse,
                SpeedUpgradeName = "Silver",
                SpeedUpgradeDescription =
                    "Copies the attack speed of the fastest adjacent blob (including diagonals). If Dealer is already fastest, attack speed increases instead.",
                SpeedPreview = BlobProjectilePreviewKind.None,
                DamageUpgradeName = "Heart Eater",
                DamageUpgradeDescription = "Increases this Dealer's Luck level by 2.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            BlobKind.Cheerful => new BlobUpgradeInfo
            {
                BaseShotLabel = "Cheer Shot",
                BasePreview = BlobProjectilePreviewKind.None,
                SpeedUpgradeName = "Viscous",
                SpeedUpgradeDescription =
                    "Cheer projectiles have a 45% chance to bounce to a second blob adjacent to the Best Friend (oldest on field, excluding Cheerful).",
                SpeedPreview = BlobProjectilePreviewKind.None,
                DamageUpgradeName = "Hugs",
                DamageUpgradeDescription = "Cheer buffs also grant +1 Luck level for the buff duration.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            BlobKind.Archivist => new BlobUpgradeInfo
            {
                BaseShotLabel = "Archivist Shot",
                BasePreview = BlobProjectilePreviewKind.TintedPulse,
                SpeedUpgradeName = "Rewind",
                SpeedUpgradeDescription =
                    "Attack much slower with slower projectiles. Shots push glitches backward on the path.",
                SpeedPreview = BlobProjectilePreviewKind.TintedPulse,
                DamageUpgradeName = "Resume",
                DamageUpgradeDescription =
                    "Attack faster. Every third shot buffs an adjacent blob's attack damage for a few seconds.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            BlobKind.Countdown => new BlobUpgradeInfo
            {
                BaseShotLabel = "Landmines",
                BasePreview = BlobProjectilePreviewKind.TintedPulse,
                SpeedUpgradeName = "Danger",
                SpeedUpgradeDescription =
                    "Stack up to 5 mines per tile. Each mine explodes in a larger 5×5 radius.",
                SpeedPreview = BlobProjectilePreviewKind.None,
                DamageUpgradeName = "Higher",
                DamageUpgradeDescription =
                    "All mines on the same tile detonate together when a glitch touches them.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            BlobKind.Lantern => new BlobUpgradeInfo
            {
                BaseShotLabel = "Looped Sight Pulse",
                BasePreview = BlobProjectilePreviewKind.None,
                SpeedUpgradeName = "Mind",
                SpeedUpgradeDescription =
                    "Pulse faster. Affected blobs gain Strife +1 for the pulse duration.",
                SpeedPreview = BlobProjectilePreviewKind.None,
                DamageUpgradeName = "Heart",
                DamageUpgradeDescription =
                    "Pulse damage hits all blobs; Bench Trio gains extra on top.",
                DamagePreview = BlobProjectilePreviewKind.None
            },
            _ => new BlobUpgradeInfo
            {
                BaseShotLabel = "Shot",
                BasePreview = BlobProjectilePreviewKind.TintedPulse,
                SpeedUpgradeName = "Attack Speed",
                SpeedUpgradeDescription = "Attack faster.",
                SpeedPreview = BlobProjectilePreviewKind.None,
                DamageUpgradeName = "Attack Damage",
                DamageUpgradeDescription = "Deal more damage.",
                DamagePreview = BlobProjectilePreviewKind.None
            }
        };

        static string BuildGlitchBody(GlitchDefinition def, bool isMiniBoss, bool isRoundBoss = false)
        {
            var lines = new List<string> { def.displayName };
            if (isRoundBoss)
                lines.Add("Round Boss");
            else if (isMiniBoss)
                lines.Add("Mini Boss");

            if (!string.IsNullOrWhiteSpace(def.fantasyNote))
                lines.Add(def.fantasyNote);

            lines.Add($"Health: {def.maxHealth}");
            if (def.shellHealth > 0)
                lines.Add($"Shell: {def.shellHealth}");
            lines.Add($"Move speed: {def.moveSpeed:0.##}");
            lines.Add($"Bite: {def.biteDamage} every {def.biteInterval:0.#}s");
            lines.Add($"Reward: {def.ponderReward} Ponders");

            if (def.kind == GlitchKind.Faze)
                lines.Add($"Dodge chance: {MiniBossRules.FazeDodgeChance:P0}");
            if (def.kind == GlitchKind.CicadianRhythm)
            {
                lines.Add("Looped — Looped Sight blobs deal full damage; others deal half.");
                lines.Add($"Dashes forward every {CicadianRhythmRules.DashInterval:0.#}s.");
                lines.Add($"Appears on round {CicadianRhythmRules.SpawnRound} instead of a mini boss.");
            }
            if (isMiniBoss)
                lines.Add($"Size: ~{MiniBossRules.DefaultVisualScaleMultiplier:P0} larger");
            if (isRoundBoss)
                lines.Add($"Size: ~{CicadianRhythmRules.DefaultVisualScaleMultiplier:P0} larger");

            return string.Join("\n", lines);
        }

        static int BlobSortIndex(BlobKind kind)
        {
            for (var i = 0; i < BlobSortOrder.Length; i++)
            {
                if (BlobSortOrder[i] == kind)
                    return i;
            }

            return BlobSortOrder.Length + (int)kind;
        }
    }
}
