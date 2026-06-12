using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_GameManager : MonoBehaviour
    {
        public static TM_GameManager instance { get; private set; }

        public const int ROUND_COUNT = 5;

        [Header("스테이지 데이터 (stageIndex 오름차순)")]
        [SerializeField] private List<TM_StageDataSO> stages = new();

        [Header("참조")]
        [SerializeField] private TM_Stopwatch stopwatch;

        [Header("라운드 시작 연출")]
        [Tooltip("1라운드 시작 시 타이머 숫자가 랜덤하게 돌아가는 시간 (초)")]
        [SerializeField] private float firstRoundScrambleDuration = 2f;
        [Tooltip("초기값 확정(바운스) 후 초시계가 출발하기까지의 대기 시간 (초)")]
        [SerializeField] private float introBounceDuration = 0.35f;

        [Header("라운드 결과 연출")]
        [Tooltip("라운드 오차/점수 연출이 표시되는 시간 (초)")]
        [SerializeField] private float roundResultDuration = 2f;

        [Header("라운드 랜덤 생성")]
        [SerializeField] private float minTargetTime      = 3f;
        [SerializeField] private float maxTargetTime      = 12f;
        [Tooltip("1배속 이상만 허용")]
        [SerializeField] private float maxSpeedMultiplier = 1.3f;

        public TM_GameState  state              { get; private set; } = TM_GameState.StageSelect;
        public TM_StageDataSO currentStage      { get; private set; }
        public int           currentRoundIndex  { get; private set; }
        public int           stageScore         { get; private set; }
        public float         totalError         { get; private set; }
        public bool          isIntroScrambling  { get; private set; }
        public bool          isPaused           { get; private set; }
        public TM_RoundSpec  currentRound       { get; private set; }

        public IReadOnlyList<TM_StageDataSO> allStages    => stages;
        public IReadOnlyList<TM_RoundResult> roundResults => _results;
        public TM_Stopwatch                  watch        => stopwatch;

        public event Action                 onStateChanged;
        public event Action<bool>           onPauseChanged;
        public event Action<int>            onRoundStarted;
        public event Action                 onIntroSettled;
        public event Action<TM_RoundResult> onRoundFinished;
        public event Action<bool, int>      onStageFinished;

        private readonly List<TM_RoundResult> _results = new();
        private Coroutine _flowRoutine;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public void OpenStageSelect()
        {
            StopFlowRoutine();
            stopwatch?.Stop();
            currentStage = null;
            currentRound = null;
            SetState(TM_GameState.StageSelect);
        }

        public void SetPaused(bool paused)
        {
            if (paused)
            {
                if (isPaused) return;
                if (state != TM_GameState.Playing) return;
                if (stopwatch == null || !stopwatch.isRunning) return;
            }
            else if (!isPaused) return;

            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            onPauseChanged?.Invoke(paused);
        }

        public bool CanPause()
        {
            return !isPaused
                && state == TM_GameState.Playing
                && stopwatch != null
                && stopwatch.isRunning;
        }

        public void StartStage(TM_StageDataSO stage)
        {
            if (stage == null) return;

            ResetPause();
            currentStage = stage;
            stageScore   = 0;
            totalError   = 0f;
            _results.Clear();
            StartRound(0);
        }

        public bool StartStageByIndex(int stageIndex)
        {
            foreach (var stage in stages)
            {
                if (stage == null || stage.stageIndex != stageIndex) continue;
                StartStage(stage);
                return true;
            }
            return false;
        }

        public void RetryStage()
        {
            if (currentStage != null) StartStage(currentStage);
        }

        public void ExitGame()
        {
            StopGame();
            if (!MiniGameSession.RequestExit()) OpenStageSelect();
        }

        public void StopRound()
        {
            if (isPaused) return;
            if (state != TM_GameState.Playing) return;
            if (stopwatch == null || !stopwatch.isRunning) return;

            stopwatch.Stop();

            var result = TM_ScoreCalculator.Evaluate(
                currentRoundIndex, currentRound, stopwatch.displayTime);
            _results.Add(result);
            stageScore += result.score;
            totalError += result.error;

            SetState(TM_GameState.RoundResult);
            onRoundFinished?.Invoke(result);

            StopFlowRoutine();
            _flowRoutine = StartCoroutine(RoundResultRoutine());
        }

        public void StopGame()
        {
            StopFlowRoutine();
            stopwatch?.Stop();
            isIntroScrambling = false;
            ResetPause();
        }

        private void ResetPause()
        {
            Time.timeScale = 1f;
            if (!isPaused) return;
            isPaused = false;
            onPauseChanged?.Invoke(false);
        }

        private void StartRound(int index)
        {
            currentRoundIndex = index;
            currentRound      = BuildRoundSpec();
            stopwatch.Prime(currentRound);
            SetState(TM_GameState.Playing);
            onRoundStarted?.Invoke(index);

            StopFlowRoutine();
            _flowRoutine = StartCoroutine(IntroRoutine(index == 0));
        }

        private TM_RoundSpec BuildRoundSpec()
        {
            float target = UnityEngine.Random.Range(minTargetTime, maxTargetTime);
            target = Mathf.Round(target * 100f) / 100f;

            return new TM_RoundSpec
            {
                targetTime      = target,
                speedMultiplier = UnityEngine.Random.Range(1f, Mathf.Max(1f, maxSpeedMultiplier)),
                startDirection  = UnityEngine.Random.value < 0.5f
                    ? StartDirection.FromZero
                    : StartDirection.FromMax,
                hideStartRatio  = currentStage.hideStartRatio,
            };
        }

        private IEnumerator IntroRoutine(bool isFirstRound)
        {
            if (isFirstRound)
            {
                isIntroScrambling = true;
                yield return new WaitForSeconds(firstRoundScrambleDuration);
                isIntroScrambling = false;
            }

            onIntroSettled?.Invoke();
            yield return new WaitForSeconds(introBounceDuration);

            stopwatch.Run();
            _flowRoutine = null;
        }

        private IEnumerator RoundResultRoutine()
        {
            yield return new WaitForSeconds(roundResultDuration);
            _flowRoutine = null;

            if (totalError > currentStage.maxTotalError)
            {
                FinishStage(false);
                yield break;
            }

            int next = currentRoundIndex + 1;
            if (next < ROUND_COUNT) StartRound(next);
            else FinishStage(true);
        }

        private void FinishStage(bool isClear)
        {
            if (isClear) TM_StageUnlock.Unlock(currentStage.stageIndex + 1);
            TM_StageUnlock.TryUpdateHighScore(currentStage.stageIndex, stageScore);

            SetState(TM_GameState.StageResult);
            onStageFinished?.Invoke(isClear, stageScore);
        }

        private void StopFlowRoutine()
        {
            if (_flowRoutine == null) return;
            StopCoroutine(_flowRoutine);
            _flowRoutine = null;
            isIntroScrambling = false;
        }

        private void SetState(TM_GameState next)
        {
            state = next;
            onStateChanged?.Invoke();
        }
    }
}
