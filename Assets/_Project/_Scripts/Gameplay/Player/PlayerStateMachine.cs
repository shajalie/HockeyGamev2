using System;

namespace HockeyGame.Gameplay.Player
{
    public class PlayerStateMachine
    {
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
        public PlayerState PreviousState { get; private set; } = PlayerState.Idle;

        public event Action<PlayerState, PlayerState> OnStateChanged;

        public void ChangeState(PlayerState newState)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(PreviousState, newState);
        }

        public bool IsInState(PlayerState state)
        {
            return CurrentState == state;
        }

        public bool CanTransitionTo(PlayerState targetState)
        {
            // Define valid transitions
            switch (CurrentState)
            {
                case PlayerState.Stunned:
                    return targetState == PlayerState.Idle; // Can only recover to Idle
                case PlayerState.Celebrating:
                    return false; // Can't leave celebration early
                default:
                    return true;
            }
        }
    }
}
