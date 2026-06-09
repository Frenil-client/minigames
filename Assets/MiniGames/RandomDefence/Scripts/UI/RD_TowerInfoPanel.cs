using TMPro;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TowerInfoPanel : MonoBehaviour
    {
        public static RD_TowerInfoPanel instance { get; private set; }

        [Header("패널 루트 (null이면 this.gameObject)")]
        [SerializeField] private GameObject panelRoot;

        [Header("텍스트 필드")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI damageText;

        [Header("공격 속도 등급 기준 (초당 공격 횟수)")]
        [Tooltip("이 값 이상이면 '상'")]
        [SerializeField] private float speedHighThreshold = 1.5f;
        [Tooltip("이 값 미만이면 '하'  /  사이면 '중'")]
        [SerializeField] private float speedLowThreshold  = 0.8f;

        [Header("패널 위치")]
        [Tooltip("Background 자식 RectTransform")]
        [SerializeField] private RectTransform backgroundRect;
        [Tooltip("Background 너비 (픽셀) — Inspector에서 확인한 값과 맞추세요")]
        [SerializeField] private float backgroundWidth = 570f;
        [Tooltip("타워와 Background 사이 가로 간격 (픽셀)")]
        [SerializeField] private float gapFromTower    = 20f;
        [Tooltip("타워 기준 세로 오프셋 (픽셀, 양수 = 위)")]
        [SerializeField] private float yOffset         = 0f;

        private GameObject    Root     => panelRoot != null ? panelRoot : gameObject;
        private RectTransform _myRect;
        private Canvas        _canvas;

        private void Awake()
        {
            instance = this;
            _myRect  = (RectTransform)transform;
            _canvas  = GetComponentInParent<Canvas>();
            Root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Show(RD_TowerBase tower)
        {
            if (instance == null || tower == null || tower.data == null) return;
            instance.Refresh(tower);
        }

        public static void Hide()
        {
            if (instance != null)
                instance.Root.SetActive(false);
        }

        public static void Track(RD_TowerBase tower)
        {
            if (instance == null || tower == null) return;
            if (!instance.Root.activeSelf) return;
            instance.PositionPanel(tower);
        }

        private void Refresh(RD_TowerBase tower)
        {
            PositionPanel(tower);
            Root.SetActive(true);

            string towerName = tower.type switch
            {
                TowerType.Worrior => "전사",
                TowerType.Mage    => "마법사",
                TowerType.Bowman  => "궁수",
                _                 => tower.type.ToString()
            };

            SetText(nameText,   towerName);
            SetText(speedText,  SpeedTier(tower.attackSpeed));
            SetText(damageText, $"{tower.attackDamage:0.#}");
        }

        private void PositionPanel(RD_TowerBase tower)
        {
            if (_canvas == null || _myRect == null) return;

            var cam = RD_GameManager.instance?.cam;
            if (cam == null) return;

            Vector3 worldPos  = tower.transform.position;
            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);

            Camera uiCam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : _canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform, screenPos, uiCam, out Vector2 canvasPos);

            _myRect.anchoredPosition = new Vector2(canvasPos.x, canvasPos.y + yOffset);

            if (backgroundRect != null)
            {
                bool towerOnRight = worldPos.x >= 0f;
                float bgX = towerOnRight
                    ? -(backgroundWidth + gapFromTower)
                    :   gapFromTower;
                Vector2 bgPos = backgroundRect.anchoredPosition;
                backgroundRect.anchoredPosition = new Vector2(bgX, bgPos.y);
            }
        }

        private string SpeedTier(float speed)
            => speed >= speedHighThreshold ? "상" :
               speed <  speedLowThreshold  ? "하" : "중";

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null) label.text = text;
        }
    }
}
