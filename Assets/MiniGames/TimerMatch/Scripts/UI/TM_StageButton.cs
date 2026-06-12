using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.TimerMatch
{
    public class TM_StageButton : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private Button          button;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject      lockIcon;
        [SerializeField] private GameObject      clearIcon;

        private TM_StageDataSO _stage;
        private Action<TM_StageDataSO> _onClick;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(HandleClick);
        }

        public void Setup(TM_StageDataSO stage, Action<TM_StageDataSO> onClick)
        {
            _stage   = stage;
            _onClick = onClick;

            gameObject.SetActive(stage != null);
            if (stage == null) return;

            bool unlocked = TM_StageUnlock.IsUnlocked(stage.stageIndex);
            bool cleared  = TM_StageUnlock.IsCleared(stage.stageIndex);

            if (button    != null) button.interactable = unlocked;
            if (lockIcon  != null) lockIcon.SetActive(!unlocked);
            if (clearIcon != null) clearIcon.SetActive(cleared);
            if (label     != null) label.text = $"Stage {stage.stageIndex}";
        }

        private void HandleClick()
        {
            if (_stage != null) _onClick?.Invoke(_stage);
        }
    }
}
