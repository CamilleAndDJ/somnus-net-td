using UnityEngine;

namespace SomnusNet.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SomnusBackground : MonoBehaviour
    {
        public static void ApplyTo(SpriteRenderer sr, float width = 28f, float height = 16f)
        {
            var tex = new Texture2D(4, 4);
            var deep = new Color(0.08f, 0.06f, 0.16f);
            var glow = new Color(0.15f, 0.12f, 0.28f);
            tex.SetPixel(0, 0, deep);
            tex.SetPixel(1, 0, glow);
            tex.SetPixel(0, 1, glow);
            tex.SetPixel(1, 1, deep);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(width, height);
            sr.sortingOrder = -20;
        }
    }
}
