using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Collections;
using Core.Game;
using Core.Services;
using Core.Steering;
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
        //TODO: Remove once we have spawn points
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

        [Header("Combat")]
        [SerializeField] private int expectedUniqueDestinationQuantity = 20;
        [SerializeField] private int maxAmountOfBoidsPerObjective = 10;

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
        private NativeList<Boid> _flock;
        private NativeList<BoidFlags> _flockFlags;
        private NativeHashMap<float3, NativeList<int>> _boidIndexesPerDestination;

        private IObjectivesRepository _objectivesRepository;
        public float3 Destination { get; private set; } = float3.zero;

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
            if (_flock.IsCreated)
                _flock.Dispose();
            _flock = new NativeList<Boid>(flockCount, Allocator.Persistent);

            if (_flockFlags.IsCreated)
                _flockFlags.Dispose();
            _flockFlags = new NativeList<BoidFlags>(flockCount, Allocator.Persistent);

            if (_boidIndexesPerDestination.IsCreated)
                _boidIndexesPerDestination.Dispose();
            _boidIndexesPerDestination = new NativeHashMap<float3, NativeList<int>>(expectedUniqueDestinationQuantity, Allocator.Persistent);
            _ = SpawnFlock(DisableCancellationToken);
        }

        protected override void Start()
        {
            base.Start();

            //TODO: Move to OnEnable when done testing
            if (Service.TryGet(out _objectivesRepository))
            {
                _objectivesRepository.OnDamageableAdded += HandleDamageableAdded;
                _objectivesRepository.OnDamageableRemoved += HandleDamageableRemoved;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_flockGrid.IsCreated)
                _flockGrid.Dispose();
            if (_flock.IsCreated)
                _flock.Dispose();
            if (_flockFlags.IsCreated)
                _flockFlags.Dispose();
            if (_boidIndexesPerDestination.IsCreated)
                _boidIndexesPerDestination.Dispose();
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
                _flockFlags.Add(BoidFlags.IsActive | BoidFlags.IsFollowingPlayer);
            }
        }

        private void Update()
        {
            _flockGrid.Clear();
            var spatialHashJob = _flockGrid.GetCompute(Flock.AsArray());
            JobHandle computeSpatialHash = spatialHashJob.ScheduleByRef(Flock.Length, 64);

            var steeringJob = Flocking.GetJob(Flock.AsArray(),
                                              _flockGrid,
                                              separation,
                                              alignment,
                                              cohesion,
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


        private void HandleDamageableAdded(DestroyableObjective objective)
        {
            float3 destination = objective.Position;
            if (!_boidIndexesPerDestination.TryGetValue(destination, out var indexes))
            {
                indexes = new NativeList<int>(maxAmountOfBoidsPerObjective, Allocator.Persistent);
                _boidIndexesPerDestination.Add(destination, indexes);
            }

            int freeSpots = maxAmountOfBoidsPerObjective - indexes.Count;
            if (freeSpots > 0)
            {
                for (int i = 0; i < _flockFlags.Count; i++)
                {
                    if (!_flockFlags[i].HasFlag(BoidFlags.IsFollowingPlayer))
                        continue;

                    Boid boid = _flock[i];
                    boid.Destination = objective.Position;
                    _flock[i] = boid;
                    _flockFlags[i] &= ~BoidFlags.IsFollowingPlayer;

                    indexes.Add(i);
                    freeSpots--;
                    if (freeSpots <= 0)
                        break;
                }
            }
            _boidIndexesPerDestination[destination] = indexes;
        }

        private void HandleDamageableRemoved(DestroyableObjective objective)
        {
            float3 destination = objective.Position;
            if (!_boidIndexesPerDestination.TryGetValue(destination, out var indexes))
                return;
            foreach (int index in indexes)
            {
                _flockFlags[index] |= BoidFlags.IsFollowingPlayer;
                _flockFlags[index] &= ~BoidFlags.HasDestination;
                Boid boid = _flock[index];
                boid.Destination = new float3();
                _flock[index] = boid;
            }
            //TODO: Move these indices to other objectives.
            //TODO: Add a max quantity of attackers serialized field
            _boidIndexesPerDestination.Remove(destination);
        }
        private void Handle()
        {
            for (int i = 0; i < Flock.Count; i++)
            {
                Boid boid = Flock[i];
                boid.Destination = Destination;
                _flock[i] = boid;
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