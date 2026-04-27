using System.Collections.Generic;
using Core.Game;
using Core.Game.Enums;
using Core.Utils;

namespace Core.Services
{
    public interface IUnitsRepository
    {
        Factory<IShip> GetShipFactory(ShipType type);
        Factory<IBullet> GetBulletFactory(BulletType type);
        /// <summary /> Try to get the currently spawned ships with the given type
        bool TryGetShipsOfType(ShipType type, out IReadOnlyList<IShip> result);
        /// <summary /> Try to get the currently spawned bullets with the given type
        bool TryGetBulletsOfType(BulletType type, out IReadOnlyList<IBullet> result);
    }
}