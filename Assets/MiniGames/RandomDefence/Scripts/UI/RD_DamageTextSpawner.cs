using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_DamageTextSpawner : MonoBehaviour
    {
        public static RD_DamageTextSpawner instance { get; private set; }

        [Header("데미지 텍스트 프리팹")]
        [SerializeField] private RD_DamageText damageTextPrefab;

        [Tooltip("생성된 텍스트 오브젝트의 부모. 비워두면 이 오브젝트 하위에 생성됩니다.")]
        [SerializeField] private Transform container;

        [Header("색상")]
        [SerializeField] private Color normalColor    = Color.white;
        [SerializeField] private Color armorBreakColor = new Color(1f, 0.55f, 0f);

        private void Awake()
        {
            instance = this;
            if (container == null) container = transform;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Show(float damage, Vector3 worldPos, bool armorBreak = false)
        {
            if (instance == null || instance.damageTextPrefab == null) return;
            instance.Spawn(damage, worldPos, armorBreak);
        }

        private void Spawn(float damage, Vector3 worldPos, bool armorBreak)
        {
            var text = Instantiate(damageTextPrefab, worldPos, Quaternion.identity, container);
            text.Play(damage, armorBreak ? armorBreakColor : normalColor);
        }
    }
}
