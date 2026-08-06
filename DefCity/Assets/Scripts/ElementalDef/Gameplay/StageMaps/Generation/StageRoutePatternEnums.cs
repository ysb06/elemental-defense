using System;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public enum StageRoutePatternKind
    {
        Straight = 0,
        Corner = 1,
        DisconnectedCross = 2,
    }

    [Flags]
    public enum StageRoutePatternKinds
    {
        None = 0,
        Straight = 1 << 0,
        Corner = 1 << 1,
        DisconnectedCross = 1 << 2,
        All = Straight | Corner | DisconnectedCross,
    }

    /// <summary>
    /// Quadrant numbering follows the current MapGenerator convention:
    /// bottom-left, bottom-right, top-right, then top-left.
    /// </summary>
    public enum StageRoutePatternSlot
    {
        Quadrant1 = 0,
        Quadrant2 = 1,
        Quadrant3 = 2,
        Quadrant4 = 3,
        Center = 4,
    }

    public enum StageRoutePassageAxis
    {
        Horizontal = 0,
        Vertical = 1,
        Turn = 2,
    }

    public enum StageRoutePatternCandidateFailureReason
    {
        None = 0,
        NoFeasiblePatternLayout = 1,
        NoValidPassageOrder = 2,
    }
}
