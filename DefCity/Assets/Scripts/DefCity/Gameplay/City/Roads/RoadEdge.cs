using System;
using QuikGraph;

namespace DefCity.Gameplay.City.Roads
{
    public sealed class RoadEdge : IEdge<RoadNode>
    {
        public RoadNode Source { get; }
        public RoadNode Target { get; }
        public RoadSegment Segment { get; }

        public RoadEdge(RoadNode source, RoadNode target, RoadSegment segment)
        {
            Source = source != null ? source : throw new ArgumentNullException(nameof(source));
            Target = target != null ? target : throw new ArgumentNullException(nameof(target));
            Segment = segment != null ? segment : throw new ArgumentNullException(nameof(segment));
        }
    }
}
