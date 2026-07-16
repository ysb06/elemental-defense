using UnityEngine;
using UnityEngine.AI;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Navigation;

namespace DefCity.Presentation.Animation
{
    public class SyntyHumanAniManager : MonoBehaviour
    {
        private const string WeaponTypeParameter = "WeaponType_int";
        private const string ShootParameter = "Shoot_b";
        private const string SpeedParameter = "Speed_f";
        private const string DeathTypeParameter = "DeathType_int";
        private const string DeathParameter = "Death_b";

        [SerializeField] private Damageable damageable;
        [SerializeField] private Collider unitCollider;
        [SerializeField] private Animator animator;
        [SerializeField] private BaseCombatController combatController;
        [SerializeField] private AttackCapable attacker;
        [SerializeField] private Movable movable;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private int weaponType = 2;
        private bool isDead;
        public bool IsAlive => damageable.IsAlive;

        private void Start()
        {
            InitializeAnimator();
        }

        private void OnEnable()
        {
            if (damageable != null)
            {
                damageable.OnDeath.AddListener(OnDeath);
            }

            if (movable != null)
            {
                movable.OnMovingStateChanged.AddListener(OnMovingStateChanged);
            }

            if (combatController != null)
            {
                combatController.OnStateChanged.AddListener(OnCombatStateChanged);
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDeath.RemoveListener(OnDeath);
            }

            if (movable != null)
            {
                movable.OnMovingStateChanged.RemoveListener(OnMovingStateChanged);
            }

            if (combatController != null)
            {
                combatController.OnStateChanged.RemoveListener(OnCombatStateChanged);
            }
        }

        public void OnDeath(GameObject sender, DamageEventArgs args)
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            if (combatController != null)
            {
                combatController.enabled = false;
            }

            if (attacker != null)
            {
                attacker.enabled = false;
            }

            if (movable != null)
            {
                movable.StopMoving();
                movable.enabled = false;
            }

            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
            }

            if (unitCollider != null)
            {
                unitCollider.enabled = false;
            }

            PlayDeathAnimation();
        }

        private void InitializeAnimator()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(WeaponTypeParameter, weaponType);
            animator.SetBool(ShootParameter, false);
            animator.SetFloat(SpeedParameter, movable != null && movable.IsMoving ? 1f : 0f);
        }

        private void OnMovingStateChanged(GameObject sender, bool isMoving)
        {
            if (animator == null || isDead)
            {
                return;
            }

            animator.SetFloat(SpeedParameter, isMoving ? 1f : 0f);
        }

        private void OnCombatStateChanged(GameObject sender, CombatState state)
        {
            if (animator == null || isDead)
            {
                return;
            }

            animator.SetBool(ShootParameter, state == CombatState.Attacking);
        }

        private void PlayDeathAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(SpeedParameter, 0f);
            animator.SetBool(ShootParameter, false);
            animator.SetInteger(DeathTypeParameter, Random.Range(1, 3));
            animator.SetBool(DeathParameter, true);
        }
    }
}
