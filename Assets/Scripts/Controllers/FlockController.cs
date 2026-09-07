using System.Threading;
using System.Threading.Tasks;
using Core.Collections;
using Core.Steering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using VarelaAloisio.Core;

namespace Controllers
{
    public class FlockController : MacacoBehaviour, IFlockController
    {
        [SerializeField] private InputActionReference clickInput;
        [SerializeField] private Camera camera;

        [SerializeField] private int flockCount = 100;
        [SerializeField] private Flocking.DataBlock separation;
        [SerializeField] private bool separationGizmos;
        [SerializeField] private Flocking.DataBlock alignment;
        [SerializeField] private bool alignmentGizmos;
        [SerializeField] private Flocking.DataBlock cohesion;
        [SerializeField] private bool cohesionGizmos;
        [SerializeField] private bool doDrawNeighboursGizmo;
        [SerializeField] private bool doDrawGridGizmo;
        [SerializeField] private float destinationWeight = 1f;
        [SerializeField] private float steeringSpeed = 2;
        [SerializeField] private float speed = 1;

        [Header("Spawning")]
        [SerializeField] private bool doSpawnPerFrame = true;
        [SerializeField] private int spawnsBatchSize = 30;
        [Tooltip("Only used if spawnPerFrame is false")]
        [SerializeField] private float spawnPeriod = .1f;

        [Header("Grid")]
        [Min(0.1f)]
        [SerializeField] private float cellSize;
        [SerializeField] private float gridMin;
        [SerializeField] private float gridMax;

        private Flocking _flocking;
        private NativeSpatialHash<Boid> _flockGrid;

        public float3 Destination { get; private set; } = float3.zero;
        private NativeList<Boid> _flock;

        public NativeList<Boid> Flock => _flock;

        protected override void Awake()
        {
            base.Awake();
            _flocking = new Flocking();
            if (_flockGrid.IsCreated)
                _flockGrid.Dispose();
            _flockGrid = new NativeSpatialHash<Boid>(cellSize, flockCount, Allocator.Persistent);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (clickInput)
            {
                clickInput.action.Enable();
                clickInput.action.performed += HandleClick;
            }
            if (_flock.IsCreated)
                _flock.Dispose();
            _flock = new NativeList<Boid>(flockCount, Allocator.Persistent);
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
            if (_flockGrid.IsCreated)
                _flockGrid.Dispose();
            if (_flock.IsCreated)
                _flock.Dispose();
        }

        private async Task SpawnFlock(CancellationToken token)
        {
            Vector3 position = camera.ViewportToWorldPoint(new Vector3(.05f, .5f));
            position.z = 0;
            for (int i = 0; i < flockCount; i++)
            {
                if (token.IsCancellationRequested)
                    return;
                for (int j = 0, direction = 1; j < spawnsBatchSize && i + j < flockCount; j++, direction *= -1)
                    Spawn(i, position + Vector3.up * j * direction);
                i += math.max(0, math.min(flockCount - i, spawnsBatchSize) - 1);
                if (doSpawnPerFrame)
                    await Awaitable.NextFrameAsync();
                else
                    await Awaitable.WaitForSecondsAsync(spawnPeriod);
            }

            void Spawn(int i, Vector3 position)
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
            var spatialHashJob = _flockGrid.GetCompute(Flock.AsArray());
            JobHandle computeSpatialHash = spatialHashJob.ScheduleByRef(Flock.Length, 64);

            var steeringJob = _flocking.GetJob(Flock.AsArray(),
                                               _flockGrid,
                                               separation,
                                               alignment,
                                               cohesion,
                                               Destination,
                                               destinationWeight,
                                               speed,
                                               steeringSpeed,
                                               Time.deltaTime);
            steeringJob.ScheduleByRef(Flock.Length, 64, computeSpatialHash).Complete();

            if (doDrawNeighboursGizmo)
            {
                foreach (Boid subject in Flock)
                {
                    var neighbours = new NativeList<Boid>(64, Allocator.Temp);
                    _flockGrid.Query(subject.Position.xy, separation.Range, ref neighbours);
                    foreach (Boid neighbour in neighbours)
                        DrawLine(subject.Position, neighbour.Position, Color.darkGreen);
                }
            }
        }

        private void OnDrawGizmos()
        {
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
            if (!_flock.IsCreated)
                return;
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