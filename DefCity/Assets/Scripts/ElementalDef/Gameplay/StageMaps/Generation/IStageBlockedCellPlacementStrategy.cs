using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public interface IStageBlockedCellPlacementStrategy
    {
        string StrategyId { get; }
        string Version { get; }

        IReadOnlyList<Vector2Int> SelectBlockedCells(StageBlockedCellPlacementContext context);
    }
}
