using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_MonsterHpBar : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject     barRoot;
        [SerializeField] private Transform      fillPivot;
        [SerializeField] private SpriteRenderer fillRenderer;

        [Header("색상")]
        [SerializeField] private Color highColor = new Color(0.25f, 0.9f, 0.3f);
        [SerializeField] private Color lowColor  = new Color(0.95f, 0.25f, 0.2f);

        public bool Owns(Transform t)
            => barRoot != null && t.IsChildOf(barRoot.transform);

        public void SetRatio(float ratio)
        {
            if (barRoot == null || fillPivot == null) return;

            ratio = Mathf.Clamp01(ratio);

            if (ratio >= 1f)
            {
                if (barRoot.activeSelf) barRoot.SetActive(false);
                return;
            }

            fillPivot.localScale = new Vector3(ratio, 1f, 1f);

            if (fillRenderer != null)
                fillRenderer.color = Color.Lerp(lowColor, highColor, ratio);

            if (!barRoot.activeSelf) barRoot.SetActive(true);
        }

        public void Hide()
        {
            if (barRoot != null) barRoot.SetActive(false);
        }
    }
}
