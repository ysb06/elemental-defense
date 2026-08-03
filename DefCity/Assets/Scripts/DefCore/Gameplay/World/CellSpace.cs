using System;
using UnityEngine;

namespace DefCore.Gameplay.World
{
    public readonly struct CellRef : IEquatable<CellRef>
    {
        public CellSpace Space { get; }
        public Vector3Int RefCoordinates { get; }   // 유니티 시스템 호환용 (e.g., Grid.CellToWorld)
        public Vector2Int Coordinates { get; }
        public Vector3 SurfaceCenter => Space.GetSurfaceCenter(Coordinates);
        public bool IsValid => Space != null;

        public CellRef(CellSpace space, Vector3Int refCoordinates)
        {
            Space = space;
            RefCoordinates = refCoordinates;
            Coordinates = new Vector2Int(refCoordinates.x, refCoordinates.y);
        }

        public bool Equals(CellRef other)
        {
            return Space == other.Space && Coordinates == other.Coordinates;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Space, Coordinates);
        }
    }

    public abstract class CellSpace : MonoBehaviour
    {
        [SerializeField] protected Grid grid;

        public bool TryGetCell(Vector3 worldPosition, out CellRef cell)
        {
            Vector3Int cellPosition = grid.WorldToCell(worldPosition);
            return TryGetCell(cellPosition, out cell);
        }
        public bool TryGetCell(Vector2Int coordinates, out CellRef cell)
        {
            return TryGetCell(new Vector3Int(coordinates.x, coordinates.y, 0), out cell);
        }
        protected bool TryGetCell(Vector3Int refCoordinates, out CellRef cell)
        {
            if (ContainsCell(refCoordinates))
            {
                cell = new CellRef(this, refCoordinates);
                return true;
            }
            else
            {
                cell = default;
                return false;
            }
        }
        protected abstract bool ContainsCell(Vector3Int refCoordinates);

        public Vector3 GetSurfaceCenter(Vector2Int coordinates)
        {
            return GetSurfaceCenter(new Vector3Int(coordinates.x, coordinates.y, 0));
        }
        public bool TryGetSurfaceCenter(Vector2Int coordinates, out Vector3 worldSurfaceCenter)
        {
            return TryGetSurfaceCenter(new Vector3Int(coordinates.x, coordinates.y, 0), out worldSurfaceCenter);
        }
        public abstract Vector3 GetSurfaceCenter(Vector3Int coordinates);
        public abstract bool TryGetSurfaceCenter(Vector3Int coordinates, out Vector3 worldSurfaceCenter);
    }
}