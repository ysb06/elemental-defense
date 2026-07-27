using System;
using UnityEngine;
using UnityEngine.Events;

namespace DefCore.Gameplay.Entities
{
    [DisallowMultipleComponent]
    public class Entity : MonoBehaviour
    {
        [SerializeField] private Team team;
        private EntityState state = EntityState.Uninitialized;

        public Team Team => team;

        public EntityState State => state;
        public bool IsInitialized => state != EntityState.Uninitialized;
        public bool IsDead => state == EntityState.Dead;
        public bool IsOperational =>
            state == EntityState.Active &&
            isActiveAndEnabled &&
            gameObject.activeInHierarchy;

        public EntityStateChangedEvent OnStateChanged = new();

        private void Awake()
        {
            if (team != null)
            {
                Initialize(team);
            }
        }

        public void Initialize(Team initialTeam)
        {
            if (initialTeam == null)
            {
                throw new ArgumentNullException(nameof(initialTeam), "Entity Team cannot be null.");
            }

            if (state != EntityState.Uninitialized)
            {
                throw new InvalidOperationException($"[{name}] Entity is already initialized with Team '{team?.name}'.");
            }

            team = initialTeam;
            TransitionTo(EntityState.Active);
        }

        public bool TryMarkDead()
        {
            if (state == EntityState.Uninitialized)
            {
                throw new InvalidOperationException($"[{name}] Uninitialized Entity cannot be marked dead.");
            }

            if (state == EntityState.Dead)
            {
                return false;
            }

            TransitionTo(EntityState.Dead);
            return true;
        }

        private void TransitionTo(EntityState nextState)
        {
            if (state == nextState)
            {
                return;
            }

            EntityState previousState = state;
            state = nextState;
            OnStateChanged?.Invoke(gameObject, new EntityStateChangedEventArgs(previousState, nextState));
        }
    }

    public enum EntityState
    {
        Uninitialized,
        Active,
        Dead,
    }

    [Serializable]
    public readonly struct EntityStateChangedEventArgs
    {
        public EntityState PreviousState { get; }
        public EntityState CurrentState { get; }

        public EntityStateChangedEventArgs(EntityState previousState, EntityState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    [Serializable]
    public class EntityStateChangedEvent : UnityEvent<GameObject, EntityStateChangedEventArgs> { }
}
