using System;
using System.Linq;
using System.Threading;
using Core.Game;
using Core.Game.Enums;
using HealthSystem.Runtime.Components;
using UnityEngine;
using VarelaAloisio.Core.Attributes;

namespace Units
{
    public class Bullet : MonoBehaviourAsync, IBullet
    {
        [Serializable]
        private class TeamConfiguration
        {
            [field: SerializeField] public Team Team { get; private set; }
            [field: SerializeField] public string Layer { get; private set; }
            [field: SerializeField] public Material TrailMaterial { get; private set; }
        }

        [SerializeField] private TeamConfiguration[] configurations;
        [SerializeField] private DamageDealer damageDealer;
        [SerializeField] private float speed = 10;
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Constant(0, 1, 1);
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private TrailRenderer trail;
        [SerializeField, ReadOnly, Tooltip("Injected")] private int damage;
        private float _movementStartTime;

        private void Awake()
            => trail ??= GetComponentInChildren<TrailRenderer>();

        /// <inheritdoc />
        public void Inject(int damage, Team team)
        {
            this.damage = damage;
            TeamConfiguration config = configurations.FirstOrDefault(config => config.Team == team);
            if (config is not null)
            {
                gameObject.layer = LayerMask.NameToLayer(config.Layer);
                trail?.SetMaterials(new (){config.TrailMaterial});
            }
        }

        public void Shoot()
        {
            DoUpdate(disableCancellationToken);
            _movementStartTime = Time.time;
            Destroy(gameObject, lifetime);
        }

        private async void DoUpdate(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                float localTime = Time.time - _movementStartTime;
                float currentSpeed = speed * speedCurve.Evaluate(localTime);
                transform.Translate(Vector3.forward * (currentSpeed * Time.deltaTime));
                await Awaitable.NextFrameAsync();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
            => damageDealer?.TryAttack(other);

        public void DestroySelf()
            => Destroy(gameObject);
    }
}