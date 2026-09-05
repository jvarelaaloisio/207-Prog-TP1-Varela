using Core.Steering;
using Unity.Collections;
using Unity.Mathematics;

namespace Controllers
{
    /// <summary /> Flocking implementation designed to iterate over the entire neighbourhood for each Compute operation.
    public class Flocking
    {
        /// <summary /> Calculates a vector to separate the subject from its neighbours so they don't collide.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subjectIndex">The index for the boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        public float3 ComputeSeparation(NativeList<Boid> flock, Boid subject, float rangeSqr)
        {
            float3 separation = float3.zero;
            int inRangeCount = 0;

            foreach (Boid boid in flock)
            {
                if (subject.Id == boid.Id)
                    continue;
                float3 subjectToNeighbour = boid.Position - subject.Position;
                float sqrDistance = math.lengthsq(subjectToNeighbour);

                if (sqrDistance > rangeSqr || sqrDistance < 0.001f)
                    continue;

                inRangeCount++;
                separation -= subjectToNeighbour / sqrDistance;
            }

            return inRangeCount > 0
                       ? separation / inRangeCount
                       : float3.zero;
        }

        /// <summary /> Calculates a vector to align the subject's direction and velocity to its pairs.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subjectIndex">The index for the boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        public float3 ComputeAlignment(NativeList<Boid> flock, Boid subject, float rangeSqr)
        {
            float3 velocity = float3.zero;
            int inRangeCount = 0;

            foreach (Boid neighbour in flock)
            {
                if (subject.Id == neighbour.Id)
                    continue;
                float3 subjectToNeighbour = neighbour.Position - subject.Position;
                float sqrDistance = math.lengthsq(subjectToNeighbour);

                if (sqrDistance > rangeSqr)
                    continue;

                inRangeCount++;
                velocity += neighbour.Velocity;
            }

            return inRangeCount > 0
                       ? velocity / inRangeCount
                       : float3.zero;
        }

        /// <summary /> Calculates a vector to keep flock neighbourhoods centered around a cohesive point.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subjectIndex">The index for the boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        /// <returns></returns>
        public float3 ComputeCohesion(NativeList<Boid> flock, Boid subject, float rangeSqr)
        {
            float3 center = float3.zero;
            int inRangeCount = 0;

            foreach (Boid neighbour in flock)
            {
                if (subject.Id == neighbour.Id)
                    continue;
                float3 subjectToNeighbour = neighbour.Position - subject.Position;

                if (math.lengthsq(subjectToNeighbour) > rangeSqr || math.lengthsq(subjectToNeighbour) < .5f)
                    continue;

                inRangeCount++;
                center += neighbour.Position;
            }

            return inRangeCount > 0
                       ? math.normalize(center / inRangeCount - subject.Position)
                       : float3.zero;
        }
    }
}