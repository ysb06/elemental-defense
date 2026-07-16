using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using DefCity.Gameplay.World;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DefCity.Gameplay.Flow
{
    public class SpawnPlace : MonoBehaviour
    {
        [SerializeField] private TerrainCellManager terrainCellManager;
        [SerializeField, Min(0f)] private float range = 5f;
        [SerializeField, Min(0f)] private float excludedRange;
        [SerializeField] private Vector2 excludedRangeLocalOffset;
        [SerializeField, Min(0f)] private float maxHeightDifference = 1f;
        [SerializeField] private bool calculateOnStart;

        private static readonly Color SpawnRangeGizmoColor = new(0f, 0.85f, 1f, 0.85f);
        private static readonly Color ExcludedRangeGizmoColor = new(1f, 0.35f, 0.05f, 0.85f);
        private static readonly Color SpawnableCellGizmoColor = new(0.15f, 1f, 0.25f, 0.85f);

        private readonly List<TerrainCell> spawnableCells = new();
        private ReadOnlyCollection<TerrainCell> readOnlySpawnableCells;

        public IReadOnlyList<TerrainCell> SpawnableCells
        {
            get
            {
                readOnlySpawnableCells ??= spawnableCells.AsReadOnly();
                return readOnlySpawnableCells;
            }
        }

        public float Range
        {
            get => range;
            set => range = ValidateNonNegative(value, nameof(Range));
        }

        public float ExcludedRange
        {
            get => excludedRange;
            set => excludedRange = ValidateNonNegative(value, nameof(ExcludedRange));
        }

        public Vector2 ExcludedRangeLocalOffset
        {
            get => excludedRangeLocalOffset;
            set => excludedRangeLocalOffset = value;
        }

        public float MaxHeightDifference
        {
            get => maxHeightDifference;
            set => maxHeightDifference = ValidateNonNegative(value, nameof(MaxHeightDifference));
        }

        public bool CalculateOnStart
        {
            get => calculateOnStart;
            set => calculateOnStart = value;
        }

        private void Start()
        {
            if (calculateOnStart)
            {
                RecalculateSpawnableCells();
            }
        }

        private void OnValidate()
        {
            range = Mathf.Max(0f, range);
            excludedRange = Mathf.Max(0f, excludedRange);
            maxHeightDifference = Mathf.Max(0f, maxHeightDifference);
        }

        [ContextMenu("Recalculate Spawnable Cells")]
        public void RecalculateSpawnableCells()
        {
            RecalculateSpawnableCellsInternal(null, BuildSerializedRangeExclusion());
        }

        public void RecalculateSpawnableCellsExcept(IEnumerable<TerrainCell> excludedCells)
        {
            if (excludedCells == null)
            {
                throw new ArgumentNullException(nameof(excludedCells));
            }

            HashSet<Vector3Int> excludedCellPositions = new();
            foreach (TerrainCell excludedCell in excludedCells)
            {
                excludedCellPositions.Add(excludedCell.RefPosition);
            }

            RecalculateSpawnableCellsInternal(excludedCellPositions, null);
        }

        public void RecalculateSpawnableCellsExceptRange(float excludedRange)
        {
            RecalculateSpawnableCellsExceptRange(transform.position, excludedRange);
        }

        public void RecalculateSpawnableCellsExceptRange(Vector3 excludedCenter, float excludedRange)
        {
            RangeExclusion rangeExclusion = new(excludedCenter, ValidateNonNegative(excludedRange, nameof(excludedRange)));
            RecalculateSpawnableCellsInternal(null, rangeExclusion);
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            DrawGizmos();
#endif
        }

        private void RecalculateSpawnableCellsInternal(
            HashSet<Vector3Int> excludedCellPositions,
            RangeExclusion? rangeExclusion)
        {
            if (terrainCellManager == null)
            {
                throw new InvalidOperationException($"{name} requires a TerrainCellManager.");
            }

            spawnableCells.Clear();
            Vector3 center = transform.position;
            float referenceY = center.y;

            foreach (TerrainCell cell in terrainCellManager.EnumerateTerrainCellsInRange(center, range))
            {
                if (excludedCellPositions != null && excludedCellPositions.Contains(cell.RefPosition))
                {
                    continue;
                }

                if (rangeExclusion.HasValue && rangeExclusion.Value.Contains(cell.Center))
                {
                    continue;
                }

                if (Mathf.Abs(cell.AverageWorldHeight - referenceY) > maxHeightDifference)
                {
                    continue;
                }

                spawnableCells.Add(cell);
            }
        }

        private RangeExclusion? BuildSerializedRangeExclusion()
        {
            if (excludedRange <= 0f)
            {
                return null;
            }

            return new RangeExclusion(GetExcludedRangeCenter(), excludedRange);
        }

        private Vector3 GetExcludedRangeCenter()
        {
            Vector3 localOffset = new(excludedRangeLocalOffset.x, 0f, excludedRangeLocalOffset.y);
            return transform.position + transform.TransformDirection(localOffset);
        }

        private static float ValidateNonNegative(float value, string parameterName)
        {
            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be non-negative.");
            }

            return value;
        }

        private void DrawGizmos()
        {
            Vector3 center = transform.position;

            Handles.color = SpawnRangeGizmoColor;
            Handles.DrawWireDisc(center, Vector3.up, range, 2f);

            if (excludedRange > 0f)
            {
                Handles.color = ExcludedRangeGizmoColor;
                Handles.DrawWireDisc(GetExcludedRangeCenter(), Vector3.up, excludedRange, 2f);
            }

            Gizmos.color = SpawnableCellGizmoColor;
            foreach (TerrainCell cell in spawnableCells)
            {
                DrawCellGizmo(cell);
            }
        }

        private static void DrawCellGizmo(TerrainCell cell)
        {
            Vector3[] corners = cell.CornerWorldPositions;
            if (corners == null || corners.Length < 4)
            {
                return;
            }

            Vector3 center = cell.Center;
            center.y = cell.AverageWorldHeight;
            float markerRadius = Mathf.Max(0.15f, GetCellMarkerRadius(corners));

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[(i + 1) % corners.Length];
                start.y = center.y;
                end.y = center.y;
                Gizmos.DrawLine(start, end);
            }

            Gizmos.DrawWireSphere(center, markerRadius);
        }

        private static float GetCellMarkerRadius(IReadOnlyList<Vector3> corners)
        {
            if (corners.Count < 2)
            {
                return 0.15f;
            }

            return Vector3.Distance(corners[0], corners[1]) * 0.1f;
        }

        private readonly struct RangeExclusion
        {
            private readonly Vector3 center;
            private readonly float range;

            public RangeExclusion(Vector3 center, float range)
            {
                this.center = center;
                this.range = range;
            }

            public bool Contains(Vector3 position)
            {
                float deltaX = position.x - center.x;
                float deltaZ = position.z - center.z;
                return (deltaX * deltaX) + (deltaZ * deltaZ) <= (range * range) + Mathf.Epsilon;
            }
        }
    }
}
