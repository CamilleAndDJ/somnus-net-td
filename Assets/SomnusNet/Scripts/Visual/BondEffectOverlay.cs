using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.Visual
{
    /// <summary>Top-most UI layer for bond icon VFX so effects render in front of bond icons.</summary>
    public static class BondEffectOverlay
    {
        const string LayerName = "BondEffectOverlay";

        public static RectTransform GetLayer(Canvas canvas)
        {
            if (canvas == null)
                return null;

            var existing = canvas.transform.Find(LayerName) as RectTransform;
            if (existing != null)
                return existing;

            var go = new GameObject(LayerName, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            return rt;
        }

        public static RectTransform CreateIconHost(RectTransform icon)
        {
            if (icon == null)
                return null;

            var canvas = icon.GetComponentInParent<Canvas>();
            var layer = GetLayer(canvas);
            if (layer == null)
                return null;

            LayoutRebuilder.ForceRebuildLayoutImmediate(icon);

            var go = new GameObject("BondIconEffectHost", typeof(RectTransform));
            go.transform.SetParent(layer, false);
            go.transform.SetAsLastSibling();

            var host = go.GetComponent<RectTransform>();
            SyncHostToIcon(icon, host);
            return host;
        }

        public static void SyncHostToIcon(RectTransform icon, RectTransform host)
        {
            if (icon == null || host == null)
                return;

            var canvas = host.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var center = RectTransformUtility.WorldToScreenPoint(cam, icon.TransformPoint(icon.rect.center));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                host.parent as RectTransform, center, cam, out var localPoint);

            host.anchorMin = host.anchorMax = new Vector2(0.5f, 0.5f);
            host.pivot = new Vector2(0.5f, 0.5f);
            host.anchoredPosition = localPoint;
            host.localRotation = icon.rotation;
            host.localScale = Vector3.one;
            host.sizeDelta = icon.rect.size;
        }
    }
}
