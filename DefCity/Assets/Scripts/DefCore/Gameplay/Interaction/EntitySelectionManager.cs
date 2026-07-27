using System;
using System.Collections.Generic;
using DefCore.Gameplay.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DefCore.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class EntitySelectionManager : MonoBehaviour
    {
        private const string DefaultEntityLayerName = "Game Entity";

        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField, Min(0f)] private float maxRayDistance = 10000f;

        public Entity CurrentEntity { get; private set; }
        public bool IsPointerSelectionEnabled { get; private set; } = true;

        public EntitySelectionChangedEvent OnEntitySelectionChanged = new();

        private readonly List<RaycastResult> uiRaycastResults = new();
        private InputAction selectAction;
        private bool pendingSelect;
        private bool isShutdown;
        private bool isChangingSelection;
        private bool hasQueuedSelection;
        private Entity queuedSelection;

        private void Reset()
        {
            targetLayerMask = LayerMask.GetMask(DefaultEntityLayerName);
            maxRayDistance = 10000f;
        }

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnEnable()
        {
            if (isShutdown)
            {
                enabled = false;
                return;
            }

            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                throw new InvalidOperationException("Project-wide Input Actions are not configured.");
            }

            selectAction = actions.FindAction("Select", true);
            selectAction.performed += HandleSelectPerformed;
        }

        private void OnDisable()
        {
            UnsubscribeFromInput();
            pendingSelect = false;
            ClearSelection();
        }

        private void Update()
        {
            if (!ReferenceEquals(CurrentEntity, null) &&
                (CurrentEntity == null || !CurrentEntity.IsOperational))
            {
                ChangeSelection(null);
            }

            if (!IsPointerSelectionEnabled || !pendingSelect)
            {
                return;
            }

            pendingSelect = false;

            if (!TryGetMouseScreenPosition(out Vector3 mouseScreenPosition) ||
                IsPointerOverUi(mouseScreenPosition))
            {
                return;
            }

            ChangeSelection(TryGetEntity(mouseScreenPosition, out Entity entity) ? entity : null);
        }

        public void SetPointerSelectionEnabled(bool enabled)
        {
            if (isShutdown)
            {
                return;
            }

            IsPointerSelectionEnabled = enabled;
            if (!enabled)
            {
                pendingSelect = false;
            }
        }

        public void ClearSelection()
        {
            ChangeSelection(null);
        }

        public void Shutdown()
        {
            if (isShutdown)
            {
                return;
            }

            isShutdown = true;
            IsPointerSelectionEnabled = false;
            pendingSelect = false;

            try
            {
                ClearSelection();
            }
            finally
            {
                UnsubscribeFromInput();
                enabled = false;
            }
        }

        private void HandleSelectPerformed(InputAction.CallbackContext context)
        {
            if (!isShutdown && IsPointerSelectionEnabled)
            {
                pendingSelect = true;
            }
        }

        private bool TryGetEntity(Vector3 mouseScreenPosition, out Entity entity)
        {
            entity = null;

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

            Entity hitEntity = hit.collider.GetComponentInParent<Entity>();
            if (hitEntity == null || !hitEntity.IsOperational)
            {
                return false;
            }

            entity = hitEntity;
            return true;
        }

        private void ChangeSelection(Entity nextEntity)
        {
            if (isChangingSelection)
            {
                queuedSelection = nextEntity;
                hasQueuedSelection = true;
                return;
            }

            isChangingSelection = true;
            try
            {
                Entity requestedEntity = nextEntity;
                while (true)
                {
                    hasQueuedSelection = false;
                    ApplySelection(requestedEntity);

                    if (!hasQueuedSelection)
                    {
                        break;
                    }

                    requestedEntity = queuedSelection;
                    queuedSelection = null;
                }
            }
            finally
            {
                queuedSelection = null;
                hasQueuedSelection = false;
                isChangingSelection = false;
            }
        }

        private void ApplySelection(Entity nextEntity)
        {
            if (ReferenceEquals(CurrentEntity, nextEntity))
            {
                return;
            }

            Entity previousEntity = CurrentEntity;
            if (previousEntity != null)
            {
                previousEntity.OnStateChanged.RemoveListener(HandleSelectedEntityStateChanged);
            }

            CurrentEntity = nextEntity;
            if (CurrentEntity != null)
            {
                CurrentEntity.OnStateChanged.AddListener(HandleSelectedEntityStateChanged);
            }

            try
            {
                OnEntitySelectionChanged?.Invoke(
                    gameObject,
                    new EntitySelectionChangedEventArgs(previousEntity, CurrentEntity));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void HandleSelectedEntityStateChanged(
            GameObject sender,
            EntityStateChangedEventArgs eventArgs)
        {
            if (CurrentEntity == null ||
                sender != CurrentEntity.gameObject ||
                eventArgs.CurrentState != EntityState.Dead)
            {
                return;
            }

            ChangeSelection(null);
        }

        private void UnsubscribeFromInput()
        {
            if (selectAction == null)
            {
                return;
            }

            selectAction.performed -= HandleSelectPerformed;
            selectAction = null;
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

        private bool IsPointerOverUi(Vector2 mouseScreenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            PointerEventData pointerEventData = new(eventSystem)
            {
                position = mouseScreenPosition
            };

            uiRaycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, uiRaycastResults);
            return uiRaycastResults.Count > 0;
        }

        private void EnsureConfigured()
        {
            if (targetCamera == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntitySelectionManager)} requires a {nameof(Camera)} reference.");
            }

            if (targetLayerMask.value == 0)
            {
                throw new InvalidOperationException("Target LayerMask must contain at least one layer.");
            }

            if (maxRayDistance <= 0f)
            {
                throw new InvalidOperationException("Maximum ray distance must be greater than zero.");
            }
        }
    }

    [Serializable]
    public readonly struct EntitySelectionChangedEventArgs
    {
        public Entity PreviousEntity { get; }
        public Entity CurrentEntity { get; }

        public EntitySelectionChangedEventArgs(Entity previousEntity, Entity currentEntity)
        {
            PreviousEntity = previousEntity;
            CurrentEntity = currentEntity;
        }
    }

    [Serializable]
    public class EntitySelectionChangedEvent : UnityEvent<GameObject, EntitySelectionChangedEventArgs> { }
}
