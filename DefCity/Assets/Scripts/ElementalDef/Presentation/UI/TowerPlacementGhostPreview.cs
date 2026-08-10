using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class TowerPlacementGhostPreview : MonoBehaviour
    {
        private const int ExpectedActiveRendererCount = 2;

        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;
        [SerializeField] private Material outlineMaterial;

        private TowerPlacementPreviewSource currentSource;
        private GameObject ghostRoot;
        private SkinnedMeshRenderer[] previewRenderers = Array.Empty<SkinnedMeshRenderer>();
        private Animator[] previewAnimators = Array.Empty<Animator>();
        private ParticleSystem[] previewParticles = Array.Empty<ParticleSystem>();
        private bool? currentValidity;

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnDisable()
        {
            Clear();
        }

        public void SetTarget(TowerUnit target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!target.TryGetComponent(out TowerPlacementPreviewSource source))
            {
                throw new InvalidOperationException(
                    $"[{target.name}] Tower placement preview requires a " +
                    $"{nameof(TowerPlacementPreviewSource)} component.");
            }

            source.ValidateOrThrow();
            if (source == currentSource && ghostRoot != null)
            {
                return;
            }

            Clear();
            currentSource = source;

            try
            {
                CreateGhost(source);
            }
            catch
            {
                Clear();
                throw;
            }
        }

        public void Show(Pose pose, bool canPlace)
        {
            if (ghostRoot == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] A tower placement-preview target must be set before showing it.");
            }

            ghostRoot.transform.SetPositionAndRotation(pose.position, pose.rotation);
            if (!ghostRoot.activeSelf)
            {
                ghostRoot.SetActive(true);
                FreezePreviewAnimation();
                StopPreviewParticles();
            }

            if (currentValidity != canPlace)
            {
                currentValidity = canPlace;
                ApplyPreviewMaterial(canPlace);
            }
        }

        public void Hide()
        {
            if (ghostRoot != null)
            {
                ghostRoot.SetActive(false);
            }
        }

        public void Clear()
        {
            currentSource = null;
            currentValidity = null;
            previewRenderers = Array.Empty<SkinnedMeshRenderer>();
            previewAnimators = Array.Empty<Animator>();
            previewParticles = Array.Empty<ParticleSystem>();

            if (ghostRoot == null)
            {
                return;
            }

            ghostRoot.SetActive(false);
            DestroyUnityObject(ghostRoot);
            ghostRoot = null;
        }

        private void CreateGhost(TowerPlacementPreviewSource source)
        {
            ghostRoot = new GameObject($"{source.name} Placement Ghost")
            {
                hideFlags = HideFlags.DontSave,
            };
            ghostRoot.transform.SetParent(transform, false);
            ghostRoot.SetActive(false);

            GameObject pivotObject = new($"{source.AppearancePivot.name} Ghost Pivot");
            pivotObject.transform.SetParent(ghostRoot.transform, false);
            pivotObject.transform.localPosition = source.AppearancePivot.localPosition;
            pivotObject.transform.localRotation = source.AppearancePivot.localRotation;
            pivotObject.transform.localScale = source.AppearancePivot.localScale;

            GameObject visualClone = Instantiate(
                source.LiveVisualRoot,
                pivotObject.transform,
                false);
            visualClone.name = $"{source.LiveVisualRoot.name} Ghost Visual";

            DisableGameplayParticipation(visualClone);
            CachePreviewComponents(visualClone);
            SetLayerRecursively(ghostRoot, GetPreviewLayer());
            ApplyPreviewMaterial(true);
            currentValidity = true;
        }

        private void CachePreviewComponents(GameObject visualRoot)
        {
            List<SkinnedMeshRenderer> activeRenderers = new();
            SkinnedMeshRenderer[] renderers =
                visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer == null ||
                    !renderer.enabled ||
                    !TowerPlacementPreviewSource.IsActiveRelativeTo(
                        visualRoot.transform,
                        renderer.transform))
                {
                    continue;
                }

                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                activeRenderers.Add(renderer);
            }

            if (activeRenderers.Count != ExpectedActiveRendererCount)
            {
                throw new InvalidOperationException(
                    $"[{name}] The cloned placement-preview visual must contain exactly " +
                    $"{ExpectedActiveRendererCount} active {nameof(SkinnedMeshRenderer)} components, " +
                    $"but found {activeRenderers.Count}.");
            }

            previewRenderers = activeRenderers.ToArray();
            previewAnimators = visualRoot.GetComponentsInChildren<Animator>(true);
            previewParticles = visualRoot.GetComponentsInChildren<ParticleSystem>(true);
        }

        private void DisableGameplayParticipation(GameObject visualRoot)
        {
            foreach (Collider collider in visualRoot.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            foreach (MonoBehaviour behaviour in visualRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled = false;
            }

            foreach (AudioSource audioSource in visualRoot.GetComponentsInChildren<AudioSource>(true))
            {
                audioSource.enabled = false;
            }

            foreach (Canvas canvas in visualRoot.GetComponentsInChildren<Canvas>(true))
            {
                canvas.enabled = false;
            }

            foreach (Light light in visualRoot.GetComponentsInChildren<Light>(true))
            {
                light.enabled = false;
            }

            foreach (ParticleSystem particleSystem in visualRoot.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.playOnAwake = false;
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Clear(true);
            }

            foreach (Renderer renderer in visualRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is not SkinnedMeshRenderer)
                {
                    renderer.enabled = false;
                }
            }
        }

        private void FreezePreviewAnimation()
        {
            foreach (Animator animator in previewAnimators)
            {
                if (animator == null || !animator.enabled)
                {
                    continue;
                }

                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.speed = 1f;
                animator.Rebind();
                animator.Update(0f);
                animator.speed = 0f;
            }
        }

        private void StopPreviewParticles()
        {
            foreach (ParticleSystem particleSystem in previewParticles)
            {
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Clear(true);
            }
        }

        private void ApplyPreviewMaterial(bool canPlace)
        {
            Material previewMaterial = canPlace
                ? validPreviewMaterial
                : invalidPreviewMaterial;

            foreach (SkinnedMeshRenderer renderer in previewRenderers)
            {
                Material[] materials = renderer.sharedMaterials;
                bool replacedMaterial = false;
                bool preservedOutline = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == outlineMaterial)
                    {
                        preservedOutline = true;
                        continue;
                    }

                    materials[i] = previewMaterial;
                    replacedMaterial = true;
                }

                if (!preservedOutline)
                {
                    throw new InvalidOperationException(
                        $"[{name}] {renderer.name} has no configured outline material slot for the placement preview.");
                }

                if (!replacedMaterial)
                {
                    throw new InvalidOperationException(
                        $"[{name}] {renderer.name} has no non-outline material slot for the placement preview.");
                }

                renderer.sharedMaterials = materials;
            }
        }

        private void EnsureConfigured()
        {
            if (validPreviewMaterial == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementGhostPreview)} requires a valid preview material.");
            }

            if (invalidPreviewMaterial == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementGhostPreview)} requires an invalid preview material.");
            }

            if (outlineMaterial == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementGhostPreview)} requires an outline material.");
            }

            if (validPreviewMaterial == invalidPreviewMaterial ||
                validPreviewMaterial == outlineMaterial ||
                invalidPreviewMaterial == outlineMaterial)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerPlacementGhostPreview)} requires three distinct materials.");
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static int GetPreviewLayer()
        {
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            return ignoreRaycastLayer >= 0 ? ignoreRaycastLayer : 2;
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
