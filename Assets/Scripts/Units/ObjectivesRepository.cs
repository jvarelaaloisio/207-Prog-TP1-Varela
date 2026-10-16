using System;
using System.Collections.Generic;
using Core.Game;
using Core.Services;
using VarelaAloisio.Core;
using VarelaAloisio.Core.Attributes;

namespace Units
{
    [Service(typeof(IObjectivesRepository))]
    public class ObjectivesRepository : MacacoBehaviour, IObjectivesRepository
    {
        private readonly HashSet<DestroyableObjective> _objectives = new ();

        /// <inheritdoc />
        public IReadOnlyCollection<DestroyableObjective> Objectives => _objectives;

        /// <inheritdoc />
        public event Action<DestroyableObjective> OnDamageableAdded;

        /// <inheritdoc />
        public event Action<DestroyableObjective> OnDamageableRemoved;

        /// <inheritdoc />
        public void Add(DestroyableObjective damageable)
        {
            _objectives.Add(damageable);
            OnDamageableAdded?.Invoke(damageable);
        }

        /// <inheritdoc />
        public void Remove(DestroyableObjective damageable)
        {
            _objectives.Remove(damageable);
            OnDamageableRemoved?.Invoke(damageable);
        }
    }
}