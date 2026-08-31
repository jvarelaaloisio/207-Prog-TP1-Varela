using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Game;
using Core.Steering;
using UnityEngine;
using UnityEngine.InputSystem;
using VarelaAloisio.Core;

namespace Controllers
{
    public class FlockController : MacacoBehaviour, IFlockController
    {
        [Serializable]
        private class FlockDataBlock : ISerializationCallbackReceiver
        {
            [field: SerializeField] public float Range { get; private set; } = 1;
            [field: SerializeField] public float Weight { get; private set; } = 1;
            public float RangeSqr { get; private set; }

            /// <inheritdoc />
            public void OnBeforeSerialize()
                => RangeSqr = Range * Range;

            /// <inheritdoc />
            public void OnAfterDeserialize()
                => RangeSqr = Range * Range;
        }

        [SerializeField] private InputActionReference clickInput;
        [SerializeField] private Camera camera;

        [SerializeField] private int flockCount = 100;
        [SerializeField] private FlockDataBlock separation;
        [SerializeField] private bool separationGizmos;
        [SerializeField] private FlockDataBlock alignment;
        [SerializeField] private bool alignmentGizmos;
        [SerializeField] private FlockDataBlock cohesion;
        [SerializeField] private bool cohesionGizmos;
        [SerializeField] private float destinationWeight = 1f;
        [SerializeField] private float steeringSpeed = 2;
        [SerializeField] private float speed = 1;
        [SerializeField] private float spawnPeriod = .1f;
        private Flocking _flocking;
        public Vector3 Destination { get; private set; } = Vector3.right * 3;
        public List<Boid> Flock { get; private set; } = new();

        protected override void Awake()
        {
            base.Awake();
            _flocking = new Flocking();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (clickInput)
            {
                clickInput.action.Enable();
                clickInput.action.performed += HandleClick;
            }
            Flock = new List<Boid>(flockCount);
            _ = SpawnFlock(DisableCancellationToken);
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

        private async Task SpawnFlock(CancellationToken token)
        {
            Vector3 position = camera.ViewportToWorldPoint(new Vector3(.05f, .5f));
            position.z = 0;
            for (int i = 0; i < flockCount; i++)
            {
                if (token.IsCancellationRequested)
                    return;
                Flock.Add(new Boid {
                                        Position = position,
                                        Velocity = Vector3.right * speed
                                    });
                await Awaitable.WaitForSecondsAsync(spawnPeriod);
            }
        }

        private void HandleClick(InputAction.CallbackContext data)
        {
            Vector3 destination = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            destination.z = 0;
            Destination = destination;
        }

        private void Update()
        {
            for (int i = 0; i < Flock.Count; i++)
            {
                Boid subject = Flock[i];
                Vector3 separationDirection = _flocking.ComputeSeparation(Flock, i, separation.RangeSqr) * separation.Weight;
                DrawRay(subject.Position, separationDirection, new Color(0.5450981f, 0f, 0f, separationDirection.magnitude / 25));
                Vector3 alignmentDirection = _flocking.ComputeAlignment(Flock, i, alignment.RangeSqr) * alignment.Weight;
                DrawRay(subject.Position, alignmentDirection, new Color(0f, 0.3921569f, 0f, alignmentDirection.magnitude / 5));
                Vector3 cohesionDirection = _flocking.ComputeCohesion(Flock, i, cohesion.RangeSqr) * cohesion.Weight;
                DrawRay(subject.Position, cohesionDirection, new Color(0f, 0f, 0.5450981f, cohesionDirection.magnitude / 5));
                Vector3 destinationDirection = (Destination - subject.Position).normalized * destinationWeight;
                DrawRay(subject.Position, destinationDirection, new Color(0, 0, 0, destinationDirection.magnitude / 5));
                Vector3 direction = separationDirection
                                   + alignmentDirection
                                   + cohesionDirection
                                   + destinationDirection;
                direction.z = 0;
                subject.Velocity = Vector3.RotateTowards(subject.Velocity, direction.normalized * speed, steeringSpeed * Time.deltaTime, 1.0f);
                subject.Position += subject.Velocity * Time.deltaTime;
                Flock[i] = subject;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkRed;
            Gizmos.DrawWireSphere(Destination, 1f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 1, 0.35f);
            foreach (Boid boid in Flock)
            {
                if (separationGizmos)
                    Gizmos.DrawWireSphere(boid.Position, separation.Range);
                if (alignmentGizmos)
                    Gizmos.DrawWireSphere(boid.Position, alignment.Range);
                if (cohesionGizmos)
                    Gizmos.DrawWireSphere(boid.Position, cohesion.Range);
            }
        }
    }
}