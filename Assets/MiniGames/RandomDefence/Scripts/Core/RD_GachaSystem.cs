using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_GachaSystem : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private RD_GachaTableSO  gachaTable;
        [SerializeField] private RD_TowerDataSO[] towerDataset = new RD_TowerDataSO[4];
        [SerializeField] private GameObject[]     towerPrefabs = new GameObject[4];

        public System.Action<RD_TowerBase> onPulled;
        public System.Action<string>       onPullFailed;

        public bool TryPull()
        {
            var gm = RD_GameManager.instance;

            if (!gm.economyMgr.CanAfford(gachaTable.costPerPull))
            {
                onPullFailed?.Invoke("뽑기 비용이 부족해요");
                return false;
            }
            if (!gm.towerMgr.HasEmptySlot())
            {
                onPullFailed?.Invoke("유닛이 가득차서 뽑을 수 없어요");
                return false;
            }

            gm.economyMgr.TrySpend(gachaTable.costPerPull);

            int       grade = RollGrade();
            TowerType type  = RollType();

            RD_TowerBase tower = CreateTower(type, grade);
            if (tower == null) return false;

            gm.towerMgr.PlaceTower(tower);
            onPulled?.Invoke(tower);
            return true;
        }

        private RD_TowerBase CreateTower(TowerType type, int grade)
        {
            int idx = (int)type;
            if (idx >= towerPrefabs.Length || towerPrefabs[idx] == null)
            {
                Debug.LogError($"[RD_GachaSystem] {type} 프리팹 미연결");
                return null;
            }
            if (idx >= towerDataset.Length || towerDataset[idx] == null)
            {
                Debug.LogError($"[RD_GachaSystem] {type} TowerDataSO 미연결");
                return null;
            }

            GameObject go    = Instantiate(towerPrefabs[idx]);
            var        tower = go.GetComponent<RD_TowerBase>();
            if (tower == null)
            {
                Debug.LogError($"[RD_GachaSystem] {towerPrefabs[idx].name} 에 RD_TowerBase 없음");
                Destroy(go);
                return null;
            }

            tower.Initialize(towerDataset[idx], grade);
            return tower;
        }

        private int RollGrade()
        {
            float total = 0f;
            foreach (var w in gachaTable.gradeWeights) total += w;
            float roll = Random.Range(0f, total);
            float cum  = 0f;
            for (int i = 0; i < gachaTable.gradeWeights.Length; i++)
            {
                cum += gachaTable.gradeWeights[i];
                if (roll <= cum) return i + 1;
            }
            return 1;
        }

        private TowerType RollType()
        {
            float total = 0f;
            foreach (var w in gachaTable.typeWeights) total += w;
            float roll = Random.Range(0f, total);
            float cum  = 0f;
            TowerType[] order = { TowerType.Worrior, TowerType.Mage, TowerType.Bowman };
            for (int i = 0; i < Mathf.Min(gachaTable.typeWeights.Length, order.Length); i++)
            {
                cum += gachaTable.typeWeights[i];
                if (roll <= cum) return order[i];
            }
            return TowerType.Worrior;
        }
    }
}
