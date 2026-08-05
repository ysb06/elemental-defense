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
        [SerializeField, Min(0f)] private float scanRadius = 1f;
        [SerializeField, Min(0f)] private float scanInterval = 0.5f;
        [SerializeField] private LayerMask targetLayerMask;

        public float ScanRadius => scanRadius;
        public float ScanInterval => scanInterval;

        public HostileTargetScanEvent OnScanCompleted = new();

        private readonly HostileTargetQuery targetQuery = new();

        private Entity sourceEntity;
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

            IReadOnlyList<Health> targets = targetQuery.FindTargets(
                transform.position,
                scanRadius,
                sourceTeam,
                effectiveTargetLayerMask);
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
                    $"[{name}] Hostile target scan requires a Target Layer Mask or a layer named " +
                    $"'{HostileTargetQuery.DefaultTargetLayerName}'.",
                    this);
                hasLoggedInvalidTargetLayer = true;
            }

            return false;
        }

        private void PublishTargets(IReadOnlyList<Health> targets)
        {
            OnScanCompleted.Invoke(gameObject, new HostileTargetScanEventArgs(targets));
        }

        private void ResolveTargetLayerMask()
        {
            effectiveTargetLayerMask =
                HostileTargetQuery.ResolveTargetLayerMask(targetLayerMask);
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
