using System;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public readonly struct RouteNode : IEquatable<RouteNode>
    {
        public int Id { get; }
        public Vector2Int Cell { get; }

        public RouteNode(int id, Vector2Int cell)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    id,
                    "A route node ID cannot be negative.");
            }

            Id = id;
            Cell = cell;
        }

        public bool Equals(RouteNode other)
        {
            return Id == other.Id && Cell == other.Cell;
        }

        public override bool Equals(object obj)
        {
            return obj is RouteNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Cell);
        }

        public static bool operator ==(RouteNode left, RouteNode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RouteNode left, RouteNode right)
        {
            return !left.Equals(right);
        }
    }
}
