using System;
using UnityEngine;
using DefCity.Gameplay.City.Buildings;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.City
{
    [DisallowMultipleComponent]
    public class CityCenter : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private TeamKind teamKind = TeamKind.Player;
        [SerializeField] private bool aliveOnly = true;

        public Vector3 CurrentPosition => transform.position;
        public event Action<CityCenter> CenterChanged;

        private void OnEnable()
        {
            if (buildingManager != null)
            {
                buildingManager.BuildingsChanged += RefreshCenter;
            }
        }

        private void Start()
        {
            RefreshCenter();
        }

        private void OnDisable()
        {
            if (buildingManager != null)
            {
                buildingManager.BuildingsChanged -= RefreshCenter;
            }
        }

        public void RefreshCenter()
        {
            if (TryCalculateCenterPosition(out Vector3 centerPosition))
            {
                if (transform.position == centerPosition)
                {
                    return;
                }

                transform.position = centerPosition;
                CenterChanged?.Invoke(this);
            }
        }

        protected virtual bool TryCalculateCenterPosition(out Vector3 centerPosition)
        {
            centerPosition = transform.position;
            return buildingManager != null &&
                buildingManager.TryGetAveragePosition(teamKind, out centerPosition, aliveOnly);
        }
    }
}
