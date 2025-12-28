using System;

namespace HockeyGame.Systems
{
    [Serializable]
    public class GameSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public int qualityLevel = 2;
        public bool fullscreen = true;
        public int resolutionIndex = 0;
        public bool vibrationEnabled = true;

        public GameSettings Clone()
        {
            return new GameSettings
            {
                masterVolume = this.masterVolume,
                musicVolume = this.musicVolume,
                sfxVolume = this.sfxVolume,
                qualityLevel = this.qualityLevel,
                fullscreen = this.fullscreen,
                resolutionIndex = this.resolutionIndex,
                vibrationEnabled = this.vibrationEnabled
            };
        }
    }
}
