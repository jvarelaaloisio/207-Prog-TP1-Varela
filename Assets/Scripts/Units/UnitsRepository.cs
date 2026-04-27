using System;
using System.Collections.Generic;
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
        public bool TryGetShipsOfType(ShipType type, out IReadOnlyList<IShip> result)
        {
            bool found = _ships.TryGetValue(type, out var ships);
            result = ships;
            return found;
        }

        //TODO: Use InstantiateAsync to instantiate multiple objects
        //TODO: Hook to pool
        private IShip SpawnPlayer()
        {
            Ship ship = Instantiate(playerShipPrefab, _shipsParent);
            if (_ships.TryGetValue(ShipType.Player, out var ships))
                ships.Add(ship);
            else
                _ships.Add(ShipType.Player, new() { ship });
            return ship;
        }

        private IShip SpawnEnemy()
        {
            Ship ship = Instantiate(enemyShipPrefab, _shipsParent);
            if (_ships.TryGetValue(ShipType.Enemy, out var ships))
                ships.Add(ship);
            else
                _ships.Add(ShipType.Enemy, new() { ship });
            return ship;
        }

        private IShip SpawnGenericShip()
        {
            Ship ship = Instantiate(shipPrefab, _shipsParent);
            if (_ships.TryGetValue(ShipType.None, out var ships))
                ships.Add(ship);
            else
                _ships.Add(ShipType.None, new() { ship });
            return ship;
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
        public bool TryGetBulletsOfType(BulletType type, out IReadOnlyList<IBullet> result)
        {
            bool found = _bullets.TryGetValue(type, out var bullets);
            result = bullets;
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
