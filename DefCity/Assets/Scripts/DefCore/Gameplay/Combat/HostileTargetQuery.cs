using System;
using System.Collections.Generic;
using DefCore.Gameplay.Entities;
using UnityEngine;

namespace DefCore.Gameplay.Combat
{
    public sealed class HostileTargetQuery
    {
        public const string DefaultTargetLayerName = "Game Entity";

        private const int InitialOverlapBufferSize = 32;

        private readonly HashSet<Health> uniqueTargets = new();
        private readonly List<TargetCandidate> targetCandidates = new();
        private Collider[] overlapBuffer = new Collider[InitialOverlapBufferSize];

        public IReadOnlyList<Health> FindTargets(
            Vector3 origin,
            float radius,
            Team sourceTeam,
            int targetLayerMask)
        {
            float radiusSqr = radius * radius;
            int overlapCount = CollectOverlaps(origin, radius, targetLayerMask);

            uniqueTargets.Clear();
            targetCandidates.Clear();

            for (int i = 0; i < overlapCount; i++)
            {
                Collider overlap = overlapBuffer[i];
                if (overlap == null)
                {
                    continue;
                }

                Health target = overlap.GetComponentInParent<Health>();
                if (target == null || !uniqueTargets.Add(target))
                {
                    continue;
                }

                Collider damageCollider = target.DamageCollider;
                if (damageCollider == null ||
                    !damageCollider.enabled ||
                    !damageCollider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (Attacker.GetTargetRejectReason(
                        sourceTeam,
                        target,
                        true,
                        out _,
                        out _) != AttackRejectReason.None)
                {
                    continue;
                }

                float distanceSqr = target.GetDistanceSqrTo(origin);
                if (distanceSqr > radiusSqr)
                {
                    continue;
                }

                targetCandidates.Add(new TargetCandidate(target, distanceSqr));
            }

            targetCandidates.Sort(CompareTargetCandidates);

            if (targetCandidates.Count == 0)
            {
                return Array.Empty<Health>();
            }

            Health[] targets = new Health[targetCandidates.Count];
            for (int i = 0; i < targetCandidates.Count; i++)
            {
                targets[i] = targetCandidates[i].Target;
            }

            return targets;
        }

        public static int ResolveTargetLayerMask(LayerMask configuredMask)
        {
            if (configuredMask.value != 0)
            {
                return configuredMask.value;
            }

            int defaultTargetLayer = LayerMask.NameToLayer(DefaultTargetLayerName);
            return defaultTargetLayer >= 0 ? 1 << defaultTargetLayer : 0;
        }

        private int CollectOverlaps(Vector3 origin, float radius, int targetLayerMask)
        {
            while (true)
            {
                int overlapCount = Physics.OverlapSphereNonAlloc(
                    origin,
                    radius,
                    overlapBuffer,
                    targetLayerMask,
                    QueryTriggerInteraction.Collide);

                if (overlapCount < overlapBuffer.Length)
                {
                    return overlapCount;
                }

                Array.Resize(ref overlapBuffer, overlapBuffer.Length * 2);
            }
        }

        private static int CompareTargetCandidates(
            TargetCandidate left,
            TargetCandidate right)
        {
            int distanceComparison = left.DistanceSqr.CompareTo(right.DistanceSqr);
            return distanceComparison != 0
                ? distanceComparison
                : left.InstanceId.CompareTo(right.InstanceId);
        }

        private readonly struct TargetCandidate
        {
            public Health Target { get; }
            public float DistanceSqr { get; }
            public int InstanceId { get; }

            public TargetCandidate(Health target, float distanceSqr)
            {
                Target = target;
                DistanceSqr = distanceSqr;
                InstanceId = target.GetInstanceID();
            }
        }
    }
}
