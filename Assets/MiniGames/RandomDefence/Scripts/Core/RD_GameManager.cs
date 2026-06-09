using System;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_GameManager : MonoBehaviour
    {
        public static RD_GameManager instance { get; private set; }

        [Header("카메라")]
        [SerializeField] private Camera gameCamera;
        public Camera cam => gameCamera;

        [Header("도메인 매니저")]
        [SerializeField] private RD_RoundManager        roundManager;
        [SerializeField] private RD_WaveSpawner         waveSpawner;
        [SerializeField] private RD_TowerManager        towerManager;
        [SerializeField] private RD_EconomyManager      economyManager;
        [SerializeField] private RD_ScoreManager        scoreManager;
        [SerializeField] private RD_TimeScaleController timeScaleController;
        [SerializeField] private RD_GachaSystem         gachaSystem;
        [SerializeField] private RD_TowerLevelManager   towerLevelManager;

        public RD_RoundManager        roundMgr   => roundManager;
        public RD_WaveSpawner         waveMgr    => waveSpawner;
        public RD_TowerManager        towerMgr   => towerManager;
        public RD_EconomyManager      economyMgr => economyManager;
        public RD_ScoreManager        scoreMgr   => scoreManager;
        public RD_TimeScaleController timeMgr    => timeScaleController;
        public RD_GachaSystem         gachaSys   => gachaSystem;
        public RD_TowerLevelManager   levelMgr   => towerLevelManager;

        public GameState state { get; private set; } = GameState.Idle;

        public Action onGameOver;
        public Action onClear;
        public Action onPauseChanged;

        private void Awake()     { instance = this; }
        private void OnDestroy() { if (instance == this) instance = null; }

        public void StartGame()
        {
            timeMgr.Reset();
            state = GameState.Playing;
            economyMgr.Initialize();
            scoreMgr.Initialize();
            roundMgr.StartRounds();
        }

        public void TogglePause()
        {
            if (state is GameState.GameOver or GameState.Clear) return;
            bool pause = state != GameState.Paused;
            state = pause ? GameState.Paused : GameState.Playing;
            timeMgr.SetPause(pause);
            onPauseChanged?.Invoke();
        }

        public void TriggerGameOver()
        {
            if (state is GameState.GameOver or GameState.Clear) return;
            state = GameState.GameOver;
            timeMgr.SetPause(true);
            onGameOver?.Invoke();
        }

        public void TriggerClear()
        {
            if (state == GameState.Clear) return;
            state = GameState.Clear;
            onClear?.Invoke();
        }

        public void RestartGame()
        {
            roundMgr.StopRounds();
            waveMgr.ClearAll();
            towerMgr.ClearAll();
            levelMgr.Reset();
            timeMgr.Reset();

            state = GameState.Playing;
            economyMgr.Initialize();
            scoreMgr.Initialize();
            roundMgr.StartRounds();
        }

        public void StopGame()
        {
            waveMgr.ClearAll();
            timeMgr.Reset();
            state = GameState.Idle;
        }
    }
}
