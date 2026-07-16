using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DefCity.Gameplay.World;

namespace DefCity.Gameplay.Interaction
{
    public class TerrainMouseEventManager : MonoBehaviour
    {
        [SerializeField] private Terrain targetTerrain;
        [SerializeField] private TerrainCollider targetTerrainCollider;
        [SerializeField] private Grid targetGrid;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float maxRayDistance = 10000f;
        
        public TerrainCellMouseHitEvent OnTerrainCellMouseOver = new();
        public TerrainCellMouseHitEvent OnTerrainCellMouseClick = new();

        private bool pendingSelect;

        private void OnEnable()
        {
            InputSystem.actions.FindAction("Look").performed += OnLookPerformed;
            InputSystem.actions.FindAction("Select").performed += OnSelectPerformed;
        }

        private void OnDisable()
        {
            InputSystem.actions.FindAction("Look").performed -= OnLookPerformed;
            InputSystem.actions.FindAction("Select").performed -= OnSelectPerformed;
        }

        private void Update()
        {
            if (!pendingSelect)
            {
                return;
            }

            pendingSelect = false;

            if (IsPointerOverUi())
            {
                return;
            }

            if (!TryGetTerrainCellEventArgs(out TerrainCellEventArgs eventArgs))
            {
                return;
            }

            OnTerrainCellMouseClick.Invoke(gameObject, eventArgs);
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            // OnLookPerformed Context: 마우스가 조이스틱처럼 처리, 움직인 방향 제공
            // Todo: 추후 게임패드/조이스틱 대응 필요
            // Mouse.current.position: 실제 마우스 위치 제공
            if (!TryGetTerrainCellEventArgs(out TerrainCellEventArgs eventArgs))
            {
                return;
            }

            OnTerrainCellMouseOver.Invoke(gameObject, eventArgs);
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            pendingSelect = true;
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        public Vector3Int GetGridCellPosition(Vector3 worldPosition)
        {
            return targetGrid.WorldToCell(worldPosition);
        }

        private static bool TryGetMouseScreenPosition(out Vector3 mouseScreenPosition)
        {
            if (Mouse.current != null)
            {
                Vector2 position = Mouse.current.position.ReadValue();
                mouseScreenPosition = (Vector3)position;
                return true;
            }

            mouseScreenPosition = new Vector3(0f, 0f, 0f);
            return false;
        }

        public bool TryGetTerrainCellEventArgs(out TerrainCellEventArgs eventArgs)
        {
            eventArgs = default;

            if (!TryGetMouseScreenPosition(out Vector3 mouseScreenPosition))
            {
                return false;
            }

            Ray ray = targetCamera.ScreenPointToRay(mouseScreenPosition);
            if (!targetTerrainCollider.Raycast(ray, out RaycastHit hit, maxRayDistance))
            {
                return false;
            }

            eventArgs = new TerrainCellEventArgs
            {
                Cell = new TerrainCell(targetTerrain, targetGrid, GetGridCellPosition(hit.point)),
                HitPoint = hit.point
            };
            return true;
        }
    }

    public struct TerrainCellEventArgs
    {
        public TerrainCell Cell;
        public Vector3 HitPoint;
    }

    [Serializable]
    public class TerrainCellMouseHitEvent : UnityEvent<GameObject, TerrainCellEventArgs> { }
}
