using System;
using Core.Steering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Core.Collections
{
    /// <summary>Unmanaged spatial hash grid, Burst-compatible.</summary>
    public struct NativeSpatialHash<T> : IDisposable where T : unmanaged, IPosition
    {
        private readonly float _cellConversionFactor;
        private NativeParallelMultiHashMap<int2, T> _buckets;

        public NativeSpatialHash(float cellSize, int capacity, Allocator allocator)
        {
            CellSize = cellSize;
            _cellConversionFactor = 1 / cellSize;
            _buckets = new NativeParallelMultiHashMap<int2, T>(capacity, allocator);
        }

        public float CellSize { get; }

        public bool IsCreated => _buckets.IsCreated;
        public int2 CalculateCell(float2 position)
            => CalculateCell(position, _cellConversionFactor);
        public static int2 CalculateCell(float2 position, float cellConversionFactor)
            => new((int)math.floor(position.x * cellConversionFactor),
                   (int)math.floor(position.y * cellConversionFactor));

        /// <summary>Parallel-safe writer, intended to be held by an IJobParallelFor.</summary>
        public NativeParallelMultiHashMap<int2, T>.ParallelWriter AsParallelWriter()
            => _buckets.AsParallelWriter();

        /// <summary>Adds an element from within a job, writing through the given parallel writer (atomic, thread-safe).</summary>
        public void AddParallel(NativeParallelMultiHashMap<int2, T>.ParallelWriter writer, T value, float2 position)
            => writer.Add(CalculateCell(position), value);

        /// <summary>Adds to <paramref name="resultOutput"/> every element stored in the cells overlapped by the AABB
        /// around <paramref name="position"/> with the given <paramref name="radius"/> (1, 2 or 4 cells when the radius
        /// is at most half the cell size).</summary>
        public void Query(float2 position, float radius, ref NativeList<T> resultOutput)
        {
            int2 min = CalculateCell(position - radius);
            int2 max = CalculateCell(position + radius);
            for (int cx = min.x; cx <= max.x; cx++)
            for (int cy = min.y; cy <= max.y; cy++)
                foreach (T value in _buckets.GetValuesForKey(new int2(cx, cy)))
                    resultOutput.Add(value);
        }

        /// <summary/> Creates a Job that computes all elements into this Spatial Hash
        /// <param name="elements">All the elements that need to be added.</param>
        /// <returns>A Compute Job to be scheduled</returns>
        public Compute GetCompute(NativeArray<T> elements)
            => new(this, elements, _cellConversionFactor);

        public void Clear()
            => _buckets.Clear();

        public void Dispose()
        {
            if (_buckets.IsCreated)
                _buckets.Dispose();
        }

        [BurstCompile]
        public struct Compute : IJobParallelFor
        {
            private NativeParallelMultiHashMap<int2, T>.ParallelWriter _mapWriter;
            [ReadOnly] private NativeArray<T> _elements;
            private readonly float _cellConversionFactor;

            public Compute(NativeSpatialHash<T> grid,
                           NativeArray<T> elements,
                           float cellConversionFactor)
            {
                _mapWriter = grid.AsParallelWriter();
                _elements = elements;
                _cellConversionFactor = cellConversionFactor;
            }

            /// <inheritdoc />
            public void Execute(int index)
                => _mapWriter.Add(CalculateCell(_elements[index].Position.xy, _cellConversionFactor),
                                  _elements[index]);
        }
    }
}