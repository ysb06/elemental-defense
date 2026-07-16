using System;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DefCity.Gameplay.Interaction
{
    public struct UnitSelectEventArgs
    {
        public Entity Entity;
        public Damageable Damageable;
        public Vector3 HitPoint;
    }

    public class UnitSelectEventManager : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private float maxRayDistance = 10000f;

        public Camera TargetCamera => targetCamera;

        public UnitSelectEvent OnUnitSelected = new();
        public UnitSelectMissEvent OnUnitSelectMiss = new();

        private InputAction selectAction;
        private bool pendingSelect;

        private void Reset()
        {
            targetLayerMask = LayerMask.GetMask("Game Entity");
            maxRayDistance = 10000f;
        }

        private void OnEnable()
        {
            selectAction = InputSystem.actions.FindAction("Select", true);
            selectAction.performed += OnSelectPerformed;
        }

        private void OnDisable()
        {
            if (selectAction != null)
            {
                selectAction.performed -= OnSelectPerformed;
                selectAction = null;
            }

            pendingSelect = false;
        }

        private void Update()
        {
            if (!pendingSelect)
            {
                return;
            }

            pendingSelect = false;

            if (IsPointerOverUi() || !TryGetMouseScreenPosition(out Vector3 mouseScreenPosition))
            {
                return;
            }

            if (TryGetUnitSelectEventArgs(mouseScreenPosition, out UnitSelectEventArgs eventArgs))
            {
                OnUnitSelected.Invoke(gameObject, eventArgs);
                return;
            }

            OnUnitSelectMiss.Invoke(gameObject);
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            pendingSelect = true;
        }

        private bool TryGetUnitSelectEventArgs(
            Vector3 mouseScreenPosition,
            out UnitSelectEventArgs eventArgs)
        {
            eventArgs = default;

            Ray ray = targetCamera.ScreenPointToRay(mouseScreenPosition);
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maxRayDistance,
                    targetLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            Entity entity = hit.collider.GetComponentInParent<Entity>();
            if (entity == null ||
                !entity.TryGetComponent(out Damageable damageable) ||
                !damageable.IsAlive)
            {
                return false;
            }

            eventArgs = new UnitSelectEventArgs
            {
                Entity = entity,
                Damageable = damageable,
                HitPoint = hit.point
            };
            return true;
        }

        private static bool TryGetMouseScreenPosition(out Vector3 mouseScreenPosition)
        {
            if (Mouse.current != null)
            {
                mouseScreenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            mouseScreenPosition = default;
            return false;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }

    [Serializable]
    public class UnitSelectEvent : UnityEvent<GameObject, UnitSelectEventArgs> { }

    [Serializable]
    public class UnitSelectMissEvent : UnityEvent<GameObject> { }
}
