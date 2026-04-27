using System.Collections;
using HealthSystem.Runtime.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.Runtime
{
    public class HealthComponentTests
    {
        private const int MaxHP = 10;
        private const int InititalHP = 5;
        private const int OverflowHP = 15;
        private GameObject _gameObject;
        private HealthComponent _healthComponent;


        [UnitySetUp]
        private IEnumerator SetUp()
        {
            _gameObject = new GameObject();
            _healthComponent = _gameObject.AddComponent<HealthComponent>();
            yield break;
        }

        [Test]
        public void GivenSetup_HealthIsNotNull()
        {
            _healthComponent.Setup();
            Assert.IsNotNull(_healthComponent.Health);
        }

        [Test]
        public void GivenMaxHP_HealthHasMaxHP()
        {
            _healthComponent.MaxHp = MaxHP;
            _healthComponent.Setup();
            Assert.AreEqual(_healthComponent.Health.MaxHP, MaxHP);
        }
        
        [Test]
        public void GivenInitialHP_HealthHasHPEqualToInitialHP()
        {
            _healthComponent.MaxHp = MaxHP;
            _healthComponent.InitialHp = InititalHP;
            _healthComponent.Setup();
            Assert.AreEqual(_healthComponent.Health.HP, InititalHP);
        }
        
        [Test]
        public void GivenCanOverflowHP_HealthHasHPOverflowed()
        {
            _healthComponent.MaxHp = MaxHP;
            _healthComponent.InitialHp = OverflowHP;
            _healthComponent.CanOverFlowHp = true;
            _healthComponent.Setup();
            Assert.AreEqual(_healthComponent.Health.HP, OverflowHP);
        }
    }
}
