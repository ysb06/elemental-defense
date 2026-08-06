using System;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public readonly struct StageMapCellEntry : IEquatable<StageMapCellEntry>
    {
        public Vector2Int Coordinates { get; }
        public StageMapCell Cell { get; }

        public StageMapCellEntry(Vector2Int coordinates, StageMapCell cell)
        {
            Coordinates = coordinates;
            Cell = cell;
        }

        public bool Equals(StageMapCellEntry other)
        {
            return Coordinates == other.Coordinates && Cell == other.Cell;
        }

        public override bool Equals(object obj)
        {
            return obj is StageMapCellEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coordinates, Cell);
        }
    }
}
