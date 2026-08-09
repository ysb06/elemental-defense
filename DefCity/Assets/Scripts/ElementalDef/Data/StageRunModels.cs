using System;
using System.Collections.Generic;

namespace ElementalDef.Data
{
    public enum DataStoreState
    {
        Uninitialized,
        Ready,
        Faulted,
        Disposed
    }

    public enum StageRunOutcome
    {
        Victory = 1,
        Defeat = 2
    }

    public enum CompletedStageRunCommitStatus
    {
        Committed,
        AlreadyCommitted,
        RunIdConflict
    }

    public sealed class CompletedStageRunSnapshot
    {
        public string RunId { get; }
        public string StageId { get; }
        public int StageDisplayOrder { get; }
        public long PlayDurationMilliseconds { get; }
        public double HeadquartersRemainingHealth { get; }
        public double HeadquartersMaxHealth { get; }
        public long DefeatedEnemyCount { get; }
        public long AttackCount { get; }
        public long EarnedCredits { get; }
        public long EarnedExperience { get; }
        public StageRunOutcome Outcome { get; }
        public DateTimeOffset CompletedAtUtc { get; }

        public CompletedStageRunSnapshot(
            string runId,
            string stageId,
            int stageDisplayOrder,
            long playDurationMilliseconds,
            double headquartersRemainingHealth,
            double headquartersMaxHealth,
            long defeatedEnemyCount,
            long attackCount,
            long earnedCredits,
            long earnedExperience,
            StageRunOutcome outcome,
            DateTimeOffset completedAtUtc)
        {
            RunId = PersistenceValidation.NormalizeGuidN(runId, nameof(runId));
            StageId = PersistenceValidation.RequireId(stageId, nameof(stageId));
            PersistenceValidation.RequireStageDisplayOrder(stageDisplayOrder, nameof(stageDisplayOrder));
            PersistenceValidation.RequireNonNegative(playDurationMilliseconds, nameof(playDurationMilliseconds));
            PersistenceValidation.RequireNonNegativeFinite(headquartersRemainingHealth, nameof(headquartersRemainingHealth));
            PersistenceValidation.RequirePositiveFinite(headquartersMaxHealth, nameof(headquartersMaxHealth));
            PersistenceValidation.RequireNotGreaterThan(
                headquartersRemainingHealth,
                headquartersMaxHealth,
                nameof(headquartersRemainingHealth));
            PersistenceValidation.RequireNonNegative(defeatedEnemyCount, nameof(defeatedEnemyCount));
            PersistenceValidation.RequireNonNegative(attackCount, nameof(attackCount));
            PersistenceValidation.RequireNonNegative(earnedCredits, nameof(earnedCredits));
            PersistenceValidation.RequireNonNegative(earnedExperience, nameof(earnedExperience));
            PersistenceValidation.RequireOutcome(outcome, nameof(outcome));
            PersistenceValidation.RequireNoDefeatRewards(outcome, earnedCredits, earnedExperience);

            StageDisplayOrder = stageDisplayOrder;
            PlayDurationMilliseconds = playDurationMilliseconds;
            HeadquartersRemainingHealth = headquartersRemainingHealth;
            HeadquartersMaxHealth = headquartersMaxHealth;
            DefeatedEnemyCount = defeatedEnemyCount;
            AttackCount = attackCount;
            EarnedCredits = earnedCredits;
            EarnedExperience = earnedExperience;
            Outcome = outcome;
            CompletedAtUtc = completedAtUtc.ToUniversalTime();
        }
    }

    public sealed class CompletedStageRunRecord
    {
        public long CompletionSequence { get; }
        public string RunId { get; }
        public string StageId { get; }
        public int StageDisplayOrder { get; }
        public long PlayDurationMilliseconds { get; }
        public double HeadquartersRemainingHealth { get; }
        public double? HeadquartersMaxHealth { get; }
        public long DefeatedEnemyCount { get; }
        public long AttackCount { get; }
        public long EarnedCredits { get; }
        public long EarnedExperience { get; }
        public StageRunOutcome Outcome { get; }
        public DateTimeOffset CompletedAtUtc { get; }

        internal CompletedStageRunRecord(
            long completionSequence,
            string runId,
            string stageId,
            int stageDisplayOrder,
            long playDurationMilliseconds,
            double headquartersRemainingHealth,
            double? headquartersMaxHealth,
            long defeatedEnemyCount,
            long attackCount,
            long earnedCredits,
            long earnedExperience,
            StageRunOutcome outcome,
            DateTimeOffset completedAtUtc)
        {
            if (completionSequence <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completionSequence));
            }

            CompletionSequence = completionSequence;
            RunId = PersistenceValidation.NormalizeGuidN(runId, nameof(runId));
            StageId = PersistenceValidation.RequireId(stageId, nameof(stageId));
            PersistenceValidation.RequireStageDisplayOrder(stageDisplayOrder, nameof(stageDisplayOrder));
            PersistenceValidation.RequireNonNegative(playDurationMilliseconds, nameof(playDurationMilliseconds));
            PersistenceValidation.RequireNonNegativeFinite(headquartersRemainingHealth, nameof(headquartersRemainingHealth));
            if (headquartersMaxHealth.HasValue)
            {
                PersistenceValidation.RequirePositiveFinite(
                    headquartersMaxHealth.Value,
                    nameof(headquartersMaxHealth));
                PersistenceValidation.RequireNotGreaterThan(
                    headquartersRemainingHealth,
                    headquartersMaxHealth.Value,
                    nameof(headquartersRemainingHealth));
            }

            PersistenceValidation.RequireNonNegative(defeatedEnemyCount, nameof(defeatedEnemyCount));
            PersistenceValidation.RequireNonNegative(attackCount, nameof(attackCount));
            PersistenceValidation.RequireNonNegative(earnedCredits, nameof(earnedCredits));
            PersistenceValidation.RequireNonNegative(earnedExperience, nameof(earnedExperience));
            PersistenceValidation.RequireOutcome(outcome, nameof(outcome));
            PersistenceValidation.RequireNoDefeatRewards(outcome, earnedCredits, earnedExperience);

            StageDisplayOrder = stageDisplayOrder;
            PlayDurationMilliseconds = playDurationMilliseconds;
            HeadquartersRemainingHealth = headquartersRemainingHealth;
            HeadquartersMaxHealth = headquartersMaxHealth;
            DefeatedEnemyCount = defeatedEnemyCount;
            AttackCount = attackCount;
            EarnedCredits = earnedCredits;
            EarnedExperience = earnedExperience;
            Outcome = outcome;
            CompletedAtUtc = completedAtUtc.ToUniversalTime();
        }
    }

    public sealed class CompletedStageRunCommitResult
    {
        public CompletedStageRunCommitStatus Status { get; }
        public CompletedStageRunRecord Record { get; }
        public bool IsNewlyCommitted => Status == CompletedStageRunCommitStatus.Committed;

        internal CompletedStageRunCommitResult(
            CompletedStageRunCommitStatus status,
            CompletedStageRunRecord record)
        {
            Status = status;
            Record = record ?? throw new ArgumentNullException(nameof(record));
        }
    }

    public sealed class PlayerProgressSnapshot
    {
        public long TotalCredits { get; }
        public long TotalExperience { get; }
        public int MaxStageProgress { get; }
        public long Loop { get; }
        public long TotalDefeatCount { get; }
        public DateTimeOffset UpdatedAtUtc { get; }

        internal PlayerProgressSnapshot(
            long totalCredits,
            long totalExperience,
            int maxStageProgress,
            long loop,
            long totalDefeatCount,
            DateTimeOffset updatedAtUtc)
        {
            PersistenceValidation.RequireNonNegative(totalCredits, nameof(totalCredits));
            PersistenceValidation.RequireNonNegative(totalExperience, nameof(totalExperience));
            PersistenceValidation.RequireMaxStageProgress(maxStageProgress, nameof(maxStageProgress));
            PersistenceValidation.RequireNonNegative(loop, nameof(loop));
            PersistenceValidation.RequireNonNegative(totalDefeatCount, nameof(totalDefeatCount));

            TotalCredits = totalCredits;
            TotalExperience = totalExperience;
            MaxStageProgress = maxStageProgress;
            Loop = loop;
            TotalDefeatCount = totalDefeatCount;
            UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
        }
    }

    public sealed class PlayerProgressService
    {
        private readonly IElementalDefRunStore runStore;

        public PlayerProgressService(IElementalDefRunStore runStore)
        {
            this.runStore = runStore ?? throw new ArgumentNullException(nameof(runStore));
        }

        public PlayerProgressSnapshot GetProgress()
        {
            return runStore.GetPlayerProgress();
        }
    }

    public interface IElementalDefRunStore : IDisposable
    {
        DataStoreState State { get; }

        event Action<CompletedStageRunRecord> RunCommitted;

        void Initialize();
        CompletedStageRunCommitResult Commit(CompletedStageRunSnapshot snapshot);
        PlayerProgressSnapshot GetPlayerProgress();
        IReadOnlyList<CompletedStageRunRecord> GetRecentRuns(int limit);
        IReadOnlyList<CompletedStageRunRecord> GetRecentRunsForStage(string stageId, int limit);
        bool TryGetRun(string runId, out CompletedStageRunRecord record);
    }

    internal static class PersistenceValidation
    {
        public static string NormalizeGuidN(string value, string parameterName)
        {
            if (!Guid.TryParseExact(value, "N", out Guid parsed))
            {
                throw new ArgumentException("A 32-character GUID in N format is required.",parameterName);
            }

            return parsed.ToString("N");
        }

        public static string RequireId(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value) || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                throw new ArgumentException("A non-empty, trimmed ID is required.", parameterName);
            }

            return value;
        }

        public static void RequireNonNegative(long value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequireStageDisplayOrder(int value, string parameterName)
        {
            if (value < 1 || value > 10)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequireMaxStageProgress(int value, string parameterName)
        {
            if (value < 0 || value > 9)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequireNonNegativeFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequirePositiveFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequireNotGreaterThan(
            double value,
            double maximum,
            string parameterName)
        {
            if (value > maximum)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequireOutcome(StageRunOutcome value, string parameterName)
        {
            if (value != StageRunOutcome.Victory && value != StageRunOutcome.Defeat)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void RequireNoDefeatRewards(
            StageRunOutcome outcome,
            long earnedCredits,
            long earnedExperience)
        {
            if (outcome == StageRunOutcome.Defeat &&
                (earnedCredits != 0 || earnedExperience != 0))
            {
                throw new ArgumentException(
                    "A defeated stage run cannot award credits or experience.");
            }
        }
    }
}
