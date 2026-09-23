using UnityEngine;

namespace SomnusNet.UI
{
    static class StoryModeUiSprites
    {
        static Sprite _chevronSprite;

        public static Sprite Chevron
        {
            get
            {
                if (_chevronSprite != null)
                    return _chevronSprite;

                const int size = 32;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;

                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    tex.SetPixel(x, y, Color.clear);

                // Simple ">" chevron pointing right (rotate for other directions).
                for (var t = 0; t < size - 4; t++)
                {
                    var thickness = Mathf.Max(2, (size - t) / 7);
                    var cx = size / 2 - 2;
                    var cy = size / 2;
                    Fill(tex, cx + t / 2, cy + t / 2, thickness);
                    Fill(tex, cx + t / 2, cy - t / 2, thickness);
                }

                tex.Apply();
                _chevronSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                return _chevronSprite;
            }
        }

        static void Fill(Texture2D tex, int cx, int cy, int radius)
        {
            for (var y = -radius; y <= radius; y++)
            for (var x = -radius; x <= radius; x++)
            {
                var px = cx + x;
                var py = cy + y;
                if (px < 0 || py < 0 || px >= tex.width || py >= tex.height)
                    continue;
                if (x * x + y * y <= radius * radius)
                    tex.SetPixel(px, py, Color.white);
            }
        }
    }
}
