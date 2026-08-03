using System;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public readonly struct RouteCrossingDefinition : IEquatable<RouteCrossingDefinition>
    {
        public Vector2Int Cell { get; }
        public int HorizontalNodeId { get; }
        public int VerticalNodeId { get; }

        public RouteCrossingDefinition(
            Vector2Int cell,
            int horizontalNodeId,
            int verticalNodeId)
        {
            if (horizontalNodeId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(horizontalNodeId),
                    horizontalNodeId,
                    "A crossing route node ID cannot be negative.");
            }

            if (verticalNodeId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(verticalNodeId),
                    verticalNodeId,
                    "A crossing route node ID cannot be negative.");
            }

            if (horizontalNodeId == verticalNodeId)
            {
                throw new ArgumentException(
                    "A disconnected crossing requires two different route nodes.");
            }

            Cell = cell;
            HorizontalNodeId = horizontalNodeId;
            VerticalNodeId = verticalNodeId;
        }

        public bool Equals(RouteCrossingDefinition other)
        {
            return Cell == other.Cell &&
                   HorizontalNodeId == other.HorizontalNodeId &&
                   VerticalNodeId == other.VerticalNodeId;
        }

        public override bool Equals(object obj)
        {
            return obj is RouteCrossingDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Cell, HorizontalNodeId, VerticalNodeId);
        }

        public static bool operator ==(
            RouteCrossingDefinition left,
            RouteCrossingDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            RouteCrossingDefinition left,
            RouteCrossingDefinition right)
        {
            return !left.Equals(right);
        }
    }
}
