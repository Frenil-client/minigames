using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Common.UI
{
    /// <summary>
    /// 플래그 하나로 재사용(Recycle) / 루프(Loop) 스크롤을 전환하는 수평 스크롤 뷰.
    ///
    /// ■ Recycle 모드
    ///   · 뷰포트에 보이는 슬롯만 활성화, 나머지는 풀에 반환
    ///   · 슬롯 전체 폭 ≤ 뷰포트 폭  →  슬롯 묶음 중앙 정렬, 스크롤 비활성
    ///   · 슬롯 전체 폭 >  뷰포트 폭  →  좌→우 정렬, 스크롤 활성
    ///
    /// ■ Loop 모드
    ///   · 데이터를 3벌 복제한 콘텐츠 너비로 초기화, 가운데에서 시작
    ///   · 양 끝 도달 시 무음 텔레포트 + 좌표 기준(_startX) 이동 → 무한 루프
    ///   · 슬롯 수는 뷰포트 크기 기반으로만 생성 (데이터 수와 무관)
    ///
    /// ■ ScrollRect Inspector 설정
    ///   · Viewport / Content 연결 필수
    ///   · Horizontal Scrollbar : 선택
    ///   · Movement Type : Elastic(Recycle) / Unrestricted(Loop) — 스크립트가 자동 설정
    ///
    /// ■ 사용 예
    ///   scrollView.onSlotClicked = data => Debug.Log(data);
    ///   scrollView.Initialize(dataList);
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class RecycleScrollView : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [Header("모드")]
        [SerializeField] private ScrollMode scrollMode = ScrollMode.Recycle;

        [Header("슬롯 프리팹")]
        [SerializeField] private ScrollSlotView slotPrefab;

        [Header("간격")]
        [Tooltip("슬롯 사이 간격 (px)")]
        [SerializeField] private float slotSpacing = 44f;
        [Tooltip("Recycle 스크롤 활성 시 좌우 여백 (px)")]
        [SerializeField] private float sidePadding = 44f;

        // ── Public ────────────────────────────────────────────────────────

        /// <summary>슬롯 클릭 이벤트. Initialize 전후 어느 시점에 등록해도 된다.</summary>
        public Action<IScrollSlotData> onSlotClicked;

        // ── Private ───────────────────────────────────────────────────────

        private ScrollRect    _scroll;
        private RectTransform _viewport;
        private RectTransform _content;

        private IList<IScrollSlotData> _data;

        private readonly Queue<ScrollSlotView>           _pool   = new();
        private readonly Dictionary<int, ScrollSlotView> _active = new();

        // 가상 인덱스 범위 (Loop 모드에서는 음수도 가능)
        private int   _firstV  = int.MinValue;
        private int   _lastV   = int.MinValue;

        // 가상 인덱스 0의 슬롯 왼쪽 X (content 로컬 기준).
        // Loop 텔레포트 시 _startX 도 이동하여 슬롯 좌표계를 일관되게 유지.
        private float _startX;

        // 프리팹에서 읽은 슬롯 크기
        private float _slotWidth;
        private float _slotHeight;

        // Loop 전용
        private const int LOOP_COPIES = 3;   // 데이터 복사 세트 수
        private float _loopOneWidth;          // 데이터 한 세트의 너비

        private float Step => _slotWidth + slotSpacing;

        // ── Public Methods ─────────────────────────────────────────────────

        /// <summary>
        /// 데이터 리스트로 스크롤을 초기화한다.
        /// Canvas 레이아웃이 완성된 Start() 이후에 호출해야 한다.
        /// </summary>
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

        /// <summary>
        /// 프리팹의 RectTransform.sizeDelta 에서 슬롯 크기를 읽는다.
        /// 슬롯 프리팹은 앵커를 point(고정 크기)로 설정하고
        /// sizeDelta 에 원하는 W×H 를 지정해두면 된다.
        /// </summary>
        private void ReadSlotSizeFromPrefab()
        {
            RectTransform prefabRt = slotPrefab.GetComponent<RectTransform>();
            _slotWidth  = prefabRt.sizeDelta.x;
            _slotHeight = prefabRt.sizeDelta.y;
        }

        // ── Layout ────────────────────────────────────────────────────────

        private void SetupLayout()
        {
            if (scrollMode == ScrollMode.Loop)
                SetupLoopLayout();
            else
                SetupRecycleLayout();
        }

        private void SetupRecycleLayout()
        {
            int   count    = _data.Count;
            float totalW   = count * _slotWidth + Mathf.Max(0, count - 1) * slotSpacing;
            float vpW      = _viewport.rect.width;

            if (totalW <= vpW)
            {
                // 전체 슬롯이 뷰포트 안에 들어옴 → 중앙 정렬, 스크롤 비활성
                _scroll.horizontal     = false;
                _scroll.movementType   = ScrollRect.MovementType.Clamped;
                SetContentW(vpW);
                _startX = (vpW - totalW) / 2f;
            }
            else
            {
                // 좌→우 스크롤 활성
                _scroll.horizontal     = true;
                _scroll.movementType   = ScrollRect.MovementType.Elastic;
                SetContentW(totalW + sidePadding * 2f);
                _startX = sidePadding;
            }
        }

        private void SetupLoopLayout()
        {
            if (_data.Count == 0) return;

            _scroll.horizontal   = true;
            _scroll.movementType = ScrollRect.MovementType.Unrestricted;

            _loopOneWidth = _data.Count * Step;
            SetContentW(_loopOneWidth * LOOP_COPIES + sidePadding * 2f);
            _startX = sidePadding;

            // 가운데 복사본(1세트 오프셋)에서 시작
            _content.anchoredPosition = new Vector2(-_loopOneWidth, _content.anchoredPosition.y);
        }

        // ── Loop Wrap ─────────────────────────────────────────────────────

        /// <summary>
        /// 스크롤이 안전 구간(safeMin ~ safeMax)을 벗어나면 반대쪽으로 텔레포트.
        /// 텔레포트량이 정확히 _loopOneWidth이므로 모든 슬롯의 데이터 인덱스가 유지된다.
        /// </summary>
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

        /// <summary>
        /// 활성 슬롯 딕셔너리의 가상 인덱스 키를 delta만큼 이동.
        /// 슬롯의 anchoredPosition은 변경하지 않는다.
        /// (_startX도 같은 양만큼 이동하므로 GetSlotX 결과가 일치함)
        /// </summary>
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

            // 뷰포트 양쪽으로 슬롯 1개 여유 (팝인 방지)
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

            // 앵커를 좌중앙 고정 포인트로 설정하여 프리팹 크기를 그대로 사용
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

        private float ScrolledX() => -_content.anchoredPosition.x;

        private float GetSlotX(int vi) => _startX + vi * Step;

        /// <summary>가상 인덱스 → 실제 데이터 인덱스. Loop 모드에서는 모듈로 래핑.</summary>
        private int ToDataIndex(int vi)
        {
            if (scrollMode == ScrollMode.Recycle) return vi;
            int n = _data.Count;
            int i = vi % n;
            return i < 0 ? i + n : i;
        }

        private void SetContentW(float w) =>
            _content.sizeDelta = new Vector2(w, _content.sizeDelta.y);
    }

    public enum ScrollMode
    {
        /// <summary>유한 리스트. 뷰포트 밖 슬롯을 풀링하여 재사용.</summary>
        Recycle,
        /// <summary>무한 루프. 마지막 슬롯 다음에 첫 슬롯이 이어짐.</summary>
        Loop,
    }
}
