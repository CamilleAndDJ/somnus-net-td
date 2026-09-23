using System;
using System.Collections;
using SomnusNet.Core;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    public class CountdownMinePlaceEffect : MonoBehaviour
    {
        const float FlightDuration = 0.38f;
        const float ArcHeight = 0.55f;
        const int SortOrder = 13;

        static readonly Color ChargeColor = new(1f, 0.62f, 0.18f, 0.95f);
        static readonly Color TrailColor = new(1f, 0.42f, 0.12f, 0.55f);

        DreamBlob _owner;
        int _col;
        int _row;
        int _damage;
        Action _onComplete;
        SpriteRenderer _charge;
        SpriteRenderer _trail;

        public static void Play(DreamBlob owner, int col, int row, int damage, Action onComplete)
        {
            if (owner == null || !owner.IsAlive || GridManager.Instance == null)
            {
                CountdownLandmineField.ReleasePendingMine(col, row);
                onComplete?.Invoke();
                return;
            }

            var go = new GameObject("CountdownMinePlace");
            var effect = go.AddComponent<CountdownMinePlaceEffect>();
            effect.Begin(owner, col, row, damage, onComplete);
        }

        void Begin(DreamBlob owner, int col, int row, int damage, Action onComplete)
        {
            _owner = owner;
            _col = col;
            _row = row;
            _damage = damage;
            _onComplete = onComplete;

            _charge = gameObject.AddComponent<SpriteRenderer>();
            _charge.sprite = CountdownLandmineField.MineSprite;
            _charge.color = ChargeColor;
            _charge.sortingOrder = SortOrder;

            var trailGo = new GameObject("MinePlaceTrail");
            trailGo.transform.SetParent(transform);
            _trail = trailGo.AddComponent<SpriteRenderer>();
            _trail.sprite = CountdownLandmineField.MineSprite;
            _trail.color = TrailColor;
            _trail.sortingOrder = SortOrder - 1;
            _trail.transform.localScale = Vector3.one * 0.65f;

            transform.position = owner.transform.position + Vector3.up * 0.08f;
            StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            var grid = GridManager.Instance;
            if (grid == null || _owner == null || !_owner.IsAlive)
            {
                FailPlacement();
                yield break;
            }

            var start = _owner.transform.position + Vector3.up * 0.08f;
            var end = grid.CellToWorld(_col, _row);
            var elapsed = 0f;

            while (elapsed < FlightDuration)
            {
                if (_owner == null || !_owner.IsAlive)
                {
                    FailPlacement();
                    yield break;
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / FlightDuration);
                var eased = EaseInOutQuad(t);
                var pos = Vector3.Lerp(start, end, eased);
                pos.y += ArcHeight * 4f * t * (1f - t);
                transform.position = pos;

                if (_charge != null)
                {
                    var spin = t * 720f;
                    _charge.transform.rotation = Quaternion.Euler(0f, 0f, spin);
                    _charge.transform.localScale = Vector3.one * grid.cellSize * (0.28f + (1f - t) * 0.08f);
                }

                if (_trail != null)
                {
                    _trail.transform.position = Vector3.Lerp(start, pos, 0.35f);
                    var trailTint = TrailColor;
                    trailTint.a = 0.15f + (1f - t) * 0.45f;
                    _trail.color = trailTint;
                }

                yield return null;
            }

            transform.position = end;
            SpawnLandPuff(end, grid.cellSize);
            CountdownLandmineField.CommitMineAt(_col, _row, _damage, _owner);
            _onComplete?.Invoke();
            Destroy(gameObject);
        }

        void FailPlacement()
        {
            CountdownLandmineField.ReleasePendingMine(_col, _row);
            _onComplete?.Invoke();
            Destroy(gameObject);
        }

        static void SpawnLandPuff(Vector3 position, float cellSize)
        {
            var puff = new GameObject("MineLandPuff");
            puff.transform.position = position;
            var sr = puff.AddComponent<SpriteRenderer>();
            sr.sprite = CountdownLandmineField.MineSprite;
            sr.color = new Color(1f, 0.72f, 0.22f, 0.85f);
            sr.sortingOrder = 9;
            puff.transform.localScale = Vector3.one * (cellSize * 0.55f);
            puff.AddComponent<MineLandPuffFade>();
        }

        static float EaseInOutQuad(float t) =>
            t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        sealed class MineLandPuffFade : MonoBehaviour
        {
            SpriteRenderer _sr;
            float _elapsed;
            Vector3 _startScale;

            void Awake()
            {
                _sr = GetComponent<SpriteRenderer>();
                _startScale = transform.localScale;
            }

            void Update()
            {
                _elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(_elapsed / 0.28f);
                transform.localScale = _startScale * (1f + t * 0.45f);
                if (_sr != null)
                {
                    var tint = _sr.color;
                    tint.a = 0.85f * (1f - t);
                    _sr.color = tint;
                }

                if (t >= 1f)
                    Destroy(gameObject);
            }
        }
    }
}
