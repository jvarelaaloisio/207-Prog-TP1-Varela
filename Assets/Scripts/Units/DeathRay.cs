using Core;
using Core.Game;
using Core.Game.Enums;
using UnityEngine;
using VarelaAloisio.Core;

namespace Units
{
    /// <summary>
    /// This class is not finished.
    /// Its scaffolding was added for future use, but due to a lack of time in the implementation, was delayed for future implementation.
    /// It is meant to be a secondary type ammo, which can be controlled as a bullet from outside 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    public class DeathRay : MacacoBehaviour, IBullet
    {
        private int _damage;

        /// <inheritdoc />
        public void Inject(int damage, Team team)
        {
            _damage = damage;
        }

        /// <inheritdoc />
        public void Shoot()
        {
            Debug.LogError($"{name} <color=grey>({nameof(DeathRay)})</color>: Death ray is not implemented yet!");
        }
    }
}