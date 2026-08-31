using Core.Game;
using Unity.Mathematics;

namespace Steering
{
    public class Flocking
    {
        /// <summary /> Calculates a vector to separate the subject from its neighbours so they don't collide.
        /// <param name="subject">The boid being controlled</param>
        /// <param name="other">The boid to compare to</param>
        /// <param name="desiredSqrDistance">The "personal space" the subject desires</param>
        /// <param name="hasInfluence">If the other boid is closer than the desired distance, thus returning a value greater than 0</param>
        /// <returns>If the distance between boids is greater or equal than <see cref="desiredSqrDistance"/>, then <see cref="float3.zero"/>.
        /// Otherwise, it returns a vector to separate both towards the desired distance.
        /// </returns>
        public float3 ComputeSeparation(Boid subject, Boid other, float desiredSqrDistance, out bool hasInfluence)
        {
            float3 otherToSubject = subject.Position - other.Position;
            float sqrDistance = math.lengthsq(otherToSubject);

            hasInfluence = sqrDistance < desiredSqrDistance;
            return hasInfluence
                       ? otherToSubject / math.max(sqrDistance, .001f)
                       : float3.zero;
        }

        /// <summary /> Calculates a vector to align the subject's direction to the other.
        /// <param name="subject">The boid being controlled</param>
        /// <param name="other">The boid to compare to</param>
        /// <param name="influenceSqrDistance">The influence range (squared)</param>
        /// <param name="hasInfluence">If the other boid is inside the influence range, thus returning a value greater than 0</param>
        /// <returns>If it's in the influence range, then the other's velocity
        /// Otherwise, it returns <see cref="float3.zero"/>
        /// </returns>
        public float3 ComputeAlignment(Boid subject, Boid other, float influenceSqrDistance, out bool hasInfluence)
        {
            float3 subjectToOther = other.Position - subject.Position;
            float sqrDistance = math.lengthsq(subjectToOther);

            hasInfluence = sqrDistance <= influenceSqrDistance;
            return hasInfluence
                       ? other.Velocity
                       : float3.zero;
        }

        /// <summary /> Calculates a vector to keep flock neighbourhoods centered around a cohesive point.
        /// <param name="subject">The boid being controlled</param>
        /// <param name="other">The boid to compare to</param>
        /// <param name="influenceSqrDistance">The influence range (squared)</param>
        /// <param name="hasInfluence">If the other boid is inside the influence range, thus returning a value greater than 0</param>
        /// <returns>If it's in the influence range, then a vector pointing to the center between the two boids.
        /// Otherwise, it returns <see cref="float3.zero"/>
        /// </returns>
        public float3 ComputeCohesion(Boid subject, Boid other, float influenceSqrDistance, out bool hasInfluence)
        {
            float3 subjectToOther = other.Position - subject.Position;

            hasInfluence = math.lengthsq(subjectToOther) <= influenceSqrDistance;
            return hasInfluence
                       ? subjectToOther / 2
                       : float3.zero;
        }
    }
}
