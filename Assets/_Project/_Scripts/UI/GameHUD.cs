using UnityEngine;
using UnityEngine.UI;
using HockeyGame.Core;
using HockeyGame.Gameplay.Rules;

namespace HockeyGame.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private Text _teamAScoreText;
        [SerializeField] private Text _teamBScoreText;
        [SerializeField] private Text _teamANameText;
        [SerializeField] private Text _teamBNameText;

        [Header("Timer")]
        [SerializeField] private Text _timerText;
        [SerializeField] private Text _periodText;

        [Header("Pause Menu")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPanel;

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
            UpdateScoreDisplay(0, 0);
            HidePauseMenu();

            // Show mobile controls on mobile platforms
            if (_mobileControlsPanel != null)
            {
                _mobileControlsPanel.SetActive(Application.isMobilePlatform);
            }
        }

        private void SetupButtons()
        {
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void SubscribeToEvents()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        private void Update()
        {
            // Check for pause input (Escape key)
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            if (GameStateManager.Instance == null) return;

            if (GameStateManager.Instance.CurrentState == GameState.Playing)
            {
                GameStateManager.Instance.ChangeState(GameState.Paused);
            }
            else if (GameStateManager.Instance.CurrentState == GameState.Paused)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
        }

        private void OnGameStateChanged(GameState previousState, GameState newState)
        {
            if (newState == GameState.Paused)
            {
                ShowPauseMenu();
            }
            else
            {
                HidePauseMenu();
            }
        }

        private void UpdateScoreDisplay(int teamAScore, int teamBScore)
        {
            if (_teamAScoreText != null)
            {
                _teamAScoreText.text = teamAScore.ToString();
            }

            if (_teamBScoreText != null)
            {
                _teamBScoreText.text = teamBScore.ToString();
            }
        }

        public void SetTeamNames(string teamA, string teamB)
        {
            if (_teamANameText != null)
            {
                _teamANameText.text = teamA;
            }

            if (_teamBNameText != null)
            {
                _teamBNameText.text = teamB;
            }
        }

        public void UpdateTimer(float timeRemaining)
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        public void SetPeriod(int period)
        {
            if (_periodText != null)
            {
                _periodText.text = $"Period {period}";
            }
        }

        private void ShowPauseMenu()
        {
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(true);
            }
        }

        private void HidePauseMenu()
        {
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(false);
            }
        }

        private void OnResumeClicked()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
        }

        private void OnMainMenuClicked()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.MainMenu);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
            }

            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveListener(OnResumeClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }
    }
}
