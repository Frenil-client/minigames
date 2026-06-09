using UnityEngine;

namespace MiniGames.RandomDefence
{
    [RequireComponent(typeof(Animator))]
    public class RD_AnimEventReceiver : MonoBehaviour
    {
        private RD_TowerAttack _attack;

        private void Awake()
        {
            _attack = GetComponentInParent<RD_TowerAttack>();
        }

        public void OnAttackHit() => _attack?.OnAttackHitFrame();
    }
}
