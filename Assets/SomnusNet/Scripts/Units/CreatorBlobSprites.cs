using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Units
{
    public static class CreatorBlobSprites
    {
        const float IdleFps = 6f;
        const int FrameCount = 12;
        const int FrameWidth = 200;
        const int FrameHeight = 200;
        const float VisualScale = 1.12f;
        const string FrameFolder = "DreamBlobIdleFrames";
        const string IconResourceName = "DreamBlobIcon";
        static readonly Vector2 IdlePivot = new Vector2(0.5f, 0.32f);
        static readonly Vector2 IconPivot = new Vector2(0.5f, 0.5f);
        static readonly float IdlePixelsPerUnit =
            FrameHeight / (GridManager.BlobSpriteWorldSize * VisualScale);
        static readonly float IconTargetWorldHeight = GridManager.BlobSpriteWorldSize * VisualScale;

        static Sprite[] _idleFrames;
        static Sprite _iconSprite;
        static bool _loadFailed;
        static bool _iconLoadFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _idleFrames = null;
            _iconSprite = null;
            _loadFailed = false;
            _iconLoadFailed = false;
        }

        public static float IdleFramesPerSecond => IdleFps;

        public static int IdleFrameCount => _idleFrames != null ? _idleFrames.Length : 0;

        public static Sprite BodySprite => ShopSprite;

        public static Sprite ShopSprite
        {
            get
            {
                EnsureIconLoaded();
                return _iconSprite;
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

            var frames = new Sprite[FrameCount];
            for (var i = 0; i < FrameCount; i++)
            {
                frames[i] = Resources.Load<Sprite>($"{FrameFolder}/dream_idle_{i:00}");
                if (frames[i] == null)
                {
                    _loadFailed = true;
                    return;
                }
            }

            _idleFrames = frames;
        }

        static void EnsureIconLoaded()
        {
            if (_iconLoadFailed || _iconSprite != null)
                return;

            _iconSprite = SpritesheetLoader.LoadSpriteTrimmed(
                IconResourceName,
                IconTargetWorldHeight,
                IconPivot);

            if (_iconSprite == null)
                _iconSprite = SpritesheetLoader.LoadSprite(IconResourceName, IdlePixelsPerUnit, IconPivot);

            if (_iconSprite == null)
                _iconLoadFailed = true;
        }
    }
}
