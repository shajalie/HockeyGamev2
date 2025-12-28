using UnityEngine;

namespace HockeyGame.Data
{
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "Hockey/Gameplay Settings")]
    public class GameplaySettings : ScriptableObject
    {
        [Header("Match Settings")]
        [Tooltip("Number of periods in a match")]
        [Range(1, 5)]
        public int periodCount = 3;

        [Tooltip("Length of each period in minutes")]
        [Range(1f, 20f)]
        public float periodLengthMinutes = 5f;

        [Tooltip("Length of overtime in minutes")]
        [Range(1f, 20f)]
        public float overtimeLengthMinutes = 5f;

        [Tooltip("Enable sudden death overtime")]
        public bool suddenDeathOvertime = true;

        [Header("Physics")]
        [Tooltip("Puck friction on ice")]
        [Range(0f, 1f)]
        public float puckFriction = 0.1f;

        [Tooltip("Puck bounce off walls")]
        [Range(0f, 1f)]
        public float puckBounce = 0.8f;

        [Tooltip("Force applied during player collisions")]
        [Range(1f, 50f)]
        public float playerCollisionForce = 10f;

        [Header("Gameplay Rules")]
        [Tooltip("Enable icing calls")]
        public bool icingEnabled = true;

        [Tooltip("Enable offside calls")]
        public bool offsideEnabled = true;

        [Tooltip("Enable body checking")]
        public bool bodyCheckingEnabled = true;

        [Header("AI Settings")]
        [Tooltip("AI difficulty level (0 = Easy, 1 = Medium, 2 = Hard)")]
        [Range(0, 2)]
        public int aiDifficulty = 1;

        [Tooltip("AI reaction time in seconds (lower = harder)")]
        [Range(0.1f, 1f)]
        public float aiReactionTime = 0.3f;

        [Header("Rink Dimensions (NHL Standard)")]
        [Tooltip("Rink length in meters")]
        public float rinkLength = 60f;

        [Tooltip("Rink width in meters")]
        public float rinkWidth = 26f;

        [Tooltip("Blue line distance from center")]
        public float blueLineDistance = 17.5f;

        [Tooltip("Goal crease radius")]
        public float goalCreaseRadius = 1.8f;

        public float PeriodLengthSeconds => periodLengthMinutes * 60f;
        public float OvertimeLengthSeconds => overtimeLengthMinutes * 60f;
    }
}
