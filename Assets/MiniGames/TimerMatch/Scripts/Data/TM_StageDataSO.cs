using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_RoundSpec
    {
        public const float MaxTime = 15f;

        public float          targetTime;
        public float          speedMultiplier;
        public StartDirection startDirection;
        public float          hideStartRatio;
    }

    [CreateAssetMenu(menuName = "TimerMatch/StageData", fileName = "TM_Stage")]
    public class TM_StageDataSO : ScriptableObject
    {
        public int stageIndex = 1;

        [Header("누적 오차 한도 (초) — 초과 시 게임오버")]
        public float maxTotalError = 4f;

        [Header("타이머 숨김 (1 = 숨기지 않음)")]
        [Range(0f, 1f)]
        public float hideStartRatio = 1f;
    }
}
