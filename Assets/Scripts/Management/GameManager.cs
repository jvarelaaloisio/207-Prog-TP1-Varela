using System;
using Core;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VarelaAloisio.Core;

namespace Management
{
    /// <summary>
    /// The main orchestrator for the game.
    /// It controls scene loading/unloading, level manager and decides when the player wins/looses and retries. 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    public class GameManager : MonoBehaviourAsync, IGameManager
    {
        [SerializeField] private string uiName;
        [SerializeField] private string level1Name;
        [SerializeField] private string level2Name;
        [SerializeField] private int lives = 3;
        [SerializeField] private float delayBeforeGoingIntoNextLevel = 3f;
        [SerializeField] private float delayBeforeEndingGame = 3f;

        public UnityEvent<ILevelManager> onEnterLevel;
        public event Action<ILevelManager> OnPlayerWonLevel; 
        public event Action<ILevelManager> OnPlayerLost; 
        public event Action<ILevelManager> OnAllowRetry; 
        public event Action OnGameEnded; 

        private ILevelManager _currentLevel;

        public ILevelManager CurrentLevel => _currentLevel;
        public int LivesLeft { get; private set; }
        private void Awake()
            => Service.Add<IGameManager>(this);

        private void OnDestroy()
            => Service.Remove<IGameManager>();

        private void Start()
            => SceneManager.LoadSceneAsync(uiName, LoadSceneMode.Additive);

        public void EnterGame()
        {
            LivesLeft = lives;
            EnterLevel(level1Name);
        }

        private async void EnterLevel(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByName(sceneName);
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (GameObject go in rootGameObjects)
                if (go.TryGetComponent(out _currentLevel))
                    break;

            if (_currentLevel is null)
            {
                Debug.LogError($"{name} <color=grey>({nameof(GameManager)})</color>: Level manager not found in scene {scene.name}");
                return;
            }

            _currentLevel.OnTeamDefeated += HandleTeamDefeated;
            onEnterLevel.Invoke(_currentLevel);
        }

        private async void HandleTeamDefeated(Team team)
        {
            Service.TryGet(out IUnitsRepository unitsRepository);
            if (team is Team.Player)
            {
                if (--LivesLeft <= 0)
                {
                    OnPlayerLost?.Invoke(_currentLevel);
                    if (SceneManager.GetSceneByName(level1Name).isLoaded)
                        await SceneManager.UnloadSceneAsync(level1Name);
                    if (SceneManager.GetSceneByName(level2Name).isLoaded)
                        await SceneManager.UnloadSceneAsync(level2Name);
                    await Awaitable.WaitForSecondsAsync(delayBeforeEndingGame);
                    if (disableCancellationToken.IsCancellationRequested)
                        return;
                    unitsRepository?.Flush();
                    OnGameEnded?.Invoke();
                }
                else
                    OnAllowRetry?.Invoke(_currentLevel);
                return;
            }
            _currentLevel.OnTeamDefeated -= HandleTeamDefeated;
            OnPlayerWonLevel?.Invoke(_currentLevel);
            await Awaitable.WaitForSecondsAsync(delayBeforeGoingIntoNextLevel);
            if (unitsRepository?.TryGetShipsOfType(ShipType.Player, out var ships) ?? false)
                foreach (IShip ship in ships)
                    ship.Kill();
            if (_currentLevel.Level == 1
                && SceneManager.GetSceneByName(level1Name).isLoaded)
                await SceneManager.UnloadSceneAsync(level1Name);
            if (_currentLevel.Level == 2)
            {
                if (SceneManager.GetSceneByName(level2Name).isLoaded)
                    await SceneManager.UnloadSceneAsync(level2Name);
                await Awaitable.WaitForSecondsAsync(delayBeforeEndingGame);
                OnGameEnded?.Invoke();
            }
            else
                EnterLevel(level2Name);
        }
    }
}
