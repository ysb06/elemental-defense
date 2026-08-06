using System.Collections.Generic;

namespace ElementalDef.Gameplay.StageMaps
{
    public interface IRouteChoicePolicy
    {
        int ChooseNextNode(
            int currentNodeId,
            IReadOnlyList<int> orderedCandidateNodeIds);
    }
}
