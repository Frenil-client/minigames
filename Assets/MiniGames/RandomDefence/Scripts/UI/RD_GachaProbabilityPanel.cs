using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.RandomDefence
{
    public class RD_GachaProbabilityPanel : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RD_GachaTableSO gachaTable;
        [SerializeField] private Button          toggleButton;
        [SerializeField] private GameObject      panelRoot;

        [SerializeField] private TextMeshProUGUI[] percentTexts = new TextMeshProUGUI[6];

        private void Start()
        {
            if (panelRoot   != null) panelRoot.SetActive(false);
            if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
        }

        public void Toggle()
        {
            if (panelRoot == null) return;
            bool next = !panelRoot.activeSelf;
            panelRoot.SetActive(next);
            if (next) RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (gachaTable == null)
            {
                Debug.LogWarning("[RD_GachaProbabilityPanel] GachaTable 미연결");
                return;
            }

            float[] weights = gachaTable.gradeWeights;

            float total = 0f;
            foreach (float w in weights) total += w;
            if (total <= 0f) return;

            int rowCount = Mathf.Min(weights.Length, percentTexts.Length);
            for (int i = 0; i < rowCount; i++)
            {
                if (percentTexts.Length > i && percentTexts[i] != null)
                {
                    float pct = weights[i] / total * 100f;
                    percentTexts[i].text = $"{pct:F2}%";
                }
            }
        }
    }
}
