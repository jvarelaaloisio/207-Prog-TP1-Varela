using System.Collections.Generic;
using Core.Game;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Steering
{
    public interface IFlockController
    {
        List<Boid> Flock { get; }
        float3 Destination { get; }
    }
}
