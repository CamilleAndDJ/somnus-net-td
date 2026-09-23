using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.UI
{
    public enum StargazerDialogExpression
    {
        Happy,
        Sheepish,
        Alarmed,
        Angry,
        Worried
    }

    public static class StargazerDialogSprites
    {
        const float PixelsPerUnit = 200f;
        static readonly Vector2 Pivot = new(0.5f, 0.5f);

        static readonly string[] ResourceNames =
        {
            "StargazerDialogHappy",
            "StargazerDialogSheepish",
            "StargazerDialogAlarmed",
            "StargazerDialogAngry",
            "StargazerDialogWorried"
        };

        static Sprite[] _sprites;
        static bool _loadFailed;

        public static StargazerDialogExpression ForIntroLine(int index) => index switch
        {
            0 => StargazerDialogExpression.Happy,
            1 => StargazerDialogExpression.Sheepish,
            2 => StargazerDialogExpression.Alarmed,
            3 => StargazerDialogExpression.Angry,
            _ => StargazerDialogExpression.Happy
        };

        public static Sprite Get(StargazerDialogExpression expression)
        {
            EnsureLoaded();
            var index = (int)expression;
            if (_sprites == null || index < 0 || index >= _sprites.Length)
                return FallbackSprite();

            return _sprites[index] ?? FallbackSprite();
        }

        static Sprite FallbackSprite() =>
            GatekeeperBlobSprites.AttackSprite ?? GatekeeperBlobSprites.ShopSprite;

        static void EnsureLoaded()
        {
            if (_sprites != null || _loadFailed)
                return;

            _sprites = new Sprite[ResourceNames.Length];
            var anyLoaded = false;

            for (var i = 0; i < ResourceNames.Length; i++)
            {
                _sprites[i] = SpritesheetLoader.LoadSprite(ResourceNames[i], PixelsPerUnit, Pivot);
                if (_sprites[i] != null)
                    anyLoaded = true;
            }

            if (!anyLoaded)
                _loadFailed = true;
        }
    }
}
