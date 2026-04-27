using UnityEngine;

namespace HealthSystem.Runtime.Components
{
    public class HealthComponent : MonoBehaviour, IHealthComponent
    {
        [field: SerializeField] public int MaxHp { get; set; } = 10;
        [field: SerializeField] public int InitialHp { get; set; } = 10;
        [field: SerializeField] public bool CanOverFlowHp { get; set; }
        
        [SerializeField] private bool setupInAwake = true;

        public Health Health { get; private set; }

        protected virtual void Awake()
        {
            if (setupInAwake)
                Setup();
        }

        public virtual void Setup()
        {
            Health = new Health(MaxHp, InitialHp, CanOverFlowHp);
        }

        public virtual void TakeDamage(int damage) => Health.TakeDamage(damage);
    }
}