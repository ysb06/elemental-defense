using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.Flow.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Wave Bundle")]
    public class WaveBundle : ScriptableObject
    {
        [SerializeField] private List<WaveSchedule> waves = new();

        public IReadOnlyList<WaveSchedule> Waves => waves;

        public void AddWave(WaveSchedule wave)
        {
            if (wave == null)
            {
                throw new ArgumentNullException(nameof(wave));
            }

            waves.Add(wave);
        }

        public void ClearWaves()
        {
            waves.Clear();
        }
    }
}
