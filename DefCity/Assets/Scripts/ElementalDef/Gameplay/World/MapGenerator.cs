using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.World
{
    public sealed class MapGenerator : MonoBehaviour
    {
        // The first implementation uses recursive backtracking. Keep its maximum
        // call depth bounded until the solver is replaced with an iterative stack.
        private const int MaxSupportedMapCells = 1_024;
        private const int MaxPathSearchNodes = 250_000;
        private const int MaxDemoPathSearchNodes = 10_000;
        private const int MaxDemoRandomAttempts = 2;

        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        // Bottom-left -> bottom-right -> top-right -> top-left.
        private static readonly Vector2Int[] DemoPatternDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private TileBase groundTile;
        [SerializeField] private TileBase pathTile;
        [SerializeField] private TileBase presetPatternTile;
        [SerializeField] private Vector2Int mapOrigin = new(-7, -5);
        [SerializeField] private int width = 14;
        [SerializeField] private int height = 10;

        [Header("Demo")]
        [SerializeField, Min(0)] private int demoPatternScatter = 2;
        [SerializeField] private bool randomizeDemoSeed = true;
        [SerializeField] private int demoSeed = 12345;
        [SerializeField, Range(1, MaxDemoRandomAttempts)]
        private int maxDemoPlacementAttempts = MaxDemoRandomAttempts;

        [SerializeField, HideInInspector]
        private List<Vector2Int> lastDemoPath = new();
        [SerializeField, HideInInspector]
        private List<MapPattern> lastDemoPatterns = new();
        [SerializeField, HideInInspector]
        private string lastDemoMessage = string.Empty;
        [SerializeField, HideInInspector] private int lastDemoSeed;
        [SerializeField, HideInInspector] private int lastDemoAttemptCount;
        [SerializeField, HideInInspector] private Vector2Int lastDemoStart;
        [SerializeField, HideInInspector] private Vector2Int lastDemoEnd;
        [SerializeField, HideInInspector]
        private PathSearchFailureReason lastDemoFailureReason =
            PathSearchFailureReason.None;

        public Tilemap GroundTilemap => groundTilemap;
        public Vector2Int MapOrigin => mapOrigin;
        public int Width => width;
        public int Height => height;
        public RectInt MapBounds => new(mapOrigin, new Vector2Int(width, height));
        public IReadOnlyList<Vector2Int> LastDemoPath => lastDemoPath;
        public IReadOnlyList<MapPattern> LastDemoPatterns => lastDemoPatterns;
        public string LastDemoMessage => lastDemoMessage;
        public int LastDemoSeed => lastDemoSeed;
        public int LastDemoAttemptCount => lastDemoAttemptCount;
        public Vector2Int LastDemoStart => lastDemoStart;
        public Vector2Int LastDemoEnd => lastDemoEnd;
        public PathSearchFailureReason LastDemoFailureReason =>
            lastDemoFailureReason;
        public bool HasSuccessfulDemo => lastDemoPath.Count > 0;

        /// <summary>
        /// Finds one deterministic, non-repeating path through every FixedPath
        /// in list order. This method does not modify the Tilemap. Pattern tiles
        /// outside FixedPath remain traversable by automatic connector paths.
        /// </summary>
        public PathSearchResult GetPath(
            RectInt mapBounds,
            Vector2Int start,
            Vector2Int end,
            IReadOnlyList<MapPattern> presetPatterns)
        {
            return GetPath(
                mapBounds,
                start,
                end,
                presetPatterns,
                MaxPathSearchNodes);
        }

        private PathSearchResult GetPath(
            RectInt mapBounds,
            Vector2Int start,
            Vector2Int end,
            IReadOnlyList<MapPattern> presetPatterns,
            int maxSearchNodes)
        {
            if (mapBounds.width <= 0 || mapBounds.height <= 0)
            {
                return PathSearchResult.Failure(PathSearchFailureReason.InvalidBounds);
            }

            long mapCellCount = (long)mapBounds.width * mapBounds.height;
            if (mapCellCount > MaxSupportedMapCells)
            {
                return PathSearchResult.Failure(PathSearchFailureReason.MapTooLarge);
            }

            if (!mapBounds.Contains(start))
            {
                return PathSearchResult.Failure(PathSearchFailureReason.StartOutOfBounds);
            }

            if (!mapBounds.Contains(end))
            {
                return PathSearchResult.Failure(PathSearchFailureReason.EndOutOfBounds);
            }

            IReadOnlyList<MapPattern> patterns =
                presetPatterns ?? Array.Empty<MapPattern>();
            PathSearchFailureReason placementFailure =
                GetPatternPlacementFailure(mapBounds, patterns);
            if (placementFailure != PathSearchFailureReason.None)
            {
                return PathSearchResult.Failure(placementFailure);
            }

            if (!HasValidEndpointOrder(start, end, patterns))
            {
                return PathSearchResult.Failure(PathSearchFailureReason.PathNotFound);
            }

            PathSolver solver = new(
                mapBounds,
                start,
                end,
                patterns,
                maxSearchNodes);
            return solver.Solve();
        }

        /// <summary>
        /// Builds four directed three-cell sample patterns near the centers of
        /// the four quadrants, finds a path through them, and paints the result.
        /// Randomness is limited to pattern placement; GetPath stays deterministic.
        /// The current Tilemap is left untouched if no path can be found.
        /// </summary>
        public bool GenerateDemo()
        {
            if (groundTilemap == null || groundTile == null || pathTile == null)
            {
                return FailDemo(
                    PathSearchFailureReason.InvalidBounds,
                    "Ground Tilemap, Ground Tile, and Path Tile must be assigned.");
            }

            RectInt bounds = MapBounds;
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                return FailDemo(
                    PathSearchFailureReason.InvalidBounds,
                    "The demo map width and height must be positive.");
            }

            long mapCellCount = (long)bounds.width * bounds.height;
            if (mapCellCount > MaxSupportedMapCells)
            {
                return FailDemo(
                    PathSearchFailureReason.MapTooLarge,
                    $"The demo supports at most {MaxSupportedMapCells} cells.");
            }

            int supportedScatter = Mathf.Clamp(
                demoPatternScatter,
                0,
                Mathf.Max(bounds.width, bounds.height));
            if (!TryCreateDemoCenterCandidates(
                    bounds,
                    supportedScatter,
                    out List<Vector2Int>[] centerCandidates))
            {
                return FailDemo(
                    PathSearchFailureReason.InvalidBounds,
                    "The four directed three-cell patterns require a map of at least 6 x 6 cells.");
            }

            int selectedSeed = randomizeDemoSeed
                ? Guid.NewGuid().GetHashCode()
                : demoSeed;

            DemoRandom random = new(selectedSeed);
            HashSet<string> attemptedLayouts = new();
            int randomAttemptLimit = Mathf.Clamp(
                maxDemoPlacementAttempts,
                1,
                MaxDemoRandomAttempts);
            int randomDrawLimit = Mathf.Max(16, randomAttemptLimit * 8);
            int randomDrawCount = 0;
            int attemptCount = 0;
            PathSearchFailureReason latestFailureReason =
                PathSearchFailureReason.PathNotFound;

            while (attemptCount < randomAttemptLimit &&
                   randomDrawCount < randomDrawLimit)
            {
                randomDrawCount++;
                Vector2Int[] centers =
                    SelectRandomDemoCenters(centerCandidates, ref random);
                if (!attemptedLayouts.Add(GetDemoLayoutSignature(centers)))
                {
                    continue;
                }

                attemptCount++;
                PathSearchResult result = SearchDemoLayout(
                    bounds,
                    centers,
                    out List<MapPattern> patterns,
                    out Vector2Int start,
                    out Vector2Int end);
                if (result.Succeeded)
                {
                    string successMessage =
                        $"Generated {result.Path.Count} path cells through four patterns " +
                        $"(seed {selectedSeed}, attempt {attemptCount}).";
                    return CompleteDemo(
                        bounds,
                        result,
                        patterns,
                        start,
                        end,
                        selectedSeed,
                        attemptCount,
                        successMessage);
                }

                latestFailureReason = result.FailureReason;
                if (result.FailureReason == PathSearchFailureReason.SearchBudgetExceeded)
                {
                    break;
                }

                if (result.FailureReason != PathSearchFailureReason.PathNotFound)
                {
                    break;
                }
            }

            Vector2Int[] canonicalCenters =
                SelectCanonicalDemoCenters(centerCandidates);
            if (attemptedLayouts.Add(GetDemoLayoutSignature(canonicalCenters)))
            {
                attemptCount++;
                PathSearchResult canonicalResult =
                    SearchDemoLayout(
                        bounds,
                        canonicalCenters,
                        out List<MapPattern> canonicalPatterns,
                        out Vector2Int canonicalStart,
                        out Vector2Int canonicalEnd);
                if (canonicalResult.Succeeded)
                {
                    string successMessage =
                        $"Generated {canonicalResult.Path.Count} path cells through four patterns " +
                        $"with the centered fallback (seed {selectedSeed}, " +
                        $"attempt {attemptCount}).";
                    return CompleteDemo(
                        bounds,
                        canonicalResult,
                        canonicalPatterns,
                        canonicalStart,
                        canonicalEnd,
                        selectedSeed,
                        attemptCount,
                        successMessage);
                }

                latestFailureReason = canonicalResult.FailureReason;
            }

            string failureMessage =
                $"No demo path was found after {attemptCount} unique layout attempts " +
                $"(seed {selectedSeed}, reason {latestFailureReason}). " +
                "The existing Tilemap and its path result were preserved.";
            return FailDemo(latestFailureReason, failureMessage);
        }

        public void ClearDemoPreview()
        {
            lastDemoPath.Clear();
            lastDemoPatterns.Clear();
            lastDemoMessage = string.Empty;
            lastDemoSeed = 0;
            lastDemoAttemptCount = 0;
            lastDemoStart = default;
            lastDemoEnd = default;
            lastDemoFailureReason = PathSearchFailureReason.None;
        }

        public void GenerateMap(Vector2Int mapOrigin, int width, int height)
        {
            TileBase[] tiles = new TileBase[width * height];
            for (int index = 0; index < tiles.Length; index++)
            {
                tiles[index] = groundTile;
            }
            BoundsInt CellBounds = new(mapOrigin.x, mapOrigin.y, 0, width, height, 1);

            groundTilemap.ClearAllTiles();
            groundTilemap.SetTilesBlock(CellBounds, tiles);
            groundTilemap.RefreshAllTiles();
            groundTilemap.CompressBounds();
            ClearDemoPreview();
        }

        public void ClearMap()
        {
            groundTilemap.ClearAllTiles();
            groundTilemap.CompressBounds();
            ClearDemoPreview();
        }

        private PathSearchResult SearchDemoLayout(
            RectInt bounds,
            IReadOnlyList<Vector2Int> centers,
            out List<MapPattern> patterns,
            out Vector2Int start,
            out Vector2Int end)
        {
            patterns = CreateDemoPatterns(centers);
            start = new Vector2Int(bounds.xMin, patterns[0].Entry.y);
            end = new Vector2Int(
                bounds.xMin,
                patterns[patterns.Count - 1].Exit.y);
            return GetPath(
                bounds,
                start,
                end,
                patterns,
                MaxDemoPathSearchNodes);
        }

        private List<MapPattern> CreateDemoPatterns(
            IReadOnlyList<Vector2Int> centers)
        {
            TileBase fixedPatternTile =
                presetPatternTile != null ? presetPatternTile : pathTile;
            List<MapPattern> patterns =
                new(DemoPatternDirections.Length);

            for (int patternIndex = 0;
                 patternIndex < DemoPatternDirections.Length;
                 patternIndex++)
            {
                Vector2Int direction = DemoPatternDirections[patternIndex];
                Vector2Int center = centers[patternIndex];
                Vector2Int[] fixedPath =
                {
                    center - direction,
                    center,
                    center + direction
                };
                PresetPatternTile[] tiles =
                {
                    new(fixedPath[0], fixedPatternTile),
                    new(fixedPath[1], fixedPatternTile),
                    new(fixedPath[2], fixedPatternTile)
                };
                patterns.Add(new MapPattern(tiles, fixedPath));
            }

            return patterns;
        }

        private void ApplyDemoMap(
            RectInt bounds,
            IReadOnlyList<Vector2Int> path,
            IReadOnlyList<MapPattern> patterns)
        {
            TileBase[] tiles = new TileBase[bounds.width * bounds.height];
            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                tiles[tileIndex] = groundTile;
            }

            for (int pathIndex = 0; pathIndex < path.Count; pathIndex++)
            {
                Vector2Int position = path[pathIndex];
                tiles[GetDemoTileIndex(bounds, position)] = pathTile;
            }

            for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
            {
                IReadOnlyList<PresetPatternTile> patternTiles =
                    patterns[patternIndex].Tiles;
                for (int tileIndex = 0;
                     tileIndex < patternTiles.Count;
                     tileIndex++)
                {
                    PresetPatternTile patternTile = patternTiles[tileIndex];
                    tiles[GetDemoTileIndex(bounds, patternTile.Position)] =
                        patternTile.Tile != null
                            ? patternTile.Tile
                            : pathTile;
                }
            }

            BoundsInt cellBounds = new(
                bounds.xMin,
                bounds.yMin,
                0,
                bounds.width,
                bounds.height,
                1);
            groundTilemap.ClearAllTiles();
            groundTilemap.SetTilesBlock(cellBounds, tiles);
            groundTilemap.RefreshAllTiles();
            groundTilemap.CompressBounds();
        }

        private bool CompleteDemo(
            RectInt bounds,
            PathSearchResult result,
            List<MapPattern> patterns,
            Vector2Int start,
            Vector2Int end,
            int seed,
            int attemptCount,
            string message)
        {
            ApplyDemoMap(bounds, result.Path, patterns);
            lastDemoPath = new List<Vector2Int>(result.Path);
            lastDemoPatterns = new List<MapPattern>(patterns);
            lastDemoSeed = seed;
            lastDemoAttemptCount = attemptCount;
            lastDemoStart = start;
            lastDemoEnd = end;
            lastDemoFailureReason = PathSearchFailureReason.None;
            lastDemoMessage = message;
            Debug.Log(message, this);
            return true;
        }

        private static int GetDemoTileIndex(
            RectInt bounds,
            Vector2Int position)
        {
            int localX = position.x - bounds.xMin;
            int localY = position.y - bounds.yMin;
            return localY * bounds.width + localX;
        }

        private bool FailDemo(
            PathSearchFailureReason failureReason,
            string message)
        {
            lastDemoFailureReason = failureReason;
            lastDemoMessage = message;
            Debug.LogError(message, this);
            return false;
        }

        private static bool TryCreateDemoCenterCandidates(
            RectInt bounds,
            int scatter,
            out List<Vector2Int>[] candidates)
        {
            candidates = null;
            if (bounds.width < 6 || bounds.height < 6)
            {
                return false;
            }

            int leftWidth = bounds.width / 2;
            int bottomHeight = bounds.height / 2;
            int rightWidth = bounds.width - leftWidth;
            int topHeight = bounds.height - bottomHeight;
            int xSplit = bounds.xMin + leftWidth;
            int ySplit = bounds.yMin + bottomHeight;

            RectInt[] quadrants =
            {
                new(bounds.xMin, bounds.yMin, leftWidth, bottomHeight),
                new(xSplit, bounds.yMin, rightWidth, bottomHeight),
                new(xSplit, ySplit, rightWidth, topHeight),
                new(bounds.xMin, ySplit, leftWidth, topHeight)
            };

            candidates = new List<Vector2Int>[quadrants.Length];
            for (int index = 0; index < quadrants.Length; index++)
            {
                candidates[index] = EnumerateDemoCenters(
                    quadrants[index],
                    DemoPatternDirections[index],
                    Mathf.Max(0, scatter));
                if (candidates[index].Count == 0)
                {
                    candidates = null;
                    return false;
                }
            }

            return true;
        }

        private static List<Vector2Int> EnumerateDemoCenters(
            RectInt quadrant,
            Vector2Int direction,
            int scatter)
        {
            Vector2Int quadrantCenter = new(
                quadrant.xMin + (quadrant.width - 1) / 2,
                quadrant.yMin + (quadrant.height - 1) / 2);
            int minX = Mathf.Max(quadrant.xMin, quadrantCenter.x - scatter);
            int maxX = Mathf.Min(quadrant.xMax - 1, quadrantCenter.x + scatter);
            int minY = Mathf.Max(quadrant.yMin, quadrantCenter.y - scatter);
            int maxY = Mathf.Min(quadrant.yMax - 1, quadrantCenter.y + scatter);

            List<Vector2Int> centers = new();
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2Int center = new(x, y);
                    if (!quadrant.Contains(center - direction) ||
                        !quadrant.Contains(center + direction))
                    {
                        continue;
                    }

                    centers.Add(center);
                }
            }

            return centers;
        }

        private static Vector2Int[] SelectRandomDemoCenters(
            IReadOnlyList<List<Vector2Int>> candidates,
            ref DemoRandom random)
        {
            Vector2Int[] centers = new Vector2Int[candidates.Count];
            for (int index = 0; index < candidates.Count; index++)
            {
                IReadOnlyList<Vector2Int> quadrantCandidates = candidates[index];
                centers[index] =
                    quadrantCandidates[random.Next(quadrantCandidates.Count)];
            }

            return centers;
        }

        private static Vector2Int[] SelectCanonicalDemoCenters(
            IReadOnlyList<List<Vector2Int>> candidates)
        {
            Vector2Int[] centers = new Vector2Int[candidates.Count];
            for (int index = 0; index < candidates.Count; index++)
            {
                IReadOnlyList<Vector2Int> quadrantCandidates = candidates[index];
                int minX = int.MaxValue;
                int maxX = int.MinValue;
                int minY = int.MaxValue;
                int maxY = int.MinValue;
                for (int candidateIndex = 0;
                     candidateIndex < quadrantCandidates.Count;
                     candidateIndex++)
                {
                    Vector2Int candidate = quadrantCandidates[candidateIndex];
                    minX = Mathf.Min(minX, candidate.x);
                    maxX = Mathf.Max(maxX, candidate.x);
                    minY = Mathf.Min(minY, candidate.y);
                    maxY = Mathf.Max(maxY, candidate.y);
                }

                Vector2Int target = new(
                    minX + (maxX - minX) / 2,
                    minY + (maxY - minY) / 2);
                Vector2Int bestCandidate = quadrantCandidates[0];
                int bestDistance = int.MaxValue;
                for (int candidateIndex = 0;
                     candidateIndex < quadrantCandidates.Count;
                     candidateIndex++)
                {
                    Vector2Int candidate = quadrantCandidates[candidateIndex];
                    int distance =
                        Mathf.Abs(candidate.x - target.x) +
                        Mathf.Abs(candidate.y - target.y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCandidate = candidate;
                    }
                }

                centers[index] = bestCandidate;
            }

            return centers;
        }

        private static string GetDemoLayoutSignature(
            IReadOnlyList<Vector2Int> centers)
        {
            return string.Join("|", centers);
        }

        private struct DemoRandom
        {
            private uint state;

            public DemoRandom(int seed)
            {
                state = unchecked((uint)seed) + 0x9E3779B9u;
                if (state == 0)
                {
                    state = 0x6D2B79F5u;
                }
            }

            public int Next(int exclusiveMax)
            {
                uint value = state;
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                state = value;
                return (int)(value % (uint)exclusiveMax);
            }
        }

        private static PathSearchFailureReason GetPatternPlacementFailure(
            RectInt mapBounds,
            IReadOnlyList<MapPattern> patterns)
        {
            Dictionary<Vector2Int, int> patternTileOwners = new();

            for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
            {
                IReadOnlyList<PresetPatternTile> patternTiles = patterns[patternIndex].Tiles;
                for (int tileIndex = 0; tileIndex < patternTiles.Count; tileIndex++)
                {
                    Vector2Int position = patternTiles[tileIndex].Position;
                    if (!mapBounds.Contains(position))
                    {
                        return PathSearchFailureReason.PatternOutOfBounds;
                    }

                    if (patternTileOwners.TryGetValue(position, out int ownerIndex))
                    {
                        if (ownerIndex != patternIndex)
                        {
                            return PathSearchFailureReason.PatternOverlap;
                        }

                        continue;
                    }

                    patternTileOwners.Add(position, patternIndex);
                }
            }

            return PathSearchFailureReason.None;
        }

        private static bool HasValidEndpointOrder(
            Vector2Int start,
            Vector2Int end,
            IReadOnlyList<MapPattern> patterns)
        {
            if (start == end)
            {
                if (patterns.Count == 0)
                {
                    return true;
                }

                IReadOnlyList<Vector2Int> onlyFixedPath = patterns[0].FixedPath;
                return patterns.Count == 1 &&
                    onlyFixedPath.Count == 1 &&
                    onlyFixedPath[0] == start;
            }

            for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
            {
                IReadOnlyList<Vector2Int> fixedPath = patterns[patternIndex].FixedPath;
                for (int pathIndex = 0; pathIndex < fixedPath.Count; pathIndex++)
                {
                    Vector2Int position = fixedPath[pathIndex];
                    bool isFirstRequiredCell = patternIndex == 0 && pathIndex == 0;
                    bool isLastRequiredCell =
                        patternIndex == patterns.Count - 1 &&
                        pathIndex == fixedPath.Count - 1;

                    if (position == start && !isFirstRequiredCell)
                    {
                        return false;
                    }

                    if (position == end && !isLastRequiredCell)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private sealed class PathSolver
        {
            private readonly RectInt mapBounds;
            private readonly Vector2Int end;
            private readonly IReadOnlyList<MapPattern> patterns;
            private readonly int maxSearchNodes;
            private readonly HashSet<Vector2Int> reservedCells = new();
            private readonly HashSet<Vector2Int> usedCells = new();
            private readonly List<Vector2Int> path = new();
            private readonly int[] reachabilityVisited;
            private readonly Vector2Int[] reachabilityQueue;

            private int reachabilityVisitId;
            private int exploredNodeCount;
            private bool searchBudgetExceeded;

            public PathSolver(
                RectInt mapBounds,
                Vector2Int start,
                Vector2Int end,
                IReadOnlyList<MapPattern> patterns,
                int maxSearchNodes)
            {
                this.mapBounds = mapBounds;
                this.end = end;
                this.patterns = patterns;
                this.maxSearchNodes = maxSearchNodes;

                path.Add(start);
                usedCells.Add(start);
                reservedCells.Add(end);

                for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
                {
                    IReadOnlyList<Vector2Int> fixedPath = patterns[patternIndex].FixedPath;
                    for (int pathIndex = 0; pathIndex < fixedPath.Count; pathIndex++)
                    {
                        reservedCells.Add(fixedPath[pathIndex]);
                    }
                }

                int cellCount = mapBounds.width * mapBounds.height;
                reachabilityVisited = new int[cellCount];
                reachabilityQueue = new Vector2Int[cellCount];
            }

            public PathSearchResult Solve()
            {
                if (SearchConnector(0))
                {
                    return PathSearchResult.Success(path);
                }

                PathSearchFailureReason failureReason = searchBudgetExceeded
                    ? PathSearchFailureReason.SearchBudgetExceeded
                    : PathSearchFailureReason.PathNotFound;
                return PathSearchResult.Failure(failureReason);
            }

            private bool SearchConnector(int patternIndex)
            {
                if (searchBudgetExceeded)
                {
                    return false;
                }

                exploredNodeCount++;
                if (exploredNodeCount > maxSearchNodes)
                {
                    searchBudgetExceeded = true;
                    return false;
                }

                Vector2Int current = path[path.Count - 1];
                Vector2Int target = patternIndex < patterns.Count
                    ? patterns[patternIndex].Entry
                    : end;

                if (current == target)
                {
                    if (patternIndex == patterns.Count)
                    {
                        return true;
                    }

                    return AppendFixedPathAndContinue(patternIndex);
                }

                if (usedCells.Contains(target))
                {
                    return false;
                }

                if (!CanReachTarget(current, target))
                {
                    return false;
                }

                int availableDirections = GetAvailableDirectionMask(current, target);
                while (availableDirections != 0)
                {
                    int directionIndex =
                        GetBestDirectionIndex(current, target, availableDirections);
                    availableDirections &= ~(1 << directionIndex);

                    Vector2Int next = current + CardinalDirections[directionIndex];
                    path.Add(next);
                    usedCells.Add(next);

                    if (SearchConnector(patternIndex))
                    {
                        return true;
                    }

                    usedCells.Remove(next);
                    path.RemoveAt(path.Count - 1);

                    if (searchBudgetExceeded)
                    {
                        return false;
                    }
                }

                return false;
            }

            private bool AppendFixedPathAndContinue(int patternIndex)
            {
                IReadOnlyList<Vector2Int> fixedPath = patterns[patternIndex].FixedPath;
                int originalPathCount = path.Count;

                for (int index = 1; index < fixedPath.Count; index++)
                {
                    Vector2Int position = fixedPath[index];
                    if (!mapBounds.Contains(position) || usedCells.Contains(position))
                    {
                        RollBackPath(originalPathCount);
                        return false;
                    }

                    path.Add(position);
                    usedCells.Add(position);
                }

                if (SearchConnector(patternIndex + 1))
                {
                    return true;
                }

                RollBackPath(originalPathCount);
                return false;
            }

            private int GetAvailableDirectionMask(Vector2Int current, Vector2Int target)
            {
                int availableDirections = 0;

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    Vector2Int candidate = current + CardinalDirections[directionIndex];
                    if (!mapBounds.Contains(candidate) || usedCells.Contains(candidate))
                    {
                        continue;
                    }

                    if (candidate != target && reservedCells.Contains(candidate))
                    {
                        continue;
                    }

                    availableDirections |= 1 << directionIndex;
                }

                return availableDirections;
            }

            private static int GetBestDirectionIndex(
                Vector2Int current,
                Vector2Int target,
                int availableDirections)
            {
                int bestDirectionIndex = -1;
                long bestDistance = long.MaxValue;

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    if ((availableDirections & (1 << directionIndex)) == 0)
                    {
                        continue;
                    }

                    Vector2Int candidate = current + CardinalDirections[directionIndex];
                    long distance =
                        Math.Abs((long)target.x - candidate.x) +
                        Math.Abs((long)target.y - candidate.y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestDirectionIndex = directionIndex;
                    }
                }

                return bestDirectionIndex;
            }

            private bool CanReachTarget(Vector2Int start, Vector2Int target)
            {
                reachabilityVisitId++;
                if (reachabilityVisitId == int.MaxValue)
                {
                    Array.Clear(
                        reachabilityVisited,
                        0,
                        reachabilityVisited.Length);
                    reachabilityVisitId = 1;
                }

                int queueStart = 0;
                int queueEnd = 0;
                reachabilityQueue[queueEnd++] = start;
                reachabilityVisited[GetCellIndex(start)] = reachabilityVisitId;

                while (queueStart < queueEnd)
                {
                    Vector2Int current = reachabilityQueue[queueStart++];
                    for (int directionIndex = 0;
                         directionIndex < CardinalDirections.Length;
                         directionIndex++)
                    {
                        Vector2Int next = current + CardinalDirections[directionIndex];
                        if (!mapBounds.Contains(next))
                        {
                            continue;
                        }

                        if (usedCells.Contains(next))
                        {
                            continue;
                        }

                        if (next == target)
                        {
                            return true;
                        }

                        if (reservedCells.Contains(next))
                        {
                            continue;
                        }

                        int cellIndex = GetCellIndex(next);
                        if (reachabilityVisited[cellIndex] == reachabilityVisitId)
                        {
                            continue;
                        }

                        reachabilityVisited[cellIndex] = reachabilityVisitId;
                        reachabilityQueue[queueEnd++] = next;
                    }
                }

                return false;
            }

            private int GetCellIndex(Vector2Int position)
            {
                int localX = position.x - mapBounds.xMin;
                int localY = position.y - mapBounds.yMin;
                return localY * mapBounds.width + localX;
            }

            private void RollBackPath(int pathCount)
            {
                for (int index = path.Count - 1; index >= pathCount; index--)
                {
                    usedCells.Remove(path[index]);
                    path.RemoveAt(index);
                }
            }
        }
    }
}
