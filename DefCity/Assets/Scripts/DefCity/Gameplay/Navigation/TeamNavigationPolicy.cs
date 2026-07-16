using System;
using UnityEngine.AI;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.Navigation
{
    public static class TeamNavigationPolicy
    {
        public const string WalkableAreaName = "Walkable";
        public const string PlayerBuildingAreaName = "Player Building";
        public const string EnemyBuildingAreaName = "Enemy Building";
        public const string NavigationModifierLayerName = "Navigation Modifier";

        public static int BuildAreaMaskForMover(TeamKind moverKind)
        {
            int walkableArea = ResolveAreaIndex(WalkableAreaName);
            int playerBuildingArea = ResolveAreaIndex(PlayerBuildingAreaName);
            int enemyBuildingArea = ResolveAreaIndex(EnemyBuildingAreaName);

            return BuildAreaMaskForMover(moverKind, walkableArea, playerBuildingArea, enemyBuildingArea);
        }

        public static int BuildAreaMaskForMover(TeamKind moverKind, int walkableArea, int playerBuildingArea, int enemyBuildingArea)
        {
            ValidateAreaIndex(walkableArea, nameof(walkableArea));
            ValidateAreaIndex(playerBuildingArea, nameof(playerBuildingArea));
            ValidateAreaIndex(enemyBuildingArea, nameof(enemyBuildingArea));

            return moverKind switch
            {
                TeamKind.Player => AreaToMask(walkableArea) | AreaToMask(enemyBuildingArea),
                TeamKind.Enemy => AreaToMask(walkableArea) | AreaToMask(playerBuildingArea),
                _ => throw new ArgumentOutOfRangeException(nameof(moverKind), moverKind, "Unsupported team kind.")
            };
        }

        public static string GetBuildingAreaName(TeamKind ownerKind)
        {
            return ownerKind switch
            {
                TeamKind.Player => PlayerBuildingAreaName,
                TeamKind.Enemy => EnemyBuildingAreaName,
                _ => throw new ArgumentOutOfRangeException(nameof(ownerKind), ownerKind, "Unsupported team kind.")
            };
        }

        public static int GetBuildingAreaIndex(TeamKind ownerKind)
        {
            return ResolveAreaIndex(GetBuildingAreaName(ownerKind));
        }

        public static int AreaToMask(int areaIndex)
        {
            ValidateAreaIndex(areaIndex, nameof(areaIndex));
            return 1 << areaIndex;
        }

        private static int ResolveAreaIndex(string areaName)
        {
            int areaIndex = NavMesh.GetAreaFromName(areaName);
            if (areaIndex < 0)
            {
                throw new InvalidOperationException($"NavMesh area '{areaName}' is not configured.");
            }

            return areaIndex;
        }

        private static void ValidateAreaIndex(int areaIndex, string argumentName)
        {
            if (areaIndex < 0 || areaIndex > 31)
            {
                throw new ArgumentOutOfRangeException(argumentName, areaIndex, "NavMesh area index must be between 0 and 31.");
            }
        }
    }
}
