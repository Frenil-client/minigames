using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_MergeEffect : MonoBehaviour
    {
        public static RD_MergeEffect instance { get; private set; }

        [Header("공통 이펙트")]
        [Tooltip("학년 무관 공통 파티클. gradeParticles에 해당 항목이 없을 때도 사용됩니다.")]
        [SerializeField] private ParticleSystem mergeParticle;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void PlayAt(Vector3 worldPosition, int resultGrade = 0)
        {
            if (instance == null) return;
            instance.Play(worldPosition, resultGrade);
        }

        private void Play(Vector3 position, int resultGrade)
        {
            ParticleSystem ps = GetParticleForGrade(resultGrade);
            if (ps == null) return;

            ps.transform.position = position;

            if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        private ParticleSystem GetParticleForGrade(int resultGrade)
        {
            return mergeParticle;
        }
    }
}
