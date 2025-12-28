using System;
using UnityEngine;
using HockeyGame.Core;

namespace HockeyGame.Gameplay.Rules
{
    public class ScoreManager : SingletonBase<ScoreManager>
    {
        public event Action<int, int> OnScoreChanged;
        public event Action<int> OnGoalScored;

        private int _teamAScore;
        private int _teamBScore;

        public int TeamAScore => _teamAScore;
        public int TeamBScore => _teamBScore;

        protected override void OnSingletonAwake()
        {
            ResetScores();
        }

        public void ScoreGoal(int team)
        {
            if (team == 0)
            {
                _teamAScore++;
            }
            else
            {
                _teamBScore++;
            }

            OnGoalScored?.Invoke(team);
            OnScoreChanged?.Invoke(_teamAScore, _teamBScore);

            Debug.Log($"[Score] Team {(team == 0 ? "A" : "B")} scored! Score: {_teamAScore} - {_teamBScore}");
        }

        public void ResetScores()
        {
            _teamAScore = 0;
            _teamBScore = 0;
            OnScoreChanged?.Invoke(_teamAScore, _teamBScore);
        }

        public int GetLeadingTeam()
        {
            if (_teamAScore > _teamBScore) return 0;
            if (_teamBScore > _teamAScore) return 1;
            return -1; // Tied
        }

        public int GetScoreDifference()
        {
            return Mathf.Abs(_teamAScore - _teamBScore);
        }
    }
}
