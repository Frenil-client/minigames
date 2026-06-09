using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TowerGradeStars : MonoBehaviour
    {
        [Header("별 오브젝트 (프리팹에 미리 6개 배치)")]
        [Tooltip("index 0 = 1번째 별. 총 6개 모두 채워주세요.")]
        [SerializeField] private GameObject[] starObjects = new GameObject[6];

        [Header("레이아웃")]
        [Tooltip("별 중심 사이의 간격 (월드 유닛)")]
        [SerializeField] private float starSpacing = 0.25f;
        [Tooltip("이 컴포넌트 피벗 기준 세로 오프셋 (월드 유닛, 양수 = 위)")]
        [SerializeField] private float yOffset     = 0.8f;

        private void Awake()
        {
            foreach (var s in starObjects)
                if (s != null) s.SetActive(false);
        }

        public void SetGrade(int grade)
        {
            int count = Mathf.Clamp(grade, 0, starObjects.Length);

            float totalWidth = (count - 1) * starSpacing;
            float startX     = -totalWidth * 0.5f;

            for (int i = 0; i < starObjects.Length; i++)
            {
                if (starObjects[i] == null) continue;

                bool active = i < count;
                starObjects[i].SetActive(active);

                if (active)
                    starObjects[i].transform.localPosition =
                        new Vector3(startX + i * starSpacing, yOffset, 0f);
            }
        }
    }
}
