using System;
using System.Threading;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
using Core.Utils;
using HealthSystem;
using HealthSystem.Runtime;
using HealthSystem.Runtime.Components;
using Units;
using UnityEngine;
using VarelaAloisio.Core;

namespace Management
{
    public class LevelManager : MonoBehaviourAsync
    {
        [Serializable]
        public class SpawnConfiguration : ISerializationCallbackReceiver
        {
#if UNITY_EDITOR
            [SerializeField, HideInInspector] private string editorName;
#endif
            [field: SerializeField] public ShipType Type { get; private set; }
            [field: SerializeField] public Pose Pose { get; private set; }
            [field: SerializeField] public ShipController Controller { get; private set; }
            [field: SerializeField] public int Health { get; private set; } = 10;
            [field: SerializeField] public Team Team { get; private set; }

            [field: Header("Timming")]
            [field: SerializeField] public float Delay { get; private set; }
            [field: Tooltip("If the spawn should continue periodically")]
            [field: SerializeField] public bool IsPeriodic { get; private set; }
            [field: SerializeField] public float Period { get; private set; } = 1f;
            [field: SerializeField] public int Quantity { get; private set; } = 1;

            [field: Header("Shooting")]
            [field: SerializeField] public BulletType PrimaryFireType { get; private set; }
            [field: SerializeField] public BulletType SecondaryFireType { get; private set; }

            /// <inheritdoc />
            public void OnBeforeSerialize()
            {
#if UNITY_EDITOR
                editorName = $"{Type} ({Controller?.name}) {(IsPeriodic ? $"(~{Period})" : "")}";
#endif
            }

            /// <inheritdoc />
            public void OnAfterDeserialize()
            { }
        }
        [SerializeField] private SpawnConfiguration[] spawns = Array.Empty<SpawnConfiguration>();
        private void Start()
        {
            if (!Service.TryGet(out IUnitsRepository unitsRepository))
            {
                Debug.LogError($"{name} <color=grey>({nameof(LevelManager)})</color>: {nameof(IUnitsRepository)} not found.",
                               this);
                return;
            }

            foreach (SpawnConfiguration configuration in spawns)
                Spawn(unitsRepository, configuration);

            return;

            async void Spawn(IUnitsRepository service, SpawnConfiguration configuration)
            {
                if (configuration.Delay > 0)
                    await Awaitable.WaitForSecondsAsync(configuration.Delay);
                if (disableCancellationToken.IsCancellationRequested)
                    return;

                var shipFactory = service.GetShipFactory(configuration.Type);
                var primaryFireFactory = service.GetBulletFactory(configuration.PrimaryFireType);
                var secondaryFireFactory = service.GetBulletFactory(configuration.SecondaryFireType);
                if (configuration.IsPeriodic)
                    DoSpawnPeriodically(shipFactory,
                                        primaryFireFactory,
                                        secondaryFireFactory,
                                        configuration,
                                        disableCancellationToken);
                else
                    SpawnShip(shipFactory,
                              configuration.Pose,
                              primaryFireFactory,
                              secondaryFireFactory,
                              configuration.Team,
                              configuration.Controller,
                              configuration.Health);
            }
        }

        private async void DoSpawnPeriodically(Factory<IShip> factory,
                                               Factory<IBullet> primaryBulletFactory,
                                               Factory<IBullet> secondaryBulletFactory,
                                               SpawnConfiguration config,
                                               CancellationToken token)
        {
            int remaining = config.Quantity;
            while (!token.IsCancellationRequested
                   && remaining-- > 0)
            {
                try
                {
                    SpawnShip(factory, config.Pose, primaryBulletFactory, secondaryBulletFactory, config.Team, config.Controller, config.Health);
                }
                catch (Exception e) { Debug.LogException(e); }

                await Awaitable.WaitForSecondsAsync(config.Period);
            }
        }

        //TODO: Add controller
        private static IShip SpawnShip(Factory<IShip> factory,
                                       Pose pose,
                                       Factory<IBullet> primaryBulletFactory,
                                       Factory<IBullet> secondaryBulletFactory,
                                       Team team,
                                       ShipController controllerPrefab,
                                       int health)
        {
            IShip ship = factory.Get();
            ship.gameObject.transform.SetPositionAndRotation(pose.position,
                                                             pose.rotation);
            ship.Inject(primaryBulletFactory, secondaryBulletFactory, team);
            if (controllerPrefab)
            {
                ShipController controller = Instantiate(controllerPrefab);
                controller.Inject(ship);
            }

            if (ship.gameObject.TryGetComponent(out HealthComponent healthComponent))
            {
                healthComponent.MaxHp = health;
                healthComponent.InitialHp = health;
                healthComponent.Setup();
            }
            return ship;
        }

        private void OnDrawGizmosSelected()
        {
            foreach (SpawnConfiguration enemySpawnPoint in spawns)
            {
                Gizmos.DrawIcon(enemySpawnPoint.Pose.position, "BuildSettings.Android On@2x", true, Color.darkRed);
            }
        }
    }
}
