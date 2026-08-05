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
        public long PlayDurationMilliseconds { get; }
        public double HeadquartersRemainingHealth { get; }
        public long DefeatedEnemyCount { get; }
        public long AttackCount { get; }
        public StageRunOutcome Outcome { get; }
        public DateTimeOffset CompletedAtUtc { get; }

        public CompletedStageRunSnapshot(
            string runId,
            string stageId,
            long playDurationMilliseconds,
            double headquartersRemainingHealth,
            long defeatedEnemyCount,
            long attackCount,
            StageRunOutcome outcome,
            DateTimeOffset completedAtUtc)
        {
            RunId = PersistenceValidation.NormalizeGuidN(runId, nameof(runId));
            StageId = PersistenceValidation.RequireId(stageId, nameof(stageId));
            PersistenceValidation.RequireNonNegative(playDurationMilliseconds, nameof(playDurationMilliseconds));
            PersistenceValidation.RequireNonNegativeFinite(headquartersRemainingHealth, nameof(headquartersRemainingHealth));
            PersistenceValidation.RequireNonNegative(defeatedEnemyCount, nameof(defeatedEnemyCount));
            PersistenceValidation.RequireNonNegative(attackCount, nameof(attackCount));
            PersistenceValidation.RequireOutcome(outcome, nameof(outcome));

            PlayDurationMilliseconds = playDurationMilliseconds;
            HeadquartersRemainingHealth = headquartersRemainingHealth;
            DefeatedEnemyCount = defeatedEnemyCount;
            AttackCount = attackCount;
            Outcome = outcome;
            CompletedAtUtc = completedAtUtc.ToUniversalTime();
        }
    }

    public sealed class CompletedStageRunRecord
    {
        public long CompletionSequence { get; }
        public string RunId { get; }
        public string StageId { get; }
        public long PlayDurationMilliseconds { get; }
        public double HeadquartersRemainingHealth { get; }
        public long DefeatedEnemyCount { get; }
        public long AttackCount { get; }
        public StageRunOutcome Outcome { get; }
        public DateTimeOffset CompletedAtUtc { get; }

        internal CompletedStageRunRecord(
            long completionSequence,
            string runId,
            string stageId,
            long playDurationMilliseconds,
            double headquartersRemainingHealth,
            long defeatedEnemyCount,
            long attackCount,
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
            PersistenceValidation.RequireNonNegative(playDurationMilliseconds, nameof(playDurationMilliseconds));
            PersistenceValidation.RequireNonNegativeFinite(headquartersRemainingHealth, nameof(headquartersRemainingHealth));
            PersistenceValidation.RequireNonNegative(defeatedEnemyCount, nameof(defeatedEnemyCount));
            PersistenceValidation.RequireNonNegative(attackCount, nameof(attackCount));
            PersistenceValidation.RequireOutcome(outcome, nameof(outcome));

            PlayDurationMilliseconds = playDurationMilliseconds;
            HeadquartersRemainingHealth = headquartersRemainingHealth;
            DefeatedEnemyCount = defeatedEnemyCount;
            AttackCount = attackCount;
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

    public interface IElementalDefRunStore : IDisposable
    {
        DataStoreState State { get; }

        event Action<CompletedStageRunRecord> RunCommitted;

        void Initialize();
        CompletedStageRunCommitResult Commit(CompletedStageRunSnapshot snapshot);
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

        public static void RequireNonNegativeFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
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
    }
}
