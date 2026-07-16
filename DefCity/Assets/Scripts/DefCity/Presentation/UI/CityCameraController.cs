using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DefCity.Presentation.UI
{
    public class CityCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineFollow cinemachineFollow;
        [SerializeField] private Terrain targetTerrain;

        [SerializeField] private float moveSpeed = 40f;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float dragPanSpeed = 0.08f;
        [SerializeField] private float minZoomDistance = 18f;
        [SerializeField] private float maxZoomDistance = 120f;
        [SerializeField] private bool clampToTerrainBounds = true;
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Quaternion initialRotation;
        [SerializeField] private bool enableEdgeScroll = true;
        [SerializeField] private float edgeScrollMargin = 24f;
        [SerializeField] private float edgeScrollSpeedMultiplier = 1f;

        private InputAction cameraMoveAction;
        private InputAction cameraZoomAction;
        private InputAction cameraDragAction;
        private InputAction cameraRotateAction;
        private InputAction pointerDeltaAction;
        private InputAction cameraResetAction;
        private InputAction pointerPositionAction;


        private Vector2 moveInput;
        private float rotateInput;
        private Vector3 zoomDirection;
        private float zoomDistance;
        private float initialZoomDistance;
        private float pendingZoomInput;

        private void Awake()
        {
            Vector3 offset = cinemachineFollow.FollowOffset;
            zoomDistance = Mathf.Clamp(offset.magnitude, minZoomDistance, maxZoomDistance);
            initialZoomDistance = zoomDistance;
            zoomDirection = offset.sqrMagnitude > 0f ? offset.normalized : new Vector3(0f, 1f, -1f).normalized;

            cinemachineFollow.FollowOffset = zoomDirection * zoomDistance;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void OnEnable()
        {
            cameraMoveAction = InputSystem.actions.FindAction("CameraMove");
            cameraZoomAction = InputSystem.actions.FindAction("CameraZoom");
            cameraDragAction = InputSystem.actions.FindAction("CameraDrag");
            cameraRotateAction = InputSystem.actions.FindAction("CameraRotate");
            cameraResetAction = InputSystem.actions.FindAction("CameraReset");
            pointerDeltaAction = InputSystem.actions.FindAction("PointerDelta");
            pointerPositionAction = InputSystem.actions.FindAction("PointerPosition");

            cameraMoveAction.performed += OnCameraMoveChanged;
            cameraMoveAction.canceled += OnCameraMoveChanged;
            cameraZoomAction.performed += OnCameraZoomPerformed;
            cameraRotateAction.performed += OnCameraRotateChanged;
            cameraRotateAction.canceled += OnCameraRotateChanged;
            cameraResetAction.performed += OnCameraResetPerformed;

        }

        private void OnDisable()
        {
            cameraMoveAction.performed -= OnCameraMoveChanged;
            cameraMoveAction.canceled -= OnCameraMoveChanged;
            cameraZoomAction.performed -= OnCameraZoomPerformed;
            cameraRotateAction.performed -= OnCameraRotateChanged;
            cameraRotateAction.canceled -= OnCameraRotateChanged;
            cameraResetAction.performed -= OnCameraResetPerformed;

        }

        private void Update()
        {
            float deltaTime = UnityEngine.Time.deltaTime;

            MoveByKeyboard(deltaTime);
            MoveByEdgeScroll(deltaTime);
            Rotate(deltaTime);
            DragPan();
            ApplyPendingZoom();
            ClampToTerrain();
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

        private void OnCameraResetPerformed(InputAction.CallbackContext context)
        {
            // transform.SetPositionAndRotation(initialPosition, initialRotation);
            transform.rotation = initialRotation;
            ClampToTerrain();
        }


        private void MoveByKeyboard(float deltaTime)
        {
            if (moveInput.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.position += deltaTime * GetScaledMoveSpeed() * GetPlanarMovement(moveInput);
        }

        private void Rotate(float deltaTime)
        {
            if (Mathf.Approximately(rotateInput, 0f))
            {
                return;
            }

            transform.Rotate(Vector3.up, rotateInput * rotateSpeed * deltaTime, Space.World);
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

            Vector3 movement = GetPlanarMovement(-pointerDelta);
            transform.position += dragPanSpeed * GetZoomScale() * movement;
        }

        private void ApplyPendingZoom()
        {
            if (Mathf.Approximately(pendingZoomInput, 0f))
            {
                return;
            }

            float scrollY = pendingZoomInput;
            pendingZoomInput = 0f;

            if (IsPointerOverUi())
            {
                return;
            }

            zoomDistance = Mathf.Clamp(zoomDistance - scrollY * zoomSpeed * 0.01f, minZoomDistance, maxZoomDistance);
            cinemachineFollow.FollowOffset = zoomDirection * zoomDistance;
        }

        private Vector3 GetPlanarMovement(Vector2 input)
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 movement = right * input.x + forward * input.y;
            return movement.sqrMagnitude > 1f ? movement.normalized : movement;
        }

        private float GetScaledMoveSpeed()
        {
            return moveSpeed * GetZoomScale();
        }

        private float GetZoomScale()
        {
            return zoomDistance / Mathf.Max(1f, initialZoomDistance);
        }

        private void ClampToTerrain()
        {
            if (!clampToTerrainBounds || targetTerrain == null)
            {
                return;
            }

            Vector3 position = transform.position;
            Vector3 terrainPosition = targetTerrain.transform.position;
            Vector3 terrainSize = targetTerrain.terrainData.size;

            position.x = Mathf.Clamp(position.x, terrainPosition.x, terrainPosition.x + terrainSize.x);
            position.z = Mathf.Clamp(position.z, terrainPosition.z, terrainPosition.z + terrainSize.z);
            position.y = targetTerrain.SampleHeight(position) + terrainPosition.y;

            transform.position = position;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void MoveByEdgeScroll(float deltaTime)
        {
            if (!enableEdgeScroll || IsPointerOverUi())
            {
                return;
            }

            Vector2 pointerPosition = pointerPositionAction.ReadValue<Vector2>();
            if (!IsPointerInScreen(pointerPosition))
            {
                return;
            }

            Vector2 edgeInput = GetEdgeScrollInput(pointerPosition);
            if (edgeInput.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.position += deltaTime * GetScaledMoveSpeed() * edgeScrollSpeedMultiplier * GetPlanarMovement(edgeInput);
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

    }
}
