using System;
using Core.Collections;
using Core.Steering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Controllers
{
    /// <summary /> Flocking implementation designed to iterate over the entire neighbourhood for each Compute operation.
    public struct Flocking
    {
        [Serializable]
        public struct DataBlock : ISerializationCallbackReceiver
        {
            [field: SerializeField] public float Range { get; private set; }
            [field: SerializeField] public float Weight { get; private set; }
            public float RangeSqr { get; private set; }

            /// <inheritdoc />
            public void OnBeforeSerialize()
                => RangeSqr = Range * Range;

            /// <inheritdoc />
            public void OnAfterDeserialize()
                => RangeSqr = Range * Range;
        }
        /// <summary /> Calculates a vector to separate the subject from its neighbours so they don't collide.
        /// <param name="flock">All other boids in the flock</param>
        /// <param name="subject">The boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        public static float3 ComputeSeparation(NativeList<Boid> flock, Boid subject, float rangeSqr)
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
        /// <param name="subject">The boid being controlled</param>
        /// <param name="rangeSqr">The maximum distance (squared) around the subject for neighbours to affect them.</param>
        public static float3 ComputeAlignment(NativeList<Boid> flock, Boid subject, float rangeSqr)
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
        public static float3 ComputeCohesion(NativeList<Boid> flock, Boid subject, float rangeSqr)
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

        /// <summary /> Creates a SteeringJob that steers each boid using this Flocking, leaving the parameters to be set.
        /// <param name="flock">All the boids being steered.</param>
        /// <param name="grid">The Spatial Hash used to query each boid's neighbourhood.</param>
        /// <param name="separation"></param>
        /// <param name="alignment"></param>
        /// <param name="cohesion"></param>
        /// <param name="destination"></param>
        /// <param name="destinationWeight"></param>
        /// <param name="speed"></param>
        /// <param name="steeringSpeed"></param>
        /// <param name="deltaTime"></param>
        /// <returns>A Steering Job to be scheduled</returns>
        public Steer GetJob(NativeArray<Boid> flock,
                                  NativeSpatialHash<Boid> grid,
                                  DataBlock separation,
                                  DataBlock alignment,
                                  DataBlock cohesion,
                                  float3 destination,
                                  float destinationWeight,
                                  float speed,
                                  float steeringSpeed,
                                  float deltaTime)
            => new() {
                         Flock = flock,
                         Grid = grid,
                         NeighbourhoodRadius = Mathf.Max(separation.Range, alignment.Range, cohesion.Range),
                         Separation = separation,
                         Alignment = alignment,
                         Cohesion = cohesion,
                         Destination = destination,
                         DestinationWeight = destinationWeight,
                         Speed = speed,
                         SteeringSpeed = steeringSpeed,
                         DeltaTime = deltaTime,
                     };

        [BurstCompile]
        public struct Steer : IJobParallelFor
        {
            public NativeArray<Boid> Flock;
            [ReadOnly] public NativeSpatialHash<Boid> Grid;
            public float NeighbourhoodRadius;
            public DataBlock Separation;
            public DataBlock Alignment;
            public DataBlock Cohesion;
            public float3 Destination;
            public float DestinationWeight;
            public float SteeringSpeed;
            public float Speed;
            public float DeltaTime;

            /// <inheritdoc />
            public void Execute(int index)
            {
                Boid subject = Flock[index];
                float2 position = subject.Position.xy;
                var neighbours = new NativeList<Boid>(24, Allocator.Temp);
                Grid.Query(position, NeighbourhoodRadius, ref neighbours);

                float3 separationDirection = ComputeSeparation(neighbours, subject, Separation.RangeSqr) * Separation.Weight;
                float3 alignmentDirection = ComputeAlignment(neighbours, subject, Alignment.RangeSqr) * Alignment.Weight;
                float3 cohesionDirection = ComputeCohesion(neighbours, subject, Cohesion.RangeSqr) * Cohesion.Weight;
                float3 destinationDirection = math.normalize(Destination - subject.Position) * DestinationWeight;
                float3 direction = separationDirection
                                   + alignmentDirection
                                   + cohesionDirection
                                   + destinationDirection;
                direction.z = 0;
                subject.Velocity = RotateTowards(math.normalize(subject.Velocity), math.normalize(direction), SteeringSpeed * DeltaTime);
                subject.Position += subject.Velocity * (Speed * DeltaTime);
                Flock[index] = subject;
            }

            private static float3 RotateTowards(float3 current, float3 target, float maxRadiansDelta)
            {
                float3 currentDirection = math.normalize(current);
                float3 targetDirection = math.normalize(target);
                float dot = math.clamp(math.dot(currentDirection, targetDirection), -1f, 1f);
                if (dot >= 1f - 1e-6f)
                    return currentDirection;
                float angle = math.acos(dot);
                if (angle <= maxRadiansDelta)
                    return targetDirection;
                float3 axis = math.cross(currentDirection, targetDirection);
                if (math.lengthsq(axis) < 1e-8f)
                {
                    float3 fallback = math.abs(currentDirection.y) < 0.9f
                                          ? new float3(0, 1, 0)
                                          : new float3(1, 0, 0);
                    axis = math.normalize(math.cross(currentDirection, fallback));
                }
                else
                    axis = math.normalize(axis);
                return math.mul(quaternion.AxisAngle(axis, maxRadiansDelta), currentDirection);
            }
        }
    }
}