using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Main
{
    public class GameDetailPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RawImage        thumbnailRImage;
        [SerializeField] private TextMeshProUGUI gameNameText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private Button          startButton;

        [Header("난이도 선택")]
        [SerializeField] private GameObject      stageSelectRoot;
        [SerializeField] private Button          prevStageButton;
        [SerializeField] private Button          nextStageButton;
        [SerializeField] private TextMeshProUGUI stageLabel;

        public Action<MiniGameData> onStartClicked;

        private MiniGameData _data;
        private bool         _initialized;
        private bool         _useStages;
        private int          _selectedStage = 1;
        private int          _maxSelectable = 1;

        public void Populate(MiniGameData data)
        {
            EnsureInitialized();

            _data = data;

            if (thumbnailRImage != null) thumbnailRImage.texture = data.mainBackground;
            if (gameNameText    != null) gameNameText.text       = data.gameName;

            int best = GameScoreManager.GetBest(data.gameName);
            if (bestScoreText != null)
                bestScoreText.text = best > 0 ? $"{best:N0}" : "-";

            SetupStageSelect(data);
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            if (startButton != null)
                startButton.onClick.AddListener(OnStart);
            if (prevStageButton != null)
                prevStageButton.onClick.AddListener(() => ChangeStage(-1));
            if (nextStageButton != null)
                nextStageButton.onClick.AddListener(() => ChangeStage(1));
        }

        private void SetupStageSelect(MiniGameData data)
        {
            _useStages = data.stageCount > 0;

            if (stageSelectRoot != null) stageSelectRoot.SetActive(_useStages);
            if (!_useStages) return;

            _maxSelectable = string.IsNullOrEmpty(data.stageUnlockPrefsKey)
                ? data.stageCount
                : Mathf.Clamp(PlayerPrefs.GetInt(data.stageUnlockPrefsKey, 1), 1, data.stageCount);

            _selectedStage = _maxSelectable;
            RefreshStageSelect();
        }

        private void ChangeStage(int delta)
        {
            _selectedStage = Mathf.Clamp(_selectedStage + delta, 1, _maxSelectable);
            RefreshStageSelect();
        }

        private void RefreshStageSelect()
        {
            if (stageLabel != null)
                stageLabel.text = $"난이도 {_selectedStage}";

            if (prevStageButton != null)
                prevStageButton.gameObject.SetActive(_selectedStage > 1);
            if (nextStageButton != null)
                nextStageButton.gameObject.SetActive(_selectedStage < _maxSelectable);
        }

        private void OnStart()
        {
            MiniGameSession.launchParameter = _useStages ? _selectedStage : 0;
            onStartClicked?.Invoke(_data);
        }
    }
}
