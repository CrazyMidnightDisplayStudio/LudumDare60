using TMPro;
using UnityEngine;

namespace Turbo.Localization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            LocalizationManager.LanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationManager.LanguageChanged -= UpdateText;
        }

        private void UpdateText()
        {
            _text.text = LocalizationManager.Get(key);
        }
    }
}

