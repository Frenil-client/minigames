using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGames.RandomDefence
{
    [RequireComponent(typeof(RD_TowerAttack))]
    public class RD_TowerBase : MonoBehaviour
    {
        private const string SortingLayerCharacters = "Characters";

        public RD_TowerDataSO data  { get; private set; }
        public TowerType      type  => data.type;
        public int            grade { get; private set; }

        public float attackDamage
        {
            get
            {
                float baseDmg   = data.GetAttackDamage(grade);
                float multiplier = RD_GameManager.instance?.levelMgr?.GetDamageMultiplier(type) ?? 1f;
                return baseDmg * multiplier;
            }
        }
        public float attackSpeed   => data.GetAttackSpeed(grade);
        public float attackRange   => data.attackRange;
        public float ccProbability => data.GetCCProbability(grade);
        public int   sellPrice     => data.GetSellPrice(grade);

        [Header("컴포넌트 참조")]
        [Tooltip("학년 별 표시 컴포넌트. 비워두면 별을 표시하지 않습니다.")]
        [SerializeField] private RD_TowerGradeStars gradeStars;

        private SortingGroup   _sortingGroup;
        private RD_TowerAttack _attack;

        private void Awake()
        {
            _sortingGroup = GetComponent<SortingGroup>();
            _attack       = GetComponent<RD_TowerAttack>();

            if (_sortingGroup != null)
                _sortingGroup.sortingLayerName = SortingLayerCharacters;
        }

        public void Initialize(RD_TowerDataSO towerData, int startGrade = 1)
        {
            data  = towerData;
            grade = Mathf.Max(startGrade, 1);
            ApplyStats();
        }

        public bool TryGradeUp()
        {
            grade++;
            ApplyStats();
            return true;
        }

        private void ApplyStats()
        {
            gradeStars?.SetGrade(grade);
            _attack?.OnStatsUpdated(this);
            RD_GameManager.instance?.scoreMgr?.ReportTowerGrade(grade);
        }
    }
}
