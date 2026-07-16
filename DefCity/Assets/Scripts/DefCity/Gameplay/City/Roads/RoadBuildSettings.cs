using System;
using DefCity.Gameplay.City.Roads.Geometry;
using UnityEngine;

namespace DefCity.Gameplay.City.Roads
{
    [Serializable]
    public struct RoadBuildSettings
    {
        [Min(0.01f)] public float Width;
        [Min(0.01f)] public float SampleSpacing;
        [Min(0.01f)] public float Thickness;
        public float YOffset;
        public RoadUvOrientation UvOrientation;
        public RoadThicknessUvMode ThicknessUvMode;
        public bool AllowDiagonalRoads;
        public Material Material;

        public static RoadBuildSettings Default => new()
        {
            Width = 4f,
            SampleSpacing = 20f,
            Thickness = 0.1f,
            YOffset = 0f,
            UvOrientation = RoadUvOrientation.AcrossRoad,
            ThicknessUvMode = RoadThicknessUvMode.ContinuousAcrossWidth,
            AllowDiagonalRoads = false,
            Material = null
        };

        public void Validate()
        {
            ValidatePositiveFinite(Width, nameof(Width));
            ValidatePositiveFinite(SampleSpacing, nameof(SampleSpacing));
            ValidatePositiveFinite(Thickness, nameof(Thickness));

            if (float.IsNaN(YOffset) || float.IsInfinity(YOffset))
            {
                throw new ArgumentOutOfRangeException(nameof(YOffset), YOffset, "Road Y offset must be finite.");
            }

            if (!Enum.IsDefined(typeof(RoadUvOrientation), UvOrientation))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(UvOrientation),
                    UvOrientation,
                    "Unsupported road UV orientation.");
            }

            if (!Enum.IsDefined(typeof(RoadThicknessUvMode), ThicknessUvMode))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ThicknessUvMode),
                    ThicknessUvMode,
                    "Unsupported road thickness UV mode.");
            }

            if (Material == null)
            {
                throw new InvalidOperationException("Road material must be assigned.");
            }
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            if (value <= 0f || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be positive and finite.");
            }
        }
    }
}
