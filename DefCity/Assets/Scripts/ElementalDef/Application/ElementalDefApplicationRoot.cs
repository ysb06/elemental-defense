using System;
using System.IO;
using ElementalDef.Data;
using ElementalDef.Presentation.Audio;
using UnityEngine;

namespace ElementalDef.Runtime
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class ElementalDefApplicationRoot : MonoBehaviour
    {
        private const string DatabaseDirectoryName = "ElementalDef";
        private const string DatabaseFileName = "elementaldef.sqlite3";

        private static ElementalDefApplicationRoot instance;

        [SerializeField] private ElementalDefAudioService audioService;

        public static ElementalDefApplicationRoot Instance => instance;

        public ElementalDefAudioService Audio => audioService;
        public StageLaunchService StageLaunch { get; private set; }
        public IElementalDefRunStore RunStore { get; private set; }
        public PlayerProgressService PlayerProgress { get; private set; }
        public PlayerProgressDebugService PlayerProgressDebug { get; private set; }
        public StageDifficultyService StageDifficulty { get; private set; }
        public DifficultyDebugRunStore DifficultyDebug { get; private set; }
        public string DatabasePath { get; private set; }
        public Exception InitializationException { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveAudioService();
            StageLaunch = new StageLaunchService();
            InitializeServices();
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            try
            {
                DifficultyDebug?.Dispose();
                RunStore?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "The ElementalDef stage-run database could not be disposed cleanly.",
                    exception), this);
            }

            RunStore = null;
            PlayerProgress = null;
            PlayerProgressDebug = null;
            StageDifficulty = null;
            DifficultyDebug = null;
            StageLaunch = null;
            instance = null;
        }

        private void ResolveAudioService()
        {
            if (audioService == null)
            {
                audioService = GetComponent<ElementalDefAudioService>();
            }

            if (audioService == null)
            {
                Debug.LogWarning(
                    "ElementalDef audio is unavailable because the ApplicationRoot has no audio service.",
                    this);
            }
        }

        private void InitializeServices()
        {
            try
            {
                string databaseDirectory = Path.Combine(Application.persistentDataPath, DatabaseDirectoryName);
                Directory.CreateDirectory(databaseDirectory);

                DatabasePath = Path.Combine(databaseDirectory, DatabaseFileName);
                var dataStore = new EDataStore(DatabasePath);
                RunStore = dataStore;
                dataStore.Initialize();
                PlayerProgress = new PlayerProgressService(dataStore);
                PlayerProgressDebug = new PlayerProgressDebugService(dataStore);
                DifficultyDebug = new DifficultyDebugRunStore(dataStore);
                StageDifficulty = new StageDifficultyService(DifficultyDebug);
                StageLaunch.ConfigureDifficultyService(StageDifficulty);
                StageLaunch.ConfigurePlayerProgressService(PlayerProgress);
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                RunStore?.Dispose();
                RunStore = null;
                PlayerProgress = null;
                PlayerProgressDebug = null;
                StageDifficulty = null;
                DifficultyDebug = null;
                Debug.LogException(new InvalidOperationException(
                    "ElementalDef application services could not be initialized.",
                    exception), this);
            }
        }
    }
}
