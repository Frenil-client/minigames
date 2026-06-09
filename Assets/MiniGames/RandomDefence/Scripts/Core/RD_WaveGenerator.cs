using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_WaveGenerator : MonoBehaviour
    {
        [Header("일반 몬스터")]
        [SerializeField] private RD_MonsterDataSO normalMonster;

        [Header("보스 몬스터 (라운드별)")]
        [SerializeField] private RD_MonsterDataSO boss15;
        [SerializeField] private RD_MonsterDataSO boss30;
        [SerializeField] private RD_MonsterDataSO boss45;
        [SerializeField] private RD_MonsterDataSO boss50;

        [Header("일반 웨이브 스케일링")]
        [Tooltip("라운드 1 기본 몬스터 수")]
        [SerializeField] private int   baseCount      = 5;
        [Tooltip("라운드당 추가 몬스터 수")]
        [SerializeField] private int   countPerRound  = 2;
        [Tooltip("최대 몬스터 수")]
        [SerializeField] private int   maxCount       = 60;
        [Tooltip("몬스터 간 스폰 간격 (초)")]
        [SerializeField] private float spawnInterval  = 0.5f;

        public RD_WaveDataSO GenerateWave(int round)
        {
            var wave           = ScriptableObject.CreateInstance<RD_WaveDataSO>();
            wave.roundNumber   = round;
            wave.isBoss        = round % 15 == 0 || round == 50;

            if (wave.isBoss)
                BuildBossWave(wave, round);
            else
                BuildNormalWave(wave, round);

            return wave;
        }

        private void BuildNormalWave(RD_WaveDataSO wave, int round)
        {
            wave.prepTime = 10f;

            if (normalMonster == null) return;

            int count = Mathf.Min(baseCount + (round - 1) * countPerRound, maxCount);
            wave.spawnEntries.Add(new RD_SpawnEntry
            {
                monster       = normalMonster,
                count         = count,
                spawnInterval = spawnInterval
            });
        }

        private void BuildBossWave(RD_WaveDataSO wave, int round)
        {
            wave.prepTime          = 120f;
            wave.bossTimeLimit     = 120f;
            wave.starCurrencyReward = GetBossStarReward(round);

            RD_MonsterDataSO boss = GetBossData(round);
            if (boss == null) return;

            wave.spawnEntries.Add(new RD_SpawnEntry
            {
                monster       = boss,
                count         = 1,
                spawnInterval = 0f
            });
        }

        private static int GetBossStarReward(int round)
        {
            if (round == 15) return 300;
            if (round == 30) return 500;
            return 0;
        }

        private RD_MonsterDataSO GetBossData(int round) => round switch
        {
            15 => boss15,
            30 => boss30,
            45 => boss45,
            50 => boss50,
            _  => boss15
        };
    }
}
