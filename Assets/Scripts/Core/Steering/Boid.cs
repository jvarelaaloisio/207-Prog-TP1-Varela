using Unity.Mathematics;

namespace Core.Steering
{
    public struct Boid : IPosition
    {
        public float3 Position { get; set; }
        public float3 Velocity { get; set; }
        public int Id { get; set; }
    }
}