using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public static class ShopTypeBadges
    {
        const float BadgeSize = 15f;
        const float BadgeGap = 2f;
        const float RightInset = 2f;
        const float BottomHang = 0.62f;

        public static void Apply(Button button, BlobDefinition def)
        {
            if (button == null || def == null)
                return;

            var existing = button.transform.Find("TypeBadges");
            if (existing != null)
                Object.Destroy(existing.gameObject);

            if (!BlobTypeFeatures.Enabled)
                return;

            var labels = CollectTypeLabels(def);
            if (labels.Count == 0)
                return;

            var containerGo = new GameObject("TypeBadges", typeof(RectTransform));
            containerGo.transform.SetParent(button.transform, false);
            containerGo.transform.SetAsLastSibling();

            var container = containerGo.GetComponent<RectTransform>();
            container.anchorMin = container.anchorMax = new Vector2(1f, 0f);
            container.pivot = new Vector2(1f, 0f);
            container.anchoredPosition = new Vector2(-RightInset, -BadgeSize * BottomHang);
            container.sizeDelta = new Vector2(
                labels.Count * BadgeSize + Mathf.Max(0, labels.Count - 1) * BadgeGap,
                BadgeSize);

            for (var i = 0; i < labels.Count; i++)
            {
                var badgeGo = new GameObject($"TypeBadge_{i}", typeof(RectTransform));
                badgeGo.transform.SetParent(container, false);

                var badgeRect = badgeGo.GetComponent<RectTransform>();
                badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(1f, 0f);
                badgeRect.pivot = new Vector2(1f, 0f);
                badgeRect.sizeDelta = Vector2.one * BadgeSize;
                badgeRect.anchoredPosition = new Vector2(-i * (BadgeSize + BadgeGap), 0f);

                var image = badgeGo.AddComponent<Image>();
                image.sprite = EncyclopediaData.GetTypeIcon(labels[i]);
                image.color = EncyclopediaData.GetTypeIconColor(labels[i]);
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
        }

        static List<string> CollectTypeLabels(BlobDefinition def)
        {
            var labels = new List<string>();
            if (def.typeLabels != null && def.typeLabels.Length > 0)
            {
                foreach (var label in def.typeLabels)
                {
                    if (string.IsNullOrWhiteSpace(label) || labels.Contains(label))
                        continue;
                    labels.Add(label);
                }

                return labels;
            }

            if (!string.IsNullOrWhiteSpace(def.typeLabel) && def.typeLabel != "None")
                labels.Add(def.typeLabel);

            return labels;
        }
    }
}
