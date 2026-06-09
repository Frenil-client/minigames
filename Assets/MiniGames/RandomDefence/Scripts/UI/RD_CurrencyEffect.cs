using System.Collections;
using TMPro;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_CurrencyEffect : MonoBehaviour
    {
        [Header("컴포넌트 참조")]
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private TextMeshPro    amountText;

        [Header("Phase 1 - 팝 업")]
        [Tooltip("팝 업 지속 시간 (초)")]
        [SerializeField] private float popDuration   = 0.2f;
        [Tooltip("팝 업 시 위로 이동 거리 (월드 유닛)")]
        [SerializeField] private float popRise       = 0.5f;
        [Tooltip("시작 스케일")]
        [SerializeField] private float popScaleStart = 0.4f;
        [Tooltip("팝 업 최대 스케일 (1.0 초과면 살짝 커졌다가 줄어드는 느낌)")]
        [SerializeField] private float popScalePeak  = 1.2f;

        [Header("Phase 2 - 정착 (위아래 바운스)")]
        [Tooltip("정착 지속 시간 (초)")]
        [SerializeField] private float settleDuration = 0.15f;
        [Tooltip("정착 시 아래로 내려오는 거리 (양수 입력 -> 내려감)")]
        [SerializeField] private float settleDropDist = 0.12f;

        public void Play(int amount)
        {
            if (amountText != null)
                amountText.text = $"+{amount}";

            transform.localScale = Vector3.one * popScaleStart;
            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            Vector3 origin = transform.position;

            for (float t = 0f; t < popDuration; t += Time.unscaledDeltaTime)
            {
                float n = EaseOut(t / popDuration);
                transform.position   = origin + Vector3.up * Mathf.Lerp(0f, popRise, n);
                transform.localScale = Vector3.one * Mathf.Lerp(popScaleStart, popScalePeak, n);
                yield return null;
            }

            Vector3 p1 = origin + Vector3.up * popRise;
            for (float t = 0f; t < settleDuration; t += Time.unscaledDeltaTime)
            {
                float n = t / settleDuration;
                transform.position   = p1 - Vector3.up * Mathf.Lerp(0f, settleDropDist, n);
                transform.localScale = Vector3.one * Mathf.Lerp(popScalePeak, 1.0f, n);
                yield return null;
            }


            Destroy(gameObject);
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    }
}
