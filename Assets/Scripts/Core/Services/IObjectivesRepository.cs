using System;
using System.Collections.Generic;
using Core.Game;

namespace Core.Services
{
    /// <summary /> Accumulates references to all objectives
    public interface IObjectivesRepository
    {
        /// <summary /> Full collection of damageables.
        IReadOnlyCollection<DestroyableObjective> Objectives { get; }

        /// <summary /> Called every time a damageable is added to the <see cref="Objectives"/> collection.
        event Action<DestroyableObjective> OnDamageableAdded;

        /// <summary /> Called every time a damageable is removed from the <see cref="Objectives"/> collection.
        event Action<DestroyableObjective> OnDamageableRemoved;

        /// <summary /> Adds a unique damageable objective to the <see cref="Objectives"/> collection.
        /// <param name="damageable"></param>
        void Add(DestroyableObjective damageable);

        /// <summary /> Removes a damageable objective from the <see cref="Objectives"/> collection.
        /// <param name="damageable"></param>
        void Remove(DestroyableObjective damageable);
    }
}