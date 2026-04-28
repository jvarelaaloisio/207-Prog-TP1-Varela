using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
using Core.Utils;
using UnityEngine;
using VarelaAloisio.Core;

namespace Units
{
    public class UnitsRepository : MonoBehaviour, IUnitsRepository
    {
        [SerializeField] private Ship shipPrefab;
        [SerializeField] private Ship playerShipPrefab;
        [SerializeField] private Ship enemyShipPrefab;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private DeathRay deathRayPrefab;
        private Transform _shipsParent;
        private Transform _bulletsParent;
        private readonly Dictionary<ShipType, List<IShip>> _ships = new();
        private readonly Dictionary<BulletType, List<IBullet>> _bullets = new();

        private void Awake()
            => Service.Add<IUnitsRepository>(this);

        private void Start()
        {
            _shipsParent = new GameObject("Ships").transform;
            _shipsParent.transform.SetParent(transform);
            _bulletsParent = new GameObject("Bullets").transform;
            _bulletsParent.transform.SetParent(transform);
        }

        private void OnDestroy()
            => Service.Remove<IUnitsRepository>();

    #region Ships

        /// <inheritdoc />
        public event Action<IShip, Team> OnShipSpawned;

        /// <inheritdoc />
        public event Action<IShip, Team> OnShipDestroyed;

        public Factory<IShip> GetShipFactory(ShipType type)
        {
            return type switch
                   {
                       ShipType.Player => new Factory<IShip>(SpawnPlayer, DestroyShip),
                       ShipType.Enemy => new Factory<IShip>(SpawnEnemy, DestroyShip),
                       _ => new Factory<IShip>(SpawnGenericShip, DestroyShip)
                   };
        }

        /// <inheritdoc />
        public bool TryGetShipsOfType(ShipType type, out IShip[] result)
        {
            bool found = _ships.TryGetValue(type, out var ships);
            result = new IShip[ships?.Count ?? 0];
            ships?.CopyTo(result);
            return found;
        }

        //TODO: Use InstantiateAsync to instantiate multiple objects
        //TODO: Hook to pool
        private IShip SpawnPlayer()
            => SpawnShip(playerShipPrefab, ShipType.Player, Team.Player);

        private IShip SpawnEnemy()
            => SpawnShip(enemyShipPrefab, ShipType.Enemy, Team.Enemy);

        private IShip SpawnGenericShip()
            => SpawnShip(shipPrefab, ShipType.None, Team.None);

        private IShip SpawnShip(Ship prefab, ShipType type, Team team)
        {
            Ship ship = Instantiate(prefab, _shipsParent);
            ship.name = $"Ship ({type})";
            ship.OnKill += RemoveShip;
            if (_ships.TryGetValue(type, out var ships))
                ships.Add(ship);
            else
                _ships.Add(type, new() { ship });
            OnShipSpawned?.Invoke(ship, team);
            return ship;
        }

        private void RemoveShip(IShip destroyedShip)
        {
            OnShipDestroyed?.Invoke(destroyedShip, destroyedShip.Team);
            foreach ((ShipType type, var ships) in _ships)
            {
                IShip ship = ships.FirstOrDefault(ship => ReferenceEquals(ship, destroyedShip));
                if (ship is null)
                    continue;
                _ships[type].Remove(ship);
                return;
            }
        }

        //TODO: Return to pull
        private void DestroyShip(IShip ship)
            => Destroy(ship.gameObject);

    #endregion

    #region Bullets

        public Factory<IBullet> GetBulletFactory(BulletType type)
        {
            return type switch
                   {
                       BulletType.Missile => new Factory<IBullet>(SpawnBullet, DestroyBullet),
                       BulletType.DeathRay => new Factory<IBullet>(SpawnDeathRay, DestroyBullet),
                       _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                   };
        }
        
        /// <inheritdoc />
        public bool TryGetBulletsOfType(BulletType type, out IBullet[] result)
        {
            bool found = _bullets.TryGetValue(type, out var bullets);
            result = new IBullet[bullets?.Count ?? 0];
            bullets?.CopyTo(result);
            return found;
        }

        //TODO: Use InstantiateAsync to instantiate multiple objects
        //TODO: Hook to pool
        private IBullet SpawnBullet()
        {
            IBullet bullet = Instantiate(bulletPrefab, _bulletsParent);
            if (_bullets.TryGetValue(BulletType.Missile, out var bullets))
                bullets.Add(bullet);
            else
                _bullets.Add(BulletType.Missile, new() { bullet });

            return bullet;
        }

        private IBullet SpawnDeathRay()
        {
            IBullet bullet = Instantiate(deathRayPrefab, _bulletsParent);
            
            if (_bullets.TryGetValue(BulletType.DeathRay, out var bullets))
                bullets.Add(bullet);
            else
                _bullets.Add(BulletType.DeathRay, new() { bullet });
            return bullet;
        }

        //TODO: Return to pull
        private void DestroyBullet(IBullet bullet)
            => Destroy(bullet.gameObject);

    #endregion
    }
}
