using System;
using Core;
using Core.Game;
using Core.Services;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using VarelaAloisio.Core;

namespace UI
{
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
        [SerializeField] private Transform pointee;
        [SerializeField] private float pointerTransitionDuration = 0.15f;
        [SerializeField] private float delayAfterButtonPress = 1f;

        private CustomButton[] _buttons;
        private GameObject[] _exclusiveUIs;

        private void Awake()
        {
            _buttons = new[] { playButton, creditsButton, exitButton, backButton };
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
                creditsButton.onClick.AddListener(HandleCreditsClicked);
            }

            if (exitButton)
            {
                exitButton.RequestPointer = HandlePointerRequest;
                exitButton.onClick.AddListener(HandleExitClicked);
            }

            if (backButton)
            {
                backButton.RequestPointer = HandlePointerRequest;
                backButton.onClick.AddListener(HandleBackClicked);
            }
            if (Service.TryGet(out IGameManager gameManager))
            {
                gameManager.OnAllowRetry += ShowRetryMenu;
                gameManager.OnPlayerLost += ShowYouLoseMenu;
                gameManager.OnPlayerWonLevel += HandleLevelComplete;
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
            playButton.onClick.RemoveListener(HandlePlayClicked);
            creditsButton.onClick.RemoveListener(HandleCreditsClicked);
            exitButton.onClick.RemoveListener(HandleExitClicked);
            
            if (Service.TryGet(out IGameManager gameManager))
            {
                gameManager.OnAllowRetry -= ShowRetryMenu;
                gameManager.OnPlayerLost -= ShowYouLoseMenu;
                gameManager.OnPlayerWonLevel -= HandleLevelComplete;
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
            foreach (CustomButton customButton in _buttons)
                customButton.interactable = false;
            await Awaitable.WaitForSecondsAsync(delayAfterButtonPress);
            if (!Service.TryGet(out IGameManager gameManager))
            {
                Debug.LogError($"{name}: Game manager not found");
                return;
            }
            gameManager.EnterGame();
            pointerUI?.SetActive(false);
            GoToUI(gameplayUI);
        }

        private void HandleCreditsClicked()
        {
            GoToUI(creditsUI);
            backButton.interactable = true;
        }

        private async void HandleExitClicked()
        {
            foreach (CustomButton customButton in _buttons)
                customButton.interactable = false;
            await Awaitable.WaitForSecondsAsync(delayAfterButtonPress);
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void HandleBackClicked()
            => GoToUI(menuUI);

        private void ShowRetryMenu(ILevelManager levelManager)
            => GoToUI(retryUI);

        private void ShowYouLoseMenu(ILevelManager levelManager)
            => GoToUI(youLoseUI);

        private void HandleLevelComplete(ILevelManager levelManager)
        {
            if (levelManager.Level != 2)
                return;
            pointerUI?.SetActive(true);
            GoToUI(menuUI);
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