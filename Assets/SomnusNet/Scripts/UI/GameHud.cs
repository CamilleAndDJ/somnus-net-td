using System;
using System.Collections;
using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Gameplay;
using SomnusNet.Units;
using SomnusNet.Visual;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class GameHud : MonoBehaviour
    {
        const int BlobsPerShopPage = 4;
        const float ShopButtonWidth = 0.085f;
        const float ShopButtonHeight = 0.10f;
        const float ShopButtonGap = 0.02f;
        const float ShopBottomY = 0.02f;
        const float ShopArrowWidth = 0.045f;
        const float ShopArrowGap = 0.012f;

        static readonly BlobKind[] ShopDisplayOrder =
        {
            BlobKind.Gatekeeper,
            BlobKind.Creator,
            BlobKind.FourOhFour,
            BlobKind.Blaze,
            BlobKind.Dealer,
            BlobKind.Cheerful,
            BlobKind.Archivist,
            BlobKind.Countdown,
            BlobKind.Lantern
        };

        [SerializeField] Text pondersText;
        [SerializeField] Text statusText;
        [SerializeField] Text levelCompleteText;
        [SerializeField] PlacementController placement;
        [SerializeField] GameCatalog catalog;
        [SerializeField] Button[] blobButtons;
        [SerializeField] BlobKind[] blobButtonKinds;

        bool _levelCompleteShown;
        Coroutine _bannerRoutine;
        readonly List<Button> _shopButtons = new();
        readonly List<BlobKind> _shopKinds = new();

        Button _shopPageLeft;
        Button _shopPageRight;
        int _currentShopPage;
        int _shopUnlockRound = 1;

        public event Action<BlobKind> ShopBlobSelected;

        void Start()
        {
            DisablePassiveHudRaycasts();
            InitializeShopButtons();
            SortShopButtonsByDisplayOrder();
            EnsureShopPageArrows();
            HideLegacyUi();
            RefreshShopForRound(GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1);

            PonderEconomy.Instance.OnPondersChanged += UpdatePonders;
            UpdatePonders(PonderEconomy.Instance.Current);

            var gm = GameManager.Instance;
            gm.OnDefeatCountChanged += OnDefeatCountChanged;
            gm.OnRoundComplete += HandleRoundComplete;
            gm.OnRoundStarted += HandleRoundStarted;
            gm.OnLevelComplete += HandleLevelComplete;

            EnsureLevelCompleteText();
            if (levelCompleteText != null)
                levelCompleteText.gameObject.SetActive(false);

            UpdateStatus();

            HarmonyTypeHud.EnsureExists();
            SpeedTypeHud.EnsureExists();
            DreamTeamTypeHud.EnsureExists();
            AbsentHistoryTypeHud.EnsureExists();
            BrokenRingsTypeHud.EnsureExists();
            SlimySupportTypeHud.EnsureExists();
            EntertainerTypeHud.EnsureExists();
            BenchTrioTypeHud.EnsureExists();
            BurdenedCrownTypeHud.EnsureExists();
            TypeHudLayoutDriver.EnsureExists();
            DealerSecondVisualizer.EnsureExists();
            CheerfulHeartVisualizer.EnsureExists();
            PlacementHoverVisualizer.EnsureExists();
            AllFeaturesLayoutSelect.EnsureExists();
            BondVisualFeedback.ResetBaseline();
            BondVisualFeedbackRunner.Ensure();
            PauseMenu.EnsureExists();
            if (StoryModeRules.HasDialogs)
            {
                StoryModeIntroController.EnsureExists();
                StoryModeHarmonyTutorial.EnsureExists();
                StoryModeRoundTwoTutorial.EnsureExists();
                StoryModeSkyCompassRewardPopup.EnsureExists();
            }

            WireShopButtons();
        }

        void DisablePassiveHudRaycasts()
        {
            SetTextRaycast(pondersText, false);
            SetTextRaycast(statusText, false);
            SetTextRaycast(levelCompleteText, false);
        }

        static void SetTextRaycast(Text text, bool raycastTarget)
        {
            if (text != null)
                text.raycastTarget = raycastTarget;
        }

        void InitializeShopButtons()
        {
            _shopButtons.Clear();
            _shopKinds.Clear();

            if (blobButtons != null && blobButtonKinds != null)
            {
                for (var i = 0; i < blobButtons.Length && i < blobButtonKinds.Length; i++)
                {
                    if (blobButtons[i] == null) continue;
                    _shopButtons.Add(blobButtons[i]);
                    _shopKinds.Add(BlobKinds.Normalize(blobButtonKinds[i]));
                }
            }

            if (StoryModeRules.Active)
                EnsureLimitedShopButtons(StoryModeRules.ShopBlobOrder);
            else
                EnsureCatalogShopButtons();
        }

        void SortShopButtonsByDisplayOrder()
        {
            var orderedButtons = new List<Button>();
            var orderedKinds = new List<BlobKind>();
            var used = new HashSet<Button>();

            foreach (var kind in ShopDisplayOrder)
            {
                for (var i = 0; i < _shopKinds.Count; i++)
                {
                    if (_shopKinds[i] != kind || _shopButtons[i] == null || used.Contains(_shopButtons[i]))
                        continue;

                    orderedButtons.Add(_shopButtons[i]);
                    orderedKinds.Add(kind);
                    used.Add(_shopButtons[i]);
                    break;
                }
            }

            for (var i = 0; i < _shopButtons.Count; i++)
            {
                if (_shopButtons[i] == null || used.Contains(_shopButtons[i]))
                    continue;

                orderedButtons.Add(_shopButtons[i]);
                orderedKinds.Add(_shopKinds[i]);
            }

            _shopButtons.Clear();
            _shopKinds.Clear();
            _shopButtons.AddRange(orderedButtons);
            _shopKinds.AddRange(orderedKinds);
        }

        void EnsureShopPageArrows()
        {
            if (_shopPageLeft != null && _shopPageRight != null)
                return;

            if (_shopButtons.Count == 0 || _shopButtons[0] == null)
                return;

            var parent = _shopButtons[0].transform.parent;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _shopPageLeft = CreateShopArrowButton(parent, "ShopPageLeft", "\u2190", font);
            _shopPageRight = CreateShopArrowButton(parent, "ShopPageRight", "\u2192", font);
            _shopPageLeft.onClick.AddListener(GoToPreviousShopPage);
            _shopPageRight.onClick.AddListener(GoToNextShopPage);
        }

        static Button CreateShopArrowButton(Transform parent, string name, string label, Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.25f, 0.2f, 0.45f, 0.85f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            var text = labelGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;

            return button;
        }

        void GoToPreviousShopPage()
        {
            var pageCount = GetShopPageCount();
            if (pageCount <= 1) return;

            _currentShopPage = (_currentShopPage - 1 + pageCount) % pageCount;
            RefreshShopPage();
        }

        void GoToNextShopPage()
        {
            var pageCount = GetShopPageCount();
            if (pageCount <= 1) return;

            _currentShopPage = (_currentShopPage + 1) % pageCount;
            RefreshShopPage();
        }

        int GetShopPageCount() =>
            Mathf.Max(1, Mathf.CeilToInt(_shopKinds.Count / (float)BlobsPerShopPage));

        bool IsShopEntryUnlocked(int index, int round)
        {
            if (index < 0 || index >= _shopKinds.Count)
                return false;

            return LevelShopRules.IsBlobUnlocked(_shopKinds[index], round);
        }

        void RefreshShopPage()
        {
            EnsureShopPageArrows();

            var pageCount = GetShopPageCount();
            if (_currentShopPage >= pageCount)
                _currentShopPage = 0;

            for (var i = 0; i < _shopButtons.Count; i++)
            {
                if (_shopButtons[i] == null)
                    continue;

                var onPage = i / BlobsPerShopPage == _currentShopPage;
                var unlocked = IsShopEntryUnlocked(i, _shopUnlockRound);
                _shopButtons[i].gameObject.SetActive(onPage && unlocked);
            }

            var showArrows = pageCount > 1;
            if (_shopPageLeft != null)
                _shopPageLeft.gameObject.SetActive(showArrows);
            if (_shopPageRight != null)
                _shopPageRight.gameObject.SetActive(showArrows);

            LayoutShopBar();

            if (placement == null || !placement.Selected.HasValue)
                return;

            if (!LevelShopRules.IsBlobUnlocked(placement.Selected.Value, _shopUnlockRound))
                placement.SelectBlob(BlobKind.Gatekeeper);
        }

        void EnsureCatalogShopButtons()
        {
            if (catalog == null || catalog.blobs == null || _shopButtons.Count == 0 || _shopButtons[0] == null)
                return;

            var template = _shopButtons[0];
            var parent = template.transform.parent;

            foreach (var def in catalog.blobs)
            {
                if (def == null)
                    continue;

                var kind = BlobKinds.Normalize(def.kind);
                if (_shopKinds.Contains(kind))
                    continue;

                var cloneGo = Instantiate(template.gameObject, parent);
                cloneGo.name = $"BlobBtn_{kind}";
                cloneGo.SetActive(true);

                var button = cloneGo.GetComponent<Button>();
                if (button == null)
                    continue;

                _shopButtons.Add(button);
                _shopKinds.Add(kind);
            }
        }

        void EnsureLimitedShopButtons(BlobKind[] order)
        {
            if (_shopButtons.Count == 0 || _shopButtons[0] == null)
                return;

            var template = _shopButtons[0];
            var parent = template.transform.parent;

            foreach (var kind in order)
            {
                if (_shopKinds.Contains(kind))
                    continue;

                var cloneGo = Instantiate(template.gameObject, parent);
                cloneGo.name = $"BlobBtn_{kind}";
                cloneGo.SetActive(false);

                var button = cloneGo.GetComponent<Button>();
                if (button == null)
                    continue;

                _shopButtons.Add(button);
                _shopKinds.Add(kind);
            }
        }

        void WireShopButtons()
        {
            for (var i = 0; i < _shopButtons.Count; i++)
            {
                var button = _shopButtons[i];
                if (button == null) continue;

                var kind = _shopKinds[i];
                if (!catalog.TryGetBlob(kind, out var def))
                    continue;
                SetupBlobPurchaseButton(button, def, kind);

                button.onClick.RemoveAllListeners();
                var captured = kind;
                button.onClick.AddListener(() =>
                {
                    placement.SelectBlob(captured);
                    ShopBlobSelected?.Invoke(captured);
                });
            }
        }

        public int ShopEntryCount => _shopButtons.Count;

        public bool TryGetShopEntry(int index, out Button button, out BlobKind kind)
        {
            button = null;
            kind = default;
            if (index < 0 || index >= _shopButtons.Count || _shopButtons[index] == null)
                return false;

            button = _shopButtons[index];
            kind = _shopKinds[index];
            return true;
        }

        public bool TryGetShopButtonRect(BlobKind kind, out RectTransform rect)
        {
            rect = null;
            kind = BlobKinds.Normalize(kind);

            for (var i = 0; i < _shopKinds.Count; i++)
            {
                if (_shopKinds[i] != kind || _shopButtons[i] == null)
                    continue;

                if (!_shopButtons[i].gameObject.activeInHierarchy)
                    continue;

                rect = _shopButtons[i].GetComponent<RectTransform>();
                return rect != null;
            }

            return false;
        }

        public bool TryGetPondersTextRect(out RectTransform rect)
        {
            rect = pondersText != null ? pondersText.rectTransform : null;
            return rect != null;
        }

        public bool TryGetShopButtonScreenAnchor(BlobKind kind, out Vector2 screenPoint)
        {
            screenPoint = default;
            if (!TryGetShopButtonRect(kind, out var rect))
                return false;

            return TryGetRectTopCenterScreenPoint(rect, out screenPoint);
        }

        static bool TryGetRectTopCenterScreenPoint(RectTransform rect, out Vector2 screenPoint)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            // Top-center of the shop button — arrow sits above this and points down.
            screenPoint = (corners[1] + corners[2]) * 0.5f;
            return true;
        }

        public void RefreshShopForRound(int round)
        {
            _shopUnlockRound = round;

            if (!StoryModeRules.Active)
            {
                RefreshShopPage();
                return;
            }

            for (var i = 0; i < _shopButtons.Count; i++)
            {
                if (_shopButtons[i] == null)
                    continue;

                if (!LevelShopRules.IsBlobUnlocked(_shopKinds[i], round))
                    continue;

                _currentShopPage = i / BlobsPerShopPage;
                break;
            }

            RefreshShopPage();
            RefreshShopTypeBadges();

            if (placement != null && placement.Selected.HasValue &&
                !LevelShopRules.IsBlobUnlocked(placement.Selected.Value, round))
                placement.SelectBlob(BlobKind.Gatekeeper);
        }

        public void RefreshShopTypeBadges()
        {
            if (catalog == null)
                return;

            for (var i = 0; i < _shopButtons.Count; i++)
            {
                var button = _shopButtons[i];
                if (button == null)
                    continue;

                if (!catalog.TryGetBlob(_shopKinds[i], out var def))
                    continue;

                ShopTypeBadges.Apply(button, def);
            }
        }

        static void SetupBlobPurchaseButton(Button button, BlobDefinition def, BlobKind kind)
        {
            if (button == null || def == null)
                return;

            var label = button.GetComponentInChildren<Text>();
            if (label == null)
                return;

            label.text = $"{GetShopDisplayName(def)}\n{def.ponderCost}";

            var resolvedKind = BlobKinds.Normalize(def.kind);

            var shopSprite = resolvedKind switch
            {
                BlobKind.Gatekeeper => GatekeeperBlobSprites.ShopSprite,
                BlobKind.Creator => CreatorBlobSprites.ShopSprite,
                BlobKind.FourOhFour => FourOhFourBlobSprites.ShopSprite,
                BlobKind.Blaze => BlazeBlobSprites.ShopSprite,
                BlobKind.Dealer => DealerBlobSprites.ShopSprite,
                BlobKind.Cheerful => CheerfulBlobSprites.ShopSprite,
                BlobKind.Archivist => ArchivistBlobSprites.ShopSprite,
                BlobKind.Countdown => CountdownBlobSprites.ShopSprite,
                BlobKind.Lantern => LanternBlobSprites.ShopSprite,
                _ => def.sprite
            };

            if (shopSprite == null)
            {
                label.fontSize = 11;
                label.alignment = TextAnchor.MiddleRight;
                var defaultRect = label.GetComponent<RectTransform>();
                defaultRect.anchorMin = Vector2.zero;
                defaultRect.anchorMax = Vector2.one;
                defaultRect.offsetMin = Vector2.zero;
                defaultRect.offsetMax = new Vector2(-6f, 0f);
                ShopTypeBadges.Apply(button, def);
                return;
            }

            label.fontSize = 10;
            label.alignment = TextAnchor.MiddleRight;

            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.36f, 0.05f);
            labelRect.anchorMax = new Vector2(0.96f, 0.95f);
            labelRect.offsetMin = new Vector2(0f, 0f);
            labelRect.offsetMax = new Vector2(-6f, 0f);

            var iconTransform = button.transform.Find("Icon");
            GameObject iconGo;
            if (iconTransform != null)
                iconGo = iconTransform.gameObject;
            else
            {
                iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(button.transform, false);
            }

            var iconRect = iconGo.GetComponent<RectTransform>();
            if (iconRect == null)
                iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.04f, 0.08f);
            iconRect.anchorMax = new Vector2(0.34f, 0.92f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;

            var iconImage = iconGo.GetComponent<Image>();
            if (iconImage == null)
                iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = shopSprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.color = Color.white;
            MatchBasicShopIconSize(iconRect, shopSprite, resolvedKind);
            ShopTypeBadges.Apply(button, def);
        }

        static string GetShopDisplayName(BlobDefinition def)
        {
            if (def == null)
                return "Blob";

            return BlobKinds.Normalize(def.kind) switch
            {
                BlobKind.Gatekeeper => "Gate",
                BlobKind.Cheerful => "Cheer",
                BlobKind.Archivist => "Archi",
                BlobKind.Countdown => "Count",
                _ => def.displayName
            };
        }

        static void MatchBasicShopIconSize(RectTransform iconRect, Sprite shopSprite, BlobKind kind)
        {
            iconRect.localScale = Vector3.one;
            if (kind != BlobKind.Gatekeeper || shopSprite == null)
                return;

            var dreamSprite = CreatorBlobSprites.ShopSprite;
            if (dreamSprite == null || shopSprite == dreamSprite)
                return;

            var dreamAspect = dreamSprite.rect.width / dreamSprite.rect.height;
            var basicAspect = shopSprite.rect.width / shopSprite.rect.height;
            if (basicAspect <= dreamAspect)
                return;

            iconRect.localScale = Vector3.one * (basicAspect / dreamAspect);
        }

        void HideLegacyUi()
        {
            Transform canvas = transform;
            if (blobButtons.Length > 0 && blobButtons[0] != null)
                canvas = blobButtons[0].transform.parent;

            var briefing = canvas.Find("Briefing");
            if (briefing != null)
                briefing.gameObject.SetActive(false);

            var restart = canvas.Find("Restart");
            if (restart != null)
                restart.gameObject.SetActive(false);

            for (var i = 1; i < blobButtons.Length; i++)
            {
                if (blobButtons[i] != null && i >= blobButtonKinds.Length)
                    blobButtons[i].gameObject.SetActive(false);
            }
        }

        void LayoutShopBar()
        {
            var blobStripWidth = BlobsPerShopPage * ShopButtonWidth + (BlobsPerShopPage - 1) * ShopButtonGap;
            var showArrows = _shopPageLeft != null && _shopPageLeft.gameObject.activeSelf;
            var totalWidth = showArrows
                ? ShopArrowWidth + ShopArrowGap + blobStripWidth + ShopArrowGap + ShopArrowWidth
                : blobStripWidth;
            var startX = 0.5f - totalWidth * 0.5f;
            var blobStartX = showArrows ? startX + ShopArrowWidth + ShopArrowGap : startX;

            if (_shopPageLeft != null && showArrows)
            {
                var leftRect = _shopPageLeft.GetComponent<RectTransform>();
                leftRect.anchorMin = new Vector2(startX, ShopBottomY);
                leftRect.anchorMax = new Vector2(startX + ShopArrowWidth, ShopBottomY + ShopButtonHeight);
                leftRect.offsetMin = leftRect.offsetMax = Vector2.zero;
            }

            if (_shopPageRight != null && showArrows)
            {
                var rightX = blobStartX + blobStripWidth + ShopArrowGap;
                var rightRect = _shopPageRight.GetComponent<RectTransform>();
                rightRect.anchorMin = new Vector2(rightX, ShopBottomY);
                rightRect.anchorMax = new Vector2(rightX + ShopArrowWidth, ShopBottomY + ShopButtonHeight);
                rightRect.offsetMin = rightRect.offsetMax = Vector2.zero;
            }

            for (var slot = 0; slot < BlobsPerShopPage; slot++)
            {
                var index = _currentShopPage * BlobsPerShopPage + slot;
                if (index >= _shopButtons.Count)
                    continue;

                var button = _shopButtons[index];
                if (button == null || !button.gameObject.activeSelf)
                    continue;

                var xMin = blobStartX + slot * (ShopButtonWidth + ShopButtonGap);
                var rt = button.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(xMin, ShopBottomY);
                rt.anchorMax = new Vector2(xMin + ShopButtonWidth, ShopBottomY + ShopButtonHeight);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
        }

        void EnsureLevelCompleteText()
        {
            if (levelCompleteText != null) return;

            Transform canvas = transform;
            if (blobButtons.Length > 0 && blobButtons[0] != null)
                canvas = blobButtons[0].transform.parent;

            var go = new GameObject("LevelComplete");
            go.transform.SetParent(canvas, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.4f);
            rt.anchorMax = new Vector2(0.8f, 0.6f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            levelCompleteText = go.AddComponent<Text>();
            levelCompleteText.text = "End of Level 1";
            levelCompleteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelCompleteText.fontSize = 36;
            levelCompleteText.alignment = TextAnchor.MiddleCenter;
            levelCompleteText.color = new Color(0.92f, 0.88f, 1f);
            levelCompleteText.fontStyle = FontStyle.Bold;
        }

        void HandleRoundComplete(int round)
        {
            if (levelCompleteText == null) return;

            var infinite = LevelSettings.Instance != null && LevelSettings.Instance.infiniteRounds;
            if (!StoryModeRules.Active && !infinite && round != 1)
                return;

            if (_bannerRoutine != null)
                StopCoroutine(_bannerRoutine);

            var message = StoryModeRules.Active || infinite
                ? $"End of Round {round}"
                : "End of Round 1";
            _bannerRoutine = StartCoroutine(ShowTemporaryBanner(message, GameManager.RoundEndBannerDuration));
        }

        void HandleRoundStarted(int round)
        {
            RefreshShopForRound(round);
            UpdateStatus();
        }

        void HandleLevelComplete()
        {
            if (LevelSettings.Instance != null && LevelSettings.Instance.infiniteRounds)
                return;

            if (_bannerRoutine != null)
            {
                StopCoroutine(_bannerRoutine);
                _bannerRoutine = null;
            }

            _levelCompleteShown = true;
            if (levelCompleteText != null)
            {
                var banner = LevelSettings.Instance != null
                    ? LevelSettings.Instance.levelCompleteBanner
                    : "End of Level 1";
                levelCompleteText.text = StoryModeRules.Active
                    ? $"{banner}\n\nPress Enter"
                    : banner;
                levelCompleteText.gameObject.SetActive(true);
            }

            UpdateStatus();
        }

        IEnumerator ShowTemporaryBanner(string message, float duration)
        {
            levelCompleteText.text = message;
            levelCompleteText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            if (!_levelCompleteShown && levelCompleteText != null)
                levelCompleteText.gameObject.SetActive(false);
            _bannerRoutine = null;
            UpdateStatus();
        }

        public void HideLevelCompleteBanner()
        {
            if (levelCompleteText != null)
                levelCompleteText.gameObject.SetActive(false);
        }

        void OnDefeatCountChanged(int defeated, int goal) => UpdateStatus();

        void UpdateStatus()
        {
            if (GameManager.Instance == null) return;

            var gm = GameManager.Instance;

            if (levelCompleteText != null && _levelCompleteShown)
                levelCompleteText.gameObject.SetActive(true);

            if (statusText == null) return;
            statusText.text = gm.Phase switch
            {
                GamePhase.Won => string.Empty,
                GamePhase.Lost => "The Swarm reached the Core. Try again.",
                GamePhase.RoundIntermission when gm.GlitchesDefeated == 0 =>
                    $"Round {gm.CurrentRound + 1} — Glitches defeated: 0/{gm.DefeatGoal}",
                GamePhase.RoundIntermission => string.Empty,
                _ => $"Round {gm.CurrentRound} — Glitches defeated: {gm.GlitchesDefeated}/{gm.DefeatGoal}"
            };
        }

        void UpdatePonders(int amount)
        {
            if (pondersText != null)
                pondersText.text = $"Ponders: {amount}";
        }

        void OnDestroy()
        {
            if (_shopPageLeft != null)
                _shopPageLeft.onClick.RemoveListener(GoToPreviousShopPage);
            if (_shopPageRight != null)
                _shopPageRight.onClick.RemoveListener(GoToNextShopPage);

            if (PonderEconomy.Instance != null)
                PonderEconomy.Instance.OnPondersChanged -= UpdatePonders;
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnDefeatCountChanged -= OnDefeatCountChanged;
            GameManager.Instance.OnRoundComplete -= HandleRoundComplete;
            GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
            GameManager.Instance.OnLevelComplete -= HandleLevelComplete;
        }
    }
}
