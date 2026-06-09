using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    [System.Serializable]
    public class RD_SpawnEntry
    {
        public RD_MonsterDataSO monster;
        public int              count;
        [Tooltip("각 몬스터 사이 스폰 간격 (초)")]
        public float            spawnInterval = 1f;
    }

    [CreateAssetMenu(menuName = "RandomDefence/WaveData", fileName = "RD_WaveData_R")]
    public class RD_WaveDataSO : ScriptableObject
    {
        public int  roundNumber;
        public bool isBoss;

        [Tooltip("(미사용) 구 준비 페이즈 시간. 현재 웨이브 플로우에서는 사용되지 않습니다.")]
        public float prepTime          = 0f;

        [Tooltip("보스 라운드 전투 제한 시간(초). 0이면 RoundManager 기본값(150초) 사용.")]
        public float bossTimeLimit     = 0f;

        [Tooltip("보스 라운드 진입(준비 페이즈 시작) 시 지급하는 별 재화. §3.1")]
        public int   starCurrencyReward = 0;

        public List<RD_SpawnEntry> spawnEntries = new();
    }
}
