using Units;
using UnityEngine;

namespace Core.Game
{
    public abstract class ShipController : MonoBehaviourAsync
    {
        protected IShip Ship;

        public virtual void Inject(IShip ship)
        {
            Ship = ship;
            if (ship is not null)
                return;

            Debug.LogError($"Controller ({name}) was injected with null. <color=red>Deactivating GameObject</color>", this);
            gameObject.SetActive(false);
        }
    }
}