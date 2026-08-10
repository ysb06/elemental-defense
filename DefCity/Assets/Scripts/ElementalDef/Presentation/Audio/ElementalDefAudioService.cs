using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElementalDef.Presentation.Audio
{
    [DefaultExecutionOrder(-9900)]
    [DisallowMultipleComponent]
    public sealed class ElementalDefAudioService : MonoBehaviour
    {
        private const string ElementalDefScenePrefix = "ElementalDef";
        private const float MinimumCrossfadeDurationSeconds = 0.01f;
        private const double MusicScheduleLeadSeconds = 0.05d;

        [Header("Mixer Routing")]
        [SerializeField] private AudioMixerGroup musicOutput;
        [SerializeField] private AudioMixerGroup sfxOutput;
        [SerializeField] private AudioMixerGroup uiOutput;

        [Header("Shared Clips")]
        [SerializeField] private AudioClip battleMusicClip;
        [SerializeField] private AudioClip buttonClickClip;

        [Header("Battle Music")]
        [SerializeField, Min(MinimumCrossfadeDurationSeconds)]
        private float musicCrossfadeDurationSeconds = 1.75f;

        private readonly Dictionary<string, AudioSource> exclusiveSfxSources =
            new(StringComparer.Ordinal);
        private readonly Dictionary<Button, UnityAction> registeredButtonActions = new();

        private AudioSource firstMusicSource;
        private AudioSource secondMusicSource;
        private AudioSource activeMusicSource;
        private AudioSource nextMusicSource;
        private AudioSource uiSource;
        private bool isBattleMusicActive;
        private double crossfadeStartDspTime;
        private float effectiveCrossfadeDurationSeconds;
        private bool warnedMissingBattleMusic;
        private bool warnedMissingButtonClick;

        public bool IsBattleMusicPlaying => isBattleMusicActive;

        private void Awake()
        {
            firstMusicSource = CreateAudioSource("Battle Music A", musicOutput);
            secondMusicSource = CreateAudioSource("Battle Music B", musicOutput);
            uiSource = CreateAudioSource("UI One Shot", uiOutput);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            RegisterButtons(SceneManager.GetActiveScene());
        }

        private void Start()
        {
            RegisterButtons(SceneManager.GetActiveScene());
        }

        private void Update()
        {
            UpdateBattleMusicCrossfade();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            RemoveAllButtonListeners();
            StopBattleMusic();
        }

        private void OnDestroy()
        {
            foreach (AudioSource source in exclusiveSfxSources.Values)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }

            exclusiveSfxSources.Clear();
        }

        public bool StartBattleMusic()
        {
            if (battleMusicClip == null)
            {
                if (!warnedMissingBattleMusic)
                {
                    warnedMissingBattleMusic = true;
                    Debug.LogWarning("ElementalDef battle music is not assigned.", this);
                }

                return false;
            }

            if (!IsFinitePositive(musicCrossfadeDurationSeconds))
            {
                Debug.LogError(
                    "ElementalDef battle-music crossfade duration must be a finite positive number.",
                    this);
                return false;
            }

            try
            {
                StopBattleMusic();

                effectiveCrossfadeDurationSeconds = Mathf.Min(
                    musicCrossfadeDurationSeconds,
                    Mathf.Max(MinimumCrossfadeDurationSeconds, battleMusicClip.length * 0.5f));

                activeMusicSource = firstMusicSource;
                nextMusicSource = secondMusicSource;
                ConfigureMusicSource(activeMusicSource, 1f);
                ConfigureMusicSource(nextMusicSource, 0f);

                double startDspTime = AudioSettings.dspTime + MusicScheduleLeadSeconds;
                activeMusicSource.PlayScheduled(startDspTime);
                ScheduleNextMusicIteration(startDspTime);
                isBattleMusicActive = true;
                return true;
            }
            catch (Exception exception)
            {
                StopBattleMusic();
                Debug.LogException(new InvalidOperationException(
                    "ElementalDef battle music could not be started.",
                    exception), this);
                return false;
            }
        }

        public void StopBattleMusic()
        {
            isBattleMusicActive = false;
            crossfadeStartDspTime = 0d;

            StopAndReset(firstMusicSource);
            StopAndReset(secondMusicSource);
            activeMusicSource = null;
            nextMusicSource = null;
        }

        public bool PlayExclusive2D(string key, AudioClip clip)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("An ElementalDef exclusive SFX key cannot be empty.", this);
                return false;
            }

            if (clip == null)
            {
                Debug.LogWarning($"ElementalDef exclusive SFX '{key}' has no clip.", this);
                return false;
            }

            try
            {
                AudioSource source = GetOrCreateExclusiveSfxSource(key);
                source.Stop();
                source.clip = clip;
                source.volume = 1f;
                source.Play();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    $"ElementalDef exclusive SFX '{key}' could not be played.",
                    exception), this);
                return false;
            }
        }

        public void StopExclusive2D(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (exclusiveSfxSources.TryGetValue(key, out AudioSource source) && source != null)
            {
                source.Stop();
                source.clip = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterButtons(scene);
        }

        private void RegisterButtons(Scene scene)
        {
            PruneDestroyedButtons();

            if (!scene.IsValid() || !scene.isLoaded ||
                !scene.name.StartsWith(ElementalDefScenePrefix, StringComparison.Ordinal))
            {
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Button[] buttons = roots[rootIndex].GetComponentsInChildren<Button>(true);
                for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                {
                    RegisterButton(buttons[buttonIndex]);
                }
            }
        }

        private void RegisterButton(Button button)
        {
            if (button == null || registeredButtonActions.ContainsKey(button))
            {
                return;
            }

            UnityAction action = HandleButtonClicked;
            registeredButtonActions.Add(button, action);
            button.onClick.AddListener(action);
        }

        private void HandleButtonClicked()
        {
            if (buttonClickClip == null)
            {
                if (!warnedMissingButtonClick)
                {
                    warnedMissingButtonClick = true;
                    Debug.LogWarning("ElementalDef button-click audio is not assigned.", this);
                }

                return;
            }

            try
            {
                uiSource.Stop();
                uiSource.clip = buttonClickClip;
                uiSource.volume = 1f;
                uiSource.Play();
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "ElementalDef button-click audio could not be played.",
                    exception), this);
            }
        }

        private void UpdateBattleMusicCrossfade()
        {
            if (!isBattleMusicActive || activeMusicSource == null || nextMusicSource == null)
            {
                return;
            }

            try
            {
                double currentDspTime = AudioSettings.dspTime;
                if (currentDspTime < crossfadeStartDspTime)
                {
                    return;
                }

                double normalizedTime =
                    (currentDspTime - crossfadeStartDspTime) / effectiveCrossfadeDurationSeconds;
                float progress = Mathf.Clamp01((float)normalizedTime);
                float angle = progress * Mathf.PI * 0.5f;
                activeMusicSource.volume = Mathf.Cos(angle);
                nextMusicSource.volume = Mathf.Sin(angle);

                if (normalizedTime < 1d)
                {
                    return;
                }

                AudioSource previousSource = activeMusicSource;
                activeMusicSource = nextMusicSource;
                nextMusicSource = previousSource;

                nextMusicSource.Stop();
                nextMusicSource.clip = null;
                activeMusicSource.volume = 1f;
                ScheduleNextMusicIteration(crossfadeStartDspTime);
            }
            catch (Exception exception)
            {
                StopBattleMusic();
                Debug.LogException(new InvalidOperationException(
                    "ElementalDef battle-music crossfade stopped after an audio error.",
                    exception), this);
            }
        }

        private void ScheduleNextMusicIteration(double activeIterationStartDspTime)
        {
            ConfigureMusicSource(nextMusicSource, 0f);
            crossfadeStartDspTime = activeIterationStartDspTime +
                battleMusicClip.length - effectiveCrossfadeDurationSeconds;
            nextMusicSource.PlayScheduled(crossfadeStartDspTime);
        }

        private void ConfigureMusicSource(AudioSource source, float volume)
        {
            source.Stop();
            source.clip = battleMusicClip;
            source.loop = false;
            source.volume = volume;
            source.spatialBlend = 0f;
        }

        private AudioSource GetOrCreateExclusiveSfxSource(string key)
        {
            if (exclusiveSfxSources.TryGetValue(key, out AudioSource source) && source != null)
            {
                return source;
            }

            source = CreateAudioSource($"Exclusive SFX ({key})", sfxOutput);
            exclusiveSfxSources[key] = source;
            return source;
        }

        private AudioSource CreateAudioSource(string sourceName, AudioMixerGroup output)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);

            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = output;
            return source;
        }

        private void PruneDestroyedButtons()
        {
            if (registeredButtonActions.Count == 0)
            {
                return;
            }

            var destroyedButtons = new List<Button>();
            foreach (Button button in registeredButtonActions.Keys)
            {
                if (button == null)
                {
                    destroyedButtons.Add(button);
                }
            }

            for (int index = 0; index < destroyedButtons.Count; index++)
            {
                registeredButtonActions.Remove(destroyedButtons[index]);
            }
        }

        private void RemoveAllButtonListeners()
        {
            foreach (KeyValuePair<Button, UnityAction> pair in registeredButtonActions)
            {
                if (pair.Key != null)
                {
                    pair.Key.onClick.RemoveListener(pair.Value);
                }
            }

            registeredButtonActions.Clear();
        }

        private static void StopAndReset(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.volume = 1f;
        }

        private static bool IsFinitePositive(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }
}
