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
    }

    [Serializable]
    public class WaveTurnEntry
    {
        [SerializeField, Min(0)] private int turn;
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
