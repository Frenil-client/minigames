using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MiniGames.TimerMatch
{
    public class TM_HUD : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject      hudRoot;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI targetTimeText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI totalErrorText;
        [SerializeField] private TextMeshProUGUI stageScoreText;
        [SerializeField] private Button          stopButton;
        [SerializeField] private Button          pauseButton;
        [SerializeField] private TM_GaugeBar     gaugeBar;

        [Header("입력")]
        [Tooltip("화면 아무 곳이나 탭해도 정지")]
        [SerializeField] private bool tapAnywhereToStop = true;

        [Header("시작 연출")]
        [SerializeField] private float bounceScale    = 1.1f;
        [SerializeField] private float bounceDuration = 0.3f;

        [Header("숨김 연출")]
        [Tooltip("숨김 시작 후 완전히 사라질 때까지 걸리는 시간 (초)")]
        [SerializeField] private float hideFadeDuration = 1f;

        private TM_GameManager _gm;
        private float          _hideFade;
        private Coroutine      _bounceRoutine;

        private void Start()
        {
            _gm = TM_GameManager.instance;
            if (_gm == null) return;

            if (stopButton  != null) stopButton.onClick.AddListener(OnStop);
            if (pauseButton != null) pauseButton.onClick.AddListener(() => _gm.SetPaused(true));

            _gm.onStateChanged  += Refresh;
            _gm.onRoundStarted  += OnRoundStarted;
            _gm.onIntroSettled  += OnIntroSettled;
            _gm.onRoundFinished += _ => RefreshRound();
            Refresh();
        }

        private void OnDestroy()
        {
            if (_gm == null) return;
            _gm.onStateChanged -= Refresh;
            _gm.onRoundStarted -= OnRoundStarted;
            _gm.onIntroSettled -= OnIntroSettled;
        }

        private void Update()
        {
            if (_gm == null) return;

            if (pauseButton != null)
            {
                bool canPause = _gm.CanPause();
                if (pauseButton.interactable != canPause)
                    pauseButton.interactable = canPause;
            }

            if (_gm.state != TM_GameState.Playing || _gm.isPaused) return;

            var watch = _gm.watch;
            var round = _gm.currentRound;
            if (watch == null || round == null) return;

            UpdateHideFade(watch);
            UpdateTimerText(watch);

            gaugeBar?.UpdateIndicator(watch.displayTime, round.targetTime);
            gaugeBar?.SetIndicatorAlpha(1f - _hideFade);

            if (tapAnywhereToStop && IsPointerDown() && !IsPointerOverInteractableUI())
                OnStop();
        }

        private void UpdateHideFade(TM_Stopwatch watch)
        {
            if (!watch.isTimerHidden || _hideFade >= 1f) return;
            _hideFade = Mathf.Min(1f, _hideFade + Time.deltaTime / Mathf.Max(0.01f, hideFadeDuration));
        }

        private void UpdateTimerText(TM_Stopwatch watch)
        {
            if (timerText == null) return;

            if (_gm.isIntroScrambling)
            {
                float fake = Random.Range(0f, 99.99f);
                timerText.text = $"{fake:00.00}";
                return;
            }

            if (!watch.isTimerHidden)
            {
                timerText.text = $"{watch.displayTime:00.00}";
            }
            else if (_hideFade < 1f)
            {
                float fake = Random.Range(0f, TM_RoundSpec.MaxTime);
                timerText.text = $"{fake:00.00}";
            }

            Color c = timerText.color;
            c.a = 1f - _hideFade;
            timerText.color = c;
        }

        private void OnRoundStarted(int roundIndex)
        {
            ResetHideEffect();
            RefreshRound();
        }

        private void OnIntroSettled()
        {
            if (_bounceRoutine != null) StopCoroutine(_bounceRoutine);
            _bounceRoutine = StartCoroutine(BounceRoutine());
        }

        private IEnumerator BounceRoutine()
        {
            if (timerText == null) yield break;

            Transform t    = timerText.transform;
            float     half = Mathf.Max(0.01f, bounceDuration * 0.5f);

            for (float e = 0f; e < half; e += Time.deltaTime)
            {
                t.localScale = Vector3.one * Mathf.Lerp(1f, bounceScale, e / half);
                yield return null;
            }
            for (float e = 0f; e < half; e += Time.deltaTime)
            {
                t.localScale = Vector3.one * Mathf.Lerp(bounceScale, 1f, e / half);
                yield return null;
            }

            t.localScale   = Vector3.one;
            _bounceRoutine = null;
        }

        private void ResetHideEffect()
        {
            _hideFade = 0f;

            if (timerText != null)
            {
                Color c = timerText.color;
                c.a = 1f;
                timerText.color = c;
                timerText.transform.localScale = Vector3.one;
            }

            gaugeBar?.SetIndicatorAlpha(1f);
        }

        private void Refresh()
        {
            bool active = _gm.state is TM_GameState.Playing or TM_GameState.RoundResult;
            if (hudRoot != null) hudRoot.SetActive(active);
            if (active) RefreshRound();
        }

        private void RefreshRound()
        {
            if (_gm.currentStage == null) return;

            SetText(roundText, $"{_gm.currentRoundIndex + 1} / {TM_GameManager.ROUND_COUNT} ROUND");
            SetText(stageScoreText, $"{_gm.stageScore}점");
            SetText(totalErrorText,
                $"{_gm.totalError:00.00} / {_gm.currentStage.maxTotalError:00.00}");

            var round = _gm.currentRound;
            if (round != null)
                SetText(targetTimeText, $"{round.targetTime:00.00}");
        }

        private void OnStop() => _gm.StopRound();

        private static bool IsPointerDown()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }

        private static readonly List<RaycastResult> s_uiHits = new();

        private static bool IsPointerOverInteractableUI()
        {
            var es = EventSystem.current;
            if (es == null) return false;

            Vector2 pos;
            if (Mouse.current != null)
                pos = Mouse.current.position.ReadValue();
            else if (Touchscreen.current != null)
                pos = Touchscreen.current.primaryTouch.position.ReadValue();
            else
                return false;

            var ped = new PointerEventData(es) { position = pos };
            s_uiHits.Clear();
            es.RaycastAll(ped, s_uiHits);

            foreach (var hit in s_uiHits)
            {
                if (hit.gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            }
            return false;
        }

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null) label.text = text;
        }
    }
}
