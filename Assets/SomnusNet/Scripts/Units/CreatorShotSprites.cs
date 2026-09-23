using UnityEngine;

namespace SomnusNet.Units
{
    public static class CreatorShotSprites
    {
        const int FrameCount = 4;
        const int FrameWidth = 32;
        const int FrameHeight = 32;
        const float PixelsPerUnit = 32f;
        static readonly Vector2 Pivot = new(0.5f, 0.5f);
        public const float FramesPerSecond = 10f;

        static Sprite[] _frames;
        static bool _loadFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _frames = null;
            _loadFailed = false;
        }

        public static bool IsReady => _frames != null && _frames.Length > 0;

        public static Sprite Sprite => GetFrame(0f);

        public static Sprite GetFrame(float elapsedSeconds, float framesPerSecond = FramesPerSecond)
        {
            EnsureLoaded();
            if (_frames == null || _frames.Length == 0)
                return null;

            var index = Mathf.FloorToInt(elapsedSeconds * framesPerSecond) % _frames.Length;
            if (index < 0)
                index += _frames.Length;
            return _frames[index];
        }

        static void EnsureLoaded()
        {
            if (_frames != null || _loadFailed)
                return;

            _frames = SpritesheetLoader.LoadHorizontalStrip("DreamShot", FrameCount, FrameWidth, FrameHeight,
                PixelsPerUnit, Pivot);

            if (_frames == null || _frames.Length == 0)
                _frames = SpritesheetLoader.LoadHorizontalStripByContentBounds("DreamShot", PixelsPerUnit, Pivot);

            if (_frames == null || _frames.Length == 0)
                _loadFailed = true;
        }
    }
}
