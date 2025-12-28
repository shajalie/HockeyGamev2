using UnityEngine;
using HockeyGame.Gameplay.Player;

namespace HockeyGame.Data
{
    [CreateAssetMenu(fileName = "TeamConfig", menuName = "Hockey/Team Config")]
    public class TeamConfig : ScriptableObject
    {
        [Header("Team Identity")]
        public string teamName = "Team";
        public string abbreviation = "TM";

        [Header("Colors")]
        public Color primaryColor = Color.red;
        public Color secondaryColor = Color.white;

        [Header("Roster")]
        [Tooltip("Array of player configs: [0]=Goalie, [1-2]=Defense, [3-5]=Forwards")]
        public PlayerConfig[] roster = new PlayerConfig[6];

        [Header("Team Stats (AI Modifiers)")]
        [Range(0.5f, 1.5f)]
        public float offenseMultiplier = 1f;
        [Range(0.5f, 1.5f)]
        public float defenseMultiplier = 1f;
        [Range(0.5f, 1.5f)]
        public float speedMultiplier = 1f;

        public PlayerConfig GetGoalie()
        {
            return roster != null && roster.Length > 0 ? roster[0] : null;
        }

        public PlayerConfig[] GetDefense()
        {
            if (roster == null || roster.Length < 3) return new PlayerConfig[0];
            return new PlayerConfig[] { roster[1], roster[2] };
        }

        public PlayerConfig[] GetForwards()
        {
            if (roster == null || roster.Length < 6) return new PlayerConfig[0];
            return new PlayerConfig[] { roster[3], roster[4], roster[5] };
        }
    }
}
