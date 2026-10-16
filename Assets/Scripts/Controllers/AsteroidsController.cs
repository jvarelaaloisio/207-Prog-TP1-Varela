using System.Collections.Generic;
using Core.Game;
using Core.Services;
using HealthSystem.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using VarelaAloisio.Core;

namespace Controllers
{
    public class AsteroidsController : MacacoBehaviour
    {
        [SerializeField] private InputActionReference clickInput;
        [SerializeField] private Camera camera;
        [SerializeField] private Ref<IHealthComponent> asteroidPrefab;
        private List<DestroyableObjective> _objectives = new ();
        protected override void OnEnable()
        {
            base.OnEnable();
            if (clickInput)
            {
                clickInput.action.Enable();
                clickInput.action.performed += HandleClick;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (clickInput)
            {
                clickInput.action.Disable();
                clickInput.action.performed -= HandleClick;
            }
        }

        private void HandleClick(InputAction.CallbackContext _)
        {
            Vector3 position = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            position.z = 0;
            IHealthComponent asteroid = asteroidPrefab.Instantiate(transform);
            if (asteroid is Component component)
                component.transform.position = position;
            var objective = new DestroyableObjective(asteroid, position);
            _objectives.Add(objective);
            asteroid.Health.OnDeath += HandleAsteroidDeath;
            if (Service.TryGet(out IObjectivesRepository objectivesRepository))
                objectivesRepository.Add(objective);
            else
                LogError($"{nameof(IObjectivesRepository)} not found");
        }

        private void HandleAsteroidDeath()
        {
            //Remove asteroid
        }
    }
}
