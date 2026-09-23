using System.Collections;
using System.Collections.Generic;
using SomnusNet.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class NotebookPreStartController : MonoBehaviour
    {
        const int OverlaySortOrder = 210;

        const float PageWidth = 620f;
        const float PageHeight = 780f;
        const float CoverMargin = 14f;
        const float SpineWidth = 10f;
        const int FlipSheetCount = 6;

        const float OpenDelay = 0.4f;
        const float CoverFlipDuration = 0.75f;
        const float SheetFlipDuration = 0.32f;
        const float SheetFlipStagger = 0.09f;
        const float FlipLiftScale = 0.04f;
        const float FlipShade = 0.25f;
        const float StartButtonFadeDuration = 0.35f;

        const int ShopColumns = 4;
        const float ShopCellWidth = 120f;
        const float ShopCellHeight = 80f;
        const float ShopCellGap = 12f;
        const float PageContentLeft = 78f;
        const float PageTitleTop = 28f;
        const float PageTitleHeight = 56f;
        const float ShopGridTop = 108f;

        const float RuledLineTop = 96f;
        const float RuledLineSpacing = 36f;
        const float RuledLineBottomPadding = 30f;
        const float MarginLineX = 64f;

        static readonly Color BackgroundColor = Color.black;
        static readonly Color CoverColor = new(0.24f, 0.18f, 0.38f, 1f);
        static readonly Color PaperColor = new(0.97f, 0.95f, 0.89f, 1f);
        static readonly Color RuledLineColor = new(0.55f, 0.70f, 0.90f, 0.55f);
        static readonly Color MarginLineColor = new(0.90f, 0.40f, 0.42f, 0.6f);
        static readonly Color SpineColor = new(0.10f, 0.08f, 0.16f, 0.85f);
        static readonly Color InkColor = new(0.18f, 0.16f, 0.30f, 1f);

        RectTransform _bookRoot;
        RectTransform _cover;
        Image _coverImage;
        GameObject _coverTitle;
        RectTransform _leftCover;
        RectTransform _leftPage;
        RectTransform _spine;
        readonly List<RectTransform> _sheets = new();
        CanvasGroup _startButtonGroup;
        Button _startButton;

        public static void Show()
        {
            if (FindFirstObjectByType<NotebookPreStartController>() != null)
                return;

            var go = new GameObject("NotebookPreStart");
            go.AddComponent<NotebookPreStartController>();
        }

        void Awake()
        {
            BuildUi();
        }

        void Start()
        {
            StartCoroutine(PlayOpenSequence());
        }

        void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = OverlaySortOrder;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            var background = UiPanelFactory.CreateImage(transform, "Background", Vector2.zero, Vector2.one,
                BackgroundColor);
            background.raycastTarget = true;

            _bookRoot = UiPanelFactory.CreateRect(transform, "Notebook",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _bookRoot.sizeDelta = Vector2.zero;
            _bookRoot.anchoredPosition = new Vector2(ClosedBookOffsetX, 0f);

            var coverWidth = PageWidth + CoverMargin;
            var coverHeight = PageHeight + CoverMargin * 2f;

            CreateBookPiece("RightBackCover", CoverColor, coverWidth, coverHeight, 0f);

            _leftCover = CreateBookPiece("LeftCover", CoverColor, coverWidth, coverHeight, 1f).rectTransform;
            _leftCover.gameObject.SetActive(false);

            var rightPage = CreateBookPiece("RightPage", PaperColor, PageWidth, PageHeight, 0f).rectTransform;
            AddRuledLines(rightPage);
            BuildShopPage(rightPage);

            _leftPage = CreateBookPiece("LeftPage", PaperColor, PageWidth, PageHeight, 1f).rectTransform;
            AddRuledLines(_leftPage);
            _leftPage.gameObject.SetActive(false);

            for (var i = 0; i < FlipSheetCount; i++)
                _sheets.Add(CreateBookPiece($"Sheet{i}", PaperColor, PageWidth, PageHeight, 0f).rectTransform);

            _spine = CreateBookPiece("Spine", SpineColor, SpineWidth, PageHeight, 0.5f).rectTransform;
            _spine.gameObject.SetActive(false);

            _coverImage = CreateBookPiece("Cover", CoverColor, coverWidth, coverHeight, 0f);
            _cover = _coverImage.rectTransform;
            BuildCoverTitle();

            BuildStartButton();
        }

        static float ClosedBookOffsetX => -(PageWidth + CoverMargin) * 0.5f;

        Image CreateBookPiece(string name, Color color, float width, float height, float pivotX)
        {
            var image = UiPanelFactory.CreateImage(_bookRoot, name,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), color);
            var rect = image.rectTransform;
            rect.pivot = new Vector2(pivotX, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
            image.raycastTarget = false;
            return image;
        }

        static void AddRuledLines(RectTransform page)
        {
            for (var y = RuledLineTop; y < PageHeight - RuledLineBottomPadding; y += RuledLineSpacing)
            {
                var line = UiPanelFactory.CreateImage(page, "RuledLine",
                    new Vector2(0f, 1f), new Vector2(1f, 1f), RuledLineColor);
                line.raycastTarget = false;
                var rect = line.rectTransform;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(0f, 2f);
                rect.anchoredPosition = new Vector2(0f, -y);
            }

            var margin = UiPanelFactory.CreateImage(page, "MarginLine",
                new Vector2(0f, 0f), new Vector2(0f, 1f), MarginLineColor);
            margin.raycastTarget = false;
            var marginRect = margin.rectTransform;
            marginRect.pivot = new Vector2(0.5f, 0.5f);
            marginRect.sizeDelta = new Vector2(2f, 0f);
            marginRect.anchoredPosition = new Vector2(MarginLineX, 0f);
        }

        void BuildCoverTitle()
        {
            var plate = UiPanelFactory.CreateImage(_cover, "TitlePlate",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PaperColor);
            plate.raycastTarget = false;
            var plateRect = plate.rectTransform;
            plateRect.sizeDelta = new Vector2(340f, 100f);
            plateRect.anchoredPosition = new Vector2(0f, 120f);

            var title = UiPanelFactory.CreateText(plate.transform, "Title", Vector2.zero, Vector2.one,
                40, TextAnchor.MiddleCenter, "Notebook");
            title.fontStyle = FontStyle.Bold;
            title.color = InkColor;
            title.raycastTarget = false;

            _coverTitle = plate.gameObject;
        }

        void BuildShopPage(RectTransform page)
        {
            var title = UiPanelFactory.CreateText(page, "PageTitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), 34, TextAnchor.MiddleLeft, "Dream Blobs");
            title.fontStyle = FontStyle.Bold;
            title.color = InkColor;
            title.raycastTarget = false;
            var titleRect = title.rectTransform;
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.offsetMin = new Vector2(PageContentLeft, -(PageTitleTop + PageTitleHeight));
            titleRect.offsetMax = new Vector2(-24f, -PageTitleTop);

            var grid = UiPanelFactory.CreateRect(page, "ShopGrid", new Vector2(0f, 1f), new Vector2(0f, 1f));
            grid.pivot = new Vector2(0f, 1f);
            grid.anchoredPosition = new Vector2(PageContentLeft, -ShopGridTop);
            grid.sizeDelta = new Vector2(
                ShopColumns * ShopCellWidth + (ShopColumns - 1) * ShopCellGap,
                PageHeight - ShopGridTop);

            var gridGroup = grid.gameObject.AddComponent<CanvasGroup>();
            gridGroup.interactable = false;
            gridGroup.blocksRaycasts = false;

            var hud = FindFirstObjectByType<GameHud>();
            if (hud == null)
                return;

            var round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
            var slot = 0;
            for (var i = 0; i < hud.ShopEntryCount; i++)
            {
                if (!hud.TryGetShopEntry(i, out var source, out var kind))
                    continue;

                if (!LevelShopRules.IsBlobUnlocked(kind, round))
                    continue;

                var clone = Instantiate(source.gameObject, grid);
                clone.name = $"NotebookShop_{kind}";
                clone.SetActive(true);

                var rect = (RectTransform)clone.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.localScale = Vector3.one;
                rect.sizeDelta = new Vector2(ShopCellWidth, ShopCellHeight);
                rect.anchoredPosition = new Vector2(
                    slot % ShopColumns * (ShopCellWidth + ShopCellGap),
                    -(slot / ShopColumns) * (ShopCellHeight + ShopCellGap));

                var button = clone.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    var colors = button.colors;
                    colors.disabledColor = colors.normalColor;
                    button.colors = colors;
                    button.interactable = false;
                }

                slot++;
            }
        }

        void BuildStartButton()
        {
            _startButton = UiPanelFactory.CreateButton(transform, "GameStartButton",
                new Vector2(0.02f, 0.03f), new Vector2(0.16f, 0.11f), "Game Start", 26);
            _startButton.onClick.AddListener(StartLevel);

            _startButtonGroup = _startButton.gameObject.AddComponent<CanvasGroup>();
            _startButtonGroup.alpha = 0f;
            _startButtonGroup.interactable = false;
            _startButtonGroup.blocksRaycasts = false;
        }

        IEnumerator PlayOpenSequence()
        {
            yield return new WaitForSecondsRealtime(OpenDelay);
            yield return FlipCover();

            for (var i = 0; i < _sheets.Count; i++)
            {
                StartCoroutine(FlipSheet(_sheets[_sheets.Count - 1 - i]));
                yield return new WaitForSecondsRealtime(SheetFlipStagger);
            }

            yield return new WaitForSecondsRealtime(SheetFlipDuration);
            yield return FadeInStartButton();
        }

        IEnumerator FlipCover()
        {
            var passedHalf = false;
            var t = 0f;
            while (t < CoverFlipDuration)
            {
                t += Time.unscaledDeltaTime;
                var eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / CoverFlipDuration));
                _bookRoot.anchoredPosition = new Vector2(Mathf.Lerp(ClosedBookOffsetX, 0f, eased), 0f);
                ApplyFlip(_cover, _coverImage, CoverColor, eased);

                if (!passedHalf && eased >= 0.5f)
                {
                    passedHalf = true;
                    _coverTitle.SetActive(false);
                    _leftCover.gameObject.SetActive(true);
                }

                yield return null;
            }

            _bookRoot.anchoredPosition = Vector2.zero;
            _cover.gameObject.SetActive(false);
            _spine.gameObject.SetActive(true);
        }

        IEnumerator FlipSheet(RectTransform sheet)
        {
            var image = sheet.GetComponent<Image>();
            var passedHalf = false;
            var t = 0f;
            while (t < SheetFlipDuration)
            {
                t += Time.unscaledDeltaTime;
                var eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / SheetFlipDuration));
                ApplyFlip(sheet, image, PaperColor, eased);

                if (!passedHalf && eased >= 0.5f)
                {
                    passedHalf = true;
                    // Pages flipped later land on top of earlier ones on the left side.
                    sheet.SetSiblingIndex(_spine.GetSiblingIndex() - 1);
                    _leftPage.gameObject.SetActive(true);
                }

                yield return null;
            }

            sheet.gameObject.SetActive(false);
        }

        static void ApplyFlip(RectTransform rect, Image image, Color baseColor, float progress)
        {
            var arc = Mathf.Sin(progress * Mathf.PI);
            rect.localRotation = Quaternion.Euler(0f, -180f * progress, 0f);
            rect.localScale = new Vector3(1f, 1f + FlipLiftScale * arc, 1f);
            if (image == null)
                return;

            var shaded = Color.Lerp(baseColor, baseColor * (1f - FlipShade), arc);
            shaded.a = baseColor.a;
            image.color = shaded;
        }

        IEnumerator FadeInStartButton()
        {
            var t = 0f;
            while (t < StartButtonFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _startButtonGroup.alpha = Mathf.Clamp01(t / StartButtonFadeDuration);
                yield return null;
            }

            _startButtonGroup.alpha = 1f;
            _startButtonGroup.interactable = true;
            _startButtonGroup.blocksRaycasts = true;
        }

        void StartLevel()
        {
            _startButton.interactable = false;
            GameManager.Instance?.ReleaseLevelStart();
            Destroy(gameObject);
        }
    }
}
