using System;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
using Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VarelaAloisio.Core;

namespace Management
{
    public class GameManager : MonoBehaviourAsync, IGameManager
    {
        [SerializeField] private string uiName;
        [SerializeField] private string level1Name;
        [SerializeField] private string level2Name;
        [SerializeField] private int lives = 3;
        [SerializeField] private float delayBeforeGoingIntoNextLevel = 3f;

        public UnityEvent<ILevelManager> onEnterLevel;
        public event Action<ILevelManager> OnPlayerWonLevel; 
        public event Action<ILevelManager> OnPlayerLost; 
        public event Action<ILevelManager> OnAllowRetry; 

        private ILevelManager _currentLevel;

        public ILevelManager CurrentLevel => _currentLevel;

        private void Awake()
            => Service.Add<IGameManager>(this);

        private void OnDestroy()
            => Service.Remove<IGameManager>();

        private void Start()
            => SceneManager.LoadSceneAsync(uiName, LoadSceneMode.Additive);

        public void EnterGame()
            => EnterLevel(level1Name);

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
                Debug.LogError($"{name}: Level manager not found in scene {scene.name}");
                return;
            }

            _currentLevel.OnTeamDefeated += HandleTeamDefeated;
            onEnterLevel.Invoke(_currentLevel);
        }

        private async void HandleTeamDefeated(Team team)
        {
            if (team is Team.Player)
            {
                if (--lives <= 0)
                    OnPlayerLost?.Invoke(_currentLevel);
                else
                    OnAllowRetry?.Invoke(_currentLevel);
                return;
            }
            _currentLevel.OnTeamDefeated -= HandleTeamDefeated;
            OnPlayerWonLevel?.Invoke(_currentLevel);
            await Awaitable.WaitForSecondsAsync(delayBeforeGoingIntoNextLevel);
            if (Service.TryGet(out IUnitsRepository unitsRepository)
                && unitsRepository.TryGetShipsOfType(ShipType.Player, out var ships))
                foreach (IShip ship in ships)
                    ship.Kill();
            if (SceneManager.GetSceneByName(level1Name).isLoaded)
                SceneManager.UnloadSceneAsync(level1Name);
            if (SceneManager.GetSceneByName(level2Name).isLoaded)
                SceneManager.UnloadSceneAsync(level2Name);
            else
                EnterLevel(level2Name);
        }
    }
}
