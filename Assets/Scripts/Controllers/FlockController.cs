using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Game;
using Core.Steering;
using Unity.Mathematics;
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

        [Header("Spawning")]
        [SerializeField] private bool doSpawnPerFrame = true;
        [SerializeField] private int spawnsPerFrame = 10;
        [Tooltip("Only used if spawnPerFrame is false")]
        [SerializeField] private float spawnPeriod = .1f;

        private Flocking _flocking;

        public float3 Destination { get; private set; } = float3.zero;
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
                if (doSpawnPerFrame)
                {
                    for (int j = 0; i < flockCount && j < spawnsPerFrame; i++, j++)
                        Spawn();
                    await Awaitable.NextFrameAsync();
                }
                else
                {
                    Spawn();
                    await Awaitable.WaitForSecondsAsync(spawnPeriod);
                }
            }

            void Spawn()
            {
                Flock.Add(new Boid {
                                       Position = position,
                                       Velocity = Vector3.right * speed
                                   });
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
                float3 separationDirection = _flocking.ComputeSeparation(Flock, i, separation.RangeSqr) * separation.Weight;
                float3 alignmentDirection = _flocking.ComputeAlignment(Flock, i, alignment.RangeSqr) * alignment.Weight;
                float3 cohesionDirection = _flocking.ComputeCohesion(Flock, i, cohesion.RangeSqr) * cohesion.Weight;
                float3 destinationDirection = math.normalize(Destination - subject.Position) * destinationWeight;
                float3 direction = separationDirection
                                   + alignmentDirection
                                   + cohesionDirection
                                   + destinationDirection;
                direction.z = 0;
                subject.Velocity = Vector3.RotateTowards(subject.Velocity, math.normalize(direction) * speed, steeringSpeed * Time.deltaTime, 1.0f);
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