using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.City.Buildings
{
    // BuildingManager 전체 Refactoring 필요
    // 이벤트 형식을 다른 컴포넌트의 이벤트와 일관되게 변경 필요

    public class BuildingManager : MonoBehaviour
    {
        private readonly List<Building> buildings = new();

        public event Action BuildingsChanged;
        public IReadOnlyList<Building> Buildings => buildings;

        private void Awake()
        {
            RegisterSceneBuildings();
        }

        public void RegisterBuilding(Building building)
        {
            RegisterBuildingInternal(building);
            BuildingsChanged?.Invoke();
        }

        private void RegisterBuildingInternal(Building building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (buildings.Contains(building))
            {
                throw new InvalidOperationException($"{building.name} is already registered.");
            }

            if (building.IsRegistered)
            {
                throw new InvalidOperationException($"{building.name} is already registered to another BuildingManager.");
            }

            building.AssignBuildingManager(this);
            buildings.Add(building);
        }

        public bool UnregisterBuilding(Building building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (!buildings.Remove(building))
            {
                return false;
            }

            building.ClearBuildingManager(this);
            BuildingsChanged?.Invoke();
            return true;
        }

        public IEnumerable<Building> GetBuildings(TeamKind teamKind, bool aliveOnly = true)
        {
            return buildings.Where(building => MatchesBuildingFilter(building, teamKind, aliveOnly));
        }

        public int CountBuildings(TeamKind teamKind, bool aliveOnly = true)
        {
            int count = 0;
            foreach (Building building in buildings)
            {
                if (MatchesBuildingFilter(building, teamKind, aliveOnly))
                {
                    count++;
                }
            }

            return count;
        }

        public bool TryGetAveragePosition(TeamKind teamKind, out Vector3 averagePosition, bool aliveOnly = true)
        {
            averagePosition = Vector3.zero;

            int count = 0;
            Vector3 sum = Vector3.zero;

            foreach (Building building in buildings)
            {
                if (!MatchesBuildingFilter(building, teamKind, aliveOnly))
                {
                    continue;
                }

                sum += building.transform.position;
                count++;
            }

            if (count == 0)
            {
                return false;
            }

            averagePosition = sum / count;
            return true;
        }

        public int RegisterSceneBuildings()
        {
            int registeredCount = 0;
            Building[] sceneBuildings = FindObjectsByType<Building>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Building building in sceneBuildings)
            {
                if (building == null || !building.isActiveAndEnabled || building.IsRegistered)
                {
                    continue;
                }

                RegisterBuildingInternal(building);
                registeredCount++;
            }

            if (registeredCount > 0)
            {
                BuildingsChanged?.Invoke();
            }

            return registeredCount;
        }

        private static bool MatchesBuildingFilter(Building building, TeamKind teamKind, bool aliveOnly)
        {
            if (building == null)
            {
                return false;
            }

            if (aliveOnly && !building.IsAlive)
            {
                return false;
            }

            Team team = building.Team;
            return team != null && team.Kind == teamKind;
        }
    }
}
