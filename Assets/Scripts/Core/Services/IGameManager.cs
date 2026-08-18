using System;
using Core.Game;

namespace Core.Services
{
    public interface IGameManager
    {
        void EnterGame();
        ILevelManager CurrentLevel { get; }
        int LivesLeft { get; }
        event Action<ILevelManager> OnPlayerWonLevel;
        event Action<ILevelManager> OnPlayerLost;
        event Action<ILevelManager> OnAllowRetry;
        event Action OnGameEnded;
    }
}