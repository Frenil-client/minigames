using UnityEngine;

namespace MiniGames.RandomDefence
{
    [CreateAssetMenu(menuName = "RandomDefence/MonsterData", fileName = "RD_MonsterData_")]
    public class RD_MonsterDataSO : ScriptableObject
    {
        public string     monsterId;
        public Sprite     sprite;
        public float      hp;
        public float      defense;
        public float      moveSpeed;
        public int        rewardCurrency;
        public int        scoreValue;
        [Tooltip("RD_MonsterBase 가 붙은 몬스터 프리팹")]
        public GameObject prefab;
    }
}
