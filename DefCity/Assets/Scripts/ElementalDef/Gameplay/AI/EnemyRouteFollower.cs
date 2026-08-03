using System;
using DefCore.Gameplay.Navigation;
using ElementalDef.Gameplay.World;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalDef.Gameplay.AI
{
    [RequireComponent(typeof(UnitMovement))]
    [DisallowMultipleComponent]
    public class EnemyRouteFollower : MonoBehaviour
    {
        [SerializeField] private EnemyRoute route;
        [SerializeField] private UnitMovement movement;

        public EnemyRoute Route
        {
            set => route = value;
        }

        private int targetPathIndex = -1;
        private bool isFollowing;

        public EnemyRouteFollowerEvent OnRouteCompleted = new();
        public EnemyRouteFollowerEvent OnRouteFailed = new();

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<UnitMovement>();
            }
        }

        private void OnEnable()
        {
            movement.OnMovingStart.AddListener(HandleMovingStart);
            movement.OnMovingComplete.AddListener(HandleMovingComplete);
            movement.OnMovingFailed.AddListener(HandleMovingFailed);
            movement.OnMovingStopped.AddListener(HandleMovingStopped);
            movement.OnMovingReady.AddListener(HandleMovingReady);
        }

        private void OnDisable()
        {
            movement.OnMovingStart.RemoveListener(HandleMovingStart);
            movement.OnMovingComplete.RemoveListener(HandleMovingComplete);
            movement.OnMovingFailed.RemoveListener(HandleMovingFailed);
            movement.OnMovingStopped.RemoveListener(HandleMovingStopped);
            movement.OnMovingReady.RemoveListener(HandleMovingReady);

            bool wasFollowing = isFollowing;

            isFollowing = false;
            targetPathIndex = -1;

            if (wasFollowing && movement != null)
            {
                Debug.LogWarning($"EnemyRouteFollower disabled while following a route. Stopping movement.");
                movement.Stop();
            }
        }

        public void FollowRoute()
        {
            // Todo: 현재는 Validation을 의도적으로 단순화 했음. Validation을 명확히 할지는 추후 결정
            if (isFollowing)
            {
                throw new InvalidOperationException($"Already following a route. Stopping current route and starting new one.");
            }

            if (route == null)
            {
                FailRoute();
                return;
            }

            if (!route.IsPathConnected())
            {
                FailRoute();
                return;
            }

            targetPathIndex = 1;
            isFollowing = true;
            movement.MoveToCell(route[targetPathIndex]);
        }

        public void CancelFollowing()
        {
            isFollowing = false;
            targetPathIndex = -1;

            if (movement != null)
            {
                movement.Stop();
            }
        }

        private void HandleMovingStart(GameObject sender, UnitMovementEventArgs args)
        {
            if (!isFollowing)
            {
                return;
            }

            if (args.TargetCellCoordinates != route[targetPathIndex])
            {
                Debug.LogWarning($"It looks like movement requested by other component. Stopping route following.");
                FailRoute();
            }
        }

        private void HandleMovingComplete(GameObject sender, UnitMovementEventArgs args)
        {
            if (!isFollowing)
            {
                return;
            }

            if (args.TargetCellCoordinates != route[targetPathIndex])
            {
                Debug.LogWarning($"Movement completed to unexpected cell. Stopping route following.");
                FailRoute();
                return;
            }

            // 경로 마지막에 도달했는지 확인하고 완료 처리
            if (targetPathIndex == route.PathLength - 1)
            {
                isFollowing = false;
                targetPathIndex = -1;

                OnRouteCompleted?.Invoke(gameObject);
                return;
            }

            // 다음 셀로 이동
            targetPathIndex++;
        }

        private void HandleMovingFailed(GameObject sender, UnitMovementEventArgs args)
        {
            if (!isFollowing)
            {
                return;
            }

            FailRoute();
        }

        private void HandleMovingStopped(GameObject sender, UnitMovementEventArgs args)
        {
            if (!isFollowing)
            {
                return;
            }

            // 추종 중 발생한 모든 중단은 경로 실패
            isFollowing = false;
            targetPathIndex = -1;
            OnRouteFailed?.Invoke(gameObject);
        }

        private void HandleMovingReady(GameObject sender, UnitMovementEventArgs args)
        {
            if (!isFollowing)
            {
                // 정상적으로 경로를 완주한 경우 여기서 코드 정지
                // 단, 경로 이동 중이 아님에도 불구하고 호출될 수 있음
                return;
            }

            if (args.State == UnitMovementState.IsStopped)
            {
                isFollowing = false;
                targetPathIndex = -1;
                return;
            }

            if (args.State != UnitMovementState.IsCompleted)
            {
                return;
            }

            movement.MoveToCell(route[targetPathIndex]);
        }

        private void FailRoute()
        {
            isFollowing = false;
            targetPathIndex = -1;
            movement.Stop();
            OnRouteFailed?.Invoke(gameObject);
        }
    }

    [Serializable]
    public class EnemyRouteFollowerEvent : UnityEvent<GameObject> { }
}
