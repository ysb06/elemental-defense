using System;
using System.IO;
using ElementalDef.Data;
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

        public static ElementalDefApplicationRoot Instance => instance;

        public StageLaunchService StageLaunch { get; private set; }
        public IElementalDefRunStore RunStore { get; private set; }
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
                RunStore?.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "The ElementalDef stage-run database could not be disposed cleanly.",
                    exception), this);
            }

            RunStore = null;
            StageLaunch = null;
            instance = null;
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
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                RunStore?.Dispose();
                RunStore = null;
                Debug.LogException(new InvalidOperationException(
                    "ElementalDef application services could not be initialized.",
                    exception), this);
            }
        }
    }
}
