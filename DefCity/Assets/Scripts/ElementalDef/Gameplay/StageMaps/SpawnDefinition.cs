using System;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public readonly struct SpawnDefinition : IEquatable<SpawnDefinition>
    {
        public string Id { get; }
        public Vector2Int Cell { get; }
        public int StartNodeId { get; }

        public SpawnDefinition(string id, Vector2Int cell, int startNodeId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A spawn ID is required.", nameof(id));
            }

            if (startNodeId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startNodeId),
                    startNodeId,
                    "A route node ID cannot be negative.");
            }

            Id = id;
            Cell = cell;
            StartNodeId = startNodeId;
        }

        public bool Equals(SpawnDefinition other)
        {
            return string.Equals(Id, other.Id, StringComparison.Ordinal) &&
                   Cell == other.Cell &&
                   StartNodeId == other.StartNodeId;
        }

        public override bool Equals(object obj)
        {
            return obj is SpawnDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id != null ? StringComparer.Ordinal.GetHashCode(Id) : 0,
                Cell,
                StartNodeId);
        }

        public static bool operator ==(SpawnDefinition left, SpawnDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpawnDefinition left, SpawnDefinition right)
        {
            return !left.Equals(right);
        }
    }
}
