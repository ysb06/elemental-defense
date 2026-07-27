using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DefCity.Gameplay.City.Buildings;
using DefCity.Gameplay.Entities;
using DefCore.Gameplay.Flow;

namespace DefCity.Gameplay.Flow
{
    public enum GameOutcome
    {
        InProgress,
        Victory,
        Defeat
    }

    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TeamKind playerTeamKind = TeamKind.Player;
        [SerializeField] private string gameOverSceneName = "SampleGameOverScene";
        [SerializeField, Min(1)] private int victoryTurn = 120;
        [SerializeField] private string victorySceneName = "VictoryScene";

        private bool isSubscribedToBuildingChanges;
        private bool isSubscribedToTurnChanges;

        public GameOutcome Outcome { get; private set; } = GameOutcome.InProgress;
        public bool IsGameOver => Outcome == GameOutcome.Defeat;
        public bool IsVictory => Outcome == GameOutcome.Victory;
        public int VictoryTurn => victoryTurn;
        private bool HasGameEnded => Outcome != GameOutcome.InProgress;

        private void OnEnable()
        {
            SubscribeToBuildingChanges();
            SubscribeToTurnChanges();
        }

        private void Start()
        {
            EnsureConfigured();
            buildingManager.RegisterSceneBuildings();
            EvaluateDefeatCondition();
            EvaluateVictoryCondition();
        }

        private void OnDisable()
        {
            UnsubscribeFromBuildingChanges();
            UnsubscribeFromTurnChanges();
        }

        public void EvaluateDefeatCondition()
        {
            if (HasGameEnded)
            {
                return;
            }

            EnsureConfigured();

            if (buildingManager.CountBuildings(playerTeamKind, aliveOnly: true) > 0)
            {
                return;
            }

            CompleteGame(GameOutcome.Defeat);
        }

        public void EvaluateVictoryCondition()
        {
            if (HasGameEnded)
            {
                return;
            }

            EnsureConfigured();

            if (buildingManager.CountBuildings(playerTeamKind, aliveOnly: true) == 0)
            {
                CompleteGame(GameOutcome.Defeat);
                return;
            }

            if (turnManager.CurrentTurn < victoryTurn)
            {
                return;
            }

            CompleteGame(GameOutcome.Victory);
        }

        protected virtual void LoadGameOverScene()
        {
            SceneManager.LoadScene(gameOverSceneName);
        }

        protected virtual void LoadVictoryScene()
        {
            SceneManager.LoadScene(victorySceneName);
        }

        private void SubscribeToBuildingChanges()
        {
            if (buildingManager == null || isSubscribedToBuildingChanges)
            {
                return;
            }

            buildingManager.BuildingsChanged += EvaluateDefeatCondition;
            isSubscribedToBuildingChanges = true;
        }

        private void UnsubscribeFromBuildingChanges()
        {
            if (buildingManager == null || !isSubscribedToBuildingChanges)
            {
                return;
            }

            buildingManager.BuildingsChanged -= EvaluateDefeatCondition;
            isSubscribedToBuildingChanges = false;
        }

        private void SubscribeToTurnChanges()
        {
            if (turnManager == null || isSubscribedToTurnChanges)
            {
                return;
            }

            turnManager.OnTurnChanged.AddListener(OnTurnChanged);
            isSubscribedToTurnChanges = true;
        }

        private void UnsubscribeFromTurnChanges()
        {
            if (turnManager == null || !isSubscribedToTurnChanges)
            {
                return;
            }

            turnManager.OnTurnChanged.RemoveListener(OnTurnChanged);
            isSubscribedToTurnChanges = false;
        }

        private void OnTurnChanged(GameObject sender, TurnChangedEventArgs args)
        {
            EvaluateDefeatCondition();
            EvaluateVictoryCondition();
        }

        private void CompleteGame(GameOutcome outcome)
        {
            if (HasGameEnded)
            {
                return;
            }

            if (outcome == GameOutcome.InProgress)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(outcome),
                    outcome,
                    "A completed game requires a terminal outcome.");
            }

            Outcome = outcome;

            if (outcome == GameOutcome.Defeat)
            {
                LoadGameOverScene();
                return;
            }

            LoadVictoryScene();
        }

        private void EnsureConfigured()
        {
            if (buildingManager == null)
            {
                throw new InvalidOperationException($"{nameof(GameStateManager)} requires a {nameof(BuildingManager)} reference.");
            }

            if (turnManager == null)
            {
                throw new InvalidOperationException($"{nameof(GameStateManager)} requires a {nameof(TurnManager)} reference.");
            }

            if (string.IsNullOrWhiteSpace(gameOverSceneName))
            {
                throw new InvalidOperationException($"{nameof(GameStateManager)} requires a game over scene name.");
            }

            if (victoryTurn < 1)
            {
                throw new InvalidOperationException($"{nameof(GameStateManager)} requires a victory turn greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(victorySceneName))
            {
                throw new InvalidOperationException($"{nameof(GameStateManager)} requires a victory scene name.");
            }
        }
    }
}
