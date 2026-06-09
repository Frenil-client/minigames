using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.RandomDefence
{
    public class RD_StatusPanel : MonoBehaviour
    {
        private enum PanelMode { Pause, GameOver, Clear }

        [Header("타이틀")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private string          pauseTitle    = "일시 정지";
        [SerializeField] private string          gameOverTitle = "Lose";
        [SerializeField] private string          clearTitle    = "Clear";

        [Header("스탯 텍스트")]
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private TextMeshProUGUI bestGradeText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("버튼")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button resumeButton;

        private void Start()
        {
            gameObject.SetActive(false);

            var gm = RD_GameManager.instance;
            gm.onPauseChanged += OnPauseChanged;
            gm.onGameOver     += OnGameOver;
            gm.onClear        += OnClear;

            if (exitButton    != null) exitButton.onClick.AddListener(OnExit);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (resumeButton  != null) resumeButton.onClick.AddListener(OnResume);
        }

        private void OnPauseChanged()
        {
            bool paused = RD_GameManager.instance.state == GameState.Paused;
            if (paused)
                Show(PanelMode.Pause);
            else
                gameObject.SetActive(false);
        }

        private void OnGameOver() => Show(PanelMode.GameOver);
        private void OnClear()    => Show(PanelMode.Clear);

        private void Show(PanelMode mode)
        {
            gameObject.SetActive(true);

            if (titleText != null)
                titleText.text = mode switch
                {
                    PanelMode.Pause    => pauseTitle,
                    PanelMode.GameOver => gameOverTitle,
                    PanelMode.Clear    => clearTitle,
                    _                  => string.Empty
                };

            if (resumeButton != null)
                resumeButton.gameObject.SetActive(mode == PanelMode.Pause);

            RefreshStats();
        }

        private void RefreshStats()
        {
            var gm = RD_GameManager.instance;
            if (gm == null) return;

            int round     = gm.roundMgr.currentRound;
            int kills     = gm.scoreMgr.killCount;
            int bestGrade = gm.scoreMgr.bestTowerGrade;
            int score     = gm.scoreMgr.totalScore;

            if (roundText    != null) roundText.text    = $"{round} / {RD_RoundManager.TOTAL_ROUNDS}";
            if (killCountText != null) killCountText.text = kills.ToString();
            if (bestGradeText != null) bestGradeText.text = $"{bestGrade}학년";
            if (scoreText    != null) scoreText.text    = score.ToString("N0");
        }

        private void OnResume()
        {
            RD_GameManager.instance.TogglePause();
        }

        private void OnRestart()
        {
            var gm = RD_GameManager.instance;
            if (gm == null) return;

            gameObject.SetActive(false);
            gm.RestartGame();
        }

        private void OnExit()
        {
            var launcher = FindObjectOfType<Main.MiniGameLauncher>(true);
            if (launcher != null)
            {
                launcher.Exit();
            }
            else
            {
                RD_GameManager.instance?.StopGame();
                Time.timeScale = 1f;
            }
        }
    }
}
