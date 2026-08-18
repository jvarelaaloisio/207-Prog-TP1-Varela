using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
using Units;
using UnityEngine;
using VarelaAloisio.Core;

namespace Controllers
{
    public class PathAI : ShipController
    {
        [SerializeField] private LineRenderer path;
        [SerializeField] private float shootingDelay = 1;
        [SerializeField] private float shootPeriod = 2;
        private IShip _playerShip;
        private bool _hasPlayerDied = false;

        private void Awake()
        {
            if (path)
                path.enabled = false;
        }

        private void Start()
        {
            if (!Service.TryGet(out IUnitsRepository unitsRepository))
            {
                Debug.LogError($"{name} <color=grey>({nameof(PathAI)})</color>: Units repository not found");
                return;
            }

            if (unitsRepository.TryGetShipsOfType(ShipType.Player, out var ships)
                && ships.Length > 0)
            {
                _playerShip = ships[0];
                _playerShip.OnKill += HandlePlayerDied;
            }
            else
            {
                unitsRepository.OnShipSpawned += HandleShipSpawned;
                Debug.Log($"{name} <color=grey>({nameof(PathAI)})</color>: Player ship not found.");
                _hasPlayerDied = true;
            }
        }

        public override void Inject(IShip ship)
        {
            base.Inject(ship);
            if (ship is null)
                return;
            ship.OnKill += DestroySelf;
            Ship.OverrideMovement(name, disableCancellationToken);
            Ship.OverrideRotation(name, disableCancellationToken);
            FollowPath(disableCancellationToken);
            ShootAfter(shootingDelay, disableCancellationToken);
        }

        private void Update()
        {
            if (Ship is null || !Ship.transform || _hasPlayerDied)
                return;
            Ship.transform.up = ((_playerShip?.transform?.position ?? Vector3.zero) - Ship.transform.position).normalized;
        }

        private void HandleShipSpawned(IShip ship, Team team)
        {
            if (team is Team.Player)
            {
                if (Service.TryGet(out IUnitsRepository unitsRepository))
                    unitsRepository.OnShipSpawned -= HandleShipSpawned;
                _playerShip = ship;
                _playerShip.OnKill += HandlePlayerDied;
                _hasPlayerDied = false;
            }
        }

        private async void FollowPath(CancellationToken token)
        {
            if (path.positionCount < 1)
            {
                Debug.LogWarning($"Path contains no positions, this AI won't move.");
                return;
            }

            var positions = new Vector3[path.positionCount];
            path.GetPositions(positions);
            int i = 0;
            Transform shipTransform = Ship.transform;
            while (!token.IsCancellationRequested)
            {
                Vector3 position = positions[i++ % positions.Length];
                float distance = (position - shipTransform.position).magnitude;
                float start = Time.time;
                Vector3 origin = shipTransform.position;
                float now = 0;
                do
                {
                    now = Time.time;
                    var lerp = (now - start) / (distance / Ship.MaxSpeed);
                    shipTransform.position = Vector3.Lerp(origin, position, lerp);
                    await Awaitable.NextFrameAsync();
                } while (now < start + distance / Ship.MaxSpeed
                         && !token.IsCancellationRequested);

                if (token.IsCancellationRequested)
                    return;
                shipTransform.position = position;
            }
        }

        private async void ShootAfter(float seconds, CancellationToken token)
        {
            await Awaitable.WaitForSecondsAsync(seconds);
            if (token.IsCancellationRequested)
                return;
            Ship.ShootPrimaryPeriodically(token, shootPeriod);
        }

        private void DestroySelf(IShip _)
            => Destroy(gameObject);

        private void HandlePlayerDied(IShip obj)
        {
            _hasPlayerDied = true;
            if (!Service.TryGet(out IUnitsRepository unitsRepository))
            {
                Debug.LogError($"{name} <color=grey>({nameof(PathAI)})</color>: Units repository not found");
                return;
            }

            unitsRepository.OnShipSpawned += HandleShipSpawned;
        }

        private void OnDestroy()
        {
            if (Ship is not null)
            {
                Ship.OnKill -= DestroySelf;
            }
        }
    }
}
