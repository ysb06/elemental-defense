using System;
using System.Collections.Generic;
using DefCore.Gameplay.World;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DefCore.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public class CellSpaceMouseEventManager : MonoBehaviour
    {
        private const string DefaultTerrainLayerName = "Terrain";
        private const float CellLookupInset = 0.001f;

        [SerializeField] private CellSpace targetCellSpace;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask terrainLayerMask;
        [SerializeField, Min(0f)] private float maxRayDistance = 10000f;

        public CellSpaceMouseHitEvent OnCellMouseOver = new();
        public CellSpaceMouseHitEvent OnCellMouseClick = new();

        private readonly List<RaycastResult> uiRaycastResults = new();
        private InputAction lookAction;
        private InputAction selectAction;
        private bool pendingMouseOver;
        private bool pendingSelect;

        private void Reset()
        {
            terrainLayerMask = LayerMask.GetMask(DefaultTerrainLayerName);
            maxRayDistance = 10000f;
        }

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnEnable()
        {
            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                throw new InvalidOperationException("Project-wide Input Actions are not configured.");
            }

            lookAction = actions.FindAction("Look", true);
            selectAction = actions.FindAction("Select", true);

            lookAction.performed += HandleLookPerformed;
            selectAction.performed += HandleSelectPerformed;
        }

        private void OnDisable()
        {
            if (lookAction != null)
            {
                lookAction.performed -= HandleLookPerformed;
                lookAction = null;
            }

            if (selectAction != null)
            {
                selectAction.performed -= HandleSelectPerformed;
                selectAction = null;
            }

            pendingMouseOver = false;
            pendingSelect = false;
        }

        private void Update()
        {
            if (pendingMouseOver)
            {
                pendingMouseOver = false;

                if (TryGetCellSpaceEventArgs(out CellSpaceMouseEventArgs mouseOverEventArgs))
                {
                    OnCellMouseOver.Invoke(gameObject, mouseOverEventArgs);
                }
            }

            if (!isActiveAndEnabled || !pendingSelect)
            {
                return;
            }

            pendingSelect = false;

            if (!TryGetCellSpaceEventArgs(out CellSpaceMouseEventArgs eventArgs))
            {
                return;
            }

            OnCellMouseClick.Invoke(gameObject, eventArgs);
        }

        private void HandleLookPerformed(InputAction.CallbackContext context)
        {
            pendingMouseOver = true;
        }

        private void HandleSelectPerformed(InputAction.CallbackContext context)
        {
            pendingSelect = true;
        }

        public bool TryGetCellSpaceEventArgs(out CellSpaceMouseEventArgs eventArgs)
        {
            eventArgs = default;

            if (!TryGetMouseScreenPosition(out Vector3 mouseScreenPosition) ||
                IsPointerOverUi(mouseScreenPosition))
            {
                return false;
            }

            Ray ray = targetCamera.ScreenPointToRay(mouseScreenPosition);
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maxRayDistance,
                    terrainLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            Vector3 cellLookupPoint = hit.point + ray.direction * CellLookupInset;
            if (!targetCellSpace.TryGetCell(cellLookupPoint, out CellRef cell))
            {
                return false;
            }

            eventArgs = new CellSpaceMouseEventArgs
            {
                Cell = cell,
                HitPoint = hit.point
            };
            return true;
        }

        public void DiscardPendingClick()
        {
            pendingSelect = false;
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
            if (targetCellSpace == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CellSpaceMouseEventManager)} requires a {nameof(CellSpace)} reference.");
            }

            if (targetCamera == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CellSpaceMouseEventManager)} requires a {nameof(Camera)} reference.");
            }

            if (terrainLayerMask.value == 0)
            {
                throw new InvalidOperationException("Terrain LayerMask must contain at least one layer.");
            }

            if (maxRayDistance <= 0f)
            {
                throw new InvalidOperationException("Maximum ray distance must be greater than zero.");
            }
        }
    }

    public struct CellSpaceMouseEventArgs
    {
        public CellRef Cell;
        public Vector3 HitPoint;
    }

    [Serializable]
    public class CellSpaceMouseHitEvent : UnityEvent<GameObject, CellSpaceMouseEventArgs> { }
}
