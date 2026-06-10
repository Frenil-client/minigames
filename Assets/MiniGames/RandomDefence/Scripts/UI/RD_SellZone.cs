using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.RandomDefence
{
    public class RD_SellZone : MonoBehaviour
    {
        public static RD_SellZone instance { get; private set; }

        [Header("참조")]
        [SerializeField] private Canvas          canvas;
        [SerializeField] private GameObject      zoneRoot;
        [SerializeField] private RectTransform   zoneRect;
        [SerializeField] private Image           zoneImage;
        [SerializeField] private TextMeshProUGUI priceText;

        [Header("색상")]
        [SerializeField] private Color normalColor = new Color(0.9f, 0.2f, 0.2f, 0.35f);
        [SerializeField] private Color hoverColor  = new Color(0.9f, 0.2f, 0.2f, 0.6f);

        private void Awake()
        {
            instance = this;
            if (zoneRoot != null) zoneRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Show(RD_TowerBase tower)
        {
            if (instance == null || tower == null) return;
            instance.ShowInternal(tower);
        }

        public static void Hide()
        {
            if (instance != null && instance.zoneRoot != null)
                instance.zoneRoot.SetActive(false);
        }

        public static bool Contains(Vector3 worldPos)
        {
            return instance != null && instance.ContainsInternal(worldPos);
        }

        public static void UpdateHover(Vector3 worldPos)
        {
            if (instance == null || instance.zoneImage == null) return;
            if (instance.zoneRoot == null || !instance.zoneRoot.activeSelf) return;
            instance.zoneImage.color =
                Contains(worldPos) ? instance.hoverColor : instance.normalColor;
        }

        private void ShowInternal(RD_TowerBase tower)
        {
            var cam = RD_GameManager.instance?.cam;
            if (cam == null || zoneRoot == null) return;

            if (canvas != null && canvas.worldCamera == null)
                canvas.worldCamera = cam;

            if (zoneImage != null) zoneImage.color = normalColor;
            if (priceText != null) priceText.text = $"{tower.sellPrice}";

            zoneRoot.SetActive(true);
        }

        private bool ContainsInternal(Vector3 worldPos)
        {
            if (zoneRoot == null || !zoneRoot.activeSelf || zoneRect == null) return false;

            var cam = canvas != null && canvas.worldCamera != null
                ? canvas.worldCamera
                : RD_GameManager.instance?.cam;
            if (cam == null) return false;

            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
            return RectTransformUtility.RectangleContainsScreenPoint(zoneRect, screenPos, cam);
        }
    }
}
