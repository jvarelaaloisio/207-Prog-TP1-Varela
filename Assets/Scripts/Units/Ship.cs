using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Game;
using Core.Game.Enums;
using Core.Utils;
using UnityEngine;
using VarelaAloisio.Core.Attributes;

namespace Units
{
    public class Ship : MonoBehaviourAsync, IShip
    {
        [Serializable]
        private class TeamConfiguration
        {
            [field: SerializeField] public Team Team { get; private set; }
            [field: SerializeField] public string Layer { get; private set; }
        }

        [SerializeField] private TeamConfiguration[] configurations;
        [Header("Movement")]
        [field: SerializeField] public float MaxSpeed { get; private set; } = 10;
        [SerializeField] private float moveForce = 10;
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Space]
        [Range(0f, 10f)]
        [SerializeField] private float brakeForceMultiplier = .8f;

        [Space]
        [SerializeField] private float rotationSpeed = 10;

        [Header("Drifting")]
        [Range(1f, 10f)]
        [SerializeField] private float driftForceMultiplier = 2;
        [SerializeField] private float minAngleToConsiderAsDrift = 45f;

        [Header("Components")]
        [SerializeField] private Rigidbody2D rigidBody;

        [Header("Shooting")]
        [SerializeField] private int missileDamage;
        [ContextMenuItem("Populate", nameof(FetchPrimaryMuzzles))]
        [SerializeField] private List<Transform> muzzles;
        [SerializeField] private float shootingPeriod = .25f;

        [Space]
        [SerializeField] private Transform secondaryShotMuzzle;

        [SerializeField, ReadOnly, Tooltip("Injected")] private Team team;
        [Space]
        [SerializeField] private bool enableDebug;

        private Factory<IBullet> _primaryBulletFactory;
        private Factory<IBullet> _secondaryBulletFactory;
        private bool _isMovementBlocked;
        private string _movementBlockSource;
        private bool _isRotationBlocked;
        private string _rotationBlockSource;

        public event Action<IShip> OnKill;

        [field: SerializeField, ReadOnly] public bool IsBreaking { get; private set; } = false;
        [field: SerializeField, ReadOnly] public bool IsDrifting { get; private set; } = false;
        [field: SerializeField, ReadOnly] public float CurrentAngle { get; private set; }
        [field: SerializeField, ReadOnly] public float CurrentSpeed { get; private set; }

        public Team Team => team;

        [field: SerializeField, ReadOnly] public Vector2 Direction { get; set; }

        private void Reset()
            => rigidBody = GetComponent<Rigidbody2D>()
                           ?? gameObject.AddComponent<Rigidbody2D>();

        private void Awake()
        {
            IsBreaking = false;
            IsDrifting = false;
            Direction = Vector2.zero;
        }

        public void Inject(Factory<IBullet> primaryBulletFactory, Factory<IBullet> secondaryBulletFactory, Team team)
        {
            _secondaryBulletFactory = primaryBulletFactory;
            this.team = team;
            TeamConfiguration config = configurations.FirstOrDefault(config => config.Team == team);
            if (config is not null)
                gameObject.layer = LayerMask.NameToLayer(config.Layer);
        }

        private void Update()
        {
            if (!_isRotationBlocked)
                RotateTowardsVelocity();
        }

        private void FixedUpdate()
        {
            if (!_isMovementBlocked)
                Move(Direction);
        }

        /// <inheritdoc />
        public void OverrideMovement(string source, CancellationToken reEnableToken)
        {
            _isMovementBlocked = true;
            _movementBlockSource = source;
            reEnableToken.Register(() => _isMovementBlocked = false);
        }

        /// <inheritdoc />
        public void OverrideRotation(string source, CancellationToken reEnableToken)
        {
            _isRotationBlocked = true;
            _rotationBlockSource = source;
            reEnableToken.Register(() => _isRotationBlocked = false);
        }

        public async void ShootPrimaryPeriodically(CancellationToken token, float periodOverride = -1f)
        {
            float period = periodOverride > 0 ? periodOverride : shootingPeriod;
            if (muzzles.Count < 1)
            {
                Debug.LogError($"{nameof(muzzles)} is empty!");
                return;
            }

            int muzzleIndex = 0;
            while (!token.IsCancellationRequested
                   && !disableCancellationToken.IsCancellationRequested)
            {
                Transform muzzle = muzzles[muzzleIndex++ % muzzles.Count];
                IBullet bullet = _secondaryBulletFactory.Get();
                bullet.gameObject.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
                bullet.Inject(missileDamage, team);
                bullet.Shoot();
                await Awaitable.WaitForSecondsAsync(period);
            }
        }

        public void Kill()
        {
            if (enableDebug)
                Debug.Log($"{name} ({nameof(Ship)}): Raising kill event.");
            Destroy(gameObject);
            OnKill?.Invoke(this);
        }

        private void FetchPrimaryMuzzles()
        {
            muzzles = FindChildrenWithTag(transform, "Muzzle");

            return;

            static List<Transform> FindChildrenWithTag(Transform parent, string tag)
            {
                var result = new List<Transform>();

                foreach (Transform child in parent)
                {
                    if (child.CompareTag(tag))
                        result.Add(child);

                    result.AddRange(FindChildrenWithTag(child, tag));
                }

                return result;
            }
        }

        private void RotateTowardsVelocity()
            => transform.up = Vector2.Lerp(transform.up,
                                           rigidBody.linearVelocity.normalized,
                                           rotationSpeed * Time.deltaTime);

        private void Move(Vector2 direction)
        {
            Vector2 currentVelocity = rigidBody.linearVelocity;
            if (enableDebug)
                Debug.DrawRay(transform.position, currentVelocity, Color.yellow);
            float currentSpeed = CurrentSpeed = currentVelocity.magnitude;

            Vector2 currentDirection = currentVelocity.normalized;

            float angle = Vector2.Angle(currentDirection, direction);
            bool shouldDrift = IsDrifting = angle >= minAngleToConsiderAsDrift;

            bool shouldBrake = IsBreaking = Mathf.Approximately(direction.magnitude, 0);

            if (shouldBrake || shouldDrift)
                AddBrakeForce(currentSpeed, currentVelocity);

            float force = moveForce * speedCurve.Evaluate(currentSpeed / MaxSpeed);

            if (shouldDrift)
                force *= driftForceMultiplier;

            if (enableDebug)
                Debug.DrawRay(transform.position, direction * force, Color.cadetBlue);
            rigidBody.AddForce(direction * force, ForceMode2D.Force);
        }

        private void AddBrakeForce(float currentSpeed, Vector2 currentVelocity)
        {
            float multiplier = currentSpeed >= 0.1f ? brakeForceMultiplier : 1;
            rigidBody.AddForce(-currentVelocity * multiplier, ForceMode2D.Force);
            if (enableDebug)
                Debug.DrawRay(transform.position, currentVelocity, Color.red);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cornflowerBlue;
            Gizmos.DrawRay(transform.position, Direction);
        }
    }
}
