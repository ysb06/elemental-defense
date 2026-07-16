using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefCity.Gameplay.City.Roads.Geometry
{
    public enum RoadUvOrientation
    {
        AlongRoad,
        AcrossRoad
    }

    public enum RoadThicknessUvMode
    {
        SeparateFaces,
        ContinuousAcrossWidth
    }

    public static class RoadMeshGenerator
    {
        private const float MinHorizontalLength = 0.0001f;

        public static Mesh BuildStraightStripWithThickness(
            Vector3 start,
            Vector3 end,
            float width,
            float sampleSpacing,
            float thickness,
            float yOffset = 0f,
            RoadUvOrientation uvOrientation = RoadUvOrientation.AlongRoad,
            RoadThicknessUvMode thicknessUvMode = RoadThicknessUvMode.SeparateFaces)
        {
            if (width <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Road width must be greater than zero.");
            }

            if (sampleSpacing <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSpacing), sampleSpacing, "Sample spacing must be greater than zero.");
            }

            if (thickness <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), thickness, "Road thickness must be greater than zero.");
            }

            bool useContinuousThicknessUv = thicknessUvMode switch
            {
                RoadThicknessUvMode.SeparateFaces => false,
                RoadThicknessUvMode.ContinuousAcrossWidth => true,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(thicknessUvMode),
                    thicknessUvMode,
                    "Unsupported road thickness UV mode.")
            };

            Vector3 horizontalDelta = new(end.x - start.x, 0f, end.z - start.z);
            float length = horizontalDelta.magnitude;
            if (length <= MinHorizontalLength)
            {
                throw new ArgumentException("Start and end must have different XZ positions.", nameof(end));
            }

            Vector3 direction = horizontalDelta / length;
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            float halfWidth = width * 0.5f;
            Vector3 leftOffset = -right * halfWidth;
            Vector3 rightOffset = right * halfWidth;

            int sectionCount = Mathf.Max(1, Mathf.CeilToInt(length / sampleSpacing));
            Vector3[] topLeft = new Vector3[sectionCount + 1];
            Vector3[] topRight = new Vector3[sectionCount + 1];
            Vector3[] bottomLeft = new Vector3[sectionCount + 1];
            Vector3[] bottomRight = new Vector3[sectionCount + 1];
            Vector2[] leftTopUv = new Vector2[sectionCount + 1];
            Vector2[] rightTopUv = new Vector2[sectionCount + 1];
            float[] distancesAlongRoad = new float[sectionCount + 1];
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();
            float topLeftCrossSection = thickness;
            float topRightCrossSection = thickness + width;
            float rightBottomCrossSection = thickness + width + thickness;

            for (int i = 0; i <= sectionCount; i++)
            {
                float t = (float)i / sectionCount;
                Vector3 center = Vector3.Lerp(start, end, t);
                float bottomY = center.y + yOffset;
                float topY = bottomY + thickness;
                float distanceAlongRoad = length * t;
                distancesAlongRoad[i] = distanceAlongRoad;

                bottomLeft[i] = center + leftOffset;
                bottomRight[i] = center + rightOffset;
                topLeft[i] = bottomLeft[i];
                topRight[i] = bottomRight[i];
                bottomLeft[i].y = bottomY;
                bottomRight[i].y = bottomY;
                topLeft[i].y = topY;
                topRight[i].y = topY;
                leftTopUv[i] = BuildUv(distanceAlongRoad, useContinuousThicknessUv ? topLeftCrossSection : 0f, uvOrientation);
                rightTopUv[i] = BuildUv(distanceAlongRoad, useContinuousThicknessUv ? topRightCrossSection : 1f, uvOrientation);
            }

            for (int i = 0; i < sectionCount; i++)
            {
                AddQuad(
                    vertices,
                    uvs,
                    triangles,
                    topLeft[i],
                    topLeft[i + 1],
                    topRight[i],
                    topRight[i + 1],
                    leftTopUv[i],
                    leftTopUv[i + 1],
                    rightTopUv[i],
                    rightTopUv[i + 1]);

                AddQuad(
                    vertices,
                    uvs,
                    triangles,
                    bottomLeft[i],
                    bottomRight[i],
                    bottomLeft[i + 1],
                    bottomRight[i + 1]);

                if (useContinuousThicknessUv)
                {
                    AddQuad(
                        vertices,
                        uvs,
                        triangles,
                        bottomLeft[i],
                        bottomLeft[i + 1],
                        topLeft[i],
                        topLeft[i + 1],
                        BuildUv(distancesAlongRoad[i], 0f, uvOrientation),
                        BuildUv(distancesAlongRoad[i + 1], 0f, uvOrientation),
                        BuildUv(distancesAlongRoad[i], topLeftCrossSection, uvOrientation),
                        BuildUv(distancesAlongRoad[i + 1], topLeftCrossSection, uvOrientation));
                }
                else
                {
                    AddQuad(
                        vertices,
                        uvs,
                        triangles,
                        bottomLeft[i],
                        bottomLeft[i + 1],
                        topLeft[i],
                        topLeft[i + 1]);
                }

                if (useContinuousThicknessUv)
                {
                    AddQuad(
                        vertices,
                        uvs,
                        triangles,
                        bottomRight[i],
                        topRight[i],
                        bottomRight[i + 1],
                        topRight[i + 1],
                        BuildUv(distancesAlongRoad[i], rightBottomCrossSection, uvOrientation),
                        BuildUv(distancesAlongRoad[i], topRightCrossSection, uvOrientation),
                        BuildUv(distancesAlongRoad[i + 1], rightBottomCrossSection, uvOrientation),
                        BuildUv(distancesAlongRoad[i + 1], topRightCrossSection, uvOrientation));
                }
                else
                {
                    AddQuad(
                        vertices,
                        uvs,
                        triangles,
                        bottomRight[i],
                        topRight[i],
                        bottomRight[i + 1],
                        topRight[i + 1]);
                }
            }

            AddQuad(
                vertices,
                uvs,
                triangles,
                bottomLeft[0],
                topLeft[0],
                bottomRight[0],
                topRight[0]);

            AddQuad(
                vertices,
                uvs,
                triangles,
                bottomLeft[sectionCount],
                bottomRight[sectionCount],
                topLeft[sectionCount],
                topRight[sectionCount]);

            Mesh mesh = new()
            {
                name = "Road Straight Strip With Thickness",
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector2 BuildUv(float distanceAlongRoad, float side, RoadUvOrientation uvOrientation)
        {
            return uvOrientation switch
            {
                RoadUvOrientation.AlongRoad => new Vector2(distanceAlongRoad, side),
                RoadUvOrientation.AcrossRoad => new Vector2(side, distanceAlongRoad),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(uvOrientation),
                    uvOrientation,
                    "Unsupported road UV orientation.")
            };
        }

        private static void AddQuad(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d)
        {
            AddQuad(
                vertices,
                uvs,
                triangles,
                a,
                b,
                c,
                d,
                Vector2.zero,
                Vector2.right,
                Vector2.up,
                Vector2.one);
        }

        private static void AddQuad(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector2 uvA,
            Vector2 uvB,
            Vector2 uvC,
            Vector2 uvD)
        {
            int startIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            uvs.Add(uvA);
            uvs.Add(uvB);
            uvs.Add(uvC);
            uvs.Add(uvD);

            triangles.Add(startIndex);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 3);
        }

        private static Vector3 ApplyHeight(
            Vector3 position,
            float fallbackHeight,
            float yOffset,
            Func<Vector3, float> heightSampler)
        {
            position.y = (heightSampler != null ? heightSampler(position) : fallbackHeight) + yOffset;
            return position;
        }
    }
}
