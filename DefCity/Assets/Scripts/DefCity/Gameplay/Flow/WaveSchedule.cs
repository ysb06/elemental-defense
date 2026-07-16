using System;
using System.Collections.Generic;
using UnityEngine;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.Flow
{
    [CreateAssetMenu(menuName = "DefCity/Wave Schedule")]
    public class WaveSchedule : ScriptableObject
    {
        [SerializeField] private List<WaveTurnEntry> entries = new();

        public IReadOnlyList<WaveTurnEntry> Entries => entries;

        public IEnumerable<Entity> GetEntitiesForTurn(int turn)
        {
            foreach (WaveTurnEntry entry in entries)
            {
                if (entry == null || entry.Turn != turn)
                {
                    continue;
                }

                foreach (Entity entity in entry.Entities)
                {
                    yield return entity;
                }
            }
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
        [SerializeField] private List<Entity> entities = new();

        public int Turn
        {
            get => turn;
            set => turn = Mathf.Max(0, value);
        }

        public List<Entity> Entities => entities ??= new List<Entity>();

        public WaveTurnEntry() { }

        public WaveTurnEntry(int turn, IEnumerable<Entity> entities)
        {
            Turn = turn;

            if (entities != null)
            {
                Entities.AddRange(entities);
            }
        }
    }
}
