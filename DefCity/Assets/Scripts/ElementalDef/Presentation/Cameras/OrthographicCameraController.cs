using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace ElementalDef.Presentation.Cameras
{
    [DisallowMultipleComponent]
    public sealed class OrthographicCameraController : MonoBehaviour
    {
        private const float MaxContinuousInputDeltaTime = 0.05f;

        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Tilemap groundTilemap;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 8f;
        [SerializeField, Min(0f)] private float panBoundsPadding = 1f;

        private readonly List<RaycastResult> uiRaycastResults = new();

        private InputAction cameraMoveAction;
        private InputAction cameraDragAction;
        private InputAction cameraResetAction;
        private InputAction pointerPositionAction;
        private InputAction pointerDeltaAction;

        private Transform movementPlane;
        private Vector2 moveInput;
        private float minimumFocusX;
        private float maximumFocusX;
        private float minimumFocusZ;
        private float maximumFocusZ;
        private Vector3 resetPosition;
        private Quaternion resetRotation;
        private float resetOrthographicSize;
        private bool isInitialized;

        private void Awake()
        {
            EnsureConfigured();

            movementPlane = groundTilemap.layoutGrid.transform;
            BoundsInt cellBounds = groundTilemap.cellBounds;
            RectInt mapBounds = new(cellBounds.xMin, cellBounds.yMin, cellBounds.size.x, cellBounds.size.y);
            
            CalculateMapBounds(groundTilemap, mapBounds);

            CenterOnMap();
            ClampViewFocusToMapBounds();

            resetPosition = transform.position;
            resetRotation = transform.rotation;
            resetOrthographicSize = targetCamera.orthographicSize;
            isInitialized = true;
        }

        private void OnEnable()
        {
            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                throw new InvalidOperationException(
                    "Project-wide Input Actions are not configured.");
            }

            cameraMoveAction = actions.FindAction("CameraMove", true);
            cameraDragAction = actions.FindAction("CameraDrag", true);
            cameraResetAction = actions.FindAction("CameraReset", true);
            pointerPositionAction = actions.FindAction("PointerPosition", true);
            pointerDeltaAction = actions.FindAction("PointerDelta", true);

            cameraMoveAction.performed += HandleCameraMoveChanged;
            cameraMoveAction.canceled += HandleCameraMoveChanged;
            cameraResetAction.performed += HandleCameraResetPerformed;
        }

        private void OnDisable()
        {
            UnsubscribeFromInput();
            ClearTransientInput();
        }

        private void Update()
        {
            float deltaTime = Mathf.Min(
                Time.unscaledDeltaTime,
                MaxContinuousInputDeltaTime);

            MoveByKeyboard(deltaTime);
            DragPan();
            ClampViewFocusToMapBounds();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ClearTransientInput();
            }
        }

        public void ResetView()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(OrthographicCameraController)} has not been initialized.");
            }

            ClearTransientInput();
            transform.SetPositionAndRotation(resetPosition, resetRotation);
            targetCamera.orthographicSize = resetOrthographicSize;
            ClampViewFocusToMapBounds();
        }

        private void HandleCameraMoveChanged(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void HandleCameraResetPerformed(InputAction.CallbackContext context)
        {
            ResetView();
        }

        private void MoveByKeyboard(float deltaTime)
        {
            Vector2 clampedInput = Vector2.ClampMagnitude(moveInput, 1f);
            if (clampedInput.sqrMagnitude <= 0f)
            {
                return;
            }

            GetMovementBasis(out Vector3 forward, out Vector3 right);
            Vector3 movement = right * clampedInput.x + forward * clampedInput.y;
            float zoomScale = targetCamera.orthographicSize / resetOrthographicSize;
            transform.position += movement * (moveSpeed * zoomScale * deltaTime);
        }

        private void DragPan()
        {
            if (!cameraDragAction.IsPressed())
            {
                return;
            }

            Vector2 currentPointerPosition =
                pointerPositionAction.ReadValue<Vector2>();
            if (IsPointerOverUi(currentPointerPosition))
            {
                return;
            }

            Vector2 pointerDelta = pointerDeltaAction.ReadValue<Vector2>();
            if (pointerDelta.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector2 previousPointerPosition = currentPointerPosition - pointerDelta;
            if (!TryGetMovementPlanePoint(
                    targetCamera.ScreenPointToRay(previousPointerPosition),
                    out Vector3 previousWorldPoint) ||
                !TryGetMovementPlanePoint(
                    targetCamera.ScreenPointToRay(currentPointerPosition),
                    out Vector3 currentWorldPoint))
            {
                return;
            }

            transform.position += previousWorldPoint - currentWorldPoint;
        }

        private void GetMovementBasis(out Vector3 forward, out Vector3 right)
        {
            Vector3 planeNormal = GetMovementPlaneNormal();
            forward = Vector3.ProjectOnPlane(
                targetCamera.transform.forward,
                planeNormal);

            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                throw new InvalidOperationException(
                    "The camera does not define a valid forward direction on the map plane.");
            }

            forward.Normalize();
            right = Vector3.Cross(planeNormal, forward).normalized;
        }

        private void CalculateMapBounds(Tilemap groundTilemap, RectInt mapBounds)
        {
            Vector3Int bottomLeftCell = new(mapBounds.xMin, mapBounds.yMin, 0);
            Vector3Int bottomRightCell = new(mapBounds.xMax, mapBounds.yMin, 0);
            Vector3Int topRightCell = new(mapBounds.xMax, mapBounds.yMax, 0);
            Vector3Int topLeftCell = new(mapBounds.xMin, mapBounds.yMax, 0);

            Vector3[] worldCorners =
            {
                groundTilemap.CellToWorld(bottomLeftCell),
                groundTilemap.CellToWorld(bottomRightCell),
                groundTilemap.CellToWorld(topRightCell),
                groundTilemap.CellToWorld(topLeftCell)
            };

            minimumFocusX = float.PositiveInfinity;
            maximumFocusX = float.NegativeInfinity;
            minimumFocusZ = float.PositiveInfinity;
            maximumFocusZ = float.NegativeInfinity;

            foreach (Vector3 worldCorner in worldCorners)
            {
                Vector3 localCorner = movementPlane.InverseTransformPoint(worldCorner);
                minimumFocusX = Mathf.Min(minimumFocusX, localCorner.x);
                maximumFocusX = Mathf.Max(maximumFocusX, localCorner.x);
                minimumFocusZ = Mathf.Min(minimumFocusZ, localCorner.z);
                maximumFocusZ = Mathf.Max(maximumFocusZ, localCorner.z);
            }
        }

        private void CenterOnMap()
        {
            if (!TryGetViewFocusPoint(out Vector3 currentFocusPoint))
            {
                throw new InvalidOperationException(
                    "The camera center ray does not intersect the map plane.");
            }

            Vector3 localMapCenter = new(
                (minimumFocusX + maximumFocusX) * 0.5f,
                0f,
                (minimumFocusZ + maximumFocusZ) * 0.5f);
            Vector3 worldMapCenter = movementPlane.TransformPoint(localMapCenter);
            transform.position += worldMapCenter - currentFocusPoint;
        }

        private void ClampViewFocusToMapBounds()
        {
            if (movementPlane == null || !TryGetViewFocusPoint(out Vector3 worldFocusPoint))
            {
                return;
            }

            Vector3 localFocusPoint = movementPlane.InverseTransformPoint(worldFocusPoint);
            Vector3 clampedLocalFocusPoint = new(
                Mathf.Clamp(
                    localFocusPoint.x,
                    minimumFocusX - panBoundsPadding,
                    maximumFocusX + panBoundsPadding),
                0f,
                Mathf.Clamp(
                    localFocusPoint.z,
                    minimumFocusZ - panBoundsPadding,
                    maximumFocusZ + panBoundsPadding));
            Vector3 clampedWorldFocusPoint = movementPlane.TransformPoint(clampedLocalFocusPoint);

            transform.position += clampedWorldFocusPoint - worldFocusPoint;
        }

        private bool TryGetViewFocusPoint(out Vector3 worldFocusPoint)
        {
            Ray centerRay = targetCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));
            return TryGetMovementPlanePoint(centerRay, out worldFocusPoint);
        }

        private bool TryGetMovementPlanePoint(
            Ray ray,
            out Vector3 worldPoint)
        {
            Plane plane = new(GetMovementPlaneNormal(), movementPlane.position);
            if (!plane.Raycast(ray, out float enter))
            {
                worldPoint = default;
                return false;
            }

            worldPoint = ray.GetPoint(enter);
            return true;
        }

        private Vector3 GetMovementPlaneNormal()
        {
            return movementPlane.up.normalized;
        }

        private bool IsPointerOverUi(Vector2 pointerPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            PointerEventData pointerEventData = new(eventSystem)
            {
                position = pointerPosition
            };

            uiRaycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, uiRaycastResults);
            return uiRaycastResults.Count > 0;
        }

        private void UnsubscribeFromInput()
        {
            if (cameraMoveAction != null)
            {
                cameraMoveAction.performed -= HandleCameraMoveChanged;
                cameraMoveAction.canceled -= HandleCameraMoveChanged;
            }

            if (cameraResetAction != null)
            {
                cameraResetAction.performed -= HandleCameraResetPerformed;
            }

            cameraMoveAction = null;
            cameraDragAction = null;
            cameraResetAction = null;
            pointerPositionAction = null;
            pointerDeltaAction = null;
        }

        private void ClearTransientInput()
        {
            moveInput = Vector2.zero;
        }

        private void EnsureConfigured()
        {
            if (targetCamera == null)
            {
                throw new MissingReferenceException($"{nameof(OrthographicCameraController)} requires a {nameof(Camera)} reference.");
            }

            if (!targetCamera.orthographic)
            {
                throw new InvalidOperationException($"{nameof(OrthographicCameraController)} requires an orthographic Camera.");
            }

            if (targetCamera.transform != transform && !targetCamera.transform.IsChildOf(transform))
            {
                throw new InvalidOperationException("The target Camera must belong to this camera rig.");
            }

            if (!IsFiniteAndPositive(moveSpeed))
            {
                throw new InvalidOperationException("Camera move speed must be finite and greater than zero.");
            }

            if (!IsFiniteAndNonNegative(panBoundsPadding))
            {
                throw new InvalidOperationException(
                    "Camera pan bounds padding must be finite and non-negative.");
            }

            if (!IsFiniteAndPositive(targetCamera.orthographicSize))
            {
                throw new InvalidOperationException(
                    "The fixed orthographic size must be finite and greater than zero.");
            }
        }

        private static bool IsFiniteAndPositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFiniteAndNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
