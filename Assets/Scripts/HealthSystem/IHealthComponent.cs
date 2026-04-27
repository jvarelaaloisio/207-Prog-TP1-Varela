namespace HealthSystem.Runtime
{
    public interface IHealthComponent
    {
        Health Health { get; }
        int MaxHp { get; set; }
        int InitialHp { get; set; }
        bool CanOverFlowHp { get; set; }
        void TakeDamage(int damage);
    }
}