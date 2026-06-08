using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI
{
    public enum ScrollMode
    {
        /// <summary>유한 리스트. 뷰포트 밖 슬롯을 풀링하여 재사용.</summary>
        Recycle,
        /// <summary>무한 루프. 마지막 슬롯 다음에 첫 슬롯이 이어짐.</summary>
        Loop,
    }

    /// <summary>
    /// 플래그 하나로 재사용(Recycle) / 루프(Loop) 를 전환하는 수평 스크롤 뷰.
    ///
    /// ■ Recycle 모드
    ///   · 슬롯 전체 폭 ≤ 뷰포트 폭  →  중앙 정렬, 스크롤 비활성
    ///   · 슬롯 전체 폭 >  뷰포트 폭  →  좌→우 정렬, 스크롤 활성
    ///
    /// ■ Loop 모드
    ///   · 데이터 3벌 복제 콘텐츠, 가운데에서 시작
    ///   · 양 끝 도달 시 무음 텔레포트 → 무한 루프
    ///
    /// ■ 슬롯 크기 : 프리팹 RectTransform.sizeDelta 에서 자동 읽음
    ///
    /// ■ 사용 예
    ///   scrollView.onSlotClicked = data => { };
    ///   scrollView.Initialize(dataList);
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class RecycleScrollView : MonoBehaviour
    {
        [Header("모드")]
        [SerializeField] private ScrollMode scrollMode = ScrollMode.Recycle;

        [Header("슬롯 프리팹")]
        [SerializeField] private ScrollSlotView slotPrefab;

        [Header("간격")]
        [SerializeField] private float slotSpacing = 44f;
        [SerializeField] private float sidePadding = 44f;

        // ── Public ────────────────────────────────────────────────────────

        public Action<IScrollSlotData> onSlotClicked;

        // ── Private ───────────────────────────────────────────────────────

        private ScrollRect    _scroll;
        private RectTransform _viewport;
        private RectTransform _content;
        private IList<IScrollSlotData> _data;

        private readonly Queue<ScrollSlotView>           _pool   = new();
        private readonly Dictionary<int, ScrollSlotView> _active = new();

        private int   _firstV = int.MinValue;
        private int   _lastV  = int.MinValue;
        private float _startX;
        private float _slotWidth;
        private float _slotHeight;

        private const int LOOP_COPIES = 3;
        private float _loopOneWidth;

        private float Step => _slotWidth + slotSpacing;

        // ── Public Methods ─────────────────────────────────────────────────

        public void Initialize(IList<IScrollSlotData> data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));

            _scroll   = GetComponent<ScrollRect>();
            _viewport = _scroll.viewport;
            _content  = _scroll.content;

            Canvas.ForceUpdateCanvases();
            ReadSlotSizeFromPrefab();
            SetupLayout();
            RefreshSlots();

            _scroll.onValueChanged.AddListener(_ =>
            {
                if (scrollMode == ScrollMode.Loop) HandleLoopWrap();
                RefreshSlots();
            });
        }

        // ── Prefab Size ───────────────────────────────────────────────────

        private void ReadSlotSizeFromPrefab()
        {
            var rt   = slotPrefab.GetComponent<RectTransform>();
            _slotWidth  = rt.sizeDelta.x;
            _slotHeight = rt.sizeDelta.y;
        }

        // ── Layout ────────────────────────────────────────────────────────

        private void SetupLayout()
        {
            if (scrollMode == ScrollMode.Loop) SetupLoopLayout();
            else                               SetupRecycleLayout();
        }

        private void SetupRecycleLayout()
        {
            int   count  = _data.Count;
            float totalW = count * _slotWidth + Mathf.Max(0, count - 1) * slotSpacing;
            float vpW    = _viewport.rect.width;

            if (totalW <= vpW)
            {
                _scroll.horizontal   = false;
                _scroll.movementType = ScrollRect.MovementType.Clamped;
                SetContentW(vpW);
                _startX = (vpW - totalW) / 2f;
            }
            else
            {
                _scroll.horizontal   = true;
                _scroll.movementType = ScrollRect.MovementType.Elastic;
                SetContentW(totalW + sidePadding * 2f);
                _startX = sidePadding;
            }
        }

        private void SetupLoopLayout()
        {
            if (_data.Count == 0) return;
            _scroll.horizontal   = true;
            _scroll.movementType = ScrollRect.MovementType.Unrestricted;
            _loopOneWidth        = _data.Count * Step;
            SetContentW(_loopOneWidth * LOOP_COPIES + sidePadding * 2f);
            _startX = sidePadding;
            _content.anchoredPosition = new Vector2(-_loopOneWidth, _content.anchoredPosition.y);
        }

        // ── Loop Wrap ─────────────────────────────────────────────────────

        private void HandleLoopWrap()
        {
            if (_data.Count == 0) return;
            float sx      = ScrolledX();
            float safeMin = _loopOneWidth;
            float safeMax = _loopOneWidth * 2f;

            if (sx >= safeMax)
            {
                _content.anchoredPosition += new Vector2(_loopOneWidth, 0f);
                _startX += _loopOneWidth;
                ShiftActiveIndices(-_data.Count);
            }
            else if (sx < safeMin)
            {
                _content.anchoredPosition -= new Vector2(_loopOneWidth, 0f);
                _startX -= _loopOneWidth;
                ShiftActiveIndices(_data.Count);
            }
        }

        private void ShiftActiveIndices(int delta)
        {
            var shifted = new Dictionary<int, ScrollSlotView>(_active.Count);
            foreach (var kv in _active) shifted[kv.Key + delta] = kv.Value;
            _active.Clear();
            foreach (var kv in shifted) _active[kv.Key] = kv.Value;
            if (_firstV != int.MinValue) _firstV += delta;
            if (_lastV  != int.MinValue) _lastV  += delta;
        }

        // ── Recycle Logic ─────────────────────────────────────────────────

        private void RefreshSlots()
        {
            if (_data == null || _data.Count == 0) return;

            float sx  = ScrolledX();
            float vpW = _viewport.rect.width;

            int nf = Mathf.FloorToInt((sx       - _startX) / Step) - 1;
            int nl = Mathf.FloorToInt((sx + vpW - _startX) / Step) + 1;

            if (scrollMode == ScrollMode.Recycle)
            {
                nf = Mathf.Max(0, nf);
                nl = Mathf.Min(_data.Count - 1, nl);
            }

            if (_firstV == int.MinValue)
            {
                for (int i = nf; i <= nl; i++) SpawnSlot(i);
            }
            else
            {
                for (int i = _firstV;    i < nf;      i++) RecycleSlot(i);
                for (int i = nl + 1;     i <= _lastV; i++) RecycleSlot(i);
                for (int i = nf;         i < _firstV; i++) SpawnSlot(i);
                for (int i = _lastV + 1; i <= nl;     i++) SpawnSlot(i);
            }

            _firstV = nf;
            _lastV  = nl;
        }

        // ── Slot Lifecycle ────────────────────────────────────────────────

        private void SpawnSlot(int vi)
        {
            if (_active.ContainsKey(vi)) return;

            ScrollSlotView slot = GetFromPool();
            slot.gameObject.SetActive(true);
            slot.onClicked = onSlotClicked;
            slot.Bind(_data[ToDataIndex(vi)]);

            RectTransform rt  = slot.rectTransform;
            rt.anchorMin        = new Vector2(0f, 0.5f);
            rt.anchorMax        = new Vector2(0f, 0.5f);
            rt.pivot            = new Vector2(0f, 0.5f);
            rt.sizeDelta        = new Vector2(_slotWidth, _slotHeight);
            rt.anchoredPosition = new Vector2(GetSlotX(vi), 0f);

            _active[vi] = slot;
        }

        private void RecycleSlot(int vi)
        {
            if (!_active.TryGetValue(vi, out ScrollSlotView slot)) return;
            slot.gameObject.SetActive(false);
            slot.OnRecycled();
            _pool.Enqueue(slot);
            _active.Remove(vi);
        }

        private ScrollSlotView GetFromPool() =>
            _pool.Count > 0 ? _pool.Dequeue() : Instantiate(slotPrefab, _content);

        // ── Helpers ───────────────────────────────────────────────────────

        private float ScrolledX()          => -_content.anchoredPosition.x;
        private float GetSlotX(int vi)     => _startX + vi * Step;
        private void  SetContentW(float w) => _content.sizeDelta = new Vector2(w, _content.sizeDelta.y);

        private int ToDataIndex(int vi)
        {
            if (scrollMode == ScrollMode.Recycle) return vi;
            int n = _data.Count;
            int i = vi % n;
            return i < 0 ? i + n : i;
        }
    }
}
