using System.Collections;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_MonsterBase : MonoBehaviour
    {
        public RD_MonsterDataSO data      { get; private set; }
        public float            currentHp { get; private set; }
        public bool             isDead    { get; private set; }

        private float     _stunTimer;
        private float     _slowTimer;
        private float     _slowMultiplier = 1f;
        private Coroutine _hitFlash;

        public bool  isStunned      => _stunTimer > 0f;
        public float speedMultiplier => isStunned ? 0f : _slowMultiplier;

        private const string SortingLayerCharacters = "Characters";

        private RD_PathManager   _path;
        private int              _waypointIndex;
        private Animator         _animator;
        private SpriteRenderer[] _renderers;

        private MaterialPropertyBlock _mpb;
        private static readonly int FlashAmountId = Shader.PropertyToID("_HitFlashAmount");

        private static readonly int HashDeath = Animator.StringToHash("DEATH");

        private void Awake()
        {
            _animator  = GetComponentInChildren<Animator>();
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _mpb       = new MaterialPropertyBlock();

            foreach (var sr in _renderers)
                sr.sortingLayerName = SortingLayerCharacters;
        }

        private void Update()
        {
            if (isDead) return;
            UpdateCC();
            MoveAlongPath();
        }

        public void Initialize(RD_MonsterDataSO d, RD_PathManager path, float hpBonus = 0f)
        {
            data            = d;
            currentHp       = d.hp + hpBonus;
            isDead          = false;
            _path           = path;
            _waypointIndex  = 0;
            _stunTimer      = 0f;
            _slowTimer      = 0f;
            _slowMultiplier = 1f;

            if (path != null) transform.position = path.spawnPosition;
        }

        public void TakeDamage(float damage, bool armorBreak = false)
        {
            if (isDead) return;
            float def    = armorBreak ? 0f : data.defense;
            float actual = Mathf.Max(damage - def, 1f);
            currentHp -= actual;

            RD_DamageTextSpawner.Show(actual, transform.position + Vector3.up * 0.3f, armorBreak);

            if (_hitFlash != null) StopCoroutine(_hitFlash);
            _hitFlash = StartCoroutine(HitFlashRoutine());

            if (currentHp <= 0f) Die();
        }

        public void ApplyCC(CCType ccType, float duration)
        {
            switch (ccType)
            {
                case CCType.Stun:
                    _stunTimer = Mathf.Max(_stunTimer, duration);
                    break;
                case CCType.Slow:
                    _slowTimer      = Mathf.Max(_slowTimer, duration);
                    _slowMultiplier = 0.5f;
                    break;
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            SetFlashAmount(1f);
            yield return new WaitForSeconds(0.1f);
            SetFlashAmount(0f);
            _hitFlash = null;
        }

        private void SetFlashAmount(float amount)
        {
            foreach (var sr in _renderers)
            {
                if (sr == null) continue;
                sr.GetPropertyBlock(_mpb);
                _mpb.SetFloat(FlashAmountId, amount);
                sr.SetPropertyBlock(_mpb);
            }
        }

        private void UpdateCC()
        {
            if (_stunTimer > 0f) _stunTimer -= Time.deltaTime;
            if (_slowTimer  > 0f)
            {
                _slowTimer -= Time.deltaTime;
                if (_slowTimer <= 0f) _slowMultiplier = 1f;
            }
        }

        private void MoveAlongPath()
        {
            if (isStunned || _path == null) return;
            if (_waypointIndex >= _path.waypointCount) { OnCompletedLoop(); return; }

            Vector3 target = _path.GetWaypointPosition(_waypointIndex);
            Vector3 dir    = target - transform.position;

            transform.position = Vector3.MoveTowards(
                transform.position, target, data.moveSpeed * speedMultiplier * Time.deltaTime);

            if (_animator != null && Mathf.Abs(dir.x) > 0.01f)
                SetFacingX(_animator.transform, dir.x > 0f);

            if (Vector3.Distance(transform.position, target) < 0.05f)
                _waypointIndex++;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            if (_hitFlash != null) { StopCoroutine(_hitFlash); _hitFlash = null; }
            SetFlashAmount(0f);

            RD_GameManager.instance?.economyMgr?.AddCurrency(data.rewardCurrency);
            RD_GameManager.instance?.scoreMgr?.AddScore(data.scoreValue);
            RD_GameManager.instance?.waveMgr?.OnMonsterDied(this);

            if (data.rewardCurrency > 0)
                RD_CurrencyEffectSpawner.Show(data.rewardCurrency, transform.position + Vector3.up * 0.3f);

            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            if (_animator == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[RD_MonsterBase] {name}: Animator 없음 → 즉시 파괴");
#endif
                Destroy(gameObject);
                yield break;
            }

            _animator.SetTrigger(HashDeath);

            float wait = 0f;
            while (wait < 2f)
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).IsName("DEATH"))
                    break;
                wait += Time.deltaTime;
                yield return null;
            }

            float elapsed = 0f;
            while (elapsed < 10f)
            {
                var info = _animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("DEATH") && !_animator.IsInTransition(0) && info.normalizedTime >= 1f)
                    break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnCompletedLoop()
        {
            _waypointIndex = 0;
        }

        private static void SetFacingX(Transform visualRoot, bool facingRight)
        {
            Vector3 s = visualRoot.localScale;
            s.x = facingRight ? -1f : 1f;
            visualRoot.localScale = s;
        }
    }
}
