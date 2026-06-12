using TMPro;
using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_StageSelectPanel : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject       panelRoot;
        [SerializeField] private TM_StageButton[] stageButtons;
        [SerializeField] private TextMeshProUGUI  bestRecordText;

        private TM_GameManager _gm;

        private void Start()
        {
            _gm = TM_GameManager.instance;
            if (_gm == null) return;

            _gm.onStateChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_gm != null) _gm.onStateChanged -= Refresh;
        }

        private void Refresh()
        {
            bool active = _gm.state == TM_GameState.StageSelect;
            if (panelRoot != null) panelRoot.SetActive(active);
            if (!active) return;

            for (int i = 0; i < stageButtons.Length; i++)
            {
                if (stageButtons[i] == null) continue;
                var stage = i < _gm.allStages.Count ? _gm.allStages[i] : null;
                stageButtons[i].Setup(stage, OnStageSelected);
            }

            RefreshBestRecord();
        }

        private void RefreshBestRecord()
        {
            if (bestRecordText == null) return;

            int bestStage = 0;
            int bestScore = 0;
            foreach (var stage in _gm.allStages)
            {
                int score = TM_StageUnlock.GetHighScore(stage.stageIndex);
                if (score > 0 && stage.stageIndex >= bestStage)
                {
                    bestStage = stage.stageIndex;
                    bestScore = score;
                }
            }

            bestRecordText.text = bestStage > 0
                ? $"최고 기록: Stage {bestStage} / {bestScore:N0}점"
                : "최고 기록: -";
        }

        private void OnStageSelected(TM_StageDataSO stage)
        {
            _gm.StartStage(stage);
        }
    }
}
