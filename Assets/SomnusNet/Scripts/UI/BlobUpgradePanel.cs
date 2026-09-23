using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class BlobUpgradePanel : MonoBehaviour
    {
        const float SidePanelMinX = 0.02f;
        const float SidePanelMaxX = 0.42f;
        const float ModifiersPopupMinY = 0.44f;
        const float ModifiersPopupMaxY = 0.64f;

        [SerializeField] RectTransform panelRoot;
        [SerializeField] Text nameText;
        [SerializeField] Text typeText;
        [SerializeField] Image typeIconImage;
        [SerializeField] Button modifiersButton;
        [SerializeField] Button speedButton;
        [SerializeField] Button damageButton;
        [SerializeField] Button deleteButton;
        [SerializeField] Button infoButton;
        [SerializeField] Text hintText;
        [SerializeField] RectTransform modifiersPopupRoot;
        [SerializeField] Text modifiersPopupText;

        DreamBlob _selected;
        bool _hintDismissed;
        bool _modifiersOpen;

        public static BlobUpgradePanel Instance { get; private set; }

        public static void EnsureExists()
        {
            if (Instance != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<BlobUpgradePanel>();
        }

        void Awake() => Instance = this;

        void Start()
        {
            EnsureUiBuilt();
            ApplyExistingPanelLayout();
            HidePanel();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnUpgradesUnlocked += HandleUpgradesUnlocked;
                if (ShouldShowUpgradeHint() && CanShowUpgradeHintNow())
                    ShowHint();
            }

            if (modifiersButton != null)
                modifiersButton.onClick.AddListener(ToggleModifiersPopup);
            if (speedButton != null)
                speedButton.onClick.AddListener(OnSpeedClicked);
            if (damageButton != null)
                damageButton.onClick.AddListener(OnDamageClicked);
            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteClicked);
            if (infoButton != null)
                infoButton.onClick.AddListener(OnInfoClicked);

            if (PonderEconomy.Instance != null)
                PonderEconomy.Instance.OnPondersChanged += HandlePondersChanged;
        }

        void HandlePondersChanged(int amount)
        {
            if (_selected != null && IsOpen)
                Refresh();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (GameManager.Instance != null)
                GameManager.Instance.OnUpgradesUnlocked -= HandleUpgradesUnlocked;
            if (PonderEconomy.Instance != null)
                PonderEconomy.Instance.OnPondersChanged -= HandlePondersChanged;
        }

        void HandleUpgradesUnlocked()
        {
            if (ShouldShowUpgradeHint() && CanShowUpgradeHintNow())
                ShowHint();
        }

        static bool ShouldShowUpgradeHint() =>
            LevelSettings.Instance == null || !LevelSettings.Instance.upgradesUnlockedFromStart;

        static bool CanShowUpgradeHintNow()
        {
            if (GameManager.Instance == null)
                return false;

            return GameManager.Instance.UpgradesUnlocked;
        }

        public bool IsOpen => panelRoot != null && panelRoot.gameObject.activeSelf;
        public DreamBlob SelectedBlob => _selected;

        public void ClosePanel()
        {
            _selected = null;
            HideModifiersPopup();
            HidePanel();
        }

        public void SelectBlob(DreamBlob blob)
        {
            if (blob == null || !blob.IsAlive) return;

            _selected = blob;
            DismissHint();
            HideModifiersPopup();
            UpdatePanelSide(blob.Column);
            Refresh();
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(true);
        }

        public void RefreshIfOpen()
        {
            if (!IsOpen || _selected == null) return;
            Refresh();
        }

        void UpdatePanelSide(int column)
        {
            if (panelRoot == null) return;

            var grid = GridManager.Instance;
            var midColumn = grid != null ? grid.PathCenterColumn : GridManager.ColumnCount / 2;
            var onLeft = column <= midColumn;
            ApplyPanelSideAnchors(panelRoot, onLeft, 0.12f, 0.42f);
            if (modifiersPopupRoot != null)
                ApplyPanelSideAnchors(modifiersPopupRoot, onLeft, ModifiersPopupMinY, ModifiersPopupMaxY);
        }

        static void ApplyPanelSideAnchors(RectTransform rect, bool onLeft, float minY, float maxY)
        {
            if (rect == null) return;
            if (onLeft)
            {
                rect.anchorMin = new Vector2(SidePanelMinX, minY);
                rect.anchorMax = new Vector2(SidePanelMaxX, maxY);
            }
            else
            {
                rect.anchorMin = new Vector2(1f - SidePanelMaxX, minY);
                rect.anchorMax = new Vector2(1f - SidePanelMinX, maxY);
            }

            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        void ToggleModifiersPopup()
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanViewModifications)
                return;

            if (_modifiersOpen)
                HideModifiersPopup();
            else
                ShowModifiersPopup();
        }

        void ShowModifiersPopup()
        {
            if (modifiersPopupRoot == null) return;
            RefreshModifiersText();
            _modifiersOpen = true;
            modifiersPopupRoot.gameObject.SetActive(true);
        }

        void HideModifiersPopup()
        {
            _modifiersOpen = false;
            if (modifiersPopupRoot != null)
                modifiersPopupRoot.gameObject.SetActive(false);
        }

        void OnSpeedClicked()
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanPurchaseUpgrades) return;
            if (_selected == null || !_selected.IsAlive) return;
            if (_selected.TryUpgradeAttackSpeed())
                Refresh();
        }

        void OnDamageClicked()
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanPurchaseUpgrades) return;
            if (_selected == null || !_selected.IsAlive) return;
            if (_selected.TryUpgradeAttackDamage())
                Refresh();
        }

        void OnDeleteClicked()
        {
            if (_selected == null || !_selected.IsAlive) return;

            var blob = _selected;
            ClosePanel();
            blob.DeleteWithRefund();
        }

        void OnInfoClicked()
        {
            if (_selected == null || !_selected.IsAlive)
                return;

            BlobGlitchInfoPanel.EnsureExists();
            GameManager.Instance?.Pause();
            BlobGlitchInfoPanel.Instance?.ShowUpgradePopupFor(_selected.Kind, resumeOnClose: true);
        }

        void Refresh()
        {
            if (_selected == null) return;

            if (nameText != null)
                nameText.text = _selected.DisplayName;
            if (typeText != null)
                typeText.text = $"Type: {_selected.TypeDisplay}";
            RefreshTypeIcon();

            var modificationsEnabled = GameManager.Instance == null || GameManager.Instance.CanViewModifications;
            var purchasesEnabled = GameManager.Instance == null || GameManager.Instance.CanPurchaseUpgrades;

            if (_modifiersOpen)
                RefreshModifiersText();

            if (modifiersButton != null)
            {
                modifiersButton.interactable = modificationsEnabled;
                var modifiersLabel = modifiersButton.GetComponentInChildren<Text>();
                if (modifiersLabel != null)
                    modifiersLabel.text = modificationsEnabled ? "Modifiers" : "Locked";
            }

            if (speedButton != null)
            {
                var label = speedButton.GetComponentInChildren<Text>();
                var upgradeName = _selected.PrimaryUpgradeLabel;
                if (!purchasesEnabled)
                {
                    speedButton.interactable = false;
                    if (label != null)
                        label.text = "Locked";
                }
                else
                {
                    speedButton.interactable = _selected.CanBuySpeedUpgrade;
                    if (label != null)
                    {
                        label.text = _selected.HasSpeedUpgrade
                            ? $"{upgradeName} — Purchased"
                            : _selected.IsSpeedPathLocked
                                ? $"{upgradeName} — Locked"
                                : $"{upgradeName} — {DreamBlob.SpeedUpgradeCost}";
                    }
                }
            }

            if (damageButton != null)
            {
                var label = damageButton.GetComponentInChildren<Text>();
                var upgradeName = _selected.SecondaryUpgradeLabel;
                if (!purchasesEnabled)
                {
                    damageButton.interactable = false;
                    if (label != null)
                        label.text = "Locked";
                }
                else
                {
                    damageButton.interactable = _selected.CanBuyDamageUpgrade;
                    if (label != null)
                    {
                        label.text = _selected.HasDamageUpgrade
                            ? $"{upgradeName} — Purchased"
                            : _selected.IsDamagePathLocked
                                ? $"{upgradeName} — Locked"
                                : $"{upgradeName} — {DreamBlob.DamageUpgradeCost}";
                    }
                }
            }

            if (deleteButton != null)
            {
                deleteButton.interactable = true;
                var label = deleteButton.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = $"Delete Blob\nRefund {_selected.PlacementCost}";
            }
        }

        void RefreshModifiersText()
        {
            if (modifiersPopupText == null || _selected == null) return;

            var lines = BuildModifierLines(_selected);
            modifiersPopupText.text = lines.Count > 0
                ? string.Join("\n", lines)
                : "No active modifiers.";
        }

        static System.Collections.Generic.List<string> BuildModifierLines(DreamBlob blob)
        {
            var lines = new System.Collections.Generic.List<string>();

            if (blob.Kind == BlobKind.Countdown)
            {
                var maxMines = CountdownRules.GetMaxMinesPerTile(blob);
                var blast = CountdownRules.GetBlastRangeSize(blob);
                lines.Add($"Landmines — up to {maxMines}/tile, {blast}×{blast} blast");
            }
            else if (blob.Kind == BlobKind.Lantern)
            {
                lines.Add($"Pulse — {LanternRules.PulseRangeSize}×{LanternRules.PulseRangeSize} {LoopedSightRules.StatusLabel}");
            }
            else if (blob.HasStarBomb)
                lines.Add("Star Bomb");
            else if (blob.HasCrescentTrap)
                lines.Add("Crescent Trap");
            else if (blob.HasSpores)
                lines.Add("Spores — slow 5s, up to 3 nearby");
            else if (blob.HasMagmaDefense)
                lines.Add("Magma Defense — fire ring");
            else if (blob.HasSecond)
            {
                var copied = DealerSecondSynergy.GetCopiedBlob(blob);
                if (copied != null)
                    lines.Add($"Silver — copying {copied.DisplayName}'s attack speed");
                else
                    lines.Add($"Silver — self attack speed x{1f / DealerSecondSynergy.SelfSpeedIntervalMultiplier:F2}");
            }
            else if (blob.HasTooSlow)
                lines.Add("Too Slow — double shot");

            if (blob.Kind == BlobKind.Cheerful)
            {
                var friend = CheerfulBestFriendSynergy.SelectBestFriend(blob);
                if (friend != null)
                    lines.Add($"Best Friend — {friend.DisplayName}");
                else
                    lines.Add("Best Friend — none adjacent");
            }

            if (blob.HasViscous)
                lines.Add("Viscous — cheer may bounce to a nearby blob");
            else if (blob.HasHugs)
                lines.Add($"Hugs — cheer grants {LuckRules.FormatLevels(CheerfulFriendBoostRules.HugsLuckLevelsPerHit)}");
            else if (blob.HasRewind)
                lines.Add("Rewind — slower attacks; shots push glitches backward");
            else if (blob.HasResume)
                lines.Add("Resume — every 3rd shot buffs an adjacent blob");
            else if (blob.HasDanger)
                lines.Add($"Danger — up to {CountdownRules.DangerMaxMinesPerTile} mines/tile, {CountdownRules.DangerBlastRangeSize}×{CountdownRules.DangerBlastRangeSize} blast");
            else if (blob.HasHigher)
                lines.Add("Higher — all stacked mines detonate together");
            else if (blob.HasMind)
                lines.Add("Mind — pulse grants Strife +1 to affected blobs");
            else if (blob.HasHeart)
                lines.Add("Heart — pulse dmg to all blobs; extra Bench Trio bonus");

            if (blob.HasPuppeteer)
                lines.Add("Puppeteer");
            else if (blob.HasDivineAssist)
                lines.Add("Divine Assist — sky strike");
            else if (blob.HasPandas)
                lines.Add("Pandas — buff nearby blobs");
            else if (blob.HasHeartEater)
                lines.Add($"Heart Eater — {LuckRules.FormatLevels(SlimySupportTypeRules.HeartEaterLuckBonus)}");

            if (AbsentHistoryTypeRules.FeatureEnabled && blob.HasType(AbsentHistoryTypeRules.Label))
                lines.Add("Absent History — Looped Sight; far-ring bonus");

            BlobModifierSummary.AppendSummaryLines(blob, lines);
            BlobModifierSummary.AppendTemporaryBuffLines(blob, lines);

            return lines;
        }

        void ApplyExistingPanelLayout()
        {
            if (panelRoot == null) return;

            EnsureModifiersPopup();
            EnsureModifiersButton();
            EnsureDeleteButton();
            EnsureInfoButton();
            ApplyUpgradeButtonLayout();
            HideLegacyBuffsText();

            if (nameText != null)
            {
                var rt = nameText.rectTransform;
                rt.anchorMin = new Vector2(0.06f, 0.78f);
                rt.anchorMax = new Vector2(0.84f, 0.96f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }

            EnsureInfoButton();

            EnsureTypeIcon();
            ApplyTypeRowLayout();

            if (modifiersButton != null)
            {
                var rt = modifiersButton.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.06f, 0.48f);
                rt.anchorMax = new Vector2(0.94f, 0.60f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
        }

        void HidePanel()
        {
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(false);
        }

        void ShowHint()
        {
            if (!ShouldShowUpgradeHint() || _hintDismissed || hintText == null) return;
            hintText.gameObject.SetActive(true);
        }

        void DismissHint()
        {
            if (_hintDismissed || hintText == null || !hintText.gameObject.activeSelf)
                return;

            _hintDismissed = true;
            hintText.gameObject.SetActive(false);
        }

        void EnsureUiBuilt()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            if (panelRoot != null)
            {
                EnsureModifiersPopup();
                EnsureDeleteButton();
                EnsureInfoButton();
                EnsureTypeIcon();
                ApplyTypeRowLayout();
                ApplyUpgradeButtonLayout();
                HideLegacyBuffsText();
                return;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (ShouldShowUpgradeHint())
            {
                hintText = CreateText(canvas.transform, "UpgradeHint",
                    new Vector2(0.2f, 0.14f), new Vector2(0.8f, 0.2f), 16, TextAnchor.MiddleCenter);
                hintText.text = "Click on a Blob to open upgrade menu";
                hintText.gameObject.SetActive(false);
            }

            var panelGo = new GameObject("BlobUpgradePanel");
            panelGo.transform.SetParent(canvas.transform, false);
            panelRoot = panelGo.AddComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.58f, 0.12f);
            panelRoot.anchorMax = new Vector2(0.98f, 0.42f);
            panelRoot.offsetMin = panelRoot.offsetMax = Vector2.zero;

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.14f, 0.32f, 0.92f);

            nameText = CreateText(panelGo.transform, "Name",
                new Vector2(0.06f, 0.78f), new Vector2(0.84f, 0.96f), 18, TextAnchor.UpperCenter);
            nameText.fontStyle = FontStyle.Bold;

            infoButton = CreateInfoButton(panelGo.transform);

            typeText = CreateText(panelGo.transform, "Type",
                new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.78f), 14, TextAnchor.MiddleLeft);
            typeText.color = new Color(0.78f, 0.82f, 0.95f);
            EnsureTypeIcon();
            ApplyTypeRowLayout();

            modifiersButton = CreateButton(panelGo.transform, "ModifiersBtn",
                new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.60f), font);
            var modifiersLabel = modifiersButton.GetComponentInChildren<Text>();
            if (modifiersLabel != null)
                modifiersLabel.text = "Modifiers";

            speedButton = CreateButton(panelGo.transform, "SpeedBtn",
                new Vector2(0.06f, 0.28f), new Vector2(0.48f, 0.46f), font);
            damageButton = CreateButton(panelGo.transform, "DamageBtn",
                new Vector2(0.52f, 0.28f), new Vector2(0.94f, 0.46f), font);
            deleteButton = CreateButton(panelGo.transform, "DeleteBtn",
                new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.24f), font);

            var deleteLabel = deleteButton.GetComponentInChildren<Text>();
            if (deleteLabel != null)
            {
                deleteLabel.text = "Delete Blob";
                deleteLabel.color = new Color(1f, 0.82f, 0.82f);
            }

            var deleteImage = deleteButton.GetComponent<Image>();
            if (deleteImage != null)
                deleteImage.color = new Color(0.42f, 0.16f, 0.18f, 0.95f);

            EnsureModifiersPopup();
        }

        void EnsureModifiersPopup()
        {
            if (modifiersPopupRoot != null) return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var popupGo = new GameObject("BlobModifiersPopup");
            popupGo.transform.SetParent(canvas.transform, false);
            modifiersPopupRoot = popupGo.AddComponent<RectTransform>();
            modifiersPopupRoot.anchorMin = new Vector2(SidePanelMinX, ModifiersPopupMinY);
            modifiersPopupRoot.anchorMax = new Vector2(SidePanelMaxX, ModifiersPopupMaxY);
            modifiersPopupRoot.offsetMin = modifiersPopupRoot.offsetMax = Vector2.zero;

            var bg = popupGo.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.12f, 0.24f, 0.96f);

            var title = CreateText(popupGo.transform, "Title",
                new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.96f), 15, TextAnchor.UpperCenter);
            title.fontStyle = FontStyle.Bold;
            title.text = "Modifiers";

            modifiersPopupText = CreateText(popupGo.transform, "Body",
                new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.76f), 11, TextAnchor.UpperLeft);
            modifiersPopupText.color = new Color(0.85f, 0.95f, 0.88f);
            modifiersPopupText.horizontalOverflow = HorizontalWrapMode.Wrap;
            modifiersPopupText.verticalOverflow = VerticalWrapMode.Overflow;
            modifiersPopupText.lineSpacing = 1.05f;

            popupGo.SetActive(false);
        }

        void EnsureInfoButton()
        {
            if (infoButton != null || panelRoot == null)
                return;

            infoButton = CreateInfoButton(panelRoot);
        }

        static Button CreateInfoButton(Transform parent)
        {
            var button = CreateButton(parent, "InfoBtn",
                new Vector2(0.86f, 0.84f), new Vector2(0.96f, 0.96f),
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "i";
                label.fontStyle = FontStyle.Bold;
                label.fontSize = 14;
            }

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = new Color(0.22f, 0.2f, 0.38f, 0.95f);

            return button;
        }

        void HideLegacyBuffsText()
        {
            if (panelRoot == null) return;
            var legacy = panelRoot.Find("Buffs");
            if (legacy != null)
                legacy.gameObject.SetActive(false);
        }

        void EnsureModifiersButton()
        {
            if (modifiersButton != null || panelRoot == null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            modifiersButton = CreateButton(panelRoot, "ModifiersBtn",
                new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.60f), font);
            var label = modifiersButton.GetComponentInChildren<Text>();
            if (label != null)
                label.text = "Modifiers";
        }

        void EnsureDeleteButton()
        {
            if (deleteButton != null || panelRoot == null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            deleteButton = CreateButton(panelRoot, "DeleteBtn",
                new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.24f), font);

            var deleteLabel = deleteButton.GetComponentInChildren<Text>();
            if (deleteLabel != null)
            {
                deleteLabel.text = "Delete Blob";
                deleteLabel.color = new Color(1f, 0.82f, 0.82f);
            }

            var deleteImage = deleteButton.GetComponent<Image>();
            if (deleteImage != null)
                deleteImage.color = new Color(0.42f, 0.16f, 0.18f, 0.95f);
        }

        void ApplyUpgradeButtonLayout()
        {
            ApplyButtonLayout(modifiersButton, new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.60f));
            ApplyButtonLayout(speedButton, new Vector2(0.06f, 0.28f), new Vector2(0.48f, 0.46f));
            ApplyButtonLayout(damageButton, new Vector2(0.52f, 0.28f), new Vector2(0.94f, 0.46f));
            ApplyButtonLayout(deleteButton, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.24f));
        }

        void RefreshTypeIcon()
        {
            EnsureTypeIcon();
            if (typeIconImage == null) return;

            var showIcon = HarmonyTypeRules.FeatureEnabled && _selected != null && _selected.IsHarmonyType;
            typeIconImage.gameObject.SetActive(showIcon);
            if (!showIcon) return;

            var sprite = HarmonySymbolSprites.Icon;
            typeIconImage.sprite = sprite;
            typeIconImage.enabled = sprite != null;
        }

        void EnsureTypeIcon()
        {
            if (typeIconImage != null || panelRoot == null) return;

            var iconGo = new GameObject("TypeIcon");
            iconGo.transform.SetParent(panelRoot, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.08f, 0.655f);
            iconRect.anchorMax = new Vector2(0.16f, 0.745f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;

            typeIconImage = iconGo.AddComponent<Image>();
            typeIconImage.preserveAspect = true;
            typeIconImage.raycastTarget = false;
            typeIconImage.color = Color.white;
        }

        void ApplyTypeRowLayout()
        {
            if (typeText == null) return;

            var rt = typeText.rectTransform;
            rt.anchorMin = new Vector2(0.18f, 0.62f);
            rt.anchorMax = new Vector2(0.94f, 0.78f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            typeText.alignment = TextAnchor.MiddleLeft;

            if (typeIconImage != null)
            {
                var iconRect = typeIconImage.rectTransform;
                iconRect.anchorMin = new Vector2(0.08f, 0.655f);
                iconRect.anchorMax = new Vector2(0.16f, 0.745f);
                iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            }
        }

        static void ApplyButtonLayout(Button button, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (button == null) return;
            var rt = button.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = new Color(0.92f, 0.9f, 1f);
            return t;
        }

        static Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.28f, 0.22f, 0.48f, 0.95f);

            var btn = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var txt = labelGo.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 11;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }
    }
}
