using System;
using System.Collections.Generic;
using Core.Game;
using Core.Game.Enums;
using Core.Utils;

namespace Core.Services
{
    public interface IUnitsRepository
    {
        /// <summary /> Event triggered every time a ship is spawned
        event Action<IShip, Team> OnShipSpawned;
        /// <summary />
        ///  Event triggered every time a ship is destroyed
        event Action<IShip, Team> OnShipDestroyed;
        Factory<IShip> GetShipFactory(ShipType type);
        Factory<IBullet> GetBulletFactory(BulletType type);
        /// <summary /> Try to get the currently spawned ships with the given type
        bool TryGetShipsOfType(ShipType type, out IShip[] result);
        /// <summary /> Try to get the currently spawned bullets with the given type
        bool TryGetBulletsOfType(BulletType type, out IBullet[] result);
    }
}