using Unity.Mathematics;

namespace Core.Steering
{
    public struct Boid
    {
        public float3 Position { get; set; }
        public float3 Velocity { get; set; }
        public int Id { get; set; }
    }
}