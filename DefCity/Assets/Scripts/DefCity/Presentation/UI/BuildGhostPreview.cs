using UnityEngine;
using UnityEngine.AI;
using DefCity.Gameplay.Entities;

namespace DefCity.Presentation.UI
{
    public class BuildGhostPreview : MonoBehaviour
    {
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        private Entity currentTarget;
        private GameObject ghostInstance;
        private Material fallbackValidMaterial;
        private Material fallbackInvalidMaterial;
        private bool isValid = true;

        public void SetTarget(Entity target)
        {
            if (target == currentTarget && ghostInstance != null)
            {
                return;
            }

            Clear();
            currentTarget = target;
            if (currentTarget == null)
            {
                return;
            }

            CreateGhost(currentTarget);
            ApplyPreviewMaterial();
        }

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            if (ghostInstance == null)
            {
                return;
            }

            ghostInstance.transform.SetPositionAndRotation(position, rotation);
            if (!ghostInstance.activeSelf)
            {
                ghostInstance.SetActive(true);
            }

            EnsureVisibleRenderer(ghostInstance);
        }

        public void SetValid(bool isValid)
        {
            if (this.isValid == isValid)
            {
                return;
            }

            this.isValid = isValid;
            ApplyPreviewMaterial();
        }

        public void Clear()
        {
            currentTarget = null;
            DestroyUnityObject(ghostInstance);
            ghostInstance = null;
        }

        private void OnDisable()
        {
            Clear();
        }

        private void OnDestroy()
        {
            Clear();
            DestroyUnityObject(fallbackValidMaterial);
            DestroyUnityObject(fallbackInvalidMaterial);
            fallbackValidMaterial = null;
            fallbackInvalidMaterial = null;
        }

        private void CreateGhost(Entity target)
        {
            GameObject stagingRoot = new($"{target.name} Ghost Staging");
            stagingRoot.hideFlags = HideFlags.HideAndDontSave;
            stagingRoot.transform.SetParent(transform, false);
            stagingRoot.SetActive(false);

            ghostInstance = Instantiate(target.gameObject, stagingRoot.transform, false);
            ghostInstance.name = $"{target.name} Ghost Preview";
            ghostInstance.hideFlags = HideFlags.DontSave;

            DisableGameplayParticipation(ghostInstance);
            SetLayerRecursively(ghostInstance, GetPreviewLayer());

            ghostInstance.transform.SetParent(transform, true);
            ghostInstance.SetActive(false);
            DestroyUnityObject(stagingRoot);
        }

        private static void DisableGameplayParticipation(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.enabled = false;
            }

            NavMeshAgent[] agents = root.GetComponentsInChildren<NavMeshAgent>(true);
            foreach (NavMeshAgent agent in agents)
            {
                agent.enabled = false;
            }

            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            foreach (Animator animator in animators)
            {
                animator.enabled = false;
            }
        }

        private static void EnsureVisibleRenderer(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    return;
                }
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                ActivatePath(root.transform, renderer.transform);
                renderer.enabled = true;
                return;
            }
        }

        private static void ActivatePath(Transform root, Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (target != root)
            {
                ActivatePath(root, target.parent);
            }

            target.gameObject.SetActive(true);
        }

        private void ApplyPreviewMaterial()
        {
            if (ghostInstance == null)
            {
                return;
            }

            Material previewMaterial = GetPreviewMaterial();
            Renderer[] renderers = ghostInstance.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                int materialCount = Mathf.Max(1, renderer.sharedMaterials.Length);
                Material[] materials = new Material[materialCount];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = previewMaterial;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private Material GetPreviewMaterial()
        {
            if (isValid)
            {
                return validPreviewMaterial != null
                    ? validPreviewMaterial
                    : fallbackValidMaterial ??= CreateFallbackMaterial(new Color(0f, 0.65f, 1f, 0.35f));
            }

            return invalidPreviewMaterial != null
                ? invalidPreviewMaterial
                : fallbackInvalidMaterial ??= CreateFallbackMaterial(new Color(1f, 0.1f, 0.05f, 0.35f));
        }

        private static Material CreateFallbackMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new(shader)
            {
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = 3000,
                color = color,
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return material;
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

        private static void DestroyUnityObject(Object target)
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
