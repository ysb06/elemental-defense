using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ElementalDef
{
    [DisallowMultipleComponent]
    public sealed class DebugCameraController : MonoBehaviour
    {
        private const float MaxContinuousInputDeltaTime = 0.05f;

        [Header("References")]
        [SerializeField] private CinemachineFollow cinemachineFollow;
        [SerializeField] private Transform movementPlane;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float dragPanSpeed = 0.03f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 4f;
        [SerializeField] private float minZoomDistance = 6f;
        [SerializeField] private float maxZoomDistance = 40f;

        [Header("Edge Scroll")]
        [SerializeField] private bool enableEdgeScroll = true;
        [SerializeField] private float edgeScrollMargin = 24f;
        [SerializeField] private float edgeScrollSpeedMultiplier = 1f;

        private InputAction cameraMoveAction;
        private InputAction cameraZoomAction;
        private InputAction cameraDragAction;
        private InputAction cameraRotateAction;
        private InputAction cameraResetAction;
        private InputAction pointerDeltaAction;
        private InputAction pointerPositionAction;

        private Vector2 moveInput;
        private Vector2 pointerPosition;
        private float rotateInput;
        private bool hasPointerPosition;
        private Vector3 zoomDirection;
        private float zoomDistance;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float initialZoomDistance;
        private float pendingZoomInput;

        private void Awake()
        {
            ValidateConfiguration();

            Vector3 offset = cinemachineFollow.FollowOffset;
            zoomDirection = offset.sqrMagnitude > Mathf.Epsilon
                ? offset.normalized
                : new Vector3(0f, 1f, -1f).normalized;
            zoomDistance = Mathf.Clamp(offset.magnitude, minZoomDistance, maxZoomDistance);
            cinemachineFollow.FollowOffset = zoomDirection * zoomDistance;

            ProjectTargetOntoMovementPlane();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialZoomDistance = zoomDistance;
        }

        private void OnEnable()
        {
            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                throw new InvalidOperationException("Project-wide Input Actions are not configured.");
            }

            cameraMoveAction = actions.FindAction("CameraMove", true);
            cameraZoomAction = actions.FindAction("CameraZoom", true);
            cameraDragAction = actions.FindAction("CameraDrag", true);
            cameraRotateAction = actions.FindAction("CameraRotate", true);
            cameraResetAction = actions.FindAction("CameraReset", true);
            pointerDeltaAction = actions.FindAction("PointerDelta", true);
            pointerPositionAction = actions.FindAction("PointerPosition", true);

            cameraMoveAction.performed += OnCameraMoveChanged;
            cameraMoveAction.canceled += OnCameraMoveChanged;
            cameraZoomAction.performed += OnCameraZoomPerformed;
            cameraRotateAction.performed += OnCameraRotateChanged;
            cameraRotateAction.canceled += OnCameraRotateChanged;
            cameraResetAction.performed += OnCameraResetPerformed;
            pointerPositionAction.performed += OnPointerPositionChanged;
        }

        private void OnDisable()
        {
            if (cameraMoveAction != null)
            {
                cameraMoveAction.performed -= OnCameraMoveChanged;
                cameraMoveAction.canceled -= OnCameraMoveChanged;
            }

            if (cameraZoomAction != null)
            {
                cameraZoomAction.performed -= OnCameraZoomPerformed;
            }

            if (cameraRotateAction != null)
            {
                cameraRotateAction.performed -= OnCameraRotateChanged;
                cameraRotateAction.canceled -= OnCameraRotateChanged;
            }

            if (cameraResetAction != null)
            {
                cameraResetAction.performed -= OnCameraResetPerformed;
            }

            if (pointerPositionAction != null)
            {
                pointerPositionAction.performed -= OnPointerPositionChanged;
            }

            moveInput = Vector2.zero;
            rotateInput = 0f;
            hasPointerPosition = false;
            pendingZoomInput = 0f;
        }

        private void Update()
        {
            float deltaTime = Mathf.Min(Time.unscaledDeltaTime, MaxContinuousInputDeltaTime);

            MoveByKeyboard(deltaTime);
            MoveByEdgeScroll(deltaTime);
            Rotate(deltaTime);
            DragPan();
            ApplyPendingZoom();
            ProjectTargetOntoMovementPlane();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                hasPointerPosition = false;
            }
        }

        private void ValidateConfiguration()
        {
            if (cinemachineFollow == null)
            {
                throw new MissingReferenceException($"{nameof(DebugCameraController)} requires a {nameof(CinemachineFollow)} reference.");
            }

            if (movementPlane == null)
            {
                throw new MissingReferenceException($"{nameof(DebugCameraController)} requires a movement plane Transform.");
            }

            if (minZoomDistance <= 0f || maxZoomDistance < minZoomDistance)
            {
                throw new InvalidOperationException("Zoom distance limits must satisfy 0 < minZoomDistance <= maxZoomDistance.");
            }
        }

        private void OnCameraMoveChanged(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnCameraRotateChanged(InputAction.CallbackContext context)
        {
            rotateInput = context.ReadValue<float>();
        }

        private void OnCameraZoomPerformed(InputAction.CallbackContext context)
        {
            pendingZoomInput += context.ReadValue<Vector2>().y;
        }

        private void OnPointerPositionChanged(InputAction.CallbackContext context)
        {
            Vector2 currentPointerPosition = context.ReadValue<Vector2>();
            if (!hasPointerPosition && currentPointerPosition == Vector2.zero)
            {
                return;
            }

            pointerPosition = currentPointerPosition;
            hasPointerPosition = true;
        }

        private void OnCameraResetPerformed(InputAction.CallbackContext context)
        {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            zoomDistance = initialZoomDistance;
            pendingZoomInput = 0f;
            cinemachineFollow.FollowOffset = zoomDirection * zoomDistance;
            ProjectTargetOntoMovementPlane();
        }

        private void MoveByKeyboard(float deltaTime)
        {
            if (moveInput.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 movement = GetPlanarMovement(moveInput, true);
            transform.position += movement * (GetScaledMoveSpeed() * deltaTime);
        }

        private void MoveByEdgeScroll(float deltaTime)
        {
            if (!enableEdgeScroll ||
                !hasPointerPosition ||
                !Application.isFocused ||
                IsPointerOverUi())
            {
                return;
            }

            if (!IsPointerInScreen(pointerPosition))
            {
                return;
            }

            Vector2 edgeInput = GetEdgeScrollInput(pointerPosition);
            if (edgeInput.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 movement = GetPlanarMovement(edgeInput, true);
            transform.position += movement * (GetScaledMoveSpeed() * edgeScrollSpeedMultiplier * deltaTime);
        }

        private void Rotate(float deltaTime)
        {
            if (Mathf.Approximately(rotateInput, 0f))
            {
                return;
            }

            transform.Rotate(GetPlaneNormal(), rotateInput * rotateSpeed * deltaTime, Space.World);
        }

        private void DragPan()
        {
            if (!cameraDragAction.IsPressed() || IsPointerOverUi())
            {
                return;
            }

            Vector2 pointerDelta = pointerDeltaAction.ReadValue<Vector2>();
            if (pointerDelta.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 movement = GetPlanarMovement(-pointerDelta, false);
            transform.position += movement * (dragPanSpeed * GetZoomScale());
        }

        private void ApplyPendingZoom()
        {
            float scrollY = pendingZoomInput;
            pendingZoomInput = 0f;

            if (Mathf.Approximately(scrollY, 0f) || IsPointerOverUi())
            {
                return;
            }

            zoomDistance = Mathf.Clamp(
                zoomDistance - scrollY * zoomSpeed * 0.01f,
                minZoomDistance,
                maxZoomDistance);
            cinemachineFollow.FollowOffset = zoomDirection * zoomDistance;
        }

        private Vector3 GetPlanarMovement(Vector2 input, bool clampInputMagnitude)
        {
            Vector2 planarInput = clampInputMagnitude ? Vector2.ClampMagnitude(input, 1f) : input;
            GetMovementBasis(out Vector3 forward, out Vector3 right);
            return right * planarInput.x + forward * planarInput.y;
        }

        private void GetMovementBasis(out Vector3 forward, out Vector3 right)
        {
            Vector3 normal = GetPlaneNormal();
            forward = Vector3.ProjectOnPlane(transform.forward, normal);

            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                forward = Vector3.ProjectOnPlane(movementPlane.forward, normal);
            }

            if (forward.sqrMagnitude <= Mathf.Epsilon)
            {
                throw new InvalidOperationException("The movement plane does not define a valid forward direction.");
            }

            forward.Normalize();
            right = Vector3.Cross(normal, forward).normalized;
        }

        private void ProjectTargetOntoMovementPlane()
        {
            Vector3 normal = GetPlaneNormal();
            Vector3 fromPlaneOrigin = transform.position - movementPlane.position;
            transform.position -= normal * Vector3.Dot(fromPlaneOrigin, normal);
        }

        private Vector3 GetPlaneNormal()
        {
            return movementPlane.up.normalized;
        }

        private float GetScaledMoveSpeed()
        {
            return moveSpeed * GetZoomScale();
        }

        private float GetZoomScale()
        {
            return zoomDistance / Mathf.Max(Mathf.Epsilon, initialZoomDistance);
        }

        private Vector2 GetEdgeScrollInput(Vector2 pointerPosition)
        {
            Vector2 input = Vector2.zero;

            if (pointerPosition.x <= edgeScrollMargin)
            {
                input.x = -1f;
            }
            else if (pointerPosition.x >= Screen.width - edgeScrollMargin)
            {
                input.x = 1f;
            }

            if (pointerPosition.y <= edgeScrollMargin)
            {
                input.y = -1f;
            }
            else if (pointerPosition.y >= Screen.height - edgeScrollMargin)
            {
                input.y = 1f;
            }

            return input;
        }

        private static bool IsPointerInScreen(Vector2 pointerPosition)
        {
            return pointerPosition.x >= 0f
                   && pointerPosition.x <= Screen.width
                   && pointerPosition.y >= 0f
                   && pointerPosition.y <= Screen.height;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
