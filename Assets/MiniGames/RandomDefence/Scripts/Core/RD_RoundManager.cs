using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_RoundManager : MonoBehaviour
    {
        public const int TOTAL_ROUNDS = 50;

        [Header("웨이브 데이터")]
        [SerializeField] private List<RD_WaveDataSO> waveTable     = new();
        [SerializeField] private RD_WaveGenerator    waveGenerator;

        [Header("첫 웨이브 대기 시간 (초)")]
        [SerializeField] private float initialDelay = 10f;

        [Header("웨이브 제한 시간 (초)")]
        [SerializeField] private float normalWaveDuration = 50f;
        [SerializeField] private float bossWaveDuration   = 150f;

        public int        currentRound { get; private set; }
        public RoundPhase phase        { get; private set; } = RoundPhase.End;
        public RoundType  roundType    { get; private set; }
        public float      waveTimeLeft { get; private set; }

        public System.Action<int>        onRoundStart;
        public System.Action<RoundPhase> onPhaseChanged;
        public System.Action<float>      onPrepTimerTick;
        public System.Action<float>      onBossTimerTick;
        public System.Action             onBossTimerExpired;
        public System.Action<float>      onInitialCountdownTick;
        public System.Action             onNextWaveReady;
        public System.Action             onBossCleared;

        private RD_WaveSpawner _spawner;
        private bool           _nextWaveReady;
        private bool           _nextWaveRequested;
        private RD_WaveDataSO  _currentWave;

        public void StopRounds()
        {
            StopAllCoroutines();
            if (_spawner != null)
            {
                _spawner.onAllSpawned  -= OnAllSpawned;
                _spawner.onWaveCleared -= OnWaveCleared;
                _spawner = null;
            }
            currentRound       = 0;
            phase              = RoundPhase.End;
            waveTimeLeft       = 0f;
            _nextWaveReady     = false;
            _nextWaveRequested = false;
        }

        public void StartRounds()
        {
            StopRounds();

            _spawner = RD_GameManager.instance.waveMgr;
            _spawner.onAllSpawned  += OnAllSpawned;
            _spawner.onWaveCleared += OnWaveCleared;
            currentRound = 0;
            StartCoroutine(InitialDelayRoutine());
        }

        private IEnumerator InitialDelayRoutine()
        {
            float remaining = initialDelay;
            while (remaining > 0f)
            {
                onInitialCountdownTick?.Invoke(remaining);
                remaining -= Time.deltaTime;
                yield return null;
            }
            onInitialCountdownTick?.Invoke(0f);
            StartCoroutine(NextRound());
        }

        public void StartWaveManually()
        {
            if (_nextWaveReady) _nextWaveRequested = true;
        }

        private IEnumerator NextRound()
        {
            currentRound++;
            if (currentRound > TOTAL_ROUNDS)
            {
                RD_GameManager.instance.TriggerClear();
                yield break;
            }

            roundType    = GetRoundType(currentRound);
            _currentWave = GetWaveData(currentRound);

            onRoundStart?.Invoke(currentRound);
            SetPhase(RoundPhase.Wave);

            _nextWaveReady     = false;
            _nextWaveRequested = false;

            if (roundType == RoundType.Normal)
                yield return StartCoroutine(NormalWaveRoutine(_currentWave));
            else
                yield return StartCoroutine(BossWaveRoutine(_currentWave));
        }

        private IEnumerator NormalWaveRoutine(RD_WaveDataSO wave)
        {
            _spawner.StartWave(wave, currentRound);

            waveTimeLeft = normalWaveDuration;

            while (waveTimeLeft > 0f && !_nextWaveRequested)
            {
                waveTimeLeft -= Time.deltaTime;
                onPrepTimerTick?.Invoke(Mathf.Max(waveTimeLeft, 0f));
                yield return null;
            }
            onPrepTimerTick?.Invoke(0f);

            SetPhase(RoundPhase.End);
            StartCoroutine(NextRound());
        }

        private IEnumerator BossWaveRoutine(RD_WaveDataSO wave)
        {
            _spawner.StartWave(wave, currentRound);

            float duration = (wave != null && wave.bossTimeLimit > 0f)
                ? wave.bossTimeLimit
                : bossWaveDuration;

            waveTimeLeft = duration;

            while (waveTimeLeft > 0f && !_nextWaveRequested)
            {
                waveTimeLeft -= Time.deltaTime;
                onBossTimerTick?.Invoke(Mathf.Max(waveTimeLeft, 0f));
                yield return null;
            }

            if (!_nextWaveRequested)
            {
                onBossTimerTick?.Invoke(0f);
                onBossTimerExpired?.Invoke();
                RD_GameManager.instance.TriggerGameOver();
                yield break;
            }

            onBossTimerTick?.Invoke(0f);
            SetPhase(RoundPhase.End);
            StartCoroutine(NextRound());
        }

        private void OnAllSpawned()
        {
            if (roundType != RoundType.Normal || phase != RoundPhase.Wave) return;
            ActivateNextWaveButton();
        }

        private void OnWaveCleared()
        {
            var gm = RD_GameManager.instance;
            if (gm?.state is GameState.GameOver or GameState.Clear) return;

            if (currentRound == TOTAL_ROUNDS)
            {
                gm.TriggerClear();
                return;
            }

            if (roundType != RoundType.Normal && phase == RoundPhase.Wave)
            {
                if (_currentWave != null && _currentWave.starCurrencyReward > 0)
                    gm.economyMgr.AddStar(_currentWave.starCurrencyReward);

                onBossCleared?.Invoke();
                ActivateNextWaveButton();
            }
        }

        private void ActivateNextWaveButton()
        {
            _nextWaveReady = true;
            onNextWaveReady?.Invoke();
        }

        private void SetPhase(RoundPhase p) { phase = p; onPhaseChanged?.Invoke(p); }

        private RD_WaveDataSO GetWaveData(int round)
        {
            int idx = round - 1;
            if (idx >= 0 && idx < waveTable.Count && waveTable[idx] != null)
                return waveTable[idx];
            if (waveGenerator != null)
                return waveGenerator.GenerateWave(round);
            return null;
        }

        public static RoundType GetRoundType(int round)
        {
            if (round == 50)     return RoundType.FinalBoss;
            if (round % 15 == 0) return RoundType.Boss;
            return RoundType.Normal;
        }
    }
}
