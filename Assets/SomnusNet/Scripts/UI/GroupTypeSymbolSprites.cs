using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.UI
{
    /// <summary>
    /// Loads group-type HUD icons from Resources using roster membership.
    /// Full roster: {baseName}. Partial: {baseName}{Member1}{Member2}... in roster order
    /// (e.g. DreamTeamSymbolCreator404). Encyclopedia uses the full-roster {baseName} only.
    /// </summary>
    public static class GroupTypeSymbolSprites
    {
        const float PixelsPerUnit = 32f;
        static readonly Vector2 Pivot = new(0.5f, 0.5f);

        static readonly Dictionary<string, Sprite> _cache = new();
        static readonly HashSet<string> _loadFailed = new();

        public static Sprite GetFullRosterIcon(string baseResourceName) =>
            Load(baseResourceName);

        public static Sprite GetHudIcon(string baseResourceName, BlobKind[] roster, GridManager grid)
        {
            if (grid == null || roster == null || roster.Length == 0)
                return FallbackIcon();

            var mask = ComputeMask(grid, roster);
            if (mask == 0)
                return FallbackIcon();

            return Load(BuildResourceName(baseResourceName, roster, mask)) ?? FallbackIcon();
        }

        static int ComputeMask(GridManager grid, BlobKind[] roster)
        {
            var mask = 0;
            for (var i = 0; i < roster.Length; i++)
            {
                if (grid.HasAliveBlobKind(roster[i]))
                    mask |= 1 << i;
            }

            return mask;
        }

        static string BuildResourceName(string baseResourceName, BlobKind[] roster, int mask)
        {
            var fullMask = (1 << roster.Length) - 1;
            if (mask == fullMask)
                return baseResourceName;

            var suffix = "";
            for (var i = 0; i < roster.Length; i++)
            {
                if ((mask & (1 << i)) == 0)
                    continue;
                suffix += GetMemberSuffix(roster[i]);
            }

            return baseResourceName + suffix;
        }

        static string GetMemberSuffix(BlobKind kind) => kind switch
        {
            BlobKind.Creator => "Dream",
            BlobKind.FourOhFour => "404",
            BlobKind.Blaze => "Blaze",
            BlobKind.Dealer => "Dealer",
            BlobKind.Cheerful => "Slime",
            BlobKind.Countdown => "Firework",
            BlobKind.Gatekeeper => "Basic",
            _ => kind.ToString()
        };

        static Sprite Load(string resourceName)
        {
            if (_loadFailed.Contains(resourceName))
                return null;

            if (_cache.TryGetValue(resourceName, out var cached))
                return cached;

            var sprite = SpritesheetLoader.LoadSprite(resourceName, PixelsPerUnit, Pivot);
            if (sprite == null)
                sprite = SpritesheetLoader.LoadSpriteTrimmed(resourceName, 0.75f, Pivot);

            if (sprite == null)
            {
                _loadFailed.Add(resourceName);
                return null;
            }

            _cache[resourceName] = sprite;
            return sprite;
        }

        static Sprite FallbackIcon() => TypeHudSprites.CreateCircleSprite();
    }
}
