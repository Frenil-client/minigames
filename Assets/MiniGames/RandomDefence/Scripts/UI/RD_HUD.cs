using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.RandomDefence
{
    public class RD_HUD : MonoBehaviour
    {
        public static RD_HUD instance { get; private set; }

        [Header("상단 바")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private Button          speedButton;
        [SerializeField] private Button          pauseButton;

        [Header("좌측 패널 — 재화")]
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private TextMeshProUGUI starText;

        [Header("우측 — 웨이브 스킵")]
        [SerializeField] private GameObject      skipWaveRoot;
        [SerializeField] private TextMeshProUGUI skipWaveText;
        [SerializeField] private Button          skipWaveButton;

        [Header("하단 바")]
        [SerializeField] private Button          pullButton;
        [SerializeField] private TextMeshProUGUI sellPriceText;
        [SerializeField] private Button          sellButton;

        [Header("드래그 중 숨길 UI")]
        [SerializeField] private GameObject[] hideWhileDragging;

        [Header("드래그 중 캔버스 소팅 (Characters 레이어 기준)")]
        [Tooltip("드래그 중인 타워(order 100)보다 낮아야 타워가 UI 위에 보입니다.")]
        [SerializeField] private int dragSortingOrder = 60;

        private RD_TowerBase _sellTarget;
        private Canvas       _canvas;

        private void Awake()
        {
            instance = this;
            _canvas  = GetComponent<Canvas>();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Start()
        {
            var gm = RD_GameManager.instance;

            gm.economyMgr.onCurrencyChanged += v => SetText(currencyText, v.ToString());
            gm.economyMgr.onStarChanged     += v => SetText(starText,     v.ToString());

            gm.waveMgr.onAliveCountChanged  += count =>
                SetText(livesText, $"{count} / {RD_WaveSpawner.MAX_ALIVE}");
            SetText(livesText, $"0 / {RD_WaveSpawner.MAX_ALIVE}");

            gm.roundMgr.onInitialCountdownTick += t =>
                SetText(timerText, t > 0f ? FormatTime(t) : "");

            gm.roundMgr.onRoundStart    += r =>
            {
                SetText(roundText, $"{r} / {RD_RoundManager.TOTAL_ROUNDS}");
                if (skipWaveRoot != null) skipWaveRoot.SetActive(false);
            };
            gm.roundMgr.onPrepTimerTick += OnWaveTimerTick;
            gm.roundMgr.onBossTimerTick += t =>
            {
                SetText(timerText, FormatTime(t));
                if (skipWaveText != null)
                    skipWaveText.text = $"다음 웨이브\n{Mathf.CeilToInt(t):0}초";
            };

            gm.roundMgr.onNextWaveReady += () =>
            {
                if (skipWaveRoot != null) skipWaveRoot.SetActive(true);
            };

            if (gm.gachaSys != null)
                gm.gachaSys.onPullFailed += msg => ShowToast(msg);

            if (pullButton     != null) pullButton.onClick.AddListener(OnPull);
            if (speedButton    != null) speedButton.onClick.AddListener(OnSpeed);
            if (pauseButton    != null) pauseButton.onClick.AddListener(OnPause);
            if (skipWaveButton != null) skipWaveButton.onClick.AddListener(OnSkipWave);
            if (sellButton     != null) sellButton.onClick.AddListener(OnSell);

            if (skipWaveRoot  != null) skipWaveRoot.SetActive(false);
            if (speedText     != null) speedText.text = "1x";
            if (sellPriceText != null) sellPriceText.gameObject.SetActive(false);
            if (sellButton    != null) sellButton.gameObject.SetActive(false);
        }

        private void OnWaveTimerTick(float t)
        {
            SetText(timerText, FormatTime(t));

            if (skipWaveText != null)
                skipWaveText.text = $"다음 웨이브\n{Mathf.CeilToInt(t):0}초";
        }

        private void OnPull()
        {
            RD_GameManager.instance.gachaSys?.TryPull();
        }

        private void OnSpeed()
        {
            var timeMgr = RD_GameManager.instance.timeMgr;
            timeMgr.CycleSpeed();
            if (speedText != null)
            {
                speedText.text = $"{timeMgr.currentSpeed:0.#}x";
            }
        }

        private void OnPause()   => RD_GameManager.instance.TogglePause();

        private void OnSkipWave() => RD_GameManager.instance.roundMgr.StartWaveManually();

        public void ShowSellPrice(RD_TowerBase tower)
        {
            if (tower == null) return;
            _sellTarget = tower;

            if (sellPriceText != null)
            {
                sellPriceText.gameObject.SetActive(true);
                sellPriceText.text = $"판매가: {tower.sellPrice}";
            }
            if (sellButton != null) sellButton.gameObject.SetActive(true);
        }

        public void HideSellPrice()
        {
            _sellTarget = null;
            if (sellPriceText != null) sellPriceText.gameObject.SetActive(false);
            if (sellButton    != null) sellButton.gameObject.SetActive(false);
        }

        public void SetDragMode(bool dragging)
        {
            if (hideWhileDragging != null)
            {
                foreach (var go in hideWhileDragging)
                    if (go != null) go.SetActive(!dragging);
            }

            if (_canvas == null) return;

            if (dragging)
            {
                var cam = RD_GameManager.instance?.cam;
                if (cam == null) return;

                _canvas.renderMode       = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera      = cam;
                _canvas.planeDistance    = 100f;
                _canvas.sortingLayerName = "Character";
                _canvas.sortingOrder     = dragSortingOrder;
            }
            else
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        private void OnSell()
        {
            if (_sellTarget == null) return;

            RD_GameManager.instance?.towerMgr?.SellTower(_sellTarget);

            RD_TowerInfoPanel.Hide();
            RD_RangeIndicator.Hide();
            RD_TowerDragHandler.ClearSelection();
            HideSellPrice();
        }

        private void ShowToast(string msg) => Debug.Log($"[Toast] {msg}");

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null) label.text = text;
        }

        private static string FormatTime(float sec)
        {
            int m = Mathf.FloorToInt(sec / 60f);
            int s = Mathf.FloorToInt(sec % 60f);
            return $"{m:00}:{s:00}";
        }
    }
}
