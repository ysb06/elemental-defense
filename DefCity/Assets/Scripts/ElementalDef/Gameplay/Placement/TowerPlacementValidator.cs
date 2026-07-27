using System;
using DefCore.Gameplay.Placement;
using DefCore.Gameplay.World;
using ElementalDef.Gameplay.World;
using UnityEngine;

namespace ElementalDef.Gameplay.Placement
{
    public readonly struct TowerPlacementResult
    {
        public CellRef Cell { get; }
        public bool HasPose { get; }
        public bool CanPlace { get; }
        public Pose Pose { get; }
        public string FailureReason { get; }

        internal TowerPlacementResult(
            CellRef cell,
            bool hasPose,
            bool canPlace,
            Pose pose,
            string failureReason)
        {
            Cell = cell;
            HasPose = hasPose;
            CanPlace = canPlace;
            Pose = pose;
            FailureReason = failureReason ?? string.Empty;
        }
    }

    [DisallowMultipleComponent]
    public sealed class TowerPlacementValidator : MonoBehaviour
    {
        [SerializeField] private CellSpace targetCellSpace;
        [SerializeField] private EnemyRoute enemyRoute;
        [SerializeField] private ColliderPlacementValidator colliderPlacementValidator;

        private void Awake()
        {
            EnsureConfigured();
        }

        public TowerPlacementResult EvaluatePlacement(GameObject towerSource, CellRef cell)
        {
            return EvaluatePlacement(towerSource, cell, Quaternion.identity);
        }

        public TowerPlacementResult EvaluatePlacement(
            GameObject towerSource,
            CellRef cell,
            Quaternion rotation)
        {
            if (towerSource == null)
            {
                return CreateResultWithoutPose(cell, "No tower source is selected.");
            }

            if (!cell.IsValid)
            {
                return CreateResultWithoutPose(cell, "The target cell is invalid.");
            }

            if (cell.Space != targetCellSpace)
            {
                return CreateResultWithoutPose(cell, "The target cell belongs to a different CellSpace.");
            }

            if (!targetCellSpace.TryGetSurfaceCenter(cell.RefCoordinates, out Vector3 worldSurfaceCenter))
            {
                return CreateResultWithoutPose(
                    cell,
                    $"Cannot resolve the surface for cell {cell.RefCoordinates}.");
            }

            Pose pose = new(worldSurfaceCenter, rotation);
            if (enemyRoute.ContainsCell(cell.Coordinates))
            {
                return CreateResultWithPose(
                    cell,
                    pose,
                    false,
                    $"Cell {cell.Coordinates} belongs to the enemy route.");
            }

            if (!colliderPlacementValidator.CanPlace(
                    towerSource,
                    pose.position,
                    pose.rotation,
                    out string failureReason))
            {
                return CreateResultWithPose(cell, pose, false, failureReason);
            }

            return CreateResultWithPose(cell, pose, true, string.Empty);
        }

        private static TowerPlacementResult CreateResultWithoutPose(
            CellRef cell,
            string failureReason)
        {
            return new TowerPlacementResult(
                cell,
                false,
                false,
                default,
                failureReason);
        }

        private static TowerPlacementResult CreateResultWithPose(
            CellRef cell,
            Pose pose,
            bool canPlace,
            string failureReason)
        {
            return new TowerPlacementResult(
                cell,
                true,
                canPlace,
                pose,
                failureReason);
        }

        private void EnsureConfigured()
        {
            if (targetCellSpace == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementValidator)} requires a {nameof(CellSpace)} reference.");
            }

            if (enemyRoute == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementValidator)} requires an {nameof(EnemyRoute)} reference.");
            }

            if (colliderPlacementValidator == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementValidator)} requires a {nameof(ColliderPlacementValidator)} reference.");
            }
        }
    }
}
