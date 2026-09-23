using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class SpeedSymbolSprites
    {
        const string ResourceName = "SpeedSymbol";
        const float PixelsPerUnit = 32f;
        static readonly Vector2 Pivot = new(0.5f, 0.5f);

        static Sprite _sprite;
        static bool _loadFailed;

        public static Sprite Icon
        {
            get
            {
                EnsureLoaded();
                return _sprite;
            }
        }

        static void EnsureLoaded()
        {
            if (_sprite != null || _loadFailed)
                return;

            _sprite = SpritesheetLoader.LoadSprite(ResourceName, PixelsPerUnit, Pivot);
            if (_sprite == null)
                _sprite = SpritesheetLoader.LoadSpriteTrimmed(ResourceName, 0.75f, Pivot);

            if (_sprite == null)
                _loadFailed = true;
        }
    }
}
