using System;
using System.Collections;
using System.Collections.Generic;
using DefCore.Gameplay.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace DefCore.Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity))]
    public class HostileTargetScanner : MonoBehaviour
    {
        private const string DefaultTargetLayerName = "Game Entity";
        private const int InitialOverlapBufferSize = 32;

        [SerializeField, Min(0f)] private float scanRadius = 1f;
        [SerializeField, Min(0f)] private float scanInterval = 0.5f;
        [SerializeField] private LayerMask targetLayerMask;

        public float ScanRadius => scanRadius;
        public float ScanInterval => scanInterval;

        public HostileTargetScanEvent OnScanCompleted = new();

        private readonly HashSet<Health> uniqueTargets = new();
        private readonly List<TargetCandidate> targetCandidates = new();

        private Entity sourceEntity;
        private Collider[] overlapBuffer = new Collider[InitialOverlapBufferSize];
        private Coroutine scanRoutine;
        private WaitForSeconds scanDelay;
        private int effectiveTargetLayerMask;
        private bool hasLoggedInvalidSourceTeam;
        private bool hasLoggedInvalidTargetLayer;

        private void Awake()
        {
            sourceEntity = GetComponent<Entity>();
            scanDelay = scanInterval > 0f ? new WaitForSeconds(scanInterval) : null;
        }

        public void Initialize(float radius, float interval)
        {
            scanRadius = radius;
            scanInterval = interval;
            scanDelay = scanInterval > 0f ? new WaitForSeconds(scanInterval) : null;
        }

        private void OnEnable()
        {
            ResolveTargetLayerMask();
            scanRoutine = StartCoroutine(ScanRoutine());
        }

        private void OnDisable()
        {
            if (scanRoutine == null)
            {
                return;
            }

            StopCoroutine(scanRoutine);
            scanRoutine = null;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                scanDelay = scanInterval > 0f ? new WaitForSeconds(scanInterval) : null;
                ResolveTargetLayerMask();
            }
        }

        private IEnumerator ScanRoutine()
        {
            // Spawned objects receive runtime references after Awake/OnEnable.
            // Waiting one frame also gives consumers time to subscribe to the event.
            yield return null;

            while (isActiveAndEnabled)
            {
                ScanAndPublish();

                if (scanInterval <= 0f)
                {
                    yield return null;
                }
                else
                {
                    yield return scanDelay;
                }
            }

            scanRoutine = null;
        }

        private void ScanAndPublish()
        {
            if (!TryGetScanContext(out Team sourceTeam))
            {
                PublishTargets(Array.Empty<Health>());
                return;
            }

            Vector3 scanOrigin = transform.position;
            float scanRadiusSqr = scanRadius * scanRadius;
            int overlapCount = CollectOverlaps(scanOrigin);

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
                if (damageCollider == null || !damageCollider.enabled || !damageCollider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (Attacker.GetTargetRejectReason(sourceTeam, target, true, out _, out _) != AttackRejectReason.None)
                {
                    continue;
                }

                float distanceSqr = target.GetDistanceSqrTo(scanOrigin);
                if (distanceSqr > scanRadiusSqr)
                {
                    continue;
                }

                targetCandidates.Add(new TargetCandidate(target, distanceSqr));
            }

            targetCandidates.Sort(CompareTargetCandidates);

            Health[] targets = targetCandidates.Count == 0 ? Array.Empty<Health>() : new Health[targetCandidates.Count];

            for (int i = 0; i < targetCandidates.Count; i++)
            {
                targets[i] = targetCandidates[i].Target;
            }

            PublishTargets(targets);
        }

        private bool TryGetScanContext(out Team sourceTeam)
        {
            sourceTeam = sourceEntity != null ? sourceEntity.Team : null;
            if (sourceTeam == null)
            {
                if (!hasLoggedInvalidSourceTeam)
                {
                    Debug.LogWarning($"[{name}] Hostile target scan requires a source Entity with a Team.", this);
                    hasLoggedInvalidSourceTeam = true;
                }

                return false;
            }

            if (effectiveTargetLayerMask != 0)
            {
                return true;
            }

            if (!hasLoggedInvalidTargetLayer)
            {
                Debug.LogWarning(
                    $"[{name}] Hostile target scan requires a Target Layer Mask or a layer named '{DefaultTargetLayerName}'.",
                    this);
                hasLoggedInvalidTargetLayer = true;
            }

            return false;
        }

        private int CollectOverlaps(Vector3 scanOrigin)
        {
            while (true)
            {
                int overlapCount = Physics.OverlapSphereNonAlloc(
                    scanOrigin,
                    scanRadius,
                    overlapBuffer,
                    effectiveTargetLayerMask,
                    QueryTriggerInteraction.Collide);

                if (overlapCount < overlapBuffer.Length)
                {
                    return overlapCount;
                }

                Array.Resize(ref overlapBuffer, overlapBuffer.Length * 2);
            }
        }

        private void PublishTargets(IReadOnlyList<Health> targets)
        {
            OnScanCompleted.Invoke(gameObject, new HostileTargetScanEventArgs(targets));
        }

        private void ResolveTargetLayerMask()
        {
            if (targetLayerMask.value != 0)
            {
                effectiveTargetLayerMask = targetLayerMask.value;
                return;
            }

            int defaultTargetLayer = LayerMask.NameToLayer(DefaultTargetLayerName);
            effectiveTargetLayerMask = defaultTargetLayer >= 0 ? 1 << defaultTargetLayer : 0;
        }

        private static int CompareTargetCandidates(TargetCandidate left, TargetCandidate right)
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

    [Serializable]
    public readonly struct HostileTargetScanEventArgs
    {
        public IReadOnlyList<Health> Targets { get; }

        public HostileTargetScanEventArgs(IReadOnlyList<Health> targets)
        {
            Targets = targets;
        }
    }

    [Serializable]
    public class HostileTargetScanEvent : UnityEvent<GameObject, HostileTargetScanEventArgs> { }
}
