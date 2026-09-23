using UnityEngine;

namespace SomnusNet.Units
{
    public static class GatekeeperAttackSprites
    {
        const int FrameCount = 2;
        const int FrameWidth = 32;
        const int FrameHeight = 32;
        const float PixelsPerUnit = 32f;

        static Sprite[] _frames;
        static bool _loadFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _frames = null;
            _loadFailed = false;
        }

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

            _frames = SpritesheetLoader.LoadHorizontalStrip("BasicAttack", FrameCount, FrameWidth, FrameHeight,
                PixelsPerUnit, Vector2.one * 0.5f);
            if (_frames == null)
                _loadFailed = true;
        }
    }
}
