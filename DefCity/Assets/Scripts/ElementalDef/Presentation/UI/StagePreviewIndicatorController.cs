using System;
using System.Collections.Generic;
using System.Globalization;
using ElementalDef.Gameplay.StageMaps.Generation;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class StagePreviewIndicatorController : MonoBehaviour
    {
        [Header("Stage Information")]
        [SerializeField] private TMP_Text stageNameText;
        [SerializeField] private TMP_Text creditText;
        [SerializeField] private TMP_Text experienceText;

        [Header("Terrain Composition")]
        [SerializeField] private TMP_Text waterText;
        [SerializeField] private TMP_Text fireText;
        [SerializeField] private TMP_Text earthText;
        [SerializeField] private StageMapGenerationProfile generationProfile;

        public bool IsReady { get; private set; }

        private void Awake()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                Clear();
                Debug.LogError($"[{name}] {errorMessage}", this);
                enabled = false;
                return;
            }

            IsReady = true;
            Clear();
        }

        public bool TryDisplay(StageLaunchPreview preview)
        {
            if (!IsReady)
            {
                Clear();
                return false;
            }

            if (preview == null)
            {
                Clear();
                Debug.LogError($"[{name}] A stage-launch preview is required.", this);
                return false;
            }

            try
            {
                StageTerrainCompositionResult terrain =
                    StageTerrainCompositionEstimator.Estimate(
                        generationProfile,
                        preview.EffectiveMapSeed);

                string formattedStageName =
                    preview.AbsoluteStageNumber.ToString(CultureInfo.InvariantCulture) +
                    " Stage";
                string formattedCredits = preview.VictoryCreditReward.ToString(
                    "N0",
                    CultureInfo.InvariantCulture);
                string formattedExperience = preview.VictoryExperienceReward.ToString(
                    "N0",
                    CultureInfo.InvariantCulture);
                string formattedWater = FormatPercentage(terrain.WaterRatio);
                string formattedFire = FormatPercentage(terrain.FireRatio);
                string formattedEarth = FormatPercentage(terrain.EarthRatio);

                stageNameText.text = formattedStageName;
                creditText.text = formattedCredits;
                experienceText.text = formattedExperience;
                waterText.text = formattedWater;
                fireText.text = formattedFire;
                earthText.text = formattedEarth;
                return true;
            }
            catch (Exception exception)
            {
                Clear();
                Debug.LogException(
                    new InvalidOperationException(
                        "The selected ElementalDef stage preview could not be displayed.",
                        exception),
                    this);
                return false;
            }
        }

        public void Clear()
        {
            ClearText(stageNameText);
            ClearText(creditText);
            ClearText(experienceText);
            ClearText(waterText);
            ClearText(fireText);
            ClearText(earthText);
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            TMP_Text[] textReferences =
            {
                stageNameText,
                creditText,
                experienceText,
                waterText,
                fireText,
                earthText,
            };
            HashSet<TMP_Text> uniqueTexts = new();
            for (int index = 0; index < textReferences.Length; index++)
            {
                TMP_Text textReference = textReferences[index];
                if (textReference == null)
                {
                    errorMessage = $"Preview text reference {index + 1} is not assigned.";
                    return false;
                }

                if (!uniqueTexts.Add(textReference))
                {
                    errorMessage = $"Preview text reference {index + 1} is assigned more than once.";
                    return false;
                }
            }

            if (generationProfile == null)
            {
                errorMessage = "A stage-map generation profile is required.";
                return false;
            }

            try
            {
                generationProfile.ValidateOrThrow();
            }
            catch (InvalidOperationException exception)
            {
                errorMessage = $"Stage-map generation profile validation failed: {exception.Message}";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static string FormatPercentage(double ratio)
        {
            if (double.IsNaN(ratio) || double.IsInfinity(ratio) || ratio < 0d || ratio > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ratio),
                    ratio,
                    "A terrain-composition ratio must be finite and between zero and one.");
            }

            long roundedPercentage = checked((long)Math.Round(
                ratio * 100d,
                MidpointRounding.AwayFromZero));
            return roundedPercentage.ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static void ClearText(TMP_Text textReference)
        {
            if (textReference != null)
            {
                textReference.text = string.Empty;
            }
        }
    }
}
