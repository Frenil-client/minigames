using MiniGames.Main;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_Root : MonoBehaviour, IMiniGame
    {
        [SerializeField] private RD_GameManager gameManager;

        [Header("점수 저장")]
        [SerializeField] private string gameKey = "RandomDefence";

        [Header("독립 실행 테스트")]
        [SerializeField] private bool autoStartOnPlay = true;

        private void Start()
        {
            if (autoStartOnPlay) OnMiniGameStart();
        }

        public void OnMiniGameStart()
        {
            gameManager.onGameOver += OnGameEnded;
            gameManager.onClear    += OnGameEnded;
            gameManager.StartGame();
        }

        public void OnMiniGameExit()
        {
            gameManager.onGameOver -= OnGameEnded;
            gameManager.onClear    -= OnGameEnded;
            gameManager.StopGame();
            Time.timeScale = 1f;
        }

        private void OnGameEnded()
        {
            int score = gameManager.scoreMgr?.totalScore ?? 0;
            GameScoreManager.TryUpdateBest(gameKey, score);
        }
    }
}
