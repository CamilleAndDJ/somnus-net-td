using UnityEngine;

namespace SomnusNet.Units
{
    public static class PuppeteerSprites
    {
        const int FrameCount = 2;
        const int FrameWidth = 32;
        const int FrameHeight = 32;
        const float PixelsPerUnit = 32f;
        static readonly Vector2 Pivot = new(0.5f, 0.5f);
        public const float FramesPerSecond = 5f;

        static Sprite[] _frames;
        static bool _loadFailed;

        public static bool IsReady => _frames != null && _frames.Length > 0;

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

            _frames = SpritesheetLoader.LoadHorizontalStrip("Puppeteer", FrameCount, FrameWidth, FrameHeight,
                PixelsPerUnit, Pivot);

            if (_frames == null || _frames.Length == 0)
            {
                _frames = SpritesheetLoader.LoadHorizontalStripByContentBounds("Puppeteer", PixelsPerUnit, Pivot);
            }

            if (_frames == null || _frames.Length == 0)
                _loadFailed = true;
        }
    }
}
