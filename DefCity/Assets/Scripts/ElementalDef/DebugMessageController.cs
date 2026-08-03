using System.Globalization;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Entities;
using DefCore.Gameplay.Interaction;
using DefCore.Gameplay.World;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Economy;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.World;
using TMPro;
using UnityEngine;

namespace ElementalDef
{
    public class DebugMessageController : MonoBehaviour
    {
        public TMP_Text messageText;

        public EnemySpawner enemySpawner;
        public WaveBundleController waveBundleController;
        public GameFlowController gameFlowController;
        public CellSpaceMouseEventManager cellSpaceMouseEventManager;
        public EntitySelectionManager entitySelectionManager;
        public Tile3DCellManager tile3DCellManager;
        public TowerEnergyManager towerEnergyManager;

        private Entity selectedEntity;
        private Health selectedHealth;
        private string statusMessage;
        private bool hasLastDamage;
        private string lastDamageInstigatorName;
        private string lastDamageVictimName;
        private float lastRequestedDamage;
        private float lastAppliedDamage;
        private float lastRemainingHealth;

        private void OnEnable()
        {
            if (enemySpawner != null)
            {
                enemySpawner.OnWaveStarted.AddListener(HandleWaveStarted);
                enemySpawner.OnWaveCompleted.AddListener(HandleWaveCompleted);
            }
            if (waveBundleController != null)
            {
                waveBundleController.OnBundleStarted.AddListener(HandleWaveBundleStarted);
                waveBundleController.OnBundleCompleted.AddListener(HandleWaveBundleCompleted);
            }
            if (gameFlowController != null)
            {
                gameFlowController.OnVictory.AddListener(HandleGameVictory);
                gameFlowController.OnDefeat.AddListener(HandleGameDefeat);
            }

            if (cellSpaceMouseEventManager != null)
            {
                cellSpaceMouseEventManager.OnCellMouseClick.AddListener(HandleCellMouseClick);
            }

            if (entitySelectionManager != null)
            {
                entitySelectionManager.OnEntitySelectionChanged.AddListener(
                    HandleEntitySelectionChanged);
                SetSelectedEntity(entitySelectionManager.CurrentEntity);
            }
            else
            {
                RefreshMessage();
            }
        }

        private void OnDisable()
        {
            if (enemySpawner != null)
            {
                enemySpawner.OnWaveStarted.RemoveListener(HandleWaveStarted);
                enemySpawner.OnWaveCompleted.RemoveListener(HandleWaveCompleted);
            }
            if (waveBundleController != null)
            {
                waveBundleController.OnBundleStarted.RemoveListener(HandleWaveBundleStarted);
                waveBundleController.OnBundleCompleted.RemoveListener(HandleWaveBundleCompleted);
            }
            if (gameFlowController != null)
            {
                gameFlowController.OnVictory.RemoveListener(HandleGameVictory);
                gameFlowController.OnDefeat.RemoveListener(HandleGameDefeat);
            }
            if (cellSpaceMouseEventManager != null)
            {
                cellSpaceMouseEventManager.OnCellMouseClick.RemoveListener(HandleCellMouseClick);
            }

            if (entitySelectionManager != null)
            {
                entitySelectionManager.OnEntitySelectionChanged.RemoveListener(
                    HandleEntitySelectionChanged);
            }

            ClearSelectedEntitySubscription();
            ClearLastDamage();
        }

        private void LateUpdate()
        {
            if (!ReferenceEquals(selectedEntity, null))
            {
                RefreshMessage();
            }
        }

        private void HandleWaveStarted(GameObject sender)
        {
            WaveBundleController waveBundleController = sender.GetComponent<WaveBundleController>();
            SetStatusMessage($"Wave {waveBundleController.CurrentWaveIndex + 1} started!");
        }

        private void HandleWaveCompleted(GameObject sender)
        {
            WaveBundleController waveBundleController = sender.GetComponent<WaveBundleController>();
            SetStatusMessage($"Wave {waveBundleController.CurrentWaveIndex + 1} completed!");
        }

        private void HandleWaveBundleStarted(GameObject sender)
        {
            SetStatusMessage("Wave Bundle started!");
        }

        private void HandleWaveBundleCompleted(GameObject sender)
        {
            SetStatusMessage("Wave Bundle completed!");
        }

        private void HandleGameVictory(GameObject sender)
        {
            SetStatusMessage("Game Victory!");
        }

        private void HandleGameDefeat(GameObject sender)
        {
            SetStatusMessage("Game Defeat!");
        }

        private void HandleCellMouseClick(GameObject sender, CellSpaceMouseEventArgs args)
        {
            SetStatusMessage($"Cell clicked at position: {args.Cell.Coordinates}");
        }

        private void HandleEntitySelectionChanged(
            GameObject sender,
            EntitySelectionChangedEventArgs args)
        {
            if (entitySelectionManager == null || sender != entitySelectionManager.gameObject)
            {
                return;
            }

            SetSelectedEntity(args.CurrentEntity);
        }

        private void HandleSelectedHealthDamaged(GameObject sender, DamageEventArgs args)
        {
            if (selectedHealth == null || args.Victim != selectedHealth.gameObject)
            {
                return;
            }

            hasLastDamage = true;
            lastDamageInstigatorName = args.Instigator != null
                ? args.Instigator.name
                : "-";
            lastDamageVictimName = args.Victim != null
                ? args.Victim.name
                : "-";
            lastRequestedDamage = args.RequestedDamage;
            lastAppliedDamage = args.DamageAmount;
            lastRemainingHealth = args.RemainingHealth;
            RefreshMessage();
        }

        private void SetSelectedEntity(Entity entity)
        {
            ClearSelectedEntitySubscription();

            selectedEntity = entity;

            if (selectedEntity != null && selectedEntity.TryGetComponent(out selectedHealth))
            {
                selectedHealth.OnDamaged.AddListener(HandleSelectedHealthDamaged);
            }

            RefreshMessage();
        }

        private void ClearSelectedEntitySubscription()
        {
            if (selectedHealth != null)
            {
                selectedHealth.OnDamaged.RemoveListener(HandleSelectedHealthDamaged);
            }

            selectedHealth = null;
            selectedEntity = null;
        }

        private void ClearLastDamage()
        {
            hasLastDamage = false;
            lastDamageInstigatorName = null;
            lastDamageVictimName = null;
            lastRequestedDamage = 0f;
            lastAppliedDamage = 0f;
            lastRemainingHealth = 0f;
        }

        private void SetStatusMessage(string message)
        {
            statusMessage = message;
            RefreshMessage();
        }

        private void RefreshMessage()
        {
            if (messageText == null)
            {
                return;
            }

            string status = string.IsNullOrEmpty(statusMessage)
                ? "Status: -"
                : $"Status: {statusMessage}";
            string entityName = "-";
            string attackElement = "-";
            string defense = "-";
            string tileElement = "-";
            string health = "-";
            string lastDamage = "Last Damage: -";

            if (selectedEntity != null)
            {
                entityName = selectedEntity.name;

                if (selectedEntity.TryGetComponent(out Attacker attacker) &&
                    attacker.EquippedWeapon is ElementalWeaponBase elementalWeapon)
                {
                    attackElement = elementalWeapon.AttackElement.ToString();
                }

                if (selectedEntity.TryGetComponent(out ElementalCombatant combatant))
                {
                    defense = $"{combatant.DefenseElement} / {FormatNumber(combatant.Defense)}";
                }

                tileElement = GetCurrentTileElement();

                if (selectedHealth != null)
                {
                    health = $"{FormatNumber(selectedHealth.CurrentHealth)} / " +
                             FormatNumber(selectedHealth.MaxHealth);
                }

            }

            if (hasLastDamage)
            {
                lastDamage =
                    $"Last Damage: {lastDamageInstigatorName} -> {lastDamageVictimName}\n" +
                    $"Requested {FormatNumber(lastRequestedDamage)} | " +
                    $"Applied {FormatNumber(lastAppliedDamage)} | " +
                    $"Remaining {FormatNumber(lastRemainingHealth)}";
            }

            string composedMessage =
                $"{status}\n" +
                $"Selected: {entityName}\n" +
                $"Attack: {attackElement} | Defense: {defense} | Terrain: {tileElement}\n" +
                $"Health: {health}\n" +
                lastDamage + "\n" +
                $"Current Tower Energy: {towerEnergyManager?.CurrentEnergy ?? 0} / {towerEnergyManager?.MaxEnergy ?? 0}";

            if (messageText.text != composedMessage)
            {
                messageText.text = composedMessage;
            }
        }

        private string GetCurrentTileElement()
        {
            if (tile3DCellManager == null ||
                selectedEntity == null ||
                !tile3DCellManager.TryGetCell(selectedEntity.transform.position, out CellRef cell))
            {
                return "-";
            }

            GameObject tileInstance = tile3DCellManager.GetTileInstance(cell.RefCoordinates);
            if (tileInstance == null ||
                !tileInstance.TryGetComponent(out ElementalTile elementalTile))
            {
                return "-";
            }

            return elementalTile.ElementType.ToString();
        }

        private static string FormatNumber(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
