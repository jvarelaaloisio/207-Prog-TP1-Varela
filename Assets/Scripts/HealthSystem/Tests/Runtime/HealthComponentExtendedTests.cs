using System.Collections;
using HealthSystem;
using HealthSystem.Runtime.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.Runtime
{
    public class HealthComponentExtendedTests
    {
        private const int MaxHP = 10;
        private const int HalfMaxHP = 5;
        private const int NonOverflowingHealAmount = 1;
        private GameObject _gameObject;
        private HealthComponentExtended _healthComponent;
        [UnitySetUp]
        private IEnumerator SetUp()
        {
            _gameObject = new GameObject();
            _healthComponent = _gameObject.AddComponent<HealthComponentExtended>();
            yield break;
        }
        
        [Test]
        public void GivenSetup_HealthIsNotNull()
        {
            _healthComponent.Setup();
            Assert.IsNotNull(_healthComponent.Health);
        }
        
        [Test]
        public void GivenOnHeal_OnHealIsCalled()
        {
            var healWasCalled = false;
            
            _healthComponent.MaxHp = MaxHP;
            _healthComponent.InitialHp = HalfMaxHP;
            _healthComponent.onHeal.AddListener((_, _) => healWasCalled = true);
            _healthComponent.Setup();

            _healthComponent.Health.Heal(NonOverflowingHealAmount);
            Assert.IsTrue(healWasCalled);
        }
        
        [Test]
        public void GivenOnHeal_OnHealGivesCorrectNumbers()
        {
            var beforeGiven = -1;
            var afterGiven = -1;
            
            _healthComponent.MaxHp = MaxHP;
            _healthComponent.InitialHp = HalfMaxHP;
            _healthComponent.onHeal.AddListener(HandleHeal);
            _healthComponent.Setup();

            _healthComponent.Health.Heal(NonOverflowingHealAmount);
            Assert.AreEqual(beforeGiven, HalfMaxHP);
            Assert.AreEqual(afterGiven, HalfMaxHP + NonOverflowingHealAmount);
            return;

            void HandleHeal(int before, int after)
            {
                beforeGiven = before;
                afterGiven = after;
            }
        }
    }
}
