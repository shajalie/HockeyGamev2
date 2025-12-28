using UnityEngine;
using UnityEngine.SceneManagement;
using HockeyGame.Input;
using HockeyGame.Systems;

namespace HockeyGame.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;

        private void Awake()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // Initialize core managers (singletons auto-create)
            var gameStateManager = GameStateManager.Instance;
            var settingsManager = SettingsManager.Instance;
            var audioManager = AudioManager.Instance;

            // Register InputReader if assigned
            if (_inputReader != null)
            {
                ServiceLocator.Register<InputReader>(_inputReader);
            }

            Debug.Log("[Bootstrapper] Systems initialized");

            // Transition to MainMenu
            gameStateManager.ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }
}
