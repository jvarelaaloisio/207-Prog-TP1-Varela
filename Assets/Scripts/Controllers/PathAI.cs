using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
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

        private void Awake()
        {
            if (path)
                path.enabled = false;
        }

        private async void Start()
        {
            if (!Service.TryGet(out IUnitsRepository unitsRepository))
            {
                Debug.LogError($"Units repository not found");
                return;
            }

            IReadOnlyList<IShip> ships = null;
            while (!disableCancellationToken.IsCancellationRequested
                   && !unitsRepository.TryGetShipsOfType(ShipType.Player, out ships))
                await Awaitable.NextFrameAsync();
            if (disableCancellationToken.IsCancellationRequested)
                return;

            _playerShip = ships.FirstOrDefault();
            if (_playerShip is null)
                Debug.LogError($"Player ship not found");
        }

        /// <inheritdoc />
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
            if (Ship is null || !Ship.gameObject || _playerShip is null)
                return;
            Ship.gameObject.transform.up = ((_playerShip?.transform?.position ?? Vector3.zero) - Ship.transform.position).normalized;
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
    }
}
