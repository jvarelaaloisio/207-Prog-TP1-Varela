using System;
using Core.Game.Enums;

namespace Core.Game
{
    public interface ILevelManager
    {
        int Level { get; }
        /// <summary /> Returns how many ships will be spawned for a specific type, regardless of the amount spawned yet.
        int GetShipsCountForTeam(Team team);

        /// <summary /> Event triggered when all ships of a team are destroyed.
        event Action<Team> OnTeamDefeated;
    }
}