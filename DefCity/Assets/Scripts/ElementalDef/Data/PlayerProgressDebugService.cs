using System;

namespace ElementalDef.Data
{
    public sealed class PlayerProgressDebugService
    {
        private readonly EDataStore dataStore;

        public PlayerProgressDebugService(EDataStore dataStore)
        {
            this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        public PlayerProgressSnapshot SetProgress(int maxStageProgress, long loop)
        {
            return dataStore.SetPlayerProgressForDebug(maxStageProgress, loop);
        }
    }
}
