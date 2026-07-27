using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.World
{
    [Serializable]
    public struct PresetPatternTile
    {
        public Vector2Int Position;
        public TileBase Tile;

        public PresetPatternTile(Vector2Int position, TileBase tile)
        {
            Position = position;
            Tile = tile;
        }
    }

    public class MapPattern
    {
        public IReadOnlyList<PresetPatternTile> Tiles;
        public IReadOnlyList<Vector2Int> FixedPath;
        public Vector2Int Entry => FixedPath[0];
        public Vector2Int Exit => FixedPath[FixedPath.Count - 1];

        public MapPattern(IEnumerable<PresetPatternTile> tiles, IEnumerable<Vector2Int> fixedPath)
        {
            Tiles = new List<PresetPatternTile>(tiles).AsReadOnly();
            FixedPath = new List<Vector2Int>(fixedPath).AsReadOnly();
        }
    }

    // ------------------------------ 이 아래로는 리팩토링 후 제거 대상 ------------------

    public enum PathSearchFailureReason
    {
        None,
        InvalidBounds,
        MapTooLarge,
        StartOutOfBounds,
        EndOutOfBounds,
        PatternOutOfBounds,
        PatternOverlap,
        PathNotFound,
        SearchBudgetExceeded
    }

    public sealed class PathSearchResult
    {
        private static readonly IReadOnlyList<Vector2Int> EmptyPath =
            Array.Empty<Vector2Int>();

        public bool Succeeded => FailureReason == PathSearchFailureReason.None;
        public PathSearchFailureReason FailureReason { get; }
        public IReadOnlyList<Vector2Int> Path { get; }

        private PathSearchResult(
            PathSearchFailureReason failureReason,
            IReadOnlyList<Vector2Int> path)
        {
            FailureReason = failureReason;
            Path = path;
        }

        internal static PathSearchResult Success(IEnumerable<Vector2Int> path)
        {
            return new PathSearchResult(
                PathSearchFailureReason.None,
                new List<Vector2Int>(path).AsReadOnly());
        }

        internal static PathSearchResult Failure(
            PathSearchFailureReason failureReason)
        {
            return new PathSearchResult(failureReason, EmptyPath);
        }
    }
}
