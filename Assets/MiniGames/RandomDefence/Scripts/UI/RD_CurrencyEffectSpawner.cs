using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_CurrencyEffectSpawner : MonoBehaviour
    {
        public static RD_CurrencyEffectSpawner instance { get; private set; }

        [Header("Currency Effect Prefab")]
        [SerializeField] private RD_CurrencyEffect currencyEffectPrefab;

        [Header("재화 아이콘 스프라이트")]

        [Tooltip("생성된 팝업의 부모 Transform. 비워두면 이 오브젝트 하위에 생성됩니다.")]
        [SerializeField] private Transform container;

        private void Awake()
        {
            instance = this;
            if (container == null) container = transform;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Show(int amount, Vector3 worldPos)
        {
            if (instance == null || instance.currencyEffectPrefab == null || amount <= 0) return;
            instance.Spawn(amount, worldPos);
        }

        private void Spawn(int amount, Vector3 worldPos)
        {
            var go = Instantiate(currencyEffectPrefab, worldPos, Quaternion.identity, container);
            go.Play(amount);
        }
    }
}
