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

        public Action<MiniGameData> onStartClicked;

        private MiniGameData _data;
        private bool         _initialized;

        public void Populate(MiniGameData data)
        {
            EnsureInitialized();

            _data = data;

            if (thumbnailRImage != null) thumbnailRImage.texture = data.mainBackground;
            if (gameNameText    != null) gameNameText.text       = data.gameName;

            int best = GameScoreManager.GetBest(data.gameName);
            if (bestScoreText != null)
                bestScoreText.text = best > 0 ? $"{best:N0}" : "-";
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            if (startButton != null)
                startButton.onClick.AddListener(() => onStartClicked?.Invoke(_data));
        }
    }
}
