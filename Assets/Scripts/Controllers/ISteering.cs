using UnityEngine;

namespace Controllers
{
    public interface ISteering
    {
        /// <summary /> Calculates the direction to get from the current position, to the desired destination.
        /// <param name="position">The current location.</param>
        /// <param name="destination">The desired point to get to.</param>
        Vector3 GetDirection(Vector3 position, Vector3 destination);
    }
}