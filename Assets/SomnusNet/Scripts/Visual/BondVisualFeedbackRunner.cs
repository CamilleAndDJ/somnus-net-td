using System.Collections;
using SomnusNet.UI;
using UnityEngine;

namespace SomnusNet.Visual
{
    public class BondVisualFeedbackRunner : MonoBehaviour
    {
        const float HudSyncDelay = 0.24f;

        static BondVisualFeedbackRunner _instance;

        public static BondVisualFeedbackRunner Ensure()
        {
            if (_instance != null)
                return _instance;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return null;

            var go = new GameObject("BondVisualFeedbackRunner");
            go.transform.SetParent(canvas.transform, false);
            _instance = go.AddComponent<BondVisualFeedbackRunner>();
            return _instance;
        }

        public void QueueSwirl(TypeHudLayout.Kind kind) => StartCoroutine(PlayWhenReady(kind, true));

        public void QueueComplete(TypeHudLayout.Kind kind) => StartCoroutine(PlayWhenReady(kind, false));

        IEnumerator PlayWhenReady(TypeHudLayout.Kind kind, bool swirl)
        {
            yield return new WaitForSeconds(HudSyncDelay);
            TypeHudLayout.RefreshNow();

            var icon = TypeHudLayout.TryGetIconRect(kind);
            if (icon == null || !icon.gameObject.activeInHierarchy)
                yield break;

            if (swirl)
            {
                BondSwirlEffect.PlayOnIcon(icon);
            }
            else
            {
                BondCompleteSparkleEffect.PlayOnIcon(icon);
                BondCompleteSparkleEffect.PlayOnGroupMembers(kind);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
