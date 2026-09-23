using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class HarmonySymbolSprites
    {
        const string ResourceName = "HarmonySymbol";
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
                _loadFailed = true;
        }
    }
}
