using System;
using UnityEngine;
using UnityEngine.Events;

namespace Turbo.SaveSystem
{
    [Serializable]
    public sealed class SettingsChangedUnityEvent : UnityEvent<SettingsData>
    {
    }

    public sealed class SettingsManager : MonoBehaviour
    {
        private const string MasterVolumeKey = "settings.master_volume";
        private const string MusicVolumeKey = "settings.music_volume";
        private const string SfxVolumeKey = "settings.sfx_volume";
        private const string LanguageKey = "settings.language";

        // ДОБАВЛЯЙТЕ ЗДЕСЬ ключи для новых настроек

        public static SettingsManager Instance { get; private set; }

        public static SettingsData Current => Instance != null ? Instance._current : SettingsData.CreateDefault();

        [SerializeField] private SettingsChangedUnityEvent settingsChanged;

        private SettingsData _current;

        public event Action<SettingsData> SettingsChanged;

        public SettingsChangedUnityEvent SettingsChangedEvent => settingsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _current = LoadInternal();
            NotifyChanged();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static void Save(SettingsData data)
        {
            if (Instance == null)
                return;

            Instance.SaveInternal(data);
        }

        public static SettingsData Load()
        {
            if (Instance == null)
                return SettingsData.CreateDefault();

            Instance._current = Instance.LoadInternal();
            Instance.NotifyChanged();
            return Instance._current;
        }

        public static void ResetToDefaults()
        {
            Save(SettingsData.CreateDefault());
        }

        private void SaveInternal(SettingsData data)
        {
            _current = data;

            PlayerPrefs.SetFloat(MasterVolumeKey, _current.masterVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, _current.musicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, _current.sfxVolume);
            PlayerPrefs.SetString(LanguageKey, _current.language);

            // ДОБАВЛЯЙТЕ ЗДЕСЬ сохранение новых настроек

            PlayerPrefs.Save();

            NotifyChanged();
        }

        private SettingsData LoadInternal()
        {
            SettingsData data = SettingsData.CreateDefault();

            data.masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, data.masterVolume);
            data.musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, data.musicVolume);
            data.sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, data.sfxVolume);
            data.language = PlayerPrefs.GetString(LanguageKey, data.language);

            // ДОБАВЛЯЙТЕ ЗДЕСЬ загрузку новых настроек

            return data;
        }

        private void NotifyChanged()
        {
            SettingsChanged?.Invoke(_current);
            settingsChanged.Invoke(_current);
        }
    }

    [Serializable]
    public struct SettingsData
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public string language;

        // ДОБАВЛЯЙТЕ ЗДЕСЬ поля для новых настроек

        public static SettingsData CreateDefault()
        {
            return new SettingsData
            {
                masterVolume = 1f,
                musicVolume = 1f,
                sfxVolume = 1f,
                language = "en"
            };
        }
    }
}


