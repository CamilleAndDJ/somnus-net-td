using UnityEngine;

namespace SomnusNet.Units
{
    public static class CrescentTrapFlightSprites
    {
        const int FrameCount = 8;
        const int FrameWidth = 32;
        const int FrameHeight = 32;
        const float PixelsPerUnit = 32f;

        static Sprite[] _frames;
        static bool _loadFailed;

        public static bool IsReady => _frames != null;

        public static Sprite GetFrame(float elapsedSeconds, float framesPerSecond)
        {
            EnsureLoaded();
            if (_frames == null || _frames.Length == 0)
                return null;

            var index = Mathf.FloorToInt(elapsedSeconds * framesPerSecond) % FrameCount;
            if (index < 0)
                index += FrameCount;
            return _frames[index];
        }

        static void EnsureLoaded()
        {
            if (_frames != null || _loadFailed)
                return;

            _frames = SpritesheetLoader.LoadHorizontalStrip("CrescentTrapFlight", FrameCount, FrameWidth, FrameHeight,
                PixelsPerUnit, Vector2.one * 0.5f);
            if (_frames == null)
                _loadFailed = true;
        }
    }
}
