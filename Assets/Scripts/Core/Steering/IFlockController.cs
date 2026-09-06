using System.Collections.Generic;
using Core.Game;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Steering
{
    public interface IFlockController
    {
        NativeList<Boid> Flock { get; }
        float3 Destination { get; }
    }
}
