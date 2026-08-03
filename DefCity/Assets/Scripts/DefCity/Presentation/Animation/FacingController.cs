using UnityEngine;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Navigation;
using DefCore.Gameplay.Combat;

namespace DefCity.Presentation.Animation
{
    public class FacingController : MonoBehaviour
    {
        [SerializeField] private BaseCombatController combatController;
        [SerializeField] private Movable movable;
        [SerializeField] private Transform facingTarget;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;

        private void Update()
        {
            UpdateFacing(Time.deltaTime);
        }

        private void UpdateFacing(float deltaTime)
        {
            if (combatController == null)
            {
                Debug.LogError($"{name} requires a combatController to rotate.", this);
                return;
            }

            if (facingTarget == null)
            {
                Debug.LogError($"{name} requires a facingTarget to rotate.", this);
                return;
            }

            if (movable != null && movable.IsMoving)
            {
                return;
            }

            if (combatController.CurrentState != CombatState.Attacking)
            {
                return;
            }

            Health currentTarget = combatController.CurrentTarget;
            if (currentTarget == null || !currentTarget.IsAlive)
            {
                return;
            }

            Vector3 targetPosition = currentTarget.GetClosestPoint(facingTarget.position);
            Vector3 direction = targetPosition - facingTarget.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            float maxDegreesDelta = Mathf.Max(0f, rotationSpeed) * deltaTime;
            facingTarget.rotation = Quaternion.RotateTowards(facingTarget.rotation, targetRotation, maxDegreesDelta);
        }
    }
}
