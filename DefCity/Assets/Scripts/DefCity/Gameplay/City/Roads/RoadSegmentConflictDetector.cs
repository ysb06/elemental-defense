using UnityEngine;

namespace DefCity.Gameplay.City.Roads
{
    internal enum RoadSegmentConflict
    {
        None,
        Intersection,
        Overlap
    }

    internal static class RoadSegmentConflictDetector
    {
        internal static RoadSegmentConflict GetConflict(
            Vector3Int startCell,
            Vector3 startPosition,
            Vector3Int endCell,
            Vector3 endPosition,
            RoadSegment existing,
            float epsilon)
        {
            Vector2 p = new(startPosition.x, startPosition.z);
            Vector2 p2 = new(endPosition.x, endPosition.z);
            Vector2 q = new(existing.StartNode.WorldPosition.x, existing.StartNode.WorldPosition.z);
            Vector2 q2 = new(existing.EndNode.WorldPosition.x, existing.EndNode.WorldPosition.z);
            Vector2 r = p2 - p;
            Vector2 s = q2 - q;
            float rCrossS = Cross(r, s);
            float qMinusPCrossR = Cross(q - p, r);

            if (Mathf.Abs(rCrossS) <= epsilon)
            {
                if (Mathf.Abs(qMinusPCrossR) > epsilon)
                {
                    return RoadSegmentConflict.None;
                }

                // Collinear roads may only touch at a shared endpoint; any positive overlap is invalid.
                float rLengthSquared = Vector2.Dot(r, r);
                float t0 = Vector2.Dot(q - p, r) / rLengthSquared;
                float t1 = Vector2.Dot(q2 - p, r) / rLengthSquared;
                float overlapStart = Mathf.Max(0f, Mathf.Min(t0, t1));
                float overlapEnd = Mathf.Min(1f, Mathf.Max(t0, t1));
                float overlapLength = (overlapEnd - overlapStart) * Mathf.Sqrt(rLengthSquared);
                if (overlapLength > epsilon)
                {
                    return RoadSegmentConflict.Overlap;
                }

                bool touches = overlapEnd >= -epsilon
                    && overlapStart <= 1f + epsilon;
                if (!touches)
                {
                    return RoadSegmentConflict.None;
                }

                return SharesEndpoint(startCell, endCell, existing)
                    ? RoadSegmentConflict.None
                    : RoadSegmentConflict.Intersection;
            }

            float t = Cross(q - p, s) / rCrossS;
            float u = Cross(q - p, r) / rCrossS;
            if (!IsWithinSegment(t, epsilon) || !IsWithinSegment(u, epsilon))
            {
                return RoadSegmentConflict.None;
            }

            if (IsEndpoint(t, epsilon)
                && IsEndpoint(u, epsilon)
                && IntersectingEndpointsShareCell(startCell, endCell, t, existing, u, epsilon))
            {
                return RoadSegmentConflict.None;
            }

            return RoadSegmentConflict.Intersection;
        }

        private static bool SharesEndpoint(
            Vector3Int startCell,
            Vector3Int endCell,
            RoadSegment existing)
        {
            return startCell == existing.StartNode.CellPosition
                || startCell == existing.EndNode.CellPosition
                || endCell == existing.StartNode.CellPosition
                || endCell == existing.EndNode.CellPosition;
        }

        private static bool IntersectingEndpointsShareCell(
            Vector3Int startCell,
            Vector3Int endCell,
            float candidateParameter,
            RoadSegment existing,
            float existingParameter,
            float epsilon)
        {
            Vector3Int candidateCell = Mathf.Abs(candidateParameter) <= epsilon
                ? startCell
                : endCell;
            Vector3Int existingCell = Mathf.Abs(existingParameter) <= epsilon
                ? existing.StartNode.CellPosition
                : existing.EndNode.CellPosition;
            return candidateCell == existingCell;
        }

        private static bool IsWithinSegment(float parameter, float epsilon)
        {
            return parameter >= -epsilon
                && parameter <= 1f + epsilon;
        }

        private static bool IsEndpoint(float parameter, float epsilon)
        {
            return Mathf.Abs(parameter) <= epsilon
                || Mathf.Abs(parameter - 1f) <= epsilon;
        }

        private static float Cross(Vector2 a, Vector2 b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }
    }
}
