using UnityEngine;

namespace HockeyGame.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Hockey/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 8f;
        public float sprintMultiplier = 1.5f;
        public float acceleration = 15f;
        public float deceleration = 12f;
        public float rotationSpeed = 720f;

        [Header("Physics")]
        public float mass = 1f;
        public float drag = 2f;
        public PhysicsMaterial physicsMaterial;

        [Header("Shooting")]
        public float shotPower = 25f;
        public float passPower = 15f;
        public float shotCooldown = 0.5f;

        [Header("Player Info")]
        public int jerseyNumber = 1;
        public string playerName = "Player";
        public PlayerPosition position = PlayerPosition.Center;
    }

    public enum PlayerPosition
    {
        Goalie,
        LeftDefense,
        RightDefense,
        LeftWing,
        Center,
        RightWing
    }
}
