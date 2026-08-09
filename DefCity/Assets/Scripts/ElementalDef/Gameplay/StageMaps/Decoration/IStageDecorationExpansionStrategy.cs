using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public interface IStageDecorationExpansionStrategy
    {
        string StrategyId { get; }
        string Version { get; }

        IReadOnlyList<StageDecorationCellEntry> Expand(
            GeneratedStageMap map,
            Vector2Int centerCell,
            int radius,
            IReadOnlyList<Vector2Int> decorationCells,
            int seed);
    }
}
