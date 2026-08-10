using System;
using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerUnit))]
    public sealed class TowerPlacementPreviewSource : MonoBehaviour
    {
        private const int ExpectedActiveRendererCount = 2;

        [SerializeField] private Transform appearancePivot;
        [SerializeField] private GameObject liveVisualRoot;

        public Transform AppearancePivot => appearancePivot;
        public GameObject LiveVisualRoot => liveVisualRoot;

        private void Awake()
        {
            ValidateOrThrow();
        }

        public void ValidateOrThrow()
        {
            if (appearancePivot == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(TowerPlacementPreviewSource)} requires an appearance pivot.");
            }

            if (appearancePivot.parent != transform)
            {
                throw new InvalidOperationException(
                    $"[{name}] The placement-preview appearance pivot must be a direct child of the tower root.");
            }

            if (liveVisualRoot == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(TowerPlacementPreviewSource)} requires a live visual root.");
            }

            if (liveVisualRoot.transform.parent != appearancePivot)
            {
                throw new InvalidOperationException(
                    $"[{name}] The placement-preview live visual root must be a direct child of the appearance pivot.");
            }

            if (!liveVisualRoot.activeSelf)
            {
                throw new InvalidOperationException(
                    $"[{name}] The placement-preview live visual root must be active.");
            }

            SkinnedMeshRenderer[] renderers =
                liveVisualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int activeRendererCount = 0;
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer != null &&
                    renderer.enabled &&
                    IsActiveRelativeTo(liveVisualRoot.transform, renderer.transform))
                {
                    activeRendererCount++;
                }
            }

            if (activeRendererCount != ExpectedActiveRendererCount)
            {
                throw new InvalidOperationException(
                    $"[{name}] The live placement-preview visual must contain exactly " +
                    $"{ExpectedActiveRendererCount} active {nameof(SkinnedMeshRenderer)} components, " +
                    $"but found {activeRendererCount}.");
            }
        }

        internal static bool IsActiveRelativeTo(Transform root, Transform candidate)
        {
            Transform current = candidate;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                if (current == root)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}
