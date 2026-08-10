using System;
using System.Collections.Generic;
using DefCore.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.Flow.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Wave Schedule")]
    public class WaveSchedule : ScriptableObject
    {
        [SerializeField] private List<WaveTurnEntry> entries = new();

        public IReadOnlyList<WaveTurnEntry> Entries => entries;

        public Entity GetEntityForTurn(int turn)
        {
            foreach (WaveTurnEntry entry in entries)
            {
                if (entry == null || entry.Turn != turn)
                {
                    continue;
                }
                return entry.Entity;
            }

            return null;
        }

        public bool TryGetNextWaveTurn(int currentTurn, out int nextWaveTurn)
        {
            bool found = false;
            nextWaveTurn = default;

            foreach (WaveTurnEntry entry in entries)
            {
                if (entry == null || entry.Turn < currentTurn)
                {
                    continue;
                }

                if (!found || entry.Turn < nextWaveTurn)
                {
                    nextWaveTurn = entry.Turn;
                    found = true;
                }
            }

            return found;
        }

        public void AddEntry(WaveTurnEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            entries.Add(entry);
        }

        public void ClearEntries()
        {
            entries.Clear();
        }

        public void ValidateOrThrow()
        {
            List<string> errors = new();

            if (entries == null || entries.Count == 0)
            {
                errors.Add("At least one wave turn entry is required.");
            }
            else
            {
                HashSet<int> turns = new();
                int? previousTurn = null;

                for (int index = 0; index < entries.Count; index++)
                {
                    WaveTurnEntry entry = entries[index];
                    if (entry == null)
                    {
                        errors.Add($"Entry {index + 1} is null.");
                        continue;
                    }

                    if (entry.Entity == null)
                    {
                        errors.Add($"Entry {index + 1} has no Entity assigned.");
                    }

                    if (entry.Turn < 1)
                    {
                        errors.Add(
                            $"Entry {index + 1} turn must be at least 1, but is {entry.Turn}.");
                    }

                    if (!turns.Add(entry.Turn))
                    {
                        errors.Add(
                            $"Turn {entry.Turn} is duplicated at entry {index + 1}.");
                    }

                    if (previousTurn.HasValue && entry.Turn < previousTurn.Value)
                    {
                        errors.Add(
                            "Entries must be ordered by strictly increasing turn, but " +
                            $"entry {index + 1} has turn {entry.Turn} after " +
                            $"turn {previousTurn.Value}.");
                    }

                    previousTurn = entry.Turn;
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{name} has {errors.Count} wave schedule error(s):" +
                    Environment.NewLine +
                    "- " +
                    string.Join(Environment.NewLine + "- ", errors));
            }
        }

        private void OnValidate()
        {
            try
            {
                ValidateOrThrow();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
            }
        }
    }

    [Serializable]
    public class WaveTurnEntry
    {
        [SerializeField, Min(1)] private int turn;
        [SerializeField] private Entity entity;

        public int Turn
        {
            get => turn;
            set
            {
                if (value < 1)
                {
                    Debug.LogWarning("Turn value must be a greater than zero integer. Zero will be return and 0 may make side effects. Please check the value.");
                }
                turn = Mathf.Max(0, value);
            }
        }

        public Entity Entity => entity;

        public WaveTurnEntry(int turn, Entity entity)
        {
            Turn = turn;

            if (entity != null)
            {
                this.entity = entity;
            }
        }
    }
}
