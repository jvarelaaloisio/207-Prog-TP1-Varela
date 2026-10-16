using System;
using Unity.Mathematics;

namespace Core.Steering
{
    public struct Boid : IPosition
    {
        public float3 Position { get; set; }
        public float3 Velocity { get; set; }
        public float3 Destination { get; set; }
        public int Id { get; set; }
    }

    [Flags]
    public enum BoidFlags
    {
        None = 0,
        IsActive = 1,
        HasDestination = 1 << 1,
        IsFollowingPlayer = 1 << 2,
    }
}