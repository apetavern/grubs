using Sandbox;
using System.Collections.Generic;
using System.Linq;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
    public partial class Projectile : ModelEntity, IAwaitResolution
    {
        private List<ArcSegment> Segments { get; set; }
        public bool IsResolved { get; set; }
        private TimeSince TimeSinceSegmentStarted { get; set; }

        public Projectile WithModel(string modelPath)
        {
            SetModel(modelPath);
            return this;
        }

        public Projectile MoveAlongTrace(List<ArcSegment> points, float speed = 20)
        {
            Segments = points;

            // Set the initial position
            Position = Segments[0].StartPos;

            return this;
        }

        public override void Simulate(Client cl)
        {
            // This might be shite
            if (Segments is null || !Segments.Any())
                return;

            if (IsResolved == true)
                return;

            if (Position.IsNearlyEqual(Segments[0].EndPos, 0.1f))
            {
                Segments.RemoveAt(0);

                if (Segments.Count == 1)
                {
                    IsResolved = true;

                    // Delete();

                    return;
                }

                TimeSinceSegmentStarted = 0;
            }
            else
            {
                Rotation = Rotation.LookAt(Segments[0].EndPos - Segments[0].StartPos);
                Position = Vector3.Lerp(Segments[0].StartPos, Segments[0].EndPos, (TimeSinceSegmentStarted * Time.Delta) * 3000f);
            }
        }
    }
}
