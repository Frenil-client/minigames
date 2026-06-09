using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.RandomDefence
{
    public class RD_LevelUpButtons : MonoBehaviour
    {
        public static RD_LevelUpButtons instance { get; private set; }

        [Header("타입별 UI  index: 0=Worrior  1=Mage  2=Bowman")]
        [SerializeField] private Button[] levelupButtons;
        [SerializeField] private TextMeshProUGUI[] levelTexts;
        [SerializeField] private TextMeshProUGUI[] costTexts;


        private RD_TowerLevelManager _levelMgr;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            var gm = RD_GameManager.instance;
            _levelMgr = gm.levelMgr;

            _levelMgr.onLevelChanged  += (t, _) => RefreshEntry(t);
            gm.economyMgr.onStarChanged += _ => RefreshAll();

            if (levelupButtons != null)
            {
                for (int i = 0; i < levelupButtons.Length; i++)
                {
                    if (levelupButtons[i] != null)
                    {
                        int index = i;
                        levelupButtons[i].onClick.AddListener(()=> { OnUpgradeClicked(index); });
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            var gm = RD_GameManager.instance;
            if (gm == null) return;
            gm.economyMgr.onStarChanged         -= _ => RefreshAll();
            if (_levelMgr != null)
                _levelMgr.onLevelChanged -= (t, _) => RefreshEntry(t);
        }

        public void OnUpgradeClicked(int typeIndex)
        {
            var values = System.Enum.GetValues(typeof(TowerType));
            if (typeIndex < 0 || typeIndex >= values.Length) return;

            TowerType t = (TowerType)typeIndex;
            _levelMgr?.TryUpgrade(t);
            RefreshEntry(t);
        }


        private void RefreshAll()
        {
            var econMgr = RD_GameManager.instance?.economyMgr;

            foreach (TowerType t in System.Enum.GetValues(typeof(TowerType)))
                RefreshEntry(t);
        }

        private void RefreshEntry(TowerType t)
        {
            int idx = (int)t;
            if (_levelMgr == null) return;

            int  level    = _levelMgr.GetLevel(t);
            int  cost     = _levelMgr.GetUpgradeCost(t);
            bool isMaxLv  = _levelMgr.IsMaxLevel(t);
            int  stars    = RD_GameManager.instance?.economyMgr?.starCurrency ?? 0;
            bool canAfford = !isMaxLv && cost >= 0 && stars >= cost;

            if (levelTexts != null && idx < levelTexts.Length && levelTexts[idx] != null)
                levelTexts[idx].text = $"Lv.{level}";

            if (costTexts != null && idx < costTexts.Length && costTexts[idx] != null)
                costTexts[idx].text = $"{cost}";
        }
    }
}
