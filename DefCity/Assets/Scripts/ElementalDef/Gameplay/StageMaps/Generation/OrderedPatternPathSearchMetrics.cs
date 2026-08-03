using System;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    internal enum OrderedPatternPathSearchOutcome
    {
        Succeeded = 0,
        PathNotFound = 1,
        SearchLimitExceeded = 2,
    }

    /// <summary>
    /// Deterministic work counters for one ordered-pattern route candidate.
    /// Wall-clock time is deliberately excluded from the search contract.
    /// </summary>
    internal sealed class OrderedPatternPathSearchMetrics
    {
        internal int TotalWorkUnits { get; }
        internal int AStarStatesExpanded { get; }
        internal int ConnectorAlternativesTried { get; }
        internal int BacktrackCount { get; }
        internal int ReachabilityCheckCount { get; }
        internal int ReachabilityVisitedCellCount { get; }

        internal OrderedPatternPathSearchMetrics(
            int totalWorkUnits,
            int aStarStatesExpanded,
            int connectorAlternativesTried,
            int backtrackCount,
            int reachabilityCheckCount,
            int reachabilityVisitedCellCount)
        {
            EnsureNonNegative(totalWorkUnits, nameof(totalWorkUnits));
            EnsureNonNegative(aStarStatesExpanded, nameof(aStarStatesExpanded));
            EnsureNonNegative(
                connectorAlternativesTried,
                nameof(connectorAlternativesTried));
            EnsureNonNegative(backtrackCount, nameof(backtrackCount));
            EnsureNonNegative(
                reachabilityCheckCount,
                nameof(reachabilityCheckCount));
            EnsureNonNegative(
                reachabilityVisitedCellCount,
                nameof(reachabilityVisitedCellCount));

            TotalWorkUnits = totalWorkUnits;
            AStarStatesExpanded = aStarStatesExpanded;
            ConnectorAlternativesTried = connectorAlternativesTried;
            BacktrackCount = backtrackCount;
            ReachabilityCheckCount = reachabilityCheckCount;
            ReachabilityVisitedCellCount = reachabilityVisitedCellCount;
        }

        private static void EnsureNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal sealed class OrderedPatternPathSearchMetricsBuilder
    {
        private readonly int maxWorkUnits;

        internal int TotalWorkUnits { get; private set; }
        internal int AStarStatesExpanded { get; private set; }
        internal int ConnectorAlternativesTried { get; private set; }
        internal int BacktrackCount { get; private set; }
        internal int ReachabilityCheckCount { get; private set; }
        internal int ReachabilityVisitedCellCount { get; private set; }
        internal StageRouteSearchLimitKind LimitKind { get; private set; }

        internal bool IsLimited => LimitKind != StageRouteSearchLimitKind.None;

        internal OrderedPatternPathSearchMetricsBuilder(int maxWorkUnits)
        {
            if (maxWorkUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxWorkUnits),
                    maxWorkUnits,
                    "The candidate search work limit must be positive.");
            }

            this.maxWorkUnits = maxWorkUnits;
        }

        internal void BeginReachabilityCheck()
        {
            ReachabilityCheckCount = checked(ReachabilityCheckCount + 1);
        }

        internal bool TryRecordReachabilityCell()
        {
            if (!TryConsumeWork())
            {
                return false;
            }

            ReachabilityVisitedCellCount = checked(
                ReachabilityVisitedCellCount + 1);
            return true;
        }

        internal bool TryRecordAStarExpansion()
        {
            if (!TryConsumeWork())
            {
                return false;
            }

            AStarStatesExpanded = checked(AStarStatesExpanded + 1);
            return true;
        }

        internal bool TryRecordConnectorAlternative()
        {
            if (!TryConsumeWork())
            {
                return false;
            }

            ConnectorAlternativesTried = checked(
                ConnectorAlternativesTried + 1);
            return true;
        }

        internal void RecordBacktrack()
        {
            BacktrackCount = checked(BacktrackCount + 1);
        }

        internal void SetLimit(StageRouteSearchLimitKind limitKind)
        {
            if (limitKind == StageRouteSearchLimitKind.None)
            {
                throw new ArgumentOutOfRangeException(nameof(limitKind));
            }

            if (LimitKind == StageRouteSearchLimitKind.None)
            {
                LimitKind = limitKind;
            }
        }

        internal OrderedPatternPathSearchMetrics Freeze()
        {
            return new OrderedPatternPathSearchMetrics(
                TotalWorkUnits,
                AStarStatesExpanded,
                ConnectorAlternativesTried,
                BacktrackCount,
                ReachabilityCheckCount,
                ReachabilityVisitedCellCount);
        }

        private bool TryConsumeWork()
        {
            if (TotalWorkUnits >= maxWorkUnits)
            {
                SetLimit(StageRouteSearchLimitKind.PerCandidateWork);
                return false;
            }

            TotalWorkUnits++;
            return true;
        }
    }
}
