using UnityEngine;
using VarelaAloisio.Core;

namespace Core.Game
{
    /// <summary>
    /// This is a base class for all ship controllers.
    /// It's meant to be hooked 1-1 with a ship and thus, decide what said ship should do. 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    public abstract class ShipController : MacacoBehaviour
    {
        protected IShip Ship;

        public virtual void Inject(IShip ship)
        {
            Ship = ship;
            if (ship is not null)
                return;

            Debug.LogError($"{name} <color=grey>({nameof(ShipController)})</color>: I was injected with null. <color=red>Deactivating GameObject</color>", this);
            gameObject.SetActive(false);
        }
    }
}