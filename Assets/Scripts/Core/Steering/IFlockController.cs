using System.Collections.Generic;
using Core.Game;
using UnityEngine;

namespace Core.Steering
{
    public interface IFlockController
    {
        List<Boid> Flock { get; }
        Vector3 Destination { get; }
    }
}
