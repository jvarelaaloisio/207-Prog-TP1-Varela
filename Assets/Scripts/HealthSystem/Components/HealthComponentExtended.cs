using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem.Runtime.Components
{
    public class HealthComponentExtended : HealthComponent
    {
        [SerializeField] public UnityEvent onDeath = new();
        [SerializeField] public UnityEvent<int, int> onHeal = new();
        [SerializeField] public UnityEvent<int, int> onDamage = new();

        public override void Setup()
        {
            base.Setup();
            Health.OnDeath += onDeath.Invoke;
            Health.OnHeal += onHeal.Invoke;
            Health.OnDamage += onDamage.Invoke;
        }
    }
}