using System;
using DefCore.Gameplay.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.World
{
    public class Tile3DCellManager : CellSpace
    {
        private const float RaycastPadding = 0.01f;

        [SerializeField] private Tilemap groundTilemap;

        public override Vector3 GetSurfaceCenter(Vector3Int coordinates)
        {
            if (TryGetSurfaceCenter(coordinates, out Vector3 worldSurfaceCenter))
            {
                return worldSurfaceCenter;
            }

            throw new InvalidOperationException(
                $"{nameof(Tile3DCellManager)} '{name}' failed to resolve the surface center for cell {coordinates}.");
        }

        protected override bool ContainsCell(Vector3Int refCoordinates)
        {
            return groundTilemap != null && groundTilemap.HasTile(refCoordinates);
        }

        public GameObject GetTileInstance(Vector3Int coordinates)
        {
            if (groundTilemap == null)
            {
                throw new InvalidOperationException($"{nameof(Tile3DCellManager)} '{name}' has no assigned Tilemap.");
            }

            return groundTilemap.GetInstantiatedObject(coordinates);
        }

        public override bool TryGetSurfaceCenter(Vector3Int coordinates, out Vector3 worldSurfaceCenter)
        {
            worldSurfaceCenter = default;

            GameObject tileInstance = GetTileInstance(coordinates);
            if (tileInstance == null)
            {
                Debug.LogWarning("The tile does not instantiate a GameObject.");
                return false;
            }

            Collider[] colliders = tileInstance.GetComponentsInChildren<Collider>();
            Vector3 gridNormal = grid.transform.up.normalized;
            Vector3 absoluteGridNormal = new Vector3(
                Mathf.Abs(gridNormal.x),
                Mathf.Abs(gridNormal.y),
                Mathf.Abs(gridNormal.z));

            bool hasActiveCollider = false;
            float highestProjection = float.NegativeInfinity;
            float lowestProjection = float.PositiveInfinity;

            foreach (Collider collider in colliders)
            {
                if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                hasActiveCollider = true;

                Bounds bounds = collider.bounds;
                float projectedExtent = Vector3.Dot(bounds.extents, absoluteGridNormal);
                float centerProjection = Vector3.Dot(bounds.center, gridNormal);
                highestProjection = Mathf.Max(highestProjection, centerProjection + projectedExtent);
                lowestProjection = Mathf.Min(lowestProjection, centerProjection - projectedExtent);
            }

            if (!hasActiveCollider)
            {
                Debug.LogWarning("The tile instance has no active, enabled Collider.");
                return false;
            }

            Vector3 worldCellCenter = grid.GetCellCenterWorld(coordinates);
            float cellCenterProjection = Vector3.Dot(worldCellCenter, gridNormal);
            Vector3 rayOrigin = worldCellCenter + gridNormal * (highestProjection - cellCenterProjection + RaycastPadding);
            Ray ray = new(rayOrigin, -gridNormal);
            float raycastDistance = highestProjection - lowestProjection + RaycastPadding * 2f;

            bool hasHit = false;
            float nearestHitDistance = float.PositiveInfinity;

            foreach (Collider collider in colliders)
            {
                if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (collider.Raycast(ray, out RaycastHit hit, raycastDistance)
                    && hit.distance < nearestHitDistance)
                {
                    hasHit = true;
                    nearestHitDistance = hit.distance;
                    worldSurfaceCenter = hit.point;
                }
            }

            if (!hasHit)
            {
                Debug.LogWarning("No tile Collider intersects the vertical ray through the cell center.");
                return false;
            }

            return true;
        }
    }
}
