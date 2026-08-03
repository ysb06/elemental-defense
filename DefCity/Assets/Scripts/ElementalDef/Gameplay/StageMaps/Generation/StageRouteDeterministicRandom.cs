using System;
using System.Collections.Generic;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    /// <summary>
    /// Small fixed SplitMix64 implementation used instead of UnityEngine.Random,
    /// System.Random, or runtime-dependent hash codes.
    /// </summary>
    internal struct StageRouteDeterministicRandom
    {
        private const ulong GoldenRatio = 0x9E3779B97F4A7C15UL;
        private ulong state;

        internal StageRouteDeterministicRandom(int seed, int stream)
        {
            ulong combined = ((ulong)(uint)seed << 32) | (uint)stream;
            state = Mix(combined ^ 0xD1B54A32D192ED03UL);
        }

        internal int NextInt(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMaximum));
            }

            ulong bound = (uint)exclusiveMaximum;
            ulong threshold = unchecked(0UL - bound) % bound;
            ulong value;
            do
            {
                value = NextUInt64();
            }
            while (value < threshold);

            return (int)(value % bound);
        }

        internal void Shuffle<T>(IList<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            for (int index = values.Count - 1; index > 0; index--)
            {
                int swapIndex = NextInt(index + 1);
                (values[index], values[swapIndex]) =
                    (values[swapIndex], values[index]);
            }
        }

        private ulong NextUInt64()
        {
            state += GoldenRatio;
            return Mix(state);
        }

        private static ulong Mix(ulong value)
        {
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }

    internal static class StageRouteStableHash
    {
        internal static ulong Fnv1A64(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            const ulong offsetBasis = 14_695_981_039_346_656_037UL;
            const ulong prime = 1_099_511_628_211UL;
            ulong hash = offsetBasis;
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                hash ^= (byte)character;
                hash *= prime;
                hash ^= (byte)(character >> 8);
                hash *= prime;
            }

            return hash;
        }
    }
}
