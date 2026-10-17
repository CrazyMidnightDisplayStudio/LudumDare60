using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turbo.Localization
{
    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string en;
        public string ru;
    }

    [Serializable]
    public class LocalizationData
    {
        public LocalizationEntry[] entries;
    }

    public sealed class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [SerializeField] private TextAsset jsonFile;
        [SerializeField] private string defaultLanguage = "en";

        private readonly Dictionary<string, LocalizationEntry> _table = new();
        private string _currentLanguage;

        public static event Action LanguageChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Load();
            SetLanguage(defaultLanguage);
        }

        public static void SetLanguage(string language)
        {
            if (Instance == null)
                return;

            Instance._currentLanguage = language;
            LanguageChanged?.Invoke();
        }

        public static string Get(string key, params object[] args)
        {
            if (Instance == null)
                return key;

            if (!Instance._table.TryGetValue(key, out var entry))
                return key;

            string value = Instance._currentLanguage switch
            {
                "ru" => entry.ru,
                _ => entry.en
            };

            if (string.IsNullOrEmpty(value))
                value = entry.en;

            return args.Length > 0 ? string.Format(value, args) : value;
        }

        private void Load()
        {
            if (jsonFile == null)
                return;

            LocalizationData data = JsonUtility.FromJson<LocalizationData>(jsonFile.text);

            foreach (var e in data.entries)
                _table[e.key] = e;
        }
    }
}

