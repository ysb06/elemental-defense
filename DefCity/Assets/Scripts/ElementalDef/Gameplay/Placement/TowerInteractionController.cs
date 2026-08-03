using System;
using DefCore.Gameplay.Entities;
using DefCore.Gameplay.Interaction;
using DefCore.Gameplay.World;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Economy;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Presentation.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalDef.Gameplay.Placement
{
    [DisallowMultipleComponent]
    public sealed class TowerInteractionController : MonoBehaviour
    {
        private enum TowerInteractionState
        {
            Idle,
            PlacingNew,
            Relocating,
            Stopped,
        }

        [SerializeField] private TowerUnit towerPrefab;
        [SerializeField] private Team playerTeam;
        [SerializeField] private TowerRegistry towerRegistry;
        [SerializeField] private TowerEnergyManager towerEnergyManager;
        [SerializeField] private TowerPlacementValidator placementValidator;
        [SerializeField] private CellSpaceMouseEventManager cellSpaceMouseEventManager;
        [SerializeField] private EntitySelectionManager entitySelectionManager;
        [SerializeField] private CellCursor cellCursor;

        private TowerInteractionState currentState = TowerInteractionState.Idle;
        private InputAction cancelAction;
        private TowerUnit activePlacementPrefab;
        private TowerCost activePlacementCost;
        private TowerUnit relocationTarget;
        private Entity relocationTargetEntity;
        private int modeEnteredFrame = -1;
        private bool ownsPointerSelectionGate;
        private bool isSubscribed;

        private void Awake()
        {
            EnsureConfigured();
            cellCursor.Hide();
        }

        private void OnEnable()
        {
            if (currentState == TowerInteractionState.Stopped)
            {
                enabled = false;
                return;
            }

            Subscribe();
            cellCursor.Hide();
        }

        private void OnDisable()
        {
            Unsubscribe();

            if (currentState == TowerInteractionState.Stopped)
            {
                ClearInteraction(false);
                return;
            }

            ClearInteraction(true);
            currentState = TowerInteractionState.Idle;
        }

        private void Update()
        {
            if (!IsPlacementActive())
            {
                return;
            }

            if (currentState == TowerInteractionState.Relocating &&
                !IsRelocationTargetValid())
            {
                Cancel();
                return;
            }

            if (!cellSpaceMouseEventManager.TryGetCellSpaceEventArgs(
                    out CellSpaceMouseEventArgs eventArgs))
            {
                cellCursor.Hide();
                return;
            }

            TowerPlacementResult result = EvaluatePlacement(eventArgs.Cell);
            if (!result.HasPose)
            {
                cellCursor.Hide();
                return;
            }

            cellCursor.Show(result.Pose.position, result.CanPlace);
        }

        public void BeginPlacingNew()
        {
            BeginPlacingNew(towerPrefab);
        }

        public void BeginPlacingNew(TowerUnit requestedTowerPrefab)
        {
            EnsureValidTowerPrefab(requestedTowerPrefab);
            TowerCost costComponent = requestedTowerPrefab.GetComponent<TowerCost>();
            if (!towerEnergyManager.CanAfford(costComponent))
            {
                return;
            }

            if (!TryEnterPlacementMode(TowerInteractionState.PlacingNew, requestedTowerPrefab))
            {
                return;
            }

            activePlacementCost = costComponent;
            entitySelectionManager.ClearSelection();
        }

        public void BeginRelocating()
        {
            if (!TryResolveSelectedTower(
                    "Relocation",
                    entitySelectionManager.CurrentEntity,
                    out TowerUnit selectedTower,
                    out Entity selectedEntity))
            {
                return;
            }

            if (!TryEnterPlacementMode(TowerInteractionState.Relocating))
            {
                return;
            }

            relocationTarget = selectedTower;
            relocationTargetEntity = selectedEntity;
            relocationTargetEntity.OnStateChanged.AddListener(HandleRelocationTargetStateChanged);
        }

        public void DemolishSelectedTower()
        {
            if (!TryResolveSelectedTower(
                    "Demolition",
                    entitySelectionManager.CurrentEntity,
                    out TowerUnit selectedTower,
                    out _))
            {
                return;
            }

            if (!towerRegistry.UnregisterTower(selectedTower))
            {
                Debug.LogWarning(
                    $"[{name}] The selected tower is no longer registered and cannot be demolished.",
                    this);
                return;
            }

            GameObject towerObject = selectedTower.gameObject;
            entitySelectionManager.ClearSelection();

            try
            {
                selectedTower.Shutdown();
            }
            finally
            {
                towerObject.SetActive(false);
                Destroy(towerObject);
                Physics.SyncTransforms();
            }
        }

        public void Cancel()
        {
            if (!IsPlacementActive())
            {
                return;
            }

            ExitToIdle();
        }

        public void Shutdown()
        {
            if (currentState == TowerInteractionState.Stopped)
            {
                return;
            }

            currentState = TowerInteractionState.Stopped;
            Unsubscribe();
            ClearInteraction(false);
            enabled = false;
        }

        private void HandleCellMouseClick(GameObject sender, CellSpaceMouseEventArgs eventArgs)
        {
            if (sender != cellSpaceMouseEventManager.gameObject || !IsPlacementActive() || Time.frameCount == modeEnteredFrame)
            {
                return;
            }

            if (currentState == TowerInteractionState.Relocating &&
                !IsRelocationTargetValid())
            {
                Cancel();
                return;
            }

            TowerPlacementResult result = EvaluatePlacement(eventArgs.Cell);
            if (!result.CanPlace)
            {
                if (result.HasPose)
                {
                    cellCursor.Show(result.Pose.position, false);
                }
                else
                {
                    cellCursor.Hide();
                }

                Debug.LogWarning(
                    $"Cannot place tower on cell {eventArgs.Cell.RefCoordinates}: " +
                    result.FailureReason,
                    this);
                return;
            }

            try
            {
                bool interactionCompleted;
                switch (currentState)
                {
                    case TowerInteractionState.PlacingNew:
                        interactionCompleted = TryPlaceNewTower(result.Pose);
                        break;

                    case TowerInteractionState.Relocating:
                        RelocateTower(result.Pose);
                        interactionCompleted = true;
                        break;

                    default:
                        return;
                }

                if (!interactionCompleted)
                {
                    return;
                }
            }
            catch
            {
                ExitToIdle();
                throw;
            }

            ExitToIdle();
        }

        private void HandleCancelPerformed(InputAction.CallbackContext context)
        {
            Cancel();
        }

        private void HandleEntitySelectionChanged(
            GameObject sender,
            EntitySelectionChangedEventArgs eventArgs)
        {
            if (currentState != TowerInteractionState.Relocating ||
                sender != entitySelectionManager.gameObject ||
                eventArgs.CurrentEntity == relocationTargetEntity)
            {
                return;
            }

            Cancel();
        }

        private void HandleRelocationTargetStateChanged(
            GameObject sender,
            EntityStateChangedEventArgs eventArgs)
        {
            if (currentState != TowerInteractionState.Relocating ||
                relocationTargetEntity == null ||
                sender != relocationTargetEntity.gameObject ||
                eventArgs.CurrentState != EntityState.Dead)
            {
                return;
            }

            Cancel();
        }

        private bool TryEnterPlacementMode(
            TowerInteractionState nextState,
            TowerUnit requestedTowerPrefab = null)
        {
            if (currentState == TowerInteractionState.Stopped)
            {
                return false;
            }

            if (!isActiveAndEnabled)
            {
                Debug.LogWarning(
                    $"[{name}] Cannot begin tower interaction while the controller is inactive.",
                    this);
                return false;
            }

            if (currentState != TowerInteractionState.Idle)
            {
                Debug.LogWarning(
                    $"[{name}] Cannot enter {nextState} while {currentState} is active.",
                    this);
                return false;
            }

            if (!entitySelectionManager.IsPointerSelectionEnabled)
            {
                Debug.LogWarning(
                    $"[{name}] Cannot begin tower interaction while pointer selection is unavailable.",
                    this);
                return false;
            }

            if (nextState == TowerInteractionState.PlacingNew)
            {
                EnsureValidTowerPrefab(requestedTowerPrefab);
            }

            entitySelectionManager.SetPointerSelectionEnabled(false);
            ownsPointerSelectionGate = true;
            cellSpaceMouseEventManager.DiscardPendingClick();
            modeEnteredFrame = Time.frameCount;
            activePlacementPrefab = nextState == TowerInteractionState.PlacingNew ? requestedTowerPrefab : null;
            currentState = nextState;
            return true;
        }

        private bool TryResolveSelectedTower(
            string operationName,
            Entity selectedEntity,
            out TowerUnit selectedTower,
            out Entity validatedEntity)
        {
            selectedTower = null;
            validatedEntity = null;

            if (currentState != TowerInteractionState.Idle)
            {
                Debug.LogWarning(
                    $"[{name}] Cannot begin {operationName} while {currentState} is active.",
                    this);
                return false;
            }

            if (!isActiveAndEnabled)
            {
                Debug.LogWarning(
                    $"[{name}] Cannot begin {operationName} while the controller is inactive.",
                    this);
                return false;
            }

            if (selectedEntity == null || !selectedEntity.IsOperational)
            {
                Debug.LogWarning(
                    $"[{name}] {operationName} requires an operational selected Entity.",
                    this);
                return false;
            }

            if (selectedEntity.Team == null ||
                !selectedEntity.Team.IsAlliedWith(playerTeam))
            {
                Debug.LogWarning(
                    $"[{name}] Only an allied tower can be used for {operationName}.",
                    this);
                return false;
            }

            if (!selectedEntity.TryGetComponent(out selectedTower))
            {
                Debug.LogWarning(
                    $"[{name}] The selected Entity is not a {nameof(TowerUnit)}.",
                    this);
                return false;
            }

            if (!IsRegistered(selectedTower))
            {
                Debug.LogWarning(
                    $"[{name}] The selected tower is not registered.",
                    this);
                selectedTower = null;
                return false;
            }

            validatedEntity = selectedEntity;
            return true;
        }

        private TowerPlacementResult EvaluatePlacement(CellRef cell)
        {
            return currentState switch
            {
                TowerInteractionState.PlacingNew =>
                    placementValidator.EvaluatePlacement(
                        activePlacementPrefab.gameObject,
                        cell),
                TowerInteractionState.Relocating when relocationTarget != null =>
                    placementValidator.EvaluatePlacement(
                        relocationTarget.gameObject,
                        cell,
                        relocationTarget.transform.rotation),
                _ => default,
            };
        }

        private bool TryPlaceNewTower(Pose pose)
        {
            if (activePlacementCost == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] Active tower placement has no {nameof(TowerCost)}.");
            }

            if (!towerEnergyManager.CanAfford(activePlacementCost))
            {
                return false;
            }

            TowerUnit createdTower = Instantiate(
                activePlacementPrefab,
                pose.position,
                pose.rotation);
            bool isRegistered = false;

            try
            {
                Entity createdEntity = createdTower.GetComponent<Entity>();
                createdEntity.Initialize(playerTeam);
                towerRegistry.RegisterTower(createdTower);
                isRegistered = true;
                Physics.SyncTransforms();
            }
            catch
            {
                createdTower.gameObject.SetActive(false);

                if (isRegistered)
                {
                    towerRegistry.UnregisterTower(createdTower);
                }

                Destroy(createdTower.gameObject);
                throw;
            }

            TowerEnergyEventArgs energyResult =
                towerEnergyManager.TryConsumeEnergy(activePlacementCost);
            if (energyResult.Result == TowerEnergyResult.Success)
            {
                return true;
            }

            RollbackCreatedTower(createdTower);

            if (energyResult.Result == TowerEnergyResult.InsufficientEnergy)
            {
                return false;
            }

            throw new InvalidOperationException(
                $"[{name}] Tower energy consumption failed with result " +
                $"'{energyResult.Result}' after tower creation.");
        }

        private void RollbackCreatedTower(TowerUnit createdTower)
        {
            if (!towerRegistry.UnregisterTower(createdTower))
            {
                Debug.LogWarning(
                    $"[{name}] Failed to unregister a newly created tower during rollback.",
                    this);
            }

            try
            {
                createdTower.Shutdown();
            }
            finally
            {
                createdTower.gameObject.SetActive(false);
                Destroy(createdTower.gameObject);
                Physics.SyncTransforms();
            }
        }

        private void RelocateTower(Pose pose)
        {
            relocationTarget.transform.SetPositionAndRotation(pose.position, pose.rotation);
            Physics.SyncTransforms();
        }

        private bool IsRelocationTargetValid()
        {
            return relocationTarget != null &&
                   relocationTargetEntity != null &&
                   relocationTargetEntity.IsOperational &&
                   entitySelectionManager.CurrentEntity == relocationTargetEntity &&
                   IsRegistered(relocationTarget);
        }

        private bool IsRegistered(TowerUnit tower)
        {
            foreach (TowerUnit registeredTower in towerRegistry.Towers)
            {
                if (registeredTower == tower)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPlacementActive()
        {
            return currentState is
                TowerInteractionState.PlacingNew or
                TowerInteractionState.Relocating;
        }

        private void ExitToIdle()
        {
            currentState = TowerInteractionState.Idle;
            ClearInteraction(true);
        }

        private void ClearInteraction(bool restorePointerSelection)
        {
            if (relocationTargetEntity != null)
            {
                relocationTargetEntity.OnStateChanged.RemoveListener(
                    HandleRelocationTargetStateChanged);
            }

            relocationTarget = null;
            relocationTargetEntity = null;
            activePlacementPrefab = null;
            activePlacementCost = null;
            modeEnteredFrame = -1;
            cellCursor.Hide();

            bool shouldRestorePointerSelection =
                restorePointerSelection && ownsPointerSelectionGate;
            ownsPointerSelectionGate = false;

            if (shouldRestorePointerSelection)
            {
                entitySelectionManager.SetPointerSelectionEnabled(true);
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                throw new InvalidOperationException(
                    "Project-wide Input Actions are not configured.");
            }

            cancelAction = actions.FindAction("UI/Cancel", true);
            cancelAction.performed += HandleCancelPerformed;
            cellSpaceMouseEventManager.OnCellMouseClick.AddListener(
                HandleCellMouseClick);
            entitySelectionManager.OnEntitySelectionChanged.AddListener(
                HandleEntitySelectionChanged);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (cancelAction != null)
            {
                cancelAction.performed -= HandleCancelPerformed;
                cancelAction = null;
            }

            cellSpaceMouseEventManager.OnCellMouseClick.RemoveListener(
                HandleCellMouseClick);
            entitySelectionManager.OnEntitySelectionChanged.RemoveListener(
                HandleEntitySelectionChanged);
            isSubscribed = false;
        }

        private void EnsureConfigured()
        {
            EnsureValidTowerPrefab(towerPrefab);

            if (playerTeam == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a player {nameof(Team)}.");
            }

            if (towerRegistry == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a {nameof(TowerRegistry)} reference.");
            }

            if (towerEnergyManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a {nameof(TowerEnergyManager)} reference.");
            }

            if (placementValidator == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a {nameof(TowerPlacementValidator)} reference.");
            }

            if (cellSpaceMouseEventManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a {nameof(CellSpaceMouseEventManager)} reference.");
            }

            if (entitySelectionManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires an {nameof(EntitySelectionManager)} reference.");
            }

            if (cellCursor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a {nameof(CellCursor)} reference.");
            }
        }

        private static void EnsureValidTowerPrefab(TowerUnit candidate)
        {
            if (candidate == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a tower prefab.");
            }

            if (candidate.gameObject.scene.IsValid())
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerInteractionController)} requires a prefab asset, not a Scene tower.");
            }

            if (!candidate.gameObject.activeSelf)
            {
                throw new InvalidOperationException(
                    $"{candidate.name} must be an active prefab so it can be initialized immediately after instantiation.");
            }

            if (!candidate.TryGetComponent(out Entity prefabEntity))
            {
                throw new InvalidOperationException(
                    $"{candidate.name} requires an {nameof(Entity)} component.");
            }

            if (prefabEntity.Team != null)
            {
                throw new InvalidOperationException(
                    $"{candidate.name} must not have a Team before runtime initialization.");
            }

            if (!candidate.TryGetComponent(out ElementalCombatant _))
            {
                throw new InvalidOperationException(
                    $"{candidate.name} requires an {nameof(ElementalCombatant)} component.");
            }

            if (!candidate.TryGetComponent(out ElementalWeaponBase _))
            {
                throw new InvalidOperationException(
                    $"{candidate.name} requires an {nameof(ElementalWeaponBase)} component.");
            }

            if (!candidate.TryGetComponent(out TowerCost _))
            {
                throw new InvalidOperationException(
                    $"{candidate.name} requires a {nameof(TowerCost)} component.");
            }
        }
    }
}
