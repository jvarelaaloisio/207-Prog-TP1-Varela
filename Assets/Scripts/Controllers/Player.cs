using System.Threading;
using Core.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using VarelaAloisio.Core.Utils;

namespace Controllers
{
    public class Player : ShipController
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference shootAction;
        private CancellationTokenSource _shootSource;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            if (moveAction)
            {
                moveAction.action.Enable();
                moveAction.action.started += HandleMove;
                moveAction.action.performed += HandleMove;
                moveAction.action.canceled += HandleMove;
            }

            if (shootAction)
            {
                shootAction.action.Enable();
                shootAction.action.started += StartShooting;
                shootAction.action.canceled += StopShooting;
            }
        }

        protected override void OnDisable()
        {
            TokenUtils.CancelAndDispose(ref _shootSource);
            if (moveAction)
            {
                moveAction.action.Disable();
                moveAction.action.started -= HandleMove;
                moveAction.action.performed -= HandleMove;
                moveAction.action.canceled -= HandleMove;
            }

            if (shootAction)
            {
                shootAction.action.Disable();
                shootAction.action.started -= StartShooting;
                shootAction.action.canceled -= StopShooting;
            }
        }

        private void HandleMove(InputAction.CallbackContext input)
        {
            if (Ship is null)
                return;
            Ship.Direction = input.ReadValue<Vector2>();
        }

        private void StartShooting(InputAction.CallbackContext obj)
        {
            if (Ship is null)
                return;
            TokenUtils.Recreate(ref _shootSource);
            Ship.ShootPrimaryPeriodically(_shootSource.Token);
        }

        private void StopShooting(InputAction.CallbackContext obj)
            => TokenUtils.CancelAndDispose(ref _shootSource);
    }
}
