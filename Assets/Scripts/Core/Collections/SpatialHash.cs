using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Core.Collections
{
    public class SpatialHash<T> : IDisposable where T : unmanaged
    {
        private readonly float _cellConversionFactor;
        private NativeParallelMultiHashMap<int2, T> _buckets;
        public SpatialHash(float cellSize, int capacity)
        {
            CellSize = cellSize;
            _cellConversionFactor = 1 / cellSize;
            _buckets = new NativeParallelMultiHashMap<int2, T>(capacity, Allocator.Persistent);
        }

        public float CellSize { get; }

        public int2 CalculateCell(float2 position)
            => new((int)math.floor(position.x * _cellConversionFactor), (int)math.floor(position.y * _cellConversionFactor));

        public void Add(T value, float2 position)
            => _buckets.Add(CalculateCell(position), value);

        /// <summary /> Does an AABB check on the given position.
        /// <param name="position">World position.</param>
        /// <param name="radius">Will be used to create an AABB rectangle around the position,
        /// thus checking the cells colliding with it (can collide with 1, 2 or 4 cells)</param>
        /// <param name="resultOutput">The place to add all elements found inside the AABB rectangle.</param>
        public void Query(float2 position, float radius, ref NativeList<T> resultOutput)
        {
            int2 min = CalculateCell(position - radius);
            int2 max = CalculateCell(position + radius);
            for (int cx = min.x; cx <= max.x; cx++)
            for (int cy = min.y; cy <= max.y; cy++)
                foreach (T value in _buckets.GetValuesForKey(new int2(cx, cy)))
                    resultOutput.Add(value);
        }

        public void Clear()
            => _buckets.Clear();

        public void Dispose()
        {
            if (_buckets.IsCreated)
                _buckets.Dispose();
        }
    }
}
