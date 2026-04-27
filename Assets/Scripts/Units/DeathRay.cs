using Core.Game;
using Core.Game.Enums;

namespace Units
{
    public class DeathRay : MonoBehaviourAsync, IBullet
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
            
        }
    }
}