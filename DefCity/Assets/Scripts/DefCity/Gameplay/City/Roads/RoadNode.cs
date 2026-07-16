using UnityEngine;

namespace DefCity.Gameplay.City.Roads
{
    [DisallowMultipleComponent]
    public sealed class RoadNode : MonoBehaviour
    {
        [SerializeField] private Vector3Int cellPosition;
        [SerializeField] private RoadIntersection intersection;

        public Vector3Int CellPosition => cellPosition;
        public Vector3 WorldPosition => transform.position;
        public RoadIntersection Intersection => intersection;

        internal void Initialize(Vector3Int targetCellPosition, Vector3 worldPosition)
        {
            cellPosition = targetCellPosition;
            transform.position = worldPosition;
        }

        internal void AttachIntersection(RoadIntersection targetIntersection)
        {
            if (targetIntersection == null)
            {
                throw new System.ArgumentNullException(nameof(targetIntersection));
            }

            if (!ReferenceEquals(targetIntersection.Node, this))
            {
                throw new System.InvalidOperationException("Road intersection belongs to a different road node.");
            }

            if (intersection != null && !ReferenceEquals(intersection, targetIntersection))
            {
                throw new System.InvalidOperationException("Road node already owns an intersection.");
            }

            intersection = targetIntersection;
        }

        internal void DetachIntersection(RoadIntersection targetIntersection)
        {
            if (ReferenceEquals(intersection, targetIntersection))
            {
                intersection = null;
            }
        }
    }
}
