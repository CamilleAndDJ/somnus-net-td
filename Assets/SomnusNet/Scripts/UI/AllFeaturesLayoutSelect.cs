using SomnusNet.Core;
using SomnusNet.Gameplay;
using SomnusNet.Visual;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class AllFeaturesLayoutSelect : MonoBehaviour
    {
        RectTransform _overlayRoot;

        public static void EnsureExists()
        {
            if (!LevelLayoutSession.RequiresSelection || LevelLayoutSession.IsReady)
                return;

            if (FindFirstObjectByType<AllFeaturesLayoutSelect>() != null)
                return;

            var go = new GameObject("AllFeaturesLayoutSelect");
            go.AddComponent<AllFeaturesLayoutSelect>();
        }

        void Start()
        {
            if (!LevelLayoutSession.RequiresSelection || LevelLayoutSession.IsReady)
            {
                Destroy(gameObject);
                return;
            }

            Time.timeScale = 0f;
            EnsureUiBuilt();
        }

        void EnsureUiBuilt()
        {
            if (_overlayRoot != null)
                return;

            var es = new GameObject("LayoutSelectEventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var canvasGo = new GameObject("LayoutSelectCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _overlayRoot = UiPanelFactory.CreateRect(canvasGo.transform, "LayoutSelectOverlay",
                Vector2.zero, Vector2.one);
            var bg = _overlayRoot.gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.14f, 1f);
            bg.raycastTarget = true;

            var title = UiPanelFactory.CreateText(_overlayRoot, "Title",
                new Vector2(0.10f, 0.70f), new Vector2(0.90f, 0.90f),
                54, TextAnchor.MiddleCenter, "Choose Level Layout");
            title.fontStyle = FontStyle.Bold;

            const float buttonWidth = 0.14f;
            const float buttonHeight = 0.30f;
            const float buttonGap = 0.03f;
            const float rowBottom = 0.35f;
            var buttonCount = 5f;
            var pairWidth = buttonWidth * buttonCount + buttonGap * (buttonCount - 1f);
            var rowLeft = (1f - pairWidth) * 0.5f;

            CreateLayoutButton(_overlayRoot, "BeginingLayoutButton",
                new Vector2(rowLeft, rowBottom),
                new Vector2(rowLeft + buttonWidth, rowBottom + buttonHeight),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.Begining),
                () => ConfirmSelection(LevelLayoutId.Begining));

            var doubleLeft = rowLeft + buttonWidth + buttonGap;
            CreateLayoutButton(_overlayRoot, "DoubleTroubleLayoutButton",
                new Vector2(doubleLeft, rowBottom),
                new Vector2(doubleLeft + buttonWidth, rowBottom + buttonHeight),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.DoubleTrouble),
                () => ConfirmSelection(LevelLayoutId.DoubleTrouble));

            var outerRingLeft = doubleLeft + buttonWidth + buttonGap;
            CreateLayoutButton(_overlayRoot, "OuterRingLayoutButton",
                new Vector2(outerRingLeft, rowBottom),
                new Vector2(outerRingLeft + buttonWidth, rowBottom + buttonHeight),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.OuterRing),
                () => ConfirmSelection(LevelLayoutId.OuterRing));

            var notebookLeft = outerRingLeft + buttonWidth + buttonGap;
            CreateLayoutButton(_overlayRoot, "NotebookTestLayoutButton",
                new Vector2(notebookLeft, rowBottom),
                new Vector2(notebookLeft + buttonWidth, rowBottom + buttonHeight),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.NotebookTest),
                () => ConfirmSelection(LevelLayoutId.NotebookTest));

            var makerLeft = notebookLeft + buttonWidth + buttonGap;
            CreateLayoutButton(_overlayRoot, "LayoutMakerButton",
                new Vector2(makerLeft, rowBottom),
                new Vector2(makerLeft + buttonWidth, rowBottom + buttonHeight),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.LayoutMaker),
                () => ConfirmSelection(LevelLayoutId.LayoutMaker));
        }

        static void CreateLayoutButton(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            string label, UnityEngine.Events.UnityAction onClick)
        {
            var button = UiPanelFactory.CreateButton(parent, name, anchorMin, anchorMax, label, 26);
            var labelText = button.GetComponentInChildren<Text>();
            if (labelText != null)
            {
                labelText.horizontalOverflow = HorizontalWrapMode.Wrap;
                labelText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            button.onClick.AddListener(onClick);
        }

        void ConfirmSelection(LevelLayoutId layout)
        {
            var showNotebook = layout == LevelLayoutId.NotebookTest;
            if (showNotebook)
                GameManager.Instance?.HoldLevelStart();

            LevelLayoutSession.Select(layout);

            var grid = GridManager.Instance;
            if (grid != null)
                grid.InitializePath();

            var board = FindFirstObjectByType<GridBoardRenderer>();
            if (board != null)
                board.BuildGrid();

            if (layout == LevelLayoutId.LayoutMaker)
            {
                PonderEconomy.Instance?.ApplyLayoutMakerStart();
                FindFirstObjectByType<PlacementController>()?.ClearSelection();
                LayoutMakerController.EnsureExists();
            }

            if (showNotebook)
                NotebookPreStartController.Show();
            else
                Time.timeScale = 1f;

            Destroy(gameObject);
        }
    }
}
