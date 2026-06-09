using UnityEngine;

namespace MiniGames.RandomDefence
{
    [CreateAssetMenu(menuName = "RandomDefence/TowerData", fileName = "RD_TowerData_")]
    public class RD_TowerDataSO : ScriptableObject
    {
        [Header("기본 정보")]
        public TowerType type;

        [Tooltip("학년별 스프라이트. index 0 = 1학년, index 5 = 6학년")]
        public Sprite[] gradeSprites = new Sprite[6];

        [Header("공격력 (학년별 고정값)")]
        [Tooltip("index 0 = 1학년. 비어 있으면 아래 linear 공식을 사용.\n" +
                 "Ner:    [4, 7, 14, 22, 40, 80]\n" +
                 "Erphin: [4, 9, 16, 28, 56, 100]\n" +
                 "Sion:   [3, 5,  9, 14, 24, 45]")]
        public float[] attackDamageByGrade = new float[0];

        [Header("공격력 (선형 공식 폴백)")]
        [Tooltip("attackDamageByGrade 배열이 비어 있거나 grade > 배열 크기일 때 사용")]
        public float baseAttackDamage;
        public float attackDamagePerGrade;

        [Header("공격 속도 / 범위")]
        public float      baseAttackSpeed;
        public float      attackSpeedPerGrade;
        public float      attackRange;
        public AttackType attackType;
        public float      aoeRadius;

        [Header("CC")]
        public CCType ccType;
        public int    ccMinGrade;
        public float  baseCCProbability;
        public float  ccProbabilityPerGrade;
        public float  ccDuration;

        [Header("판매가 (공통: 15 × grade)")]
        [Tooltip("baseSellPrice=15, sellPricePerGrade=15 → grade1=15, grade2=30 ...")]
        public int baseSellPrice      = 15;
        public int sellPricePerGrade  = 15;

        public float GetAttackDamage(int grade)
        {
            if (attackDamageByGrade is { Length: > 0 } && grade >= 1 && grade <= attackDamageByGrade.Length)
                return attackDamageByGrade[grade - 1];
            if (attackDamageByGrade is { Length: > 0 } && grade > attackDamageByGrade.Length)
            {
                float last = attackDamageByGrade[attackDamageByGrade.Length - 1];
                float step = attackDamageByGrade.Length >= 2
                    ? last - attackDamageByGrade[attackDamageByGrade.Length - 2]
                    : attackDamagePerGrade;
                return last + step * (grade - attackDamageByGrade.Length);
            }
            return baseAttackDamage + attackDamagePerGrade * (grade - 1);
        }

        public float GetAttackSpeed(int grade) =>
            Mathf.Max(baseAttackSpeed + attackSpeedPerGrade * (grade - 1), 0.1f);

        public float GetCCProbability(int grade)
        {
            if (grade < ccMinGrade) return 0f;
            return Mathf.Clamp01(baseCCProbability + ccProbabilityPerGrade * (grade - ccMinGrade));
        }

        public int GetSellPrice(int grade) =>
            baseSellPrice + sellPricePerGrade * (grade - 1);

        public Sprite GetSprite(int grade) =>
            gradeSprites != null && grade >= 1 && grade <= gradeSprites.Length
                ? gradeSprites[grade - 1] : null;
    }
}
