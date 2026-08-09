using System;
using System.Collections.Generic;
using DefCore.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Presentation.Animation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class HeadquartersHealthAppearancePresenter : MonoBehaviour
    {
        [Serializable]
        private sealed class AppearanceEntry
        {
            [SerializeField, Range(0f, 100f)] private float maximumHealthPercent = 100f;
            [SerializeField] private GameObject appearanceRoot;

            public float MaximumHealthPercent => maximumHealthPercent;
            public GameObject AppearanceRoot => appearanceRoot;
        }

        [SerializeField] private Health health;
        [SerializeField] private AppearanceEntry[] appearanceEntries = Array.Empty<AppearanceEntry>();

        private bool isSubscribed;

        public float CurrentHealthPercent { get; private set; }
        public GameObject CurrentAppearance { get; private set; }

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                DisableWithError(errorMessage);
                return;
            }

            Subscribe();
            ApplyCurrentAppearance();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Refresh()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                DisableWithError(errorMessage);
                return;
            }

            ApplyCurrentAppearance();
        }

        private void HandleDamaged(GameObject sender, DamageEventArgs args)
        {
            Refresh();
        }

        private void ApplyCurrentAppearance()
        {
            float healthPercent = Mathf.Clamp(health.CurrentHealth / health.MaxHealth * 100f, 0f, 100f);
            AppearanceEntry selectedEntry = null;
            float selectedMaximum = float.PositiveInfinity;

            foreach (AppearanceEntry entry in appearanceEntries)
            {
                if (healthPercent <= entry.MaximumHealthPercent &&
                    entry.MaximumHealthPercent < selectedMaximum)
                {
                    selectedEntry = entry;
                    selectedMaximum = entry.MaximumHealthPercent;
                }
            }

            CurrentHealthPercent = healthPercent;
            CurrentAppearance = selectedEntry.AppearanceRoot;

            foreach (AppearanceEntry entry in appearanceEntries)
            {
                SetActiveIfNeeded(entry.AppearanceRoot, entry == selectedEntry);
            }
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (health == null)
            {
                errorMessage = $"A {nameof(Health)} reference is required.";
                return false;
            }

            if (!IsFinitePositive(health.MaxHealth))
            {
                errorMessage = $"{nameof(Health)}.{nameof(Health.MaxHealth)} must be a finite positive value.";
                return false;
            }

            if (float.IsNaN(health.CurrentHealth) || float.IsInfinity(health.CurrentHealth))
            {
                errorMessage = $"{nameof(Health)}.{nameof(Health.CurrentHealth)} must be a finite value.";
                return false;
            }

            if (appearanceEntries == null || appearanceEntries.Length == 0)
            {
                errorMessage = "At least one appearance entry is required.";
                return false;
            }

            HashSet<float> maximumHealthPercents = new();
            HashSet<GameObject> appearanceRoots = new();
            bool hasFullHealthAppearance = false;

            for (int index = 0; index < appearanceEntries.Length; index++)
            {
                AppearanceEntry entry = appearanceEntries[index];
                if (entry == null)
                {
                    errorMessage = $"Appearance entry {index} is null.";
                    return false;
                }

                float maximumHealthPercent = entry.MaximumHealthPercent;
                if (!IsFiniteInPercentRange(maximumHealthPercent))
                {
                    errorMessage =
                        $"Appearance entry {index} maximum health percent must be a finite value from 0 to 100.";
                    return false;
                }

                if (!maximumHealthPercents.Add(maximumHealthPercent))
                {
                    errorMessage =
                        $"Appearance entries must not share maximum health percent {maximumHealthPercent}.";
                    return false;
                }

                if (entry.AppearanceRoot == null)
                {
                    errorMessage = $"Appearance entry {index} requires an appearance root.";
                    return false;
                }

                if (entry.AppearanceRoot == gameObject)
                {
                    errorMessage = $"Appearance entry {index} must not reference the presenter GameObject itself.";
                    return false;
                }

                if (!appearanceRoots.Add(entry.AppearanceRoot))
                {
                    errorMessage = $"Appearance roots must be unique. Duplicate: '{entry.AppearanceRoot.name}'.";
                    return false;
                }

                hasFullHealthAppearance |= maximumHealthPercent == 100f;
            }

            if (!hasFullHealthAppearance)
            {
                errorMessage = "An appearance entry with a maximum health percent of 100 is required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            health.OnDamaged.AddListener(HandleDamaged);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (health != null)
            {
                health.OnDamaged.RemoveListener(HandleDamaged);
            }

            isSubscribed = false;
        }

        private void DisableWithError(string errorMessage)
        {
            Debug.LogError(
                $"[{name}] {nameof(HeadquartersHealthAppearancePresenter)}: {errorMessage}",
                this);
            enabled = false;
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFiniteInPercentRange(float value)
        {
            return value >= 0f &&
                   value <= 100f &&
                   !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }

        private static void SetActiveIfNeeded(GameObject appearanceRoot, bool active)
        {
            if (appearanceRoot.activeSelf != active)
            {
                appearanceRoot.SetActive(active);
            }
        }
    }
}
