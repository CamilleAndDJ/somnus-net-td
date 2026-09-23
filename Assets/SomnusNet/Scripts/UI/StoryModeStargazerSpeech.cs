using System.Collections;
using SomnusNet.Core;
using SomnusNet.Units;
using SomnusNet.Visual;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public static class StoryModeStargazerSpeech
    {
        const float Duration = 3.5f;
        const float WorldHeightOffset = 0.55f;
        const int ReadyRetryFrames = 90;

        static bool _shown;
        static StargazerSpeechFollower _active;

        public static void TryShowLeaveItToMe()
        {
            if (!StoryModeRules.Active || _shown)
                return;

            var runner = SpeechRunner.Ensure();
            runner.StartCoroutine(ShowWhenReady());
        }

        static IEnumerator ShowWhenReady()
        {
            for (var i = 0; i < ReadyRetryFrames; i++)
            {
                if (_shown)
                    yield break;

                var stargazer = StargazerRegistry.Current;
                if (stargazer != null && stargazer.IsAlive)
                {
                    _shown = true;
                    ShowSpeech(stargazer, "Leave it to me!", Duration);
                    yield break;
                }

                yield return null;
            }
        }

        public static void ShowMessage(string message, float duration = Duration)
        {
            if (!StoryModeRules.Active)
                return;

            var stargazer = StargazerRegistry.Current;
            if (stargazer == null || !stargazer.IsAlive)
                return;

            ShowSpeech(stargazer, message, duration);
        }

        static void ShowSpeech(DreamBlob stargazer, string message, float duration)
        {
            if (_active != null)
            {
                Object.Destroy(_active.gameObject);
                _active = null;
            }

            var canvas = GetHudCanvas();
            if (canvas == null)
                return;

            var go = new GameObject("StargazerSpeech");
            var follower = go.AddComponent<StargazerSpeechFollower>();
            follower.Initialize(canvas, stargazer, message, duration, WorldHeightOffset);
            _active = follower;
        }

        static Canvas GetHudCanvas()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    return canvas;
            }

            foreach (var canvas in canvases)
            {
                if (canvas.isRootCanvas)
                    return canvas;
            }

            return Object.FindFirstObjectByType<Canvas>();
        }

        internal static void ClearActive(StargazerSpeechFollower follower)
        {
            if (_active == follower)
                _active = null;
        }

        sealed class SpeechRunner : MonoBehaviour
        {
            public static SpeechRunner Ensure()
            {
                var existing = Object.FindFirstObjectByType<SpeechRunner>();
                if (existing != null)
                    return existing;

                var go = new GameObject("StoryModeSpeechRunner");
                return go.AddComponent<SpeechRunner>();
            }
        }
    }

    sealed class StargazerSpeechFollower : MonoBehaviour
    {
        RectTransform _rect;
        RectTransform _layer;
        Canvas _rootCanvas;
        DreamBlob _target;
        float _hideAt;
        float _worldHeightOffset;

        public void Initialize(Canvas rootCanvas, DreamBlob target, string message, float duration,
            float worldHeightOffset)
        {
            _rootCanvas = rootCanvas;
            _target = target;
            _hideAt = Time.unscaledTime + duration;
            _worldHeightOffset = worldHeightOffset;

            _layer = BondEffectOverlay.GetLayer(rootCanvas);
            if (_layer == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.SetParent(_layer, false);
            transform.SetAsLastSibling();

            _rect = gameObject.AddComponent<RectTransform>();
            _rect.pivot = new Vector2(0.5f, 0f);
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
            _rect.sizeDelta = new Vector2(Mathf.Clamp(message.Length * 7f + 24f, 100f, 220f), 28f);

            var text = gameObject.AddComponent<Text>();
            text.font = UiPanelFactory.DefaultFont;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UiPanelFactory.TextColor;
            text.text = message;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            UpdatePosition();
        }

        void LateUpdate()
        {
            if (_target == null || !_target.IsAlive || Time.unscaledTime >= _hideAt)
            {
                StoryModeStargazerSpeech.ClearActive(this);
                Destroy(gameObject);
                return;
            }

            UpdatePosition();
            transform.SetAsLastSibling();
        }

        void UpdatePosition()
        {
            if (_layer == null || _target == null || _rootCanvas == null || _rect == null)
                return;

            var overlayCam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _rootCanvas.worldCamera;
            var worldCam = overlayCam != null ? overlayCam : Camera.main;
            if (worldCam == null)
                return;

            var world = _target.transform.position + Vector3.up * _worldHeightOffset;
            var screen = RectTransformUtility.WorldToScreenPoint(worldCam, world);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, screen, overlayCam, out var local))
                return;

            _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.anchoredPosition = local;
            _rect.localRotation = Quaternion.identity;
            _rect.localScale = Vector3.one;
        }
    }
}
