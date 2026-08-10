using System;
using System.Collections.Generic;

namespace ElementalDef.Data
{
    public readonly struct DifficultyDebugRunInput
    {
        public StageRunOutcome Outcome { get; }
        public double PlayDurationSeconds { get; }
        public double HeadquartersRemainingHealth { get; }
        public double HeadquartersMaxHealth { get; }
        public long DefeatedEnemyCount { get; }

        public DifficultyDebugRunInput(
            StageRunOutcome outcome,
            double playDurationSeconds,
            double headquartersRemainingHealth,
            double headquartersMaxHealth,
            long defeatedEnemyCount)
        {
            Outcome = outcome;
            PlayDurationSeconds = playDurationSeconds;
            HeadquartersRemainingHealth = headquartersRemainingHealth;
            HeadquartersMaxHealth = headquartersMaxHealth;
            DefeatedEnemyCount = defeatedEnemyCount;
        }
    }

    public sealed class DifficultyDebugRunStore : IElementalDefRunStore
    {
        public const int MaxInjectedRunCount = 10;

        private const string DebugStageId = "debug_difficulty";
        private const int DebugStageDisplayOrder = 1;

        private readonly IElementalDefRunStore innerRunStore;
        private readonly List<CompletedStageRunRecord> injectedRuns =
            new List<CompletedStageRunRecord>(MaxInjectedRunCount);
        private readonly IReadOnlyList<CompletedStageRunRecord> readOnlyInjectedRuns;

        public DataStoreState State => innerRunStore.State;
        public bool HasInjectedRun => injectedRuns.Count > 0;
        public int InjectedRunCount => injectedRuns.Count;
        public IReadOnlyList<CompletedStageRunRecord> InjectedRuns => readOnlyInjectedRuns;
        public CompletedStageRunRecord InjectedRun =>
            HasInjectedRun ? injectedRuns[0] : null;

        public event Action<CompletedStageRunRecord> RunCommitted
        {
            add => innerRunStore.RunCommitted += value;
            remove => innerRunStore.RunCommitted -= value;
        }

        public DifficultyDebugRunStore(IElementalDefRunStore innerRunStore)
        {
            this.innerRunStore = innerRunStore ??
                throw new ArgumentNullException(nameof(innerRunStore));
            readOnlyInjectedRuns = injectedRuns.AsReadOnly();
        }

        public void Initialize()
        {
            innerRunStore.Initialize();
        }

        public CompletedStageRunCommitResult Commit(CompletedStageRunSnapshot snapshot)
        {
            throw new NotSupportedException(
                "The difficulty debug run store does not persist stage runs.");
        }

        public PlayerProgressSnapshot GetPlayerProgress()
        {
            return innerRunStore.GetPlayerProgress();
        }

        public IReadOnlyList<CompletedStageRunRecord> GetRecentRuns(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            if (!HasInjectedRun)
            {
                return innerRunStore.GetRecentRuns(limit);
            }

            IReadOnlyList<CompletedStageRunRecord> storedRuns =
                innerRunStore.GetRecentRuns(limit);
            var candidates = new List<RunMergeCandidate>(
                injectedRuns.Count + storedRuns.Count);
            for (int index = 0; index < injectedRuns.Count; index++)
            {
                candidates.Add(new RunMergeCandidate(
                    injectedRuns[index],
                    true,
                    index));
            }

            foreach (CompletedStageRunRecord storedRun in storedRuns)
            {
                candidates.Add(new RunMergeCandidate(storedRun, false, 0));
            }

            candidates.Sort(CompareMergeCandidates);
            int resultCount = Math.Min(limit, candidates.Count);
            var records = new List<CompletedStageRunRecord>(resultCount);
            for (int index = 0; index < resultCount; index++)
            {
                records.Add(candidates[index].Record);
            }

            return records.AsReadOnly();
        }

        public IReadOnlyList<CompletedStageRunRecord> GetRecentRunsForStage(
            string stageId,
            int limit)
        {
            return innerRunStore.GetRecentRunsForStage(stageId, limit);
        }

        public bool TryGetRun(string runId, out CompletedStageRunRecord record)
        {
            return innerRunStore.TryGetRun(runId, out record);
        }

        public void Inject(DifficultyDebugRunInput input)
        {
            CompletedStageRunRecord validatedRun = CreateInjectedRun(input);
            injectedRuns.Insert(0, validatedRun);
            if (injectedRuns.Count > MaxInjectedRunCount)
            {
                injectedRuns.RemoveAt(injectedRuns.Count - 1);
            }
        }

        public void ClearInjectedRun()
        {
            ClearInjectedRuns();
        }

        public void ClearInjectedRuns()
        {
            injectedRuns.Clear();
        }

        public void Dispose()
        {
            // This decorator does not own the underlying persistent store.
        }

        private static CompletedStageRunRecord CreateInjectedRun(
            DifficultyDebugRunInput input)
        {
            PersistenceValidation.RequireOutcome(input.Outcome, nameof(input.Outcome));
            PersistenceValidation.RequireNonNegativeFinite(
                input.PlayDurationSeconds,
                nameof(input.PlayDurationSeconds));
            PersistenceValidation.RequireNonNegativeFinite(
                input.HeadquartersRemainingHealth,
                nameof(input.HeadquartersRemainingHealth));
            PersistenceValidation.RequirePositiveFinite(
                input.HeadquartersMaxHealth,
                nameof(input.HeadquartersMaxHealth));
            PersistenceValidation.RequireNotGreaterThan(
                input.HeadquartersRemainingHealth,
                input.HeadquartersMaxHealth,
                nameof(input.HeadquartersRemainingHealth));
            PersistenceValidation.RequireNonNegative(
                input.DefeatedEnemyCount,
                nameof(input.DefeatedEnemyCount));

            long playDurationMilliseconds = checked(
                (long)(input.PlayDurationSeconds * 1000d));

            return new CompletedStageRunRecord(
                long.MaxValue,
                Guid.NewGuid().ToString("N"),
                DebugStageId,
                DebugStageDisplayOrder,
                playDurationMilliseconds,
                input.HeadquartersRemainingHealth,
                input.HeadquartersMaxHealth,
                input.DefeatedEnemyCount,
                0L,
                0L,
                0L,
                input.Outcome,
                DateTimeOffset.FromUnixTimeMilliseconds(
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        }

        private static int CompareMergeCandidates(
            RunMergeCandidate left,
            RunMergeCandidate right)
        {
            int completedAtComparison = right.Record.CompletedAtUtc.CompareTo(
                left.Record.CompletedAtUtc);
            if (completedAtComparison != 0)
            {
                return completedAtComparison;
            }

            if (left.IsInjected != right.IsInjected)
            {
                return left.IsInjected ? -1 : 1;
            }

            if (left.IsInjected)
            {
                return left.InjectionIndex.CompareTo(right.InjectionIndex);
            }

            return right.Record.CompletionSequence.CompareTo(
                left.Record.CompletionSequence);
        }

        private readonly struct RunMergeCandidate
        {
            public CompletedStageRunRecord Record { get; }
            public bool IsInjected { get; }
            public int InjectionIndex { get; }

            public RunMergeCandidate(
                CompletedStageRunRecord record,
                bool isInjected,
                int injectionIndex)
            {
                Record = record;
                IsInjected = isInjected;
                InjectionIndex = injectionIndex;
            }
        }
    }
}
