using System;
using UnityEngine;

namespace HockeyGame.Core
{
    public class GameStateManager : SingletonBase<GameStateManager>
    {
        public event Action<GameState, GameState> OnStateChanged;

        private GameState _currentState = GameState.Initialization;
        public GameState CurrentState => _currentState;

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            OnStateExit(_currentState);
            _currentState = newState;
            OnStateEnter(newState);
            OnStateChanged?.Invoke(previousState, newState);

            Debug.Log($"[GameState] {previousState} -> {newState}");
        }

        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
        }

        private void OnStateExit(GameState state)
        {
            // Cleanup logic per state if needed
        }
    }
}
