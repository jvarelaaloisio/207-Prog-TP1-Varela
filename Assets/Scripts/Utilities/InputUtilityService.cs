using System;
using Core.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using VarelaAloisio.Core;
using VarelaAloisio.Core.Attributes;

namespace Utilities
{
    public class InputUtilityService : MonoBehaviour, IInputUtilityService
    {
        /// <inheritdoc />
        [field: SerializeField, ReadOnly]
        public bool IsUsingMouse { get; private set; }

        private IDisposable _buttonPressSubscription;

        private void Awake()
        {
            _buttonPressSubscription = InputSystem.onAnyButtonPress.Call(DetectDeviceAndUpdate);
            Service.Add<IInputUtilityService>(this);
        }

        private void OnDestroy()
        {
            _buttonPressSubscription.Dispose();
            Service.Remove<IInputUtilityService>();
        }

        private void DetectDeviceAndUpdate(InputControl control)
            => SetDevice(control.device);

        private void SetDevice(InputDevice device)
        {
            bool newValue = device is Keyboard or Pointer;
            if (newValue != IsUsingMouse)
                Debug.Log($"IsUsingMouse: {IsUsingMouse} => {newValue }");

            IsUsingMouse = newValue;
        }
    }
}