using System;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TowerLevelManager : MonoBehaviour
    {
        public const int MAX_LEVEL = 10;

        [Header("레벨당 데미지 배율 증가 (0.1 = +10%)")]
        [SerializeField] private float damagePerLevel = 0.1f;

        [Header("레벨업 1회 비용 (별 재화)")]
        [SerializeField] private int upgradeCostPerLevel = 100;

        private readonly int[] _levels;

        public RD_TowerLevelManager()
        {
            int count = Enum.GetValues(typeof(TowerType)).Length;
            _levels = new int[count];
            for (int i = 0; i < count; i++) _levels[i] = 1;
        }

        public event Action<TowerType, int> onLevelChanged;

        public void Reset()
        {
            for (int i = 0; i < _levels.Length; i++)
            {
                _levels[i] = 1;
                onLevelChanged?.Invoke((TowerType)i, 1);
            }
        }

        public int   GetLevel(TowerType t) => _levels[(int)t];
        public bool  IsMaxLevel(TowerType t) => GetLevel(t) >= MAX_LEVEL;
        public int   GetUpgradeCost(TowerType t) => IsMaxLevel(t) ? -1 : upgradeCostPerLevel;

        public float GetDamageMultiplier(TowerType t)
            => 1f + (GetLevel(t) - 1) * damagePerLevel;

        public bool TryUpgrade(TowerType t)
        {
            if (IsMaxLevel(t)) return false;

            int cost = GetUpgradeCost(t);
            if (!RD_GameManager.instance.economyMgr.TrySpendStar(cost)) return false;

            _levels[(int)t]++;
            onLevelChanged?.Invoke(t, _levels[(int)t]);
            return true;
        }
    }
}
