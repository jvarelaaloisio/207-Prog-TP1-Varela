using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Collections;
using Core.Steering;
using Unity.Collections;
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
        [SerializeField] private bool doDrawNeighboursGizmo;
        [SerializeField] private bool doDrawGridGizmo;
        [SerializeField] private float destinationWeight = 1f;
        [SerializeField] private float steeringSpeed = 2;
        [SerializeField] private float speed = 1;

        [Header("Spawning")]
        [SerializeField] private bool doSpawnPerFrame = true;
        [SerializeField] private int spawnsPerFrame = 10;
        [Tooltip("Only used if spawnPerFrame is false")]
        [SerializeField] private float spawnPeriod = .1f;

        [Header("Grid")]
        [Min(0.1f)]
        [SerializeField] private float cellSize;
        [SerializeField] private float gridMin;
        [SerializeField] private float gridMax;

        private Flocking _flocking;
        private SpatialHash<Boid> _flockGrid;

        public float3 Destination { get; private set; } = float3.zero;
        public List<Boid> Flock { get; private set; } = new();

        private void OnValidate()
        {
            _flockGrid?.Dispose();
            _flockGrid = new SpatialHash<Boid>(cellSize, flockCount);
        }

        protected override void Awake()
        {
            base.Awake();
            _flocking = new Flocking();
            _flockGrid?.Dispose();
            _flockGrid = new SpatialHash<Boid>(cellSize, flockCount);
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _flockGrid?.Dispose();
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
                        Spawn(i);
                    await Awaitable.NextFrameAsync();
                }
                else
                {
                    Spawn(i);
                    await Awaitable.WaitForSecondsAsync(spawnPeriod);
                }
            }

            void Spawn(int i)
            {
                Flock.Add(new Boid {
                                       Position = position,
                                       Velocity = Vector3.right * speed,
                                       Id = i,
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
            _flockGrid.Clear();
            foreach (Boid boid in Flock)
                _flockGrid.Add(boid, boid.Position.xy);
            for (int i = 0; i < Flock.Count; i++)
            {
                Boid subject = Flock[i];
                var neighbours = new NativeList<Boid>(100, Allocator.Temp);
                _flockGrid.Query(subject.Position.xy, separation.Range, ref neighbours);
                float3 separationDirection = _flocking.ComputeSeparation(neighbours, subject, separation.RangeSqr) * separation.Weight;
                float3 alignmentDirection = _flocking.ComputeAlignment(neighbours, subject, alignment.RangeSqr) * alignment.Weight;
                float3 cohesionDirection = _flocking.ComputeCohesion(neighbours, subject, cohesion.RangeSqr) * cohesion.Weight;
                float3 destinationDirection = math.normalize(Destination - subject.Position) * destinationWeight;
                float3 direction = separationDirection
                                   + alignmentDirection
                                   + cohesionDirection
                                   + destinationDirection;
                if (doDrawNeighboursGizmo)
                    foreach (Boid neighbour in neighbours)
                        DrawLine(subject.Position, neighbour.Position, Color.darkGreen);
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

        #region Hash Grid

            if (!doDrawGridGizmo)
                return;
            var bottomLeft = new Vector3(gridMin, gridMin);
            var bottomRight = new Vector3(gridMax, gridMin);
            var topLeft = new Vector3(gridMin, gridMax);
            var topRight = new Vector3(gridMax, gridMax);
            Gizmos.color = Color.aquamarine;

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);

            if (_flockGrid is {CellSize: >0})
            {
                for (float x = gridMin + _flockGrid.CellSize; x < gridMax; x+=_flockGrid.CellSize)
                {
                    var bottom = new Vector3(x, bottomLeft.y, bottomLeft.z);
                    var top = new Vector3(x, topLeft.y, topLeft.z);
                    Gizmos.DrawLine(bottom, top);
                }

                for (float y = gridMin; y < gridMax; y += _flockGrid.CellSize)
                {
                    var left = new Vector3(bottomLeft.x, y, bottomLeft.z);
                    var right = new Vector3(bottomRight.x, y, bottomRight.z);
                    Gizmos.DrawLine(left, right);
                }
            }
        #endregion
            
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