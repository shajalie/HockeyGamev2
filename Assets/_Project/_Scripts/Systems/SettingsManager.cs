using System;
using System.IO;
using UnityEngine;
using HockeyGame.Core;

namespace HockeyGame.Systems
{
    public class SettingsManager : SingletonBase<SettingsManager>
    {
        private const string SETTINGS_FILE = "settings.json";
        private GameSettings _settings;

        public GameSettings Settings => _settings;
        public event Action<GameSettings> OnSettingsChanged;

        protected override void OnSingletonAwake()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            string path = GetSettingsPath();

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    _settings = JsonUtility.FromJson<GameSettings>(json);
                    Debug.Log("[SettingsManager] Settings loaded from disk");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SettingsManager] Failed to load settings: {e.Message}");
                    _settings = new GameSettings();
                }
            }
            else
            {
                _settings = new GameSettings();
                SaveSettings();
                Debug.Log("[SettingsManager] Created default settings");
            }

            ApplySettings();
        }

        public void SaveSettings()
        {
            try
            {
                string path = GetSettingsPath();
                string directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(_settings, true);
                File.WriteAllText(path, json);
                Debug.Log("[SettingsManager] Settings saved to disk");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SettingsManager] Failed to save settings: {e.Message}");
            }
        }

        public void ApplySettings()
        {
            // Apply audio settings
            AudioListener.volume = _settings.masterVolume;

            // Apply quality settings
            QualitySettings.SetQualityLevel(_settings.qualityLevel);

            // Apply display settings
            Screen.fullScreen = _settings.fullscreen;

            if (_settings.resolutionIndex >= 0 && _settings.resolutionIndex < Screen.resolutions.Length)
            {
                var resolution = Screen.resolutions[_settings.resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, _settings.fullscreen);
            }

            OnSettingsChanged?.Invoke(_settings);
        }

        public void UpdateSettings(GameSettings newSettings)
        {
            _settings = newSettings;
            ApplySettings();
            SaveSettings();
        }

        public void ResetToDefaults()
        {
            _settings = new GameSettings();
            ApplySettings();
            SaveSettings();
        }

        private string GetSettingsPath()
        {
            return Path.Combine(Application.persistentDataPath, SETTINGS_FILE);
        }
    }
}
