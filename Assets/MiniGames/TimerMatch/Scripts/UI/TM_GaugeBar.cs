using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.TimerMatch
{
    public class TM_GaugeBar : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("게이지 전체 영역. 인디케이터 이동 범위 기준")]
        [SerializeField] private RectTransform barArea;
        [Tooltip("이동 인디케이터 (◆). barArea의 자식, 앵커 중앙 기준")]
        [SerializeField] private RectTransform indicator;

        [Header("오차 범위 (초) — 게이지 양 끝 = ±이 값, 초과 시 인디케이터 숨김")]
        [SerializeField] private float errorRange = 5f;

        private Graphic _indicatorGraphic;

        private void Awake()
        {
            if (indicator != null)
                _indicatorGraphic = indicator.GetComponent<Graphic>();
        }

        public void UpdateIndicator(float displayTime, float targetTime)
        {
            if (barArea == null || indicator == null) return;

            float signed  = displayTime - targetTime;
            bool  visible = Mathf.Abs(signed) <= errorRange;

            if (indicator.gameObject.activeSelf != visible)
                indicator.gameObject.SetActive(visible);
            if (!visible) return;

            float gaugePosition = 0.5f + signed / (errorRange * 2f);
            float width = barArea.rect.width;
            float x     = (Mathf.Clamp01(gaugePosition) - 0.5f) * width;
            indicator.anchoredPosition = new Vector2(x, indicator.anchoredPosition.y);
        }

        public void SetIndicatorAlpha(float alpha)
        {
            if (_indicatorGraphic == null) return;
            Color c = _indicatorGraphic.color;
            c.a = Mathf.Clamp01(alpha);
            _indicatorGraphic.color = c;
        }
    }
}
