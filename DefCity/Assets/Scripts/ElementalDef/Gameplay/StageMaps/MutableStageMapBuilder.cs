using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public sealed class MutableStageMapBuilder
    {
        private readonly StageMapCell[] cells;
        private readonly bool[] assignedCells;
        private readonly List<SpawnDefinition> spawns = new();
        private readonly HashSet<string> spawnIds = new(StringComparer.Ordinal);
        private readonly HashSet<Vector2Int> spawnCells = new();

        private RectInt? headquartersFootprint;
        private Vector2Int? routeGoalCell;
        private EnemyRouteGraph routeGraph;

        public RectInt Bounds { get; }
        public int Seed { get; }
        public string GeneratorVersion { get; }
        public string PatternId { get; }

        public MutableStageMapBuilder(
            RectInt bounds,
            int seed,
            string generatorVersion,
            string patternId)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bounds),
                    bounds,
                    "Stage map bounds must have a positive width and height.");
            }

            if (string.IsNullOrWhiteSpace(generatorVersion))
            {
                throw new ArgumentException(
                    "A generator version is required.",
                    nameof(generatorVersion));
            }

            if (string.IsNullOrWhiteSpace(patternId))
            {
                throw new ArgumentException(
                    "A pattern ID is required.",
                    nameof(patternId));
            }

            int cellCount = checked(bounds.width * bounds.height);
            Bounds = bounds;
            Seed = seed;
            GeneratorVersion = generatorVersion;
            PatternId = patternId;
            cells = new StageMapCell[cellCount];
            assignedCells = new bool[cellCount];
        }

        public void SetCell(Vector2Int coordinates, StageMapCell cell)
        {
            EnsureInsideBounds(coordinates, nameof(coordinates));
            if (!cell.IsDefined)
            {
                throw new ArgumentException(
                    "A stage map builder cannot assign an undefined cell.",
                    nameof(cell));
            }

            int index = GeneratedStageMap.GetCellIndex(Bounds, coordinates);
            cells[index] = cell;
            assignedCells[index] = true;
        }

        public void AddSpawn(SpawnDefinition spawn)
        {
            if (string.IsNullOrWhiteSpace(spawn.Id))
            {
                throw new ArgumentException(
                    "A spawn definition requires an ID.",
                    nameof(spawn));
            }

            EnsureInsideBounds(spawn.Cell, nameof(spawn));

            if (!spawnIds.Add(spawn.Id))
            {
                throw new InvalidOperationException(
                    $"Spawn ID '{spawn.Id}' is already registered.");
            }

            if (!spawnCells.Add(spawn.Cell))
            {
                spawnIds.Remove(spawn.Id);
                throw new InvalidOperationException(
                    $"Spawn cell {spawn.Cell} is already registered.");
            }

            spawns.Add(spawn);
        }

        public void SetHeadquarters(RectInt footprint)
        {
            EnsureFootprintInsideBounds(footprint, nameof(footprint));
            if (headquartersFootprint.HasValue)
            {
                throw new InvalidOperationException(
                    $"Headquarters footprint {headquartersFootprint.Value} is already registered.");
            }

            headquartersFootprint = footprint;
        }

        public void SetRouteGoal(Vector2Int coordinates)
        {
            EnsureInsideBounds(coordinates, nameof(coordinates));
            if (routeGoalCell.HasValue)
            {
                throw new InvalidOperationException(
                    $"Route goal cell {routeGoalCell.Value} is already registered.");
            }

            routeGoalCell = coordinates;
        }

        public void SetRouteGraph(EnemyRouteGraph value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (routeGraph != null)
            {
                throw new InvalidOperationException(
                    "An enemy route graph is already registered.");
            }

            routeGraph = value;
        }

        public GeneratedStageMap Freeze()
        {
            for (int index = 0; index < assignedCells.Length; index++)
            {
                if (assignedCells[index])
                {
                    continue;
                }

                int localX = index % Bounds.width;
                int localY = index / Bounds.width;
                Vector2Int missingCell = new(
                    Bounds.xMin + localX,
                    Bounds.yMin + localY);
                throw new InvalidOperationException(
                    $"Stage map cell {missingCell} is not assigned.");
            }

            if (spawns.Count == 0)
            {
                throw new InvalidOperationException(
                    "A generated stage map requires at least one spawn.");
            }

            if (!headquartersFootprint.HasValue)
            {
                throw new InvalidOperationException(
                    "A generated stage map requires a Headquarters footprint.");
            }

            if (!routeGoalCell.HasValue)
            {
                throw new InvalidOperationException(
                    "A generated stage map requires a route goal cell.");
            }

            if (routeGraph == null)
            {
                throw new InvalidOperationException(
                    "A generated stage map requires an enemy route graph.");
            }

            return new GeneratedStageMap(
                Bounds,
                Seed,
                GeneratorVersion,
                PatternId,
                cells,
                spawns,
                headquartersFootprint.Value,
                routeGoalCell.Value,
                routeGraph);
        }

        private void EnsureInsideBounds(
            Vector2Int coordinates,
            string parameterName)
        {
            if (!Bounds.Contains(coordinates))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    coordinates,
                    $"Cell {coordinates} is outside map bounds {Bounds}.");
            }
        }

        private void EnsureFootprintInsideBounds(
            RectInt footprint,
            string parameterName)
        {
            if (footprint.width <= 0 || footprint.height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    footprint,
                    "The Headquarters footprint must have a positive width and height.");
            }

            if (footprint.xMin < Bounds.xMin ||
                footprint.yMin < Bounds.yMin ||
                footprint.xMax > Bounds.xMax ||
                footprint.yMax > Bounds.yMax)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    footprint,
                    $"The Headquarters footprint must be fully inside map bounds {Bounds}.");
            }
        }
    }
}
