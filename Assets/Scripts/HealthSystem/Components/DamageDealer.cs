using HealthSystem.Runtime.Helpers;
using UnityEngine;

namespace HealthSystem.Runtime.Components
{
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField] protected int damage;

        /// <summary>
        /// Deals damage to the target
        /// </summary>
        /// <param name="target"></param>
        protected virtual void Attack(IHealthComponent target) => target.TakeDamage(damage);

        /// <summary>
        /// Tries to get the HealthComponent in target. If successful, will call <see cref="Attack"/> on it
        /// </summary>
        /// <param name="target"></param>
        public virtual void TryAttack(Component target)
        {
            if (!target.TryAttack(damage))
                Debug.LogWarning($"{name}: Tried to attack {target.name}, but couldn't find the health component!");
        }
    }
}