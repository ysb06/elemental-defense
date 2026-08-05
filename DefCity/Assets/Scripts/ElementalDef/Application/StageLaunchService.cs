using System;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Flow.Settings;

namespace ElementalDef.Runtime
{
    public sealed class StageLaunchService
    {
        public StageRunContext Current { get; private set; }
        public bool HasCurrent => Current != null;

        public StageRunContext Prepare(WaveBundle stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            StageRunContext context = StageRunContext.Create(stage);
            Current = context;
            return context;
        }
    }
}
