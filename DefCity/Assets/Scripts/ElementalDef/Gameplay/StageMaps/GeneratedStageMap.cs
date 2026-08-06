using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public sealed class GeneratedStageMap
    {
        private readonly StageMapCell[] cells;
        private readonly IReadOnlyList<SpawnDefinition> spawns;

        public RectInt Bounds { get; }
        public int Seed { get; }
        public string GeneratorVersion { get; }
        public string PatternId { get; }
        public int CellCount => cells.Length;
        public IReadOnlyList<SpawnDefinition> Spawns => spawns;
        public RectInt HeadquartersFootprint { get; }
        public Vector2Int RouteGoalCell { get; }
        public EnemyRouteGraph RouteGraph { get; }

        internal GeneratedStageMap(
            RectInt bounds,
            int seed,
            string generatorVersion,
            string patternId,
            IReadOnlyList<StageMapCell> sourceCells,
            IReadOnlyList<SpawnDefinition> sourceSpawns,
            RectInt headquartersFootprint,
            Vector2Int routeGoalCell,
            EnemyRouteGraph routeGraph)
        {
            EnsureValidBounds(bounds);

            if (string.IsNullOrWhiteSpace(generatorVersion))
            {
                throw new ArgumentException( "A generator version is required.", nameof(generatorVersion));
            }

            if (string.IsNullOrWhiteSpace(patternId))
            {
                throw new ArgumentException("A pattern ID is required.", nameof(patternId));
            }

            if (sourceCells == null)
            {
                throw new ArgumentNullException(nameof(sourceCells));
            }

            int expectedCellCount = checked(bounds.width * bounds.height);
            if (sourceCells.Count != expectedCellCount)
            {
                throw new ArgumentException(
                    $"Expected {expectedCellCount} cells but received {sourceCells.Count}.",
                    nameof(sourceCells));
            }

            cells = new StageMapCell[sourceCells.Count];
            for (int index = 0; index < sourceCells.Count; index++)
            {
                StageMapCell cell = sourceCells[index];
                if (!cell.IsDefined)
                {
                    throw new ArgumentException($"Cell index {index} is not assigned.", nameof(sourceCells));
                }

                cells[index] = cell;
            }

            if (sourceSpawns == null)
            {
                throw new ArgumentNullException(nameof(sourceSpawns));
            }

            if (sourceSpawns.Count == 0)
            {
                throw new ArgumentException("A generated stage map requires at least one spawn.", nameof(sourceSpawns));
            }

            SpawnDefinition[] spawnCopies = new SpawnDefinition[sourceSpawns.Count];
            HashSet<string> spawnIds = new(StringComparer.Ordinal);
            HashSet<Vector2Int> spawnCells = new();
            for (int index = 0; index < sourceSpawns.Count; index++)
            {
                SpawnDefinition spawn = sourceSpawns[index];
                if (string.IsNullOrWhiteSpace(spawn.Id))
                {
                    throw new ArgumentException($"Spawn index {index} has no ID.", nameof(sourceSpawns));
                }

                if (!bounds.Contains(spawn.Cell))
                {
                    throw new ArgumentException($"Spawn '{spawn.Id}' at {spawn.Cell} is outside the map bounds.", nameof(sourceSpawns));
                }

                if (!spawnIds.Add(spawn.Id))
                {
                    throw new ArgumentException($"Spawn ID '{spawn.Id}' is duplicated.", nameof(sourceSpawns));
                }

                if (!spawnCells.Add(spawn.Cell))
                {
                    throw new ArgumentException($"Spawn cell {spawn.Cell} is duplicated.", nameof(sourceSpawns));
                }

                spawnCopies[index] = spawn;
            }

            EnsureValidFootprint(bounds, headquartersFootprint);

            if (!bounds.Contains(routeGoalCell))
            {
                throw new ArgumentOutOfRangeException(nameof(routeGoalCell), routeGoalCell, "The route goal cell must be inside the map bounds.");
            }

            Bounds = bounds;
            Seed = seed;
            GeneratorVersion = generatorVersion;
            PatternId = patternId;
            spawns = Array.AsReadOnly(spawnCopies);
            HeadquartersFootprint = headquartersFootprint;
            RouteGoalCell = routeGoalCell;
            RouteGraph = routeGraph ?? throw new ArgumentNullException(nameof(routeGraph));
        }

        public bool Contains(Vector2Int coordinates)
        {
            return Bounds.Contains(coordinates);
        }

        public bool IsHeadquartersCell(Vector2Int coordinates)
        {
            return HeadquartersFootprint.Contains(coordinates);
        }

        public StageMapCell GetCell(Vector2Int coordinates)
        {
            if (!TryGetCell(coordinates, out StageMapCell cell))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(coordinates),
                    coordinates,
                    $"Cell {coordinates} is outside map bounds {Bounds}.");
            }

            return cell;
        }

        public bool TryGetCell(Vector2Int coordinates, out StageMapCell cell)
        {
            if (!Bounds.Contains(coordinates))
            {
                cell = default;
                return false;
            }

            cell = cells[GetCellIndex(Bounds, coordinates)];
            return true;
        }

        public IEnumerable<StageMapCellEntry> EnumerateCells()
        {
            for (int localY = 0; localY < Bounds.height; localY++)
            {
                for (int localX = 0; localX < Bounds.width; localX++)
                {
                    int index = localY * Bounds.width + localX;
                    Vector2Int coordinates = new(Bounds.xMin + localX, Bounds.yMin + localY);
                    yield return new StageMapCellEntry(coordinates, cells[index]);
                }
            }
        }

        internal static int GetCellIndex(RectInt bounds, Vector2Int coordinates)
        {
            int localX = coordinates.x - bounds.xMin;
            int localY = coordinates.y - bounds.yMin;
            return localY * bounds.width + localX;
        }

        private static void EnsureValidBounds(RectInt bounds)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds), bounds, "Stage map bounds must have a positive width and height.");
            }

            _ = checked(bounds.width * bounds.height);
        }

        private static void EnsureValidFootprint(
            RectInt bounds,
            RectInt headquartersFootprint)
        {
            if (headquartersFootprint.width <= 0 ||
                headquartersFootprint.height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(headquartersFootprint),
                    headquartersFootprint,
                    "The Headquarters footprint must have a positive width and height.");
            }

            if (headquartersFootprint.xMin < bounds.xMin ||
                headquartersFootprint.yMin < bounds.yMin ||
                headquartersFootprint.xMax > bounds.xMax ||
                headquartersFootprint.yMax > bounds.yMax)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(headquartersFootprint),
                    headquartersFootprint,
                    $"The Headquarters footprint must be fully inside map bounds {bounds}.");
            }
        }
    }
}
