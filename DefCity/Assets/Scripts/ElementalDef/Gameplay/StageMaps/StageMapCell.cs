using System;
using ElementalDef.Gameplay.Combat;

namespace ElementalDef.Gameplay.StageMaps
{
    public enum StageTerrainKind
    {
        Unspecified = 0,
        Road = 1,
        Deployable = 2,
        Object = 3,
    }

    public enum StageCellMarker
    {
        None = 0,
        Spawn = 1,
        Headquarters = 2,
        RouteGoal = 3,
    }

    public readonly struct StageMapCell : IEquatable<StageMapCell>
    {
        public StageTerrainKind Terrain { get; }
        public ElementType Element { get; }
        public StageCellMarker Marker { get; }

        public bool IsDeployable => Terrain == StageTerrainKind.Deployable;
        public bool IsRouteCell => Terrain == StageTerrainKind.Road;
        public bool IsDefined => Terrain != StageTerrainKind.Unspecified;

        public StageMapCell(
            StageTerrainKind terrain,
            ElementType element,
            StageCellMarker marker = StageCellMarker.None)
        {
            EnsureDefinedEnum(terrain, nameof(terrain));
            EnsureDefinedEnum(element, nameof(element));
            EnsureDefinedEnum(marker, nameof(marker));

            if (terrain == StageTerrainKind.Unspecified)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(terrain),
                    terrain,
                    "A finalized stage map cell requires a concrete terrain kind.");
            }

            switch (terrain)
            {
                case StageTerrainKind.Road:
                    if (element != ElementType.Neutral)
                    {
                        throw new ArgumentException(
                            "Road cells must use the Neutral element.",
                            nameof(element));
                    }

                    if (marker != StageCellMarker.None &&
                        marker != StageCellMarker.Spawn &&
                        marker != StageCellMarker.RouteGoal)
                    {
                        throw new ArgumentException(
                            "Road cells may only use Spawn or RouteGoal markers.",
                            nameof(marker));
                    }

                    break;

                case StageTerrainKind.Deployable:
                    if (element != ElementType.Water &&
                        element != ElementType.Fire &&
                        element != ElementType.Earth)
                    {
                        throw new ArgumentException(
                            "Deployable cells require Water, Fire, or Earth.",
                            nameof(element));
                    }

                    if (marker != StageCellMarker.None)
                    {
                        throw new ArgumentException(
                            "Deployable cells cannot carry stage markers.",
                            nameof(marker));
                    }

                    break;

                case StageTerrainKind.Object:
                    if (marker == StageCellMarker.Headquarters &&
                        element != ElementType.Neutral)
                    {
                        throw new ArgumentException(
                            "The Headquarters Object cell must use the Neutral element.",
                            nameof(element));
                    }

                    if (marker != StageCellMarker.None &&
                        marker != StageCellMarker.Headquarters)
                    {
                        throw new ArgumentException(
                            "Object cells may only use the Headquarters marker.",
                            nameof(marker));
                    }

                    if (marker == StageCellMarker.None &&
                        element != ElementType.Neutral &&
                        element != ElementType.Water &&
                        element != ElementType.Fire &&
                        element != ElementType.Earth)
                    {
                        throw new ArgumentException(
                            "Object cells require Neutral, Water, Fire, or Earth.",
                            nameof(element));
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(terrain), terrain, null);
            }

            Terrain = terrain;
            Element = element;
            Marker = marker;
        }

        public bool Equals(StageMapCell other)
        {
            return Terrain == other.Terrain &&
                   Element == other.Element &&
                   Marker == other.Marker;
        }

        public override bool Equals(object obj)
        {
            return obj is StageMapCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Terrain, Element, Marker);
        }

        public static bool operator ==(StageMapCell left, StageMapCell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StageMapCell left, StageMapCell right)
        {
            return !left.Equals(right);
        }

        private static void EnsureDefinedEnum<TEnum>(TEnum value, string parameterName)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"{typeof(TEnum).Name} must be a defined value.");
            }
        }
    }
}
