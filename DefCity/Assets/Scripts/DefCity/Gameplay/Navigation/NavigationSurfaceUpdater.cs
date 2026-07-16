using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using DefCity.Gameplay.City.Buildings;

namespace DefCity.Gameplay.Navigation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshSurface))]
    public class NavigationSurfaceUpdater : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private NavMeshSurface navMeshSurface;

        private bool rebuildRequested;
        private Coroutine rebuildRoutine;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (buildingManager != null)
            {
                buildingManager.BuildingsChanged += RequestRebuild;
            }
        }

        private void Start()
        {
            RequestRebuild();
        }

        private void OnDisable()
        {
            if (buildingManager != null)
            {
                buildingManager.BuildingsChanged -= RequestRebuild;
            }

            if (rebuildRoutine != null)
            {
                StopCoroutine(rebuildRoutine);
                rebuildRoutine = null;
            }

            rebuildRequested = false;
        }

        public void RequestRebuild()
        {
            rebuildRequested = true;

            if (!isActiveAndEnabled || rebuildRoutine != null)
            {
                return;
            }

            rebuildRoutine = StartCoroutine(RebuildAtEndOfFrame());
        }

        public void RebuildNow()
        {
            ResolveReferences();

            if (navMeshSurface == null)
            {
                throw new InvalidOperationException($"{name} requires a NavMeshSurface.");
            }

            if (navMeshSurface.navMeshData != null)
            {
                navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
                return;
            }

            navMeshSurface.BuildNavMesh();
        }

        private IEnumerator RebuildAtEndOfFrame()
        {
            yield return null;

            if (rebuildRequested)
            {
                rebuildRequested = false;
                RebuildNow();
            }

            rebuildRoutine = null;
        }

        private void ResolveReferences()
        {
            if (navMeshSurface == null)
            {
                navMeshSurface = GetComponent<NavMeshSurface>();
            }
        }
    }
}
