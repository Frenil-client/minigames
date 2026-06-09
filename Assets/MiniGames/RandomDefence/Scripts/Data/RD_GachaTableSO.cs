using UnityEngine;

namespace MiniGames.RandomDefence
{
    [CreateAssetMenu(menuName = "RandomDefence/GachaTable", fileName = "RD_GachaTable")]
    public class RD_GachaTableSO : ScriptableObject
    {
        [Tooltip("1회 뽑기 비용 (물방울)")]
        public int costPerPull = 30;

        [Tooltip("학년 가중치. index 0=1학년 ~ index 5=6학년\n" +
                 "실측값: 87.21 / 9.97 / 1.87 / 0.83 / 0.10 / 0.02 (합계=100%)")]
        public float[] gradeWeights = { 87.21f, 9.97f, 1.87f, 0.83f, 0.10f, 0.02f };

        [Tooltip("타입 가중치. Ner / Erphin / Sion / Epica 순서 (균등=1:1:1:1)")]
        public float[] typeWeights = { 1f, 1f, 1f, 1f };
    }
}
