using UnityEngine;

namespace SomnusNet.Units
{
    public static class StarBombAttachedSprites
    {
        const int FrameCount = 6;
        const int FrameWidth = 32;
        const int FrameHeight = 32;
        const float PixelsPerUnit = 32f;

        static Sprite[] _frames;
        static bool _loadFailed;

        public static bool IsReady => _frames != null;

        public static Sprite GetFrame(float elapsedSeconds, float framesPerSecond, bool loop = false)
        {
            EnsureLoaded();
            if (_frames == null || _frames.Length == 0)
                return null;

            var index = Mathf.FloorToInt(elapsedSeconds * framesPerSecond);
            if (loop)
            {
                index %= FrameCount;
                if (index < 0)
                    index += FrameCount;
            }
            else
            {
                index = Mathf.Min(FrameCount - 1, index);
            }

            return _frames[index];
        }

        static void EnsureLoaded()
        {
            if (_frames != null || _loadFailed)
                return;

            _frames = SpritesheetLoader.LoadHorizontalStrip("StarBombAttached", FrameCount, FrameWidth, FrameHeight,
                PixelsPerUnit, Vector2.one * 0.5f);
            if (_frames == null)
                _loadFailed = true;
        }
    }
}
