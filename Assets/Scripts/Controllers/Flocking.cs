using System.Collections.Generic;
using Core.Game;
using UnityEngine;

namespace Controllers
{
    public class Flocking
    {
        /// <summary /> Calculates a vector to separate the subject from its neighbours so they don't collide.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subjectIndex">The index for the boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        public Vector3 ComputeSeparation(IList<Boid> flock, int subjectIndex, float rangeSqr)
        {
            Vector3 separation = Vector3.zero;
            int inRangeCount = 0;

            Boid subject = flock[subjectIndex];
            for (int i = 0; i < flock.Count; i++)
            {
                if (i == subjectIndex)
                    continue;
                Boid boid = flock[i];
                Vector3 subjectToNeighbour = boid.Position - subject.Position;
                float sqrDistance = subjectToNeighbour.sqrMagnitude;

                if (sqrDistance > rangeSqr || sqrDistance < 0.001f)
                    continue;

                inRangeCount++;
                separation -= subjectToNeighbour / sqrDistance;
            }

            return inRangeCount > 0
                       ? separation / inRangeCount
                       : Vector3.zero;
        }

        /// <summary /> Calculates a vector to align the subject's direction and velocity to its pairs.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subjectIndex">The index for the boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        public Vector3 ComputeAlignment(IList<Boid> flock, int subjectIndex, float rangeSqr)
        {
            Vector3 velocity = Vector3.zero;
            int inRangeCount = 0;

            Boid subject = flock[subjectIndex];
            for (int i = 0; i < flock.Count; i++)
            {
                if (i == subjectIndex)
                    continue;
                Boid neighbour = flock[i];
                Vector3 subjectToNeighbour = neighbour.Position - subject.Position;
                float sqrDistance = subjectToNeighbour.sqrMagnitude;

                if (sqrDistance > rangeSqr)
                    continue;

                inRangeCount++;
                velocity += neighbour.Velocity;
            }

            return inRangeCount > 0
                       ? velocity / inRangeCount
                       : Vector3.zero;
        }

        /// <summary /> Calculates a vector to keep flock neighbourhoods centered around a cohesive point.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subjectIndex">The index for the boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        /// <returns></returns>
        public Vector3 ComputeCohesion(IList<Boid> flock, int subjectIndex, float rangeSqr)
        {
            Vector3 center = Vector3.zero;
            int inRangeCount = 0;

            Boid subject = flock[subjectIndex];
            for (int i = 0; i < flock.Count; i++)
            {
                if (i == subjectIndex)
                    continue;
                Boid neighbour = flock[i];
                Vector3 subjectToNeighbour = neighbour.Position - subject.Position;

                if (subjectToNeighbour.sqrMagnitude > rangeSqr || subjectToNeighbour.sqrMagnitude < .5f)
                    continue;

                inRangeCount++;
                center += neighbour.Position;
            }

            return inRangeCount > 0
                       ? (center / inRangeCount - subject.Position).normalized
                       : Vector3.zero;
        }
    }
}