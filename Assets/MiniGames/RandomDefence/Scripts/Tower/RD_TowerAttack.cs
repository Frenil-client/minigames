using System.Collections;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TowerAttack : MonoBehaviour
    {
        private RD_TowerBase   _tower;
        private Coroutine      _attackLoop;
        private Animator       _animator;
        private RD_MonsterBase _currentTarget;

        private static readonly int HashAttack = Animator.StringToHash("ATTACK");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public void OnStatsUpdated(RD_TowerBase tower)
        {
            _tower = tower;

            if (_attackLoop != null) StopCoroutine(_attackLoop);
            if (_tower.data.attackType != AttackType.None)
                _attackLoop = StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                float interval = _tower.attackSpeed > 0f ? 1f / _tower.attackSpeed : 1f;
                yield return new WaitForSeconds(interval);

                _currentTarget = FindNearestTarget();
                if (_currentTarget == null) continue;

                if (_animator != null)
                    SetFacingX(_animator.transform, _currentTarget.transform.position.x > transform.position.x);

                if (_animator != null)
                {
                    _animator.SetTrigger(HashAttack);
                }
                else
                {
                    PerformAttack(_currentTarget);
                    _currentTarget = null;
                }
            }
        }

        public void OnAttackHitFrame()
        {
            if (_currentTarget != null && !_currentTarget.isDead)
                PerformAttack(_currentTarget);
            _currentTarget = null;
        }

        private void PerformAttack(RD_MonsterBase primary)
        {
            bool  armorBreak = _tower.data.ccType == CCType.ArmorBreak && RollCC();
            float damage     = _tower.attackDamage;

            primary.TakeDamage(damage, armorBreak);

            if (_tower.data.ccType is CCType.Stun or CCType.Slow && RollCC())
                primary.ApplyCC(_tower.data.ccType, _tower.data.ccDuration);

            if (_tower.data.attackType == AttackType.AoE)
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(
                    primary.transform.position, _tower.data.aoeRadius);

                foreach (var col in cols)
                {
                    if (!col.TryGetComponent<RD_MonsterBase>(out var splash)) continue;
                    if (splash == primary || splash.isDead) continue;
                    splash.TakeDamage(damage);
                    if (_tower.data.ccType == CCType.Slow && RollCC())
                        splash.ApplyCC(CCType.Slow, _tower.data.ccDuration);
                }
            }
        }

        private RD_MonsterBase FindNearestTarget()
        {
            Collider2D[]   hits    = Physics2D.OverlapCircleAll(transform.position, _tower.attackRange);
            RD_MonsterBase nearest = null;
            float          minDist = float.MaxValue;

            foreach (var col in hits)
            {
                if (!col.TryGetComponent<RD_MonsterBase>(out var m) || m.isDead) continue;
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist) { minDist = dist; nearest = m; }
            }
            return nearest;
        }

        private bool RollCC() => Random.value < _tower.ccProbability;

        private static void SetFacingX(Transform visualRoot, bool facingRight)
        {
            Vector3 s = visualRoot.localScale;
            s.x = facingRight ? -1f : 1f;
            visualRoot.localScale = s;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_tower == null) return;
            Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, _tower.attackRange);
        }
#endif
    }
}
