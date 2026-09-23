using System;
using System.Collections.Generic;
using System.Linq;
using SomnusNet.Core;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.UI
{
    public static class TypeHudLayout
    {
        public enum Kind
        {
            Harmony = 0,
            Speed = 1,
            DreamTeam = 2,
            AbsentHistory = 3,
            BrokenRings = 4,
            SlimySupport = 5,
            Entertainer = 6,
            BenchTrio = 7,
            BurdenedCrown = 8
        }

        const float TopRowMaxY = 0.905f;
        const float RowHeight = 0.07f;
        const float RowStep = 0.08f;
        const float IconMinX = 0.02f;
        const float IconMaxX = 0.065f;
        const float CounterMinX = 0.07f;
        const float CounterMaxX = 0.16f;
        const float PopupMinX = 0.02f;
        const float PopupMaxX = 0.42f;
        const float PopupHeight = 0.24f;
        const float GapBelowStack = 0.012f;

        struct Row
        {
            public Kind kind;
            public RectTransform icon;
            public RectTransform counter;
            public bool visible;
        }

        struct PopupEntry
        {
            public Kind kind;
            public RectTransform root;
            public Action hide;
        }

        static readonly List<Row> _rows = new();
        static readonly List<PopupEntry> _popups = new();
        static readonly Dictionary<Kind, float> _firstSeenOrder = new();
        static float _nextOrder;
        static bool _dirty;

        public static void Register(Kind kind, RectTransform icon, RectTransform counter)
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].kind != kind) continue;
                var row = _rows[i];
                row.icon = icon;
                row.counter = counter;
                _rows[i] = row;
                return;
            }

            _rows.Add(new Row { kind = kind, icon = icon, counter = counter });
        }

        public static void RegisterPopup(Kind kind, RectTransform popupRoot, Action hide)
        {
            for (var i = 0; i < _popups.Count; i++)
            {
                if (_popups[i].kind != kind) continue;
                var entry = _popups[i];
                entry.root = popupRoot;
                entry.hide = hide;
                _popups[i] = entry;
                _dirty = true;
                return;
            }

            _popups.Add(new PopupEntry { kind = kind, root = popupRoot, hide = hide });
            _dirty = true;
        }

        public static void RecordTypesFromBlob(DreamBlob blob)
        {
            if (blob == null || !blob.IsAlive || !BlobTypeFeatures.Enabled) return;

            if (blob.HasType(HarmonyTypeRules.HarmonyLabel))
                RecordKindFirstSeen(Kind.Harmony);
            if (blob.HasType(SpeedTypeRules.Label))
                RecordKindFirstSeen(Kind.Speed);
            if (blob.HasType(DreamTeamTypeRules.Label))
                RecordKindFirstSeen(Kind.DreamTeam);
            if (blob.HasType(AbsentHistoryTypeRules.Label))
                RecordKindFirstSeen(Kind.AbsentHistory);
            if (blob.HasType(BrokenRingsTypeRules.Label))
                RecordKindFirstSeen(Kind.BrokenRings);
            if (blob.HasType(SlimySupportTypeRules.Label))
                RecordKindFirstSeen(Kind.SlimySupport);
            if (blob.HasType(EntertainerTypeRules.Label))
                RecordKindFirstSeen(Kind.Entertainer);
            if (blob.HasType(BenchTrioTypeRules.Label))
                RecordKindFirstSeen(Kind.BenchTrio);
            if (blob.HasType(BurdenedCrownTypeRules.Label))
                RecordKindFirstSeen(Kind.BurdenedCrown);
        }

        static void RecordKindFirstSeen(Kind kind)
        {
            if (_firstSeenOrder.ContainsKey(kind)) return;
            _firstSeenOrder[kind] = _nextOrder++;
            _dirty = true;
        }

        public static void SetVisible(Kind kind, bool visible)
        {
            var index = FindRowIndex(kind);
            if (index < 0) return;

            var row = _rows[index];
            row.visible = visible;
            _rows[index] = row;
            _dirty = true;
        }

        public static void NotifyPopupOpening(Kind kind)
        {
            foreach (var popup in _popups)
            {
                if (popup.kind == kind) continue;
                popup.hide?.Invoke();
            }

            _dirty = true;
        }

        public static void RefreshNow()
        {
            _dirty = true;
            RefreshIfDirty();
        }

        public static RectTransform TryGetIconRect(Kind kind)
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].kind == kind && _rows[i].icon != null)
                    return _rows[i].icon;
            }

            return null;
        }

        public static void RefreshIfDirty()
        {
            if (!_dirty) return;
            _dirty = false;
            Refresh();
        }

        static void Refresh()
        {
            var visibleRows = _rows
                .Where(row => row.visible && row.icon != null && row.counter != null)
                .OrderBy(row => _firstSeenOrder.TryGetValue(row.kind, out var order) ? order : float.MaxValue)
                .ThenBy(row => row.kind)
                .ToList();

            for (var slot = 0; slot < visibleRows.Count; slot++)
                ApplyRowSlot(visibleRows[slot], slot);

            LayoutPopups(visibleRows.Count);
        }

        static void LayoutPopups(int visibleRowCount)
        {
            var stackBottomMinY = GetStackBottomMinY(visibleRowCount);
            var popupMaxY = stackBottomMinY - GapBelowStack;
            var popupMinY = popupMaxY - PopupHeight;
            var popupMin = new Vector2(PopupMinX, popupMinY);
            var popupMax = new Vector2(PopupMaxX, popupMaxY);

            foreach (var popup in _popups)
            {
                if (popup.root == null) continue;
                SetAnchors(popup.root, popupMin, popupMax);
            }
        }

        static float GetStackBottomMinY(int visibleRowCount)
        {
            if (visibleRowCount <= 0)
                return TopRowMaxY - RowHeight;
            return TopRowMaxY - (visibleRowCount - 1) * RowStep - RowHeight;
        }

        static int FindRowIndex(Kind kind)
        {
            for (var i = 0; i < _rows.Count; i++)
                if (_rows[i].kind == kind)
                    return i;
            return -1;
        }

        static void ApplyRowSlot(Row row, int slot)
        {
            var maxY = TopRowMaxY - slot * RowStep;
            var minY = maxY - RowHeight;
            SetAnchors(row.icon, new Vector2(IconMinX, minY), new Vector2(IconMaxX, maxY));
            SetAnchors(row.counter, new Vector2(CounterMinX, minY), new Vector2(CounterMaxX, maxY));
        }

        static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    public class TypeHudLayoutDriver : MonoBehaviour
    {
        public static void EnsureExists()
        {
            if (UnityEngine.Object.FindFirstObjectByType<TypeHudLayoutDriver>() != null) return;
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<TypeHudLayoutDriver>();
        }

        void LateUpdate() => TypeHudLayout.RefreshIfDirty();
    }
}
