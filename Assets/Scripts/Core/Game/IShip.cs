using System;
using System.Threading;
using Core.Game.Enums;
using Core.Utils;
using UnityEngine;

namespace Core.Game
{
    public interface IShip
    {
        event Action<IShip> OnKill;
        GameObject gameObject { get; }
        Transform transform { get; }
        Team Team { get; }
        Vector2 Direction { get; set; }
        float MaxSpeed { get; }
        void ShootPrimaryPeriodically(CancellationToken token, float periodOverride = -1f);
        void Inject(Factory<IBullet> primaryBulletFactory, Factory<IBullet> secondaryBulletFactory, Team team);
        /// <summary /> Turn off movement control or this ship. Useful if you want to force your own movement logic.
        /// <param name="source">Source for the blockage. Used for debugging</param>
        /// <param name="reEnableToken">When this token is cancelled, the movement will be re-enabled.</param>
        void OverrideMovement(string source, CancellationToken reEnableToken);
        /// <summary /> Turn off rotation control or this ship. Useful if you want to force your own rotation logic.
        /// <param name="source">Source for the blockage. Used for debugging</param>
        /// <param name="reEnableToken">When this token is cancelled, the rotation control will be re-enabled.</param>
        void OverrideRotation(string source, CancellationToken reEnableToken);

        void Kill();
    }
}