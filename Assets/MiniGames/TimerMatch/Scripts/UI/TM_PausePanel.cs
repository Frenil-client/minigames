using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.TimerMatch
{
    public class TM_PausePanel : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button     exitButton;
        [SerializeField] private Button     resumeButton;
        [SerializeField] private Button     retryButton;

        private TM_GameManager _gm;

        private void Start()
        {
            _gm = TM_GameManager.instance;
            if (_gm == null) return;

            if (exitButton   != null) exitButton.onClick.AddListener(() => _gm.ExitGame());
            if (resumeButton != null) resumeButton.onClick.AddListener(() => _gm.SetPaused(false));
            if (retryButton  != null) retryButton.onClick.AddListener(() => _gm.RetryStage());

            _gm.onPauseChanged += OnPauseChanged;
            OnPauseChanged(_gm.isPaused);
        }

        private void OnDestroy()
        {
            if (_gm != null) _gm.onPauseChanged -= OnPauseChanged;
        }

        private void OnPauseChanged(bool paused)
        {
            if (panelRoot != null) panelRoot.SetActive(paused);
        }
    }
}
