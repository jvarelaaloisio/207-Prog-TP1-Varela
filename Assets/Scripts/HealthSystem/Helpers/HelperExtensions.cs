using UnityEngine;

namespace HealthSystem.Runtime.Helpers
{
    public static class HelperExtensions
    {
        public static bool TryToKill(this Component target)
        {
            if (!target.TryGetComponent(out IHealthComponent healthTarget))
                return false;
            healthTarget.Health.Kill();
            return true;

        }
        public static bool TryAttack(this Component target, int damage)
        {
            if (!target.TryGetComponent(out IHealthComponent healthTarget))
                return false;
            healthTarget.TakeDamage(damage);
            return true;

        }
    }
}