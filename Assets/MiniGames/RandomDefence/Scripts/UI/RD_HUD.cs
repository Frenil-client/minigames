using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.RandomDefence
{
    public class RD_HUD : MonoBehaviour
    {
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

            if (skipWaveRoot  != null) skipWaveRoot.SetActive(false);
            if (speedText     != null) speedText.text = "1x";
            if (sellPriceText != null) sellPriceText.gameObject.SetActive(false);
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

        public void ShowSellPrice(int price)
        {
            if (sellPriceText == null) return;
            sellPriceText.gameObject.SetActive(true);
            sellPriceText.text = $"판매가: {price}";
        }

        public void HideSellPrice()
        {
            if (sellPriceText != null) sellPriceText.gameObject.SetActive(false);
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
