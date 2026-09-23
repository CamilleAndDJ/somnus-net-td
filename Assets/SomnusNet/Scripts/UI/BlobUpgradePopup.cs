using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class BlobUpgradePopup : MonoBehaviour
    {
        [SerializeField] RectTransform dimOverlay;
        [SerializeField] RectTransform panel;
        [SerializeField] Text titleText;
        [SerializeField] Button closeButton;
        [SerializeField] BlobUpgradePreviewCard baseCard;
        [SerializeField] BlobUpgradePreviewCard speedCard;
        [SerializeField] BlobUpgradePreviewCard damageCard;

        public void EnsureBuilt(Transform overlayRoot)
        {
            if (panel != null)
                return;

            dimOverlay = UiPanelFactory.CreateRect(overlayRoot, "UpgradeDim", Vector2.zero, Vector2.one);
            var dimImg = dimOverlay.gameObject.AddComponent<Image>();
            dimImg.color = UiPanelFactory.DimOverlay;
            dimImg.raycastTarget = true;
            dimOverlay.gameObject.SetActive(false);

            panel = UiPanelFactory.CreateRect(overlayRoot, "UpgradePopup",
                new Vector2(0.18f, 0.1f), new Vector2(0.82f, 0.9f));
            var panelBg = panel.gameObject.AddComponent<Image>();
            panelBg.color = UiPanelFactory.PanelBg;

            titleText = UiPanelFactory.CreateText(panel, "Title",
                new Vector2(0.06f, 0.9f), new Vector2(0.78f, 0.98f), 20, TextAnchor.MiddleLeft);
            titleText.fontStyle = FontStyle.Bold;

            closeButton = UiPanelFactory.CreateButton(panel, "Close",
                new Vector2(0.8f, 0.9f), new Vector2(0.96f, 0.98f), "Close", 14);
            closeButton.onClick.AddListener(Hide);

            baseCard = CreateCard(panel, "BaseCard", new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.88f));
            speedCard = CreateCard(panel, "SpeedCard", new Vector2(0.04f, 0.3f), new Vector2(0.96f, 0.54f));
            damageCard = CreateCard(panel, "DamageCard", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.26f));

            panel.gameObject.SetActive(false);
        }

        System.Action _onClose;

        public void Show(EncyclopediaData.BlobEntry entry, System.Action onClose = null)
        {
            if (entry?.Upgrades == null)
                return;

            _onClose = onClose;

            if (titleText != null)
                titleText.text = $"{entry.Name} — Upgrades";

            var upgrades = entry.Upgrades;
            baseCard?.Show(
                upgrades.BaseShotLabel,
                "Default attack before any upgrade is chosen.",
                null,
                entry.Kind,
                upgrades.BasePreview,
                upgrades.BaseEffectPreview);

            speedCard?.Show(
                upgrades.SpeedUpgradeName,
                upgrades.SpeedUpgradeDescription,
                $"{DreamBlob.SpeedUpgradeCost}",
                entry.Kind,
                upgrades.SpeedPreview,
                upgrades.SpeedEffectPreview);

            damageCard?.Show(
                upgrades.DamageUpgradeName,
                upgrades.DamageUpgradeDescription,
                $"{DreamBlob.DamageUpgradeCost}",
                entry.Kind,
                upgrades.DamagePreview,
                upgrades.DamageEffectPreview);

            if (dimOverlay != null)
                dimOverlay.gameObject.SetActive(true);
            if (panel != null)
                panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            baseCard?.Hide();
            speedCard?.Hide();
            damageCard?.Hide();

            if (dimOverlay != null)
                dimOverlay.gameObject.SetActive(false);
            if (panel != null)
                panel.gameObject.SetActive(false);

            var callback = _onClose;
            _onClose = null;
            callback?.Invoke();
        }

        static BlobUpgradePreviewCard CreateCard(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent, false);
            var cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = anchorMin;
            cardRt.anchorMax = anchorMax;
            cardRt.offsetMin = cardRt.offsetMax = Vector2.zero;

            var bg = cardGo.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.09f, 0.18f, 0.9f);

            var title = UiPanelFactory.CreateText(cardGo.transform, "Title",
                new Vector2(0.04f, 0.72f), new Vector2(0.5f, 0.98f), 13, TextAnchor.UpperLeft);
            title.fontStyle = FontStyle.Bold;

            var cost = UiPanelFactory.CreateText(cardGo.transform, "Cost",
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.98f), 12, TextAnchor.UpperRight);

            var description = UiPanelFactory.CreateText(cardGo.transform, "Description",
                new Vector2(0.04f, 0.34f), new Vector2(0.5f, 0.7f), 11, TextAnchor.UpperLeft);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.verticalOverflow = VerticalWrapMode.Overflow;

            var projectileCaption = UiPanelFactory.CreateText(cardGo.transform, "ProjectileCaption",
                new Vector2(0.52f, 0.66f), new Vector2(0.7f, 0.74f), 9, TextAnchor.MiddleCenter, "Projectile");
            projectileCaption.color = new Color(0.78f, 0.78f, 0.88f, 0.95f);

            var effectCaption = UiPanelFactory.CreateText(cardGo.transform, "EffectCaption",
                new Vector2(0.74f, 0.66f), new Vector2(0.94f, 0.74f), 9, TextAnchor.MiddleCenter, "Effect");
            effectCaption.color = new Color(0.78f, 0.78f, 0.88f, 0.95f);

            var projectilePreview = UiPanelFactory.CreateImage(cardGo.transform, "ProjectilePreview",
                new Vector2(0.52f, 0.08f), new Vector2(0.7f, 0.66f), Color.white);
            projectilePreview.preserveAspect = true;

            var effectPreview = UiPanelFactory.CreateImage(cardGo.transform, "EffectPreview",
                new Vector2(0.74f, 0.08f), new Vector2(0.94f, 0.66f), Color.white);
            effectPreview.preserveAspect = true;

            var card = cardGo.AddComponent<BlobUpgradePreviewCard>();
            card.Bind(title, description, cost, projectileCaption, effectCaption, projectilePreview, effectPreview);
            cardGo.SetActive(false);
            return card;
        }
    }
}
