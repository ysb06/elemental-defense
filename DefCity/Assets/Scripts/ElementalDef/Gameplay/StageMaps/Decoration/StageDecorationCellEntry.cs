using System;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public readonly struct StageDecorationCellEntry :
        IEquatable<StageDecorationCellEntry>
    {
        public Vector2Int Coordinates { get; }
        public StageDecorationCellKind Kind { get; }
        public ElementType Element { get; }

        public StageDecorationCellEntry(
            Vector2Int coordinates,
            ElementType element)
            : this(
                coordinates,
                StageDecorationCellKind.ElementalGround,
                element)
        {
        }

        private StageDecorationCellEntry(
            Vector2Int coordinates,
            StageDecorationCellKind kind,
            ElementType element)
        {
            EnsureValid(kind, element);

            Coordinates = coordinates;
            Kind = kind;
            Element = element;
        }

        public static StageDecorationCellEntry CreateBoundaryWall(
            Vector2Int coordinates)
        {
            return new StageDecorationCellEntry(
                coordinates,
                StageDecorationCellKind.BoundaryWall,
                ElementType.Neutral);
        }

        public bool Equals(StageDecorationCellEntry other)
        {
            return Coordinates == other.Coordinates &&
                   Kind == other.Kind &&
                   Element == other.Element;
        }

        public override bool Equals(object obj)
        {
            return obj is StageDecorationCellEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int coordinateHash = Coordinates.x * 397 ^ Coordinates.y;
                int kindHash = coordinateHash * 397 ^ (int)Kind;
                return kindHash * 397 ^ (int)Element;
            }
        }

        public static bool operator ==(
            StageDecorationCellEntry left,
            StageDecorationCellEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            StageDecorationCellEntry left,
            StageDecorationCellEntry right)
        {
            return !left.Equals(right);
        }

        internal static bool IsSupportedElement(ElementType element)
        {
            return StageGroundElementTypes.IsSupported(element);
        }

        internal static bool IsSupportedGroundSource(StageMapCell cell)
        {
            return cell.Marker == StageCellMarker.None &&
                   (cell.Terrain == StageTerrainKind.Deployable ||
                    cell.Terrain == StageTerrainKind.Object) &&
                   IsSupportedElement(cell.Element);
        }

        internal static bool IsValid(
            StageDecorationCellKind kind,
            ElementType element)
        {
            if (!Enum.IsDefined(typeof(StageDecorationCellKind), kind) ||
                !Enum.IsDefined(typeof(ElementType), element))
            {
                return false;
            }

            return kind switch
            {
                StageDecorationCellKind.ElementalGround =>
                    IsSupportedElement(element),
                StageDecorationCellKind.BoundaryWall =>
                    element == ElementType.Neutral,
                _ => false,
            };
        }

        private static void EnsureValid(
            StageDecorationCellKind kind,
            ElementType element)
        {
            if (IsValid(kind, element))
            {
                return;
            }

            throw new ArgumentException(
                $"Decoration kind {kind} cannot use element {element}.");
        }
    }
}
