using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HockeyGame.Core;

namespace HockeyGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Settings")]
        [SerializeField] private string _gameplaySceneName = "GameplaySandbox";

        private void Start()
        {
            SetupButtons();
            ShowMainPanel();
        }

        private void SetupButtons()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnStartClicked()
        {
            Debug.Log("[MainMenu] Starting game...");

            // Change game state
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }

            // Load gameplay scene
            SceneManager.LoadScene(_gameplaySceneName);
        }

        private void OnSettingsClicked()
        {
            ShowSettingsPanel();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ShowMainPanel()
        {
            if (_mainPanel != null)
            {
                _mainPanel.SetActive(true);
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }
        }

        public void ShowSettingsPanel()
        {
            if (_mainPanel != null)
            {
                _mainPanel.SetActive(false);
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(true);
            }
        }

        public void OnBackFromSettings()
        {
            ShowMainPanel();
        }

        private void OnDestroy()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(OnStartClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }
    }
}
