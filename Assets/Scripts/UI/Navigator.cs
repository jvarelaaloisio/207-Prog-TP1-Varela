using System;
using Core;
using Core.Game;
using Core.Game.Enums;
using Core.Services;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using VarelaAloisio.Core;

namespace UI
{
    /// <summary>
    /// This class is used for anything UI related.
    /// It listens to important events from the Game Manager and responds by manipulating the different menus
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    public class Navigator : MonoBehaviour
    {
        [SerializeField] private GameObject pointerUI;
        [SerializeField] private GameObject menuUI;
        [SerializeField] private GameObject gameplayUI;
        [SerializeField] private GameObject creditsUI;
        [SerializeField] private GameObject retryUI;
        [SerializeField] private GameObject youLoseUI;
        [SerializeField] private GameObject youWinUI;
        [SerializeField] private CustomButton playButton;
        [SerializeField] private CustomButton creditsButton;
        [SerializeField] private CustomButton backButton;
        [SerializeField] private CustomButton exitButton;
        [SerializeField] private CustomButton retryButton;
        [SerializeField] private Transform pointee;
        [SerializeField] private float pointerTransitionDuration = 0.15f;
        [SerializeField] private float delayAfterButtonPress = 1f;

        private CustomButton[] _mainMenuButtons;
        private GameObject[] _exclusiveUIs;

        private void Awake()
        {
            _mainMenuButtons = new[] { playButton, creditsButton, exitButton };
            _exclusiveUIs = new []{ menuUI, gameplayUI, creditsUI, retryUI, youLoseUI, youWinUI };
        }

        private void OnEnable()
        {
            if (playButton)
            {
                playButton.RequestPointer = HandlePointerRequest;
                playButton.onClick.AddListener(HandlePlayClicked);
            }

            if (creditsButton)
            {
                creditsButton.RequestPointer = HandlePointerRequest;
                creditsButton.onClick.AddListener(ShowCreditsMenu);
            }

            if (exitButton)
            {
                exitButton.RequestPointer = HandlePointerRequest;
                exitButton.onClick.AddListener(Exit);
            }

            if (backButton)
            {
                backButton.RequestPointer = HandlePointerRequest;
                backButton.onClick.AddListener(ShowMainMenu);
            }

            if (retryButton)
                retryButton.onClick.AddListener(HandleRetryClicked);
            if (Service.TryGet(out IGameManager gameManager))
            {
                gameManager.OnAllowRetry += ShowRetryMenu;
                gameManager.OnPlayerLost += ShowDefeatMenu;
                gameManager.OnPlayerWonLevel += ShowVictoryMenu;
                gameManager.OnGameEnded += ShowMainMenu;
            }
        }

        private void Start()
        {
            pointerUI?.SetActive(true);
            menuUI?.SetActive(true);
            gameplayUI?.SetActive(false);
            creditsUI?.SetActive(false);
        }

        private void OnDisable()
        {
            if (playButton)
                playButton.onClick.RemoveListener(HandlePlayClicked);
            if (creditsButton)
                creditsButton.onClick.RemoveListener(ShowCreditsMenu);
            if (exitButton)
                exitButton.onClick.RemoveListener(Exit);
            if (retryButton)
                retryButton.onClick.AddListener(HandleRetryClicked);
            
            if (Service.TryGet(out IGameManager gameManager))
            {
                gameManager.OnAllowRetry -= ShowRetryMenu;
                gameManager.OnPlayerLost -= ShowDefeatMenu;
                gameManager.OnPlayerWonLevel -= ShowVictoryMenu;
                gameManager.OnGameEnded -= ShowMainMenu;
            }
        }

        private void HandlePointerRequest(Transform target)
        {
            LMotion.Create(pointee.position, target.position, pointerTransitionDuration)
                   .WithEase(Ease.InOutQuad)
                   .BindToPosition(pointee);
        }

        private async void HandlePlayClicked()
        {
            foreach (CustomButton customButton in _mainMenuButtons)
                customButton.interactable = false;
            await Awaitable.WaitForSecondsAsync(delayAfterButtonPress);
            if (!Service.TryGet(out IGameManager gameManager))
            {
                Debug.LogError($"{name} <color=grey>({nameof(Navigator)})</color>: Game manager not found.");
                return;
            }
            gameManager.EnterGame();
            pointerUI?.SetActive(false);
            GoToUI(gameplayUI);
        }

        private void HandleRetryClicked()
        {
            if (!Service.TryGet(out IGameManager gameManager))
            {
                Debug.LogError($"{name} <color=grey>({nameof(Navigator)})</color>: Game Manager not found");
                return;
            }

            gameManager.CurrentLevel.RespawnTeam(Team.Player);
            GoToUI(gameplayUI);
        }

        private async void Exit()
        {
            foreach (CustomButton customButton in _mainMenuButtons)
                customButton.interactable = false;
            await Awaitable.WaitForSecondsAsync(delayAfterButtonPress);
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void ShowCreditsMenu()
            => GoToUI(creditsUI);

        private void ShowMainMenu()
        {
            foreach (CustomButton customButton in _mainMenuButtons)
                customButton.interactable = true;
            pointerUI?.SetActive(true);
            GoToUI(menuUI);
        }

        private void ShowRetryMenu(ILevelManager levelManager)
            => GoToUI(retryUI);

        private void ShowDefeatMenu(ILevelManager levelManager)
            => GoToUI(youLoseUI);

        private void ShowVictoryMenu(ILevelManager levelManager)
        {
            if (levelManager.Level < 2)
                return;
            GoToUI(youWinUI);
        }

        private void GoToUI(GameObject activeUI)
        {
            Debug.Log($"{name}: Going to {activeUI?.name}");
            foreach (GameObject ui in _exclusiveUIs)
                ui?.SetActive(false);
            activeUI?.SetActive(true);
        }
    }
}