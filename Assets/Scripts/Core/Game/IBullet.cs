using Core.Game.Enums;
using UnityEngine;

namespace Core.Game
{
    /// <summary /> The representation of a bullet. Can be a missile, a death ray, etc.
    public interface IBullet
    {
        GameObject gameObject { get; }
        /// <summary /> Set the base values for this bullet to work
        /// <param name="damage">The damage this bullet will apply to the hit target/s</param>
        /// <param name="team">The team corresponding to this bullet.</param>
        void Inject(int damage, Team team);
        /// <summary /> Call this to actually shoot the bullet
        /// <remarks>The bullets should be stationary/in a charging state as default
        /// and only start moving/attacking once this method is called.</remarks>
        void Shoot();
    }
}