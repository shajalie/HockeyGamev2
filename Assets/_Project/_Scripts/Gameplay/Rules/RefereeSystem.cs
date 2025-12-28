using System;
using UnityEngine;
using HockeyGame.Gameplay.Puck;

namespace HockeyGame.Gameplay.Rules
{
    public class RefereeSystem : MonoBehaviour
    {
        public enum Violation
        {
            None,
            Icing,
            Offside,
            GoalieInterference
        }

        public event Action<Violation> OnViolationCalled;
        public event Action OnFaceoff;

        [Header("References")]
        [SerializeField] private PuckController _puck;
        [SerializeField] private Transform _teamAGoal;
        [SerializeField] private Transform _teamBGoal;

        [Header("Rink Zones (NHL)")]
        [SerializeField] private float _blueLineZ = 17.5f;
        [SerializeField] private float _goalLineZ = 28f;

        private bool _isPuckInPlay = true;

        public bool IsPuckInPlay => _isPuckInPlay;

        private void OnTriggerEnter(Collider other)
        {
            // Check for goal (check by name or component since Puck tag may not exist)
            if (other.name == "Puck" || other.GetComponent<PuckController>() != null)
            {
                if (_teamAGoal != null && Vector3.Distance(other.transform.position, _teamAGoal.position) < 2f)
                {
                    HandleGoal(1); // Team B scored on Team A's goal
                }
                else if (_teamBGoal != null && Vector3.Distance(other.transform.position, _teamBGoal.position) < 2f)
                {
                    HandleGoal(0); // Team A scored on Team B's goal
                }
            }
        }

        private void HandleGoal(int scoringTeam)
        {
            _isPuckInPlay = false;
            ScoreManager.Instance?.ScoreGoal(scoringTeam);

            Debug.Log($"[Referee] Goal by Team {(scoringTeam == 0 ? "A" : "B")}!");

            // Reset for faceoff after short delay
            Invoke(nameof(CallFaceoff), 2f);
        }

        public void CallFaceoff()
        {
            _isPuckInPlay = true;

            if (_puck != null)
            {
                _puck.ResetToPosition(Vector3.zero);
            }

            OnFaceoff?.Invoke();
            Debug.Log("[Referee] Faceoff!");
        }

        public void CallViolation(Violation violation)
        {
            _isPuckInPlay = false;
            OnViolationCalled?.Invoke(violation);

            Debug.Log($"[Referee] Violation: {violation}");

            Invoke(nameof(CallFaceoff), 1f);
        }

        public void StopPlay()
        {
            _isPuckInPlay = false;
        }

        public void ResumePlay()
        {
            _isPuckInPlay = true;
        }
    }
}
