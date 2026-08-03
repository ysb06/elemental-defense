namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public interface IStageRoutePatternStrategy
    {
        string StrategyId { get; }
        string Version { get; }

        StageRoutePatternCandidateSet CreateCandidates(
            StageRouteGenerationSettings settings);
    }
}
