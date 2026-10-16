using System;
using HealthSystem.Runtime;
using Unity.Mathematics;

namespace Core.Game
{
    /// <summary>
    /// This is the representation of a static, destroyable objective.
    /// It's expected to be asteroids in the final game.
    /// The only weakness for this current structure is the inability to have a dynamic asteroid. This will probably hunt me in the future.
    /// </summary>
    public struct DestroyableObjective : IEquatable<DestroyableObjective>
    {
        public IHealthComponent Health;
        public float3 Position;
        public readonly int HashCode;

        public DestroyableObjective(IHealthComponent health, float3 position)
        {
            HashCode = System.HashCode.Combine(health, position);
            Health = health;
            Position = position;
        }

        public bool Equals(DestroyableObjective other)
            => HashCode == other.HashCode;

        public override bool Equals(object obj)
            => obj is DestroyableObjective other && Equals(other);

        public override int GetHashCode()
            => HashCode;
    }
}