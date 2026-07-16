using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefCity.Gameplay.City.Roads.Geometry
{
    public readonly struct RoadIntersectionPort
    {
        public Vector3 Direction { get; }
        public float ConnectionDistance { get; }

        public RoadIntersectionPort(Vector3 direction)
            : this(direction, 0f)
        {
        }

        public RoadIntersectionPort(Vector3 direction, float connectionDistance)
        {
            if (connectionDistance < 0f
                || float.IsNaN(connectionDistance)
                || float.IsInfinity(connectionDistance))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(connectionDistance),
                    connectionDistance,
                    "Road intersection connection distance must be non-negative and finite.");
            }

            Direction = direction;
            ConnectionDistance = connectionDistance;
        }
    }

    public static class RoadIntersectionMeshGenerator
    {
        private const float Epsilon = 0.0001f;
        private const float EpsilonSqr = Epsilon * Epsilon;

        public static Mesh BuildConvexHullIntersection(
            Vector3 center,
            IReadOnlyList<RoadIntersectionPort> ports,
            float width,
            float thickness,
            float yOffset = 0f,
            RoadUvOrientation uvOrientation = RoadUvOrientation.AcrossRoad)
        {
            if (ports == null)
            {
                throw new ArgumentNullException(nameof(ports));
            }

            if (ports.Count < 2)
            {
                throw new ArgumentException("At least two road intersection ports are required.", nameof(ports));
            }

            if (width <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Road intersection width must be greater than zero.");
            }

            if (thickness <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), thickness, "Road intersection thickness must be greater than zero.");
            }

            List<Vector2> edgePoints = BuildPortEdgePoints(center, ports, width);
            List<Vector2> hull = BuildConvexHull(edgePoints);
            if (hull.Count < 3)
            {
                throw new ArgumentException("Road intersection ports must produce at least three convex hull points.", nameof(ports));
            }

            float bottomY = center.y + yOffset;
            float topY = bottomY + thickness;
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();

            AddTopFace(vertices, uvs, triangles, center, hull, topY, uvOrientation);
            AddBottomFace(vertices, uvs, triangles, center, hull, bottomY, uvOrientation);
            AddSideFaces(vertices, uvs, triangles, center, hull, bottomY, topY, thickness, uvOrientation);

            Mesh mesh = new()
            {
                name = "Road Convex Hull Intersection",
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<Vector2> BuildPortEdgePoints(Vector3 center, IReadOnlyList<RoadIntersectionPort> ports, float width)
        {
            float halfWidth = width * 0.5f;
            List<Vector2> edgePoints = new();

            for (int i = 0; i < ports.Count; i++)
            {
                Vector3 rawDirection = ports[i].Direction;
                Vector3 horizontalDirection = new(rawDirection.x, 0f, rawDirection.z);
                float directionLength = horizontalDirection.magnitude;
                if (directionLength <= Epsilon)
                {
                    throw new ArgumentException("Road intersection port directions must have non-zero XZ length.", nameof(ports));
                }

                Vector3 direction = horizontalDirection / directionLength;
                Vector3 right = Vector3.Cross(Vector3.up, direction);
                Vector3 portCenter = center + (direction * ports[i].ConnectionDistance);
                AddUniquePoint(edgePoints, ToPoint(portCenter - (right * halfWidth)));
                AddUniquePoint(edgePoints, ToPoint(portCenter + (right * halfWidth)));
            }

            return edgePoints;
        }

        private static List<Vector2> BuildConvexHull(List<Vector2> points)
        {
            points.Sort(ComparePoints);
            List<Vector2> lower = new();
            foreach (Vector2 point in points)
            {
                while (lower.Count >= 2 && Cross(lower[^2], lower[^1], point) <= Epsilon)
                {
                    lower.RemoveAt(lower.Count - 1);
                }

                lower.Add(point);
            }

            List<Vector2> upper = new();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector2 point = points[i];
                while (upper.Count >= 2 && Cross(upper[^2], upper[^1], point) <= Epsilon)
                {
                    upper.RemoveAt(upper.Count - 1);
                }

                upper.Add(point);
            }

            if (lower.Count > 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }

            if (upper.Count > 0)
            {
                upper.RemoveAt(upper.Count - 1);
            }

            lower.AddRange(upper);
            return lower;
        }

        private static void AddTopFace(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 center,
            IReadOnlyList<Vector2> hull,
            float topY,
            RoadUvOrientation uvOrientation)
        {
            int centerIndex = vertices.Count;
            vertices.Add(new Vector3(center.x, topY, center.z));
            uvs.Add(BuildPlanarUv(center, new Vector2(center.x, center.z), uvOrientation));

            for (int i = 0; i < hull.Count; i++)
            {
                Vector2 point = hull[i];
                vertices.Add(new Vector3(point.x, topY, point.y));
                uvs.Add(BuildPlanarUv(center, point, uvOrientation));
            }

            for (int i = 0; i < hull.Count; i++)
            {
                int current = centerIndex + 1 + i;
                int next = centerIndex + 1 + ((i + 1) % hull.Count);
                triangles.Add(centerIndex);
                triangles.Add(next);
                triangles.Add(current);
            }
        }

        private static void AddBottomFace(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 center,
            IReadOnlyList<Vector2> hull,
            float bottomY,
            RoadUvOrientation uvOrientation)
        {
            int centerIndex = vertices.Count;
            vertices.Add(new Vector3(center.x, bottomY, center.z));
            uvs.Add(BuildPlanarUv(center, new Vector2(center.x, center.z), uvOrientation));

            for (int i = 0; i < hull.Count; i++)
            {
                Vector2 point = hull[i];
                vertices.Add(new Vector3(point.x, bottomY, point.y));
                uvs.Add(BuildPlanarUv(center, point, uvOrientation));
            }

            for (int i = 0; i < hull.Count; i++)
            {
                int current = centerIndex + 1 + i;
                int next = centerIndex + 1 + ((i + 1) % hull.Count);
                triangles.Add(centerIndex);
                triangles.Add(current);
                triangles.Add(next);
            }
        }

        private static void AddSideFaces(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 center,
            IReadOnlyList<Vector2> hull,
            float bottomY,
            float topY,
            float thickness,
            RoadUvOrientation uvOrientation)
        {
            float edgeDistance = 0f;
            for (int i = 0; i < hull.Count; i++)
            {
                Vector2 current = hull[i];
                Vector2 next = hull[(i + 1) % hull.Count];
                float nextEdgeDistance = edgeDistance + Vector2.Distance(current, next);
                int startIndex = vertices.Count;

                vertices.Add(new Vector3(current.x, topY, current.y));
                vertices.Add(new Vector3(next.x, topY, next.y));
                vertices.Add(new Vector3(current.x, bottomY, current.y));
                vertices.Add(new Vector3(next.x, bottomY, next.y));
                uvs.Add(BuildSideUv(edgeDistance, thickness, uvOrientation));
                uvs.Add(BuildSideUv(nextEdgeDistance, thickness, uvOrientation));
                uvs.Add(BuildSideUv(edgeDistance, 0f, uvOrientation));
                uvs.Add(BuildSideUv(nextEdgeDistance, 0f, uvOrientation));

                triangles.Add(startIndex);
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 2);
                triangles.Add(startIndex + 2);
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 3);

                edgeDistance = nextEdgeDistance;
            }
        }

        private static Vector2 BuildPlanarUv(Vector3 center, Vector2 point, RoadUvOrientation uvOrientation)
        {
            float localX = point.x - center.x;
            float localZ = point.y - center.z;
            return uvOrientation switch
            {
                RoadUvOrientation.AcrossRoad => new Vector2(localX, localZ),
                RoadUvOrientation.AlongRoad => new Vector2(localZ, localX),
                _ => throw new ArgumentOutOfRangeException(nameof(uvOrientation), uvOrientation, "Unsupported road UV orientation.")
            };
        }

        private static Vector2 BuildSideUv(float edgeDistance, float height, RoadUvOrientation uvOrientation)
        {
            return uvOrientation switch
            {
                RoadUvOrientation.AcrossRoad => new Vector2(edgeDistance, height),
                RoadUvOrientation.AlongRoad => new Vector2(height, edgeDistance),
                _ => throw new ArgumentOutOfRangeException(nameof(uvOrientation), uvOrientation, "Unsupported road UV orientation.")
            };
        }

        private static void AddUniquePoint(List<Vector2> points, Vector2 point)
        {
            foreach (Vector2 existingPoint in points)
            {
                if ((existingPoint - point).sqrMagnitude <= EpsilonSqr)
                {
                    return;
                }
            }

            points.Add(point);
        }

        private static Vector2 ToPoint(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        private static int ComparePoints(Vector2 a, Vector2 b)
        {
            if (Mathf.Abs(a.x - b.x) > Epsilon)
            {
                return a.x < b.x ? -1 : 1;
            }

            if (Mathf.Abs(a.y - b.y) > Epsilon)
            {
                return a.y < b.y ? -1 : 1;
            }

            return 0;
        }

        private static float Cross(Vector2 origin, Vector2 a, Vector2 b)
        {
            return ((a.x - origin.x) * (b.y - origin.y)) - ((a.y - origin.y) * (b.x - origin.x));
        }
    }
}
