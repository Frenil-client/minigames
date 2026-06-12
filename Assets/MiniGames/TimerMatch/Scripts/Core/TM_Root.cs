using MiniGames.Main;
using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_Root : MonoBehaviour, IMiniGame
    {
        [SerializeField] private TM_GameManager gameManager;

        [Header("점수 저장")]
        [SerializeField] private string gameKey = "TimerMatch";

        [Header("독립 실행 테스트")]
        [SerializeField] private bool autoStartOnPlay = true;

        private void Start()
        {
            if (autoStartOnPlay) OnMiniGameStart();
        }

        public void OnMiniGameStart()
        {
            gameManager.onStageFinished += OnStageFinished;

            int selectedStage = Mathf.Max(1, MiniGameSession.launchParameter);
            if (gameManager.StartStageByIndex(selectedStage)) return;
            if (gameManager.StartStageByIndex(1)) return;

            gameManager.OpenStageSelect();
        }

        public void OnMiniGameExit()
        {
            gameManager.onStageFinished -= OnStageFinished;
            gameManager.StopGame();
            Time.timeScale = 1f;
        }

        private void OnStageFinished(bool isClear, int score)
        {
            GameScoreManager.TryUpdateBest(gameKey, score);
        }
    }
}
