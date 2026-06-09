using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_RangeIndicator : MonoBehaviour
    {
        public static RD_RangeIndicator instance { get; private set; }

        [SerializeField]
        private GameObject circleObject;
        private void Awake()
        {
            instance = this;
            circleObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void ShowAt(Vector3 center, float radius)
        {
            if (instance == null) return;
            instance.transform.position   = center;
            instance.transform.localScale = Vector3.one * (radius * 2f);
            instance.circleObject.SetActive(true);
        }

        public static void Hide()
        {
            if (instance != null)
                instance.circleObject.SetActive(false);
        }
    }
}
