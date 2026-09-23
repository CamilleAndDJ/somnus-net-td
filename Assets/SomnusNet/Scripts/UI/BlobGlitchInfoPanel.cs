using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class BlobGlitchInfoPanel : MonoBehaviour
    {
        enum Tab { Types, Blobs, Glitches }

        const int OverlaySortOrder = 230;
        const float ListItemHeight = 44f;

        [SerializeField] RectTransform root;
        [SerializeField] Button typesTab;
        [SerializeField] Button blobsTab;
        [SerializeField] Button glitchesTab;
        [SerializeField] Button backButton;
        [SerializeField] RectTransform listContent;
        [SerializeField] Text detailTitle;
        [SerializeField] Text detailBody;
        [SerializeField] Image detailIcon;
        [SerializeField] Button upgradesButton;
        [SerializeField] BlobUpgradePopup upgradePopup;

        Tab _activeTab = Tab.Types;
        EncyclopediaData.BlobEntry _selectedBlob;
        readonly List<Button> _listButtons = new();
        GameCatalog _catalog;

        public static BlobGlitchInfoPanel Instance { get; private set; }

        public static void EnsureExists()
        {
            if (Instance != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<BlobGlitchInfoPanel>();
        }

        void Awake() => Instance = this;

        void Start()
        {
            _catalog = Object.FindFirstObjectByType<GameCatalog>();
            EnsureUiBuilt();
            Hide();

            if (typesTab != null)
                typesTab.onClick.AddListener(() => SwitchTab(Tab.Types));
            if (blobsTab != null)
                blobsTab.onClick.AddListener(() => SwitchTab(Tab.Blobs));
            if (glitchesTab != null)
                glitchesTab.onClick.AddListener(() => SwitchTab(Tab.Glitches));
            if (backButton != null)
                backButton.onClick.AddListener(OnBack);
            if (upgradesButton != null)
                upgradesButton.onClick.AddListener(OnUpgradesClicked);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Show()
        {
            if (root != null)
                root.gameObject.SetActive(true);
            SwitchTab(_activeTab);
        }

        public void Hide()
        {
            upgradePopup?.Hide();
            if (root != null)
                root.gameObject.SetActive(false);
        }

        public void ShowUpgradePopupFor(BlobKind kind, bool resumeOnClose = false)
        {
            EnsureUiBuilt();
            var entry = FindBlobEntry(kind);
            if (entry == null)
                return;

            upgradePopup?.Show(entry, resumeOnClose ? HandleUpgradePopupClosedFromGameplay : null);
        }

        void HandleUpgradePopupClosedFromGameplay()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                GameManager.Instance.Resume();
        }

        EncyclopediaData.BlobEntry FindBlobEntry(BlobKind kind)
        {
            if (_catalog == null)
                _catalog = Object.FindFirstObjectByType<GameCatalog>();

            var normalized = BlobKinds.Normalize(kind);
            foreach (var entry in EncyclopediaData.BuildBlobEntries(_catalog))
            {
                if (BlobKinds.Normalize(entry.Kind) == normalized)
                    return entry;
            }

            return null;
        }

        void OnBack()
        {
            upgradePopup?.Hide();
            Hide();
            PauseMenu.Instance?.ShowPausePanelFromInfo();
        }

        void OnUpgradesClicked()
        {
            if (_selectedBlob != null)
                upgradePopup?.Show(_selectedBlob);
        }

        void SwitchTab(Tab tab)
        {
            _activeTab = tab;
            upgradePopup?.Hide();
            HighlightTabs();
            RebuildList();
        }

        void HighlightTabs()
        {
            SetTabSelected(typesTab, _activeTab == Tab.Types);
            SetTabSelected(blobsTab, _activeTab == Tab.Blobs);
            SetTabSelected(glitchesTab, _activeTab == Tab.Glitches);
        }

        static void SetTabSelected(Button button, bool selected)
        {
            if (button == null) return;
            var img = button.GetComponent<Image>();
            if (img != null)
                img.color = selected ? UiPanelFactory.ButtonSelected : UiPanelFactory.ButtonBg;
        }

        void RebuildList()
        {
            ClearList();
            switch (_activeTab)
            {
                case Tab.Types:
                    PopulateTypes();
                    break;
                case Tab.Blobs:
                    PopulateBlobs();
                    break;
                case Tab.Glitches:
                    PopulateGlitches();
                    break;
            }
        }

        void ClearList()
        {
            _selectedBlob = null;
            foreach (var button in _listButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }

            _listButtons.Clear();
            if (detailTitle != null) detailTitle.text = string.Empty;
            if (detailBody != null) detailBody.text = string.Empty;
            if (detailIcon != null)
            {
                detailIcon.sprite = null;
                detailIcon.gameObject.SetActive(false);
            }

            if (upgradesButton != null)
                upgradesButton.gameObject.SetActive(false);
        }

        void PopulateTypes()
        {
            var entries = EncyclopediaData.BuildTypeEntries();
            if (entries.Count == 0)
            {
                SetDetail("Types unavailable", "Type features are disabled on this level.", null, Color.white);
                return;
            }

            var first = true;
            foreach (var entry in entries)
            {
                var captured = entry;
                var button = CreateListButton(entry.Label, entry.Icon, entry.IconColor,
                    () => SetDetail(captured.Title, captured.Body, captured.Icon, captured.IconColor));
                _listButtons.Add(button);
                if (first)
                {
                    SetDetail(captured.Title, captured.Body, captured.Icon, captured.IconColor);
                    first = false;
                }
            }
        }

        void PopulateBlobs()
        {
            var entries = EncyclopediaData.BuildBlobEntries(_catalog);
            if (entries.Count == 0)
            {
                SetDetail("No blobs", "Blob data is not available.", null, Color.white);
                return;
            }

            var first = true;
            foreach (var entry in entries)
            {
                var captured = entry;
                var button = CreateListButton(entry.Name, entry.Icon, Color.white,
                    () => SetBlobDetail(captured));
                _listButtons.Add(button);
                if (first)
                {
                    SetBlobDetail(captured);
                    first = false;
                }
            }
        }

        void PopulateGlitches()
        {
            var entries = EncyclopediaData.BuildGlitchEntries(_catalog);
            if (entries.Count == 0)
            {
                SetDetail("No glitches", "Glitch data is not available.", null, Color.white);
                return;
            }

            var first = true;
            foreach (var entry in entries)
            {
                var captured = entry;
                var label = entry.IsRoundBoss ? $"{entry.Name} (Round Boss)"
                    : entry.IsMiniBoss ? $"{entry.Name} (Mini Boss)" : entry.Name;
                var button = CreateListButton(label, entry.Icon, Color.white,
                    () => SetDetail(captured.Name, captured.Body, captured.Icon, Color.white));
                _listButtons.Add(button);
                if (first)
                {
                    SetDetail(captured.Name, captured.Body, captured.Icon, Color.white);
                    first = false;
                }
            }
        }

        void SetBlobDetail(EncyclopediaData.BlobEntry entry)
        {
            _selectedBlob = entry;
            SetDetail(entry.Name, entry.Body, entry.Icon, Color.white);

            if (upgradesButton != null)
                upgradesButton.gameObject.SetActive(entry.Upgrades != null);
        }

        Button CreateListButton(string label, Sprite icon, Color iconColor, UnityEngine.Events.UnityAction onClick)
        {
            var row = new GameObject("ListItem");
            row.transform.SetParent(listContent, false);
            var rowRt = row.AddComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0f, ListItemHeight);

            var layout = row.AddComponent<LayoutElement>();
            layout.minHeight = ListItemHeight;
            layout.preferredHeight = ListItemHeight;

            var bg = row.AddComponent<Image>();
            bg.color = UiPanelFactory.ButtonBg;

            var button = row.AddComponent<Button>();
            button.targetGraphic = bg;
            button.onClick.AddListener(onClick);

            if (icon != null)
            {
                var iconRt = UiPanelFactory.CreateRect(row.transform, "Icon",
                    new Vector2(0.02f, 0.12f), new Vector2(0.16f, 0.88f));
                var iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.color = iconColor;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            var textAnchorMin = icon != null ? new Vector2(0.18f, 0f) : new Vector2(0.06f, 0f);
            var text = UiPanelFactory.CreateText(row.transform, "Label",
                textAnchorMin, new Vector2(0.96f, 1f), 13, TextAnchor.MiddleLeft, label);
            text.raycastTarget = false;

            return button;
        }

        void SetDetail(string title, string body, Sprite icon, Color iconColor)
        {
            if (detailTitle != null)
                detailTitle.text = title;
            if (detailBody != null)
                detailBody.text = body;
            if (detailIcon != null)
            {
                detailIcon.sprite = icon;
                detailIcon.color = iconColor;
                detailIcon.gameObject.SetActive(icon != null);
            }
        }

        void EnsureUiBuilt()
        {
            if (root != null) return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var overlayRoot = UiPanelFactory.EnsureOverlayCanvas(canvas.transform, "BlobGlitchInfoOverlay", OverlaySortOrder)
                .transform;

            root = UiPanelFactory.CreateRect(overlayRoot, "BlobGlitchInfoRoot", Vector2.zero, Vector2.one);
            var rootBg = root.gameObject.AddComponent<Image>();
            rootBg.color = UiPanelFactory.PanelBg;

            typesTab = UiPanelFactory.CreateButton(root, "TypesTab",
                new Vector2(0.04f, 0.9f), new Vector2(0.2f, 0.97f), "Types", 16);
            blobsTab = UiPanelFactory.CreateButton(root, "BlobsTab",
                new Vector2(0.22f, 0.9f), new Vector2(0.38f, 0.97f), "Blobs", 16);
            glitchesTab = UiPanelFactory.CreateButton(root, "GlitchesTab",
                new Vector2(0.4f, 0.9f), new Vector2(0.58f, 0.97f), "Glitches", 16);
            backButton = UiPanelFactory.CreateButton(root, "BackButton",
                new Vector2(0.78f, 0.9f), new Vector2(0.96f, 0.97f), "Back", 16);

            var (_, content) = UiPanelFactory.CreateScrollList(root, "ListScroll",
                new Vector2(0.04f, 0.06f), new Vector2(0.38f, 0.86f));
            listContent = content;

            var detailPanel = UiPanelFactory.CreateRect(root, "DetailPanel",
                new Vector2(0.4f, 0.06f), new Vector2(0.96f, 0.86f));
            var detailBg = detailPanel.gameObject.AddComponent<Image>();
            detailBg.color = new Color(0.1f, 0.09f, 0.18f, 0.9f);

            detailIcon = UiPanelFactory.CreateImage(detailPanel, "DetailIcon",
                new Vector2(0.04f, 0.8f), new Vector2(0.2f, 0.96f), Color.white);
            detailIcon.preserveAspect = true;

            detailTitle = UiPanelFactory.CreateText(detailPanel, "DetailTitle",
                new Vector2(0.22f, 0.8f), new Vector2(0.96f, 0.96f), 20, TextAnchor.UpperLeft);
            detailTitle.fontStyle = FontStyle.Bold;

            upgradesButton = UiPanelFactory.CreateButton(detailPanel, "UpgradesButton",
                new Vector2(0.04f, 0.7f), new Vector2(0.96f, 0.78f), "View Upgrades & Effects", 14);
            upgradesButton.gameObject.SetActive(false);

            detailBody = UiPanelFactory.CreateText(detailPanel, "DetailBody",
                new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.68f), 14, TextAnchor.UpperLeft);

            detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            detailBody.verticalOverflow = VerticalWrapMode.Overflow;

            var popupGo = new GameObject("BlobUpgradePopup");
            popupGo.transform.SetParent(overlayRoot, false);
            upgradePopup = popupGo.AddComponent<BlobUpgradePopup>();
            upgradePopup.EnsureBuilt(overlayRoot);

            root.gameObject.SetActive(false);
        }
    }
}
