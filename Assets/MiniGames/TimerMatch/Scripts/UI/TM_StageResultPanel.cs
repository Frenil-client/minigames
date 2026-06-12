using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.TimerMatch
{
    public class TM_StageResultPanel : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject      panelRoot;
        [SerializeField] private TextMeshProUGUI bannerText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI totalErrorText;
        [SerializeField] private TextMeshProUGUI roundListText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button          confirmButton;
        [SerializeField] private Button          retryButton;

        [Header("연출")]
        [Tooltip("SUCCESS / GAME OVER 배너 표시 시간 (초)")]
        [SerializeField] private float bannerDuration = 1.5f;

        [Header("오차 색상")]
        [SerializeField] private Color plusColor  = new Color(0.35f, 1f, 0.35f);
        [SerializeField] private Color minusColor = new Color(1f, 0.35f, 0.35f);

        private TM_GameManager _gm;
        private Coroutine      _showRoutine;

        private void Start()
        {
            _gm = TM_GameManager.instance;
            if (_gm == null) return;

            if (confirmButton != null) confirmButton.onClick.AddListener(() => _gm.ExitGame());
            if (retryButton   != null) retryButton.onClick.AddListener(() => _gm.RetryStage());

            _gm.onStateChanged  += Refresh;
            _gm.onStageFinished += Show;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_gm == null) return;
            _gm.onStateChanged  -= Refresh;
            _gm.onStageFinished -= Show;
        }

        private void Refresh()
        {
            if (_gm.state == TM_GameState.StageResult) return;

            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
                _showRoutine = null;
            }
            if (panelRoot != null) panelRoot.SetActive(false);
            if (bannerText != null) bannerText.gameObject.SetActive(false);
        }

        private void Show(bool isClear, int totalScore)
        {
            if (_showRoutine != null) StopCoroutine(_showRoutine);
            _showRoutine = StartCoroutine(ShowRoutine(isClear, totalScore));
        }

        private IEnumerator ShowRoutine(bool isClear, int totalScore)
        {
            string verdict = isClear ? "SUCCESS" : "GAME OVER";

            if (bannerText != null)
            {
                bannerText.text = verdict;
                bannerText.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(bannerDuration);

            if (bannerText != null) bannerText.gameObject.SetActive(false);

            Populate(verdict, totalScore);
            if (panelRoot != null) panelRoot.SetActive(true);
            _showRoutine = null;
        }

        private void Populate(string verdict, int totalScore)
        {
            var stage = _gm.currentStage;
            if (stage == null) return;

            SetText(titleText, verdict);
            SetText(totalErrorText,
                $"{_gm.totalError:00.00} / {stage.maxTotalError:00.00}");

            string plusHex  = ColorUtility.ToHtmlStringRGB(plusColor);
            string minusHex = ColorUtility.ToHtmlStringRGB(minusColor);

            var sb = new StringBuilder();
            foreach (var result in _gm.roundResults)
            {
                bool   plus = result.signedError >= 0f;
                string hex  = plus ? plusHex : minusHex;
                sb.AppendLine(
                    $"{result.roundIndex + 1}   <color=#{hex}>{(plus ? "+" : "-")} {result.error:0.00}</color>");
            }
            SetText(roundListText, sb.ToString());

            SetText(scoreText, $"점수  {totalScore}점");
        }

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null) label.text = text;
        }
    }
}
