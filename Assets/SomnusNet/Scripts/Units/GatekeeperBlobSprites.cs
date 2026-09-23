using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Units
{
    public static class GatekeeperBlobSprites
    {
        const float IdleFps = 3f;
        const int IdleFrameHeight = 93;
        const float VisualScale = 1.12f;
        static readonly Vector2 Pivot = new Vector2(0.5f, 0.32f);
        static readonly Vector2 ShopIconPivot = new Vector2(0.5f, 0.5f);
        static readonly float IdlePixelsPerUnit =
            IdleFrameHeight / (GridManager.BlobSpriteWorldSize * VisualScale);

        // Measured from BasicBlobIdle.png — frames are uneven width, not a uniform grid.
        static readonly Rect[] IdleFrameRects =
        {
            new Rect(4, 0, 86, 93),
            new Rect(99, 0, 83, 93),
            new Rect(190, 0, 80, 93),
            new Rect(281, 0, 83, 93),
            new Rect(376, 0, 83, 93),
            new Rect(473, 0, 81, 93),
            new Rect(563, 0, 87, 93),
            new Rect(659, 0, 81, 93),
            new Rect(746, 0, 85, 93),
            new Rect(839, 0, 87, 93),
            new Rect(931, 0, 88, 93),
        };

        static Sprite[] _idleFrames;
        static Sprite _shopSprite;
        static bool _loadFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _idleFrames = null;
            _shopSprite = null;
            _loadFailed = false;
        }

        public static float IdleFramesPerSecond => IdleFps;

        public static int IdleFrameCount => _idleFrames != null ? _idleFrames.Length : 0;

        public static Sprite AttackSprite
        {
            get
            {
                EnsureLoaded();
                return _shopSprite;
            }
        }

        public static Sprite ShopSprite
        {
            get
            {
                EnsureLoaded();
                return _shopSprite ?? (_idleFrames != null && _idleFrames.Length > 0 ? _idleFrames[0] : null);
            }
        }

        public static Sprite GetIdleFrame(float elapsedSeconds)
        {
            EnsureLoaded();
            if (_idleFrames == null || _idleFrames.Length == 0)
                return null;

            return _idleFrames[GetIdleFrameIndex(elapsedSeconds)];
        }

        public static int GetIdleFrameIndex(float elapsedSeconds)
        {
            if (_idleFrames == null || _idleFrames.Length == 0)
                return 0;

            var index = Mathf.FloorToInt(elapsedSeconds * IdleFps) % _idleFrames.Length;
            if (index < 0)
                index += _idleFrames.Length;
            return index;
        }

        static void EnsureLoaded()
        {
            if (_loadFailed || _idleFrames != null)
                return;

            _idleFrames = SpritesheetLoader.LoadHorizontalStripFromRects(
                "BasicBlobIdle",
                IdleFrameRects,
                IdlePixelsPerUnit,
                Pivot);

            if (_idleFrames == null || _idleFrames.Length == 0)
                _idleFrames = SpritesheetLoader.LoadHorizontalStripByContentBounds(
                    "BasicBlobIdle",
                    IdlePixelsPerUnit,
                    Pivot);

            _shopSprite = SpritesheetLoader.LoadSpriteTrimmed(
                "BasicBlobAttack",
                GridManager.BlobSpriteWorldSize * VisualScale,
                ShopIconPivot);

            if (_idleFrames == null || _idleFrames.Length == 0)
                _loadFailed = true;
        }
    }
}
