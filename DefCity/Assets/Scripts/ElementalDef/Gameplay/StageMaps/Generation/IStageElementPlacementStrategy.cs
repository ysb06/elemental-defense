using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public interface IStageElementPlacementStrategy
    {
        string StrategyId { get; }
        string Version { get; }

        IReadOnlyDictionary<Vector2Int, ElementType> PlaceElements(
            StageElementPlacementContext context);
    }
}
