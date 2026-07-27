using System.Collections.Generic;
using UnityEngine;

namespace DefCore.Gameplay.Placement
{
    public class ColliderPlacementValidator : MonoBehaviour
    {
        private const int MaxOverlapCount = 64;

        [SerializeField] private LayerMask blockingLayerMask;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField, Min(0f)] private float boundsPadding;

        private readonly Collider[] overlapBuffer = new Collider[MaxOverlapCount];
        private readonly HashSet<Collider> sourceColliders = new();

        public LayerMask BlockingLayerMask
        {
            get => blockingLayerMask;
            set => blockingLayerMask = value;
        }

        public bool CanPlace(GameObject source, Vector3 position, Quaternion rotation, out string failureReason)
        {
            if (!TryGetPlacementBounds(source, position, rotation, out Bounds placementBounds, out failureReason))
            {
                return false;
            }

            int overlapCount = Physics.OverlapBoxNonAlloc(
                placementBounds.center,
                placementBounds.extents,
                overlapBuffer,
                Quaternion.identity,
                blockingLayerMask,
                triggerInteraction);

            for (int i = 0; i < overlapCount; i++)
            {
                Collider blockingCollider = overlapBuffer[i];
                if (blockingCollider == null || sourceColliders.Contains(blockingCollider))
                {
                    continue;
                }

                failureReason = $"Placement overlaps {blockingCollider.name}.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        public bool TryGetPlacementBounds(
            GameObject source,
            Vector3 position,
            Quaternion rotation,
            out Bounds placementBounds,
            out string failureReason)
        {
            placementBounds = default;
            sourceColliders.Clear();

            if (source == null)
            {
                failureReason = "No entity prefab is selected.";
                return false;
            }

            Collider[] colliders = source.GetComponentsInChildren<Collider>(false);
            Matrix4x4 targetRootMatrix = Matrix4x4.TRS(position, rotation, source.transform.localScale);
            if (!TryGetCombinedWorldBounds(
                    source.transform,
                    colliders,
                    targetRootMatrix,
                    out placementBounds,
                    out failureReason))
            {
                return false;
            }

            if (boundsPadding > 0f)
            {
                placementBounds.Expand(boundsPadding * 2f);
            }

            failureReason = string.Empty;
            return true;
        }

        private bool TryGetCombinedWorldBounds(
            Transform root,
            Collider[] colliders,
            Matrix4x4 targetRootMatrix,
            out Bounds worldBounds,
            out string failureReason)
        {
            worldBounds = default;
            bool hasBounds = false;
            Matrix4x4 worldToRootLocalMatrix = root.worldToLocalMatrix;

            foreach (Collider collider in colliders)
            {
                if (collider == null || !collider.enabled)
                {
                    continue;
                }

                sourceColliders.Add(collider);
                if (!TryGetColliderLocalBounds(collider, out Bounds colliderLocalBounds, out failureReason))
                {
                    return false;
                }

                if (colliderLocalBounds.size.sqrMagnitude <= Mathf.Epsilon)
                {
                    failureReason = $"{collider.name} ({collider.GetType().Name}) has zero-size placement bounds.";
                    return false;
                }

                Matrix4x4 colliderToTargetWorldMatrix =
                    targetRootMatrix * worldToRootLocalMatrix * collider.transform.localToWorldMatrix;
                Bounds colliderWorldBounds = TransformBounds(colliderToTargetWorldMatrix, colliderLocalBounds);

                if (!hasBounds)
                {
                    worldBounds = colliderWorldBounds;
                    hasBounds = true;
                    continue;
                }

                worldBounds.Encapsulate(colliderWorldBounds);
            }

            if (!hasBounds)
            {
                failureReason = $"{root.name} requires at least one active Collider for placement.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        private static bool TryGetColliderLocalBounds(
            Collider collider,
            out Bounds localBounds,
            out string failureReason)
        {
            switch (collider)
            {
                case BoxCollider boxCollider:
                    localBounds = new Bounds(boxCollider.center, boxCollider.size);
                    break;
                case SphereCollider sphereCollider:
                    float sphereDiameter = sphereCollider.radius * 2f;
                    localBounds = new Bounds(sphereCollider.center, Vector3.one * sphereDiameter);
                    break;
                case CapsuleCollider capsuleCollider:
                    localBounds = CreateCapsuleBounds(
                        capsuleCollider.center,
                        capsuleCollider.radius,
                        capsuleCollider.height,
                        capsuleCollider.direction);
                    break;
                case MeshCollider meshCollider when meshCollider.sharedMesh != null:
                    localBounds = meshCollider.sharedMesh.bounds;
                    break;
                case MeshCollider meshCollider:
                    localBounds = default;
                    failureReason = $"{meshCollider.name} ({nameof(MeshCollider)}) requires a shared Mesh for placement bounds.";
                    return false;
                case CharacterController characterController:
                    localBounds = CreateCapsuleBounds(
                        characterController.center,
                        characterController.radius,
                        characterController.height,
                        1);
                    break;
                default:
                    localBounds = default;
                    failureReason = $"{collider.name} uses unsupported placement Collider type {collider.GetType().Name}.";
                    return false;
            }

            failureReason = string.Empty;
            return true;
        }

        private static Bounds CreateCapsuleBounds(Vector3 center, float radius, float height, int direction)
        {
            float diameter = radius * 2f;
            float axisSize = Mathf.Max(height, diameter);
            Vector3 size = direction switch
            {
                0 => new Vector3(axisSize, diameter, diameter),
                1 => new Vector3(diameter, axisSize, diameter),
                2 => new Vector3(diameter, diameter, axisSize),
                _ => Vector3.zero,
            };

            return new Bounds(center, size);
        }

        private static Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Bounds transformedBounds = new(matrix.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z)), Vector3.zero);
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z)));
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z)));
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z)));
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z)));
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z)));
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z)));
            transformedBounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z)));

            return transformedBounds;
        }
    }
}
