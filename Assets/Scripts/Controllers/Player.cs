using System.Threading;
using Core.Game;
using Core.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VarelaAloisio.Core;
using VarelaAloisio.Core.Utils;

namespace Controllers
{
    public class Player : ShipController
    {
        [SerializeField] private InputActionReference moveInput;
        [SerializeField] private InputActionReference shootInput;
        [SerializeField] private InputActionReference controllerLookInput;
        [SerializeField] private InputActionReference mouseLookInput;

        private CancellationTokenSource _shootSource;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (moveInput)
            {
                moveInput.action.Enable();
                moveInput.action.started += HandleMove;
                moveInput.action.performed += HandleMove;
                moveInput.action.canceled += HandleMove;
            }

            if (shootInput)
            {
                shootInput.action.Enable();
                shootInput.action.started += StartShooting;
                shootInput.action.canceled += StopShooting;
            }

            if (controllerLookInput)
            {
                controllerLookInput.action.Enable();
                controllerLookInput.action.started += HandleLookViaGamepad;
                controllerLookInput.action.performed += HandleLookViaGamepad;
                controllerLookInput.action.canceled += HandleLookViaGamepad;
            }

            if (mouseLookInput)
            {
                mouseLookInput.action.Enable();
                mouseLookInput.action.started += HandleLookViaMouse;
                mouseLookInput.action.performed += HandleLookViaMouse;
                mouseLookInput.action.canceled += HandleLookViaMouse;
            }
        }

        protected override void OnDisable()
        {
            TokenUtils.CancelAndDispose(ref _shootSource);
            if (moveInput)
            {
                moveInput.action.Disable();
                moveInput.action.started -= HandleMove;
                moveInput.action.performed -= HandleMove;
                moveInput.action.canceled -= HandleMove;
            }

            if (shootInput)
            {
                shootInput.action.Disable();
                shootInput.action.started -= StartShooting;
                shootInput.action.canceled -= StopShooting;
            }

            if (controllerLookInput)
            {
                controllerLookInput.action.Disable();
                controllerLookInput.action.started -= HandleLookViaGamepad;
                controllerLookInput.action.performed -= HandleLookViaGamepad;
                controllerLookInput.action.canceled -= HandleLookViaGamepad;
            }

            if (mouseLookInput)
            {
                mouseLookInput.action.Disable();
                mouseLookInput.action.started -= HandleLookViaMouse;
                mouseLookInput.action.performed -= HandleLookViaMouse;
                mouseLookInput.action.canceled -= HandleLookViaMouse;
            }
        }

        /// <inheritdoc />
        public override void Inject(IShip ship)
        {
            base.Inject(ship);
            ship.OnKill += DestroySelf;
        }

        private void HandleMove(InputAction.CallbackContext input)
        {
            if (Ship is null)
                return;
            Ship.MoveDirection = input.ReadValue<Vector2>();
        }

        private void StartShooting(InputAction.CallbackContext _)
        {
            if (Ship is null)
                return;
            TokenUtils.Recreate(ref _shootSource);
            Ship.ShootPrimaryPeriodically(_shootSource.Token);
        }

        private void StopShooting(InputAction.CallbackContext _)
            => TokenUtils.CancelAndDispose(ref _shootSource);

        private void HandleLookViaGamepad(InputAction.CallbackContext input)
        {
            if (Ship is null)
                return;
            var inputDirection = input.ReadValue<Vector2>();
            Ship.Direction = inputDirection;
        }
        private void HandleLookViaMouse(InputAction.CallbackContext input)
        {
            if (Ship is null)
                return;
            Vector2 shipPosition = Camera.main.WorldToScreenPoint(Ship.transform.position);
            var inputDirection = input.ReadValue<Vector2>();
            Ship.Direction = (inputDirection - shipPosition).normalized;
        }

        private void DestroySelf(IShip _)
            => Destroy(gameObject);
    }
}
