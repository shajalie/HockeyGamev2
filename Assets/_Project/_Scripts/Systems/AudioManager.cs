using System.Collections.Generic;
using UnityEngine;
using HockeyGame.Core;

namespace HockeyGame.Systems
{
    public class AudioManager : SingletonBase<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _ambientSource;

        [Header("Settings")]
        [SerializeField] private int _sfxPoolSize = 10;

        private List<AudioSource> _sfxPool;
        private int _currentPoolIndex;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        protected override void OnSingletonAwake()
        {
            InitializeAudioSources();
            InitializeSFXPool();

            // Subscribe to settings changes
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged += OnSettingsChanged;
            }
        }

        private void InitializeAudioSources()
        {
            if (_musicSource == null)
            {
                var musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                var sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                _sfxSource = sfxObj.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
            }

            if (_ambientSource == null)
            {
                var ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                _ambientSource = ambientObj.AddComponent<AudioSource>();
                _ambientSource.loop = true;
                _ambientSource.playOnAwake = false;
            }
        }

        private void InitializeSFXPool()
        {
            _sfxPool = new List<AudioSource>();
            var poolParent = new GameObject("SFXPool");
            poolParent.transform.SetParent(transform);

            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var sfxObj = new GameObject($"PooledSFX_{i}");
                sfxObj.transform.SetParent(poolParent.transform);
                var source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxPool.Add(source);
            }
        }

        private void OnSettingsChanged(GameSettings settings)
        {
            _musicVolume = settings.musicVolume;
            _sfxVolume = settings.sfxVolume;

            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            var source = GetPooledSource();
            source.clip = clip;
            source.volume = volume * _sfxVolume;
            source.Play();
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, volume * _sfxVolume);
        }

        private AudioSource GetPooledSource()
        {
            var source = _sfxPool[_currentPoolIndex];
            _currentPoolIndex = (_currentPoolIndex + 1) % _sfxPool.Count;
            return source;
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 1f)
        {
            if (_musicSource == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (_ambientSource == null) return;

            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        public void StopAmbient()
        {
            if (_ambientSource != null)
            {
                _ambientSource.Stop();
            }
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= OnSettingsChanged;
            }
        }
    }
}
