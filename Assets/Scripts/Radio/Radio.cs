using System;
using UnityEngine;

namespace LD60
{
    public class Radio : MonoBehaviour
    {
        public static event Action<string> WaveChanged;
        public event Action<int> FrequencyChanged;

        public int Frequency => WaveNumber;
        public bool IsEnteringFrequency => TemporaryDigitsCount > 0;

        private const float InputTimeout = 3.0f;

        private int TemporaryWaveNumber = 0;
        private int TemporaryDigitsCount = 0;
        private int WaveNumber = Constexpr.DEFAULT_WAVE;

        private float InputTimer;

        public const int MIN_WAVE_NUMBER = 13600;
        public const int MAX_WAVE_NUMBER = 48000;

        private void OnEnable()
        {
            RadioInput.ButtonPressed += OnButtonPressed;
        }

        private void OnDisable()
        {
            RadioInput.ButtonPressed -= OnButtonPressed;
        }

        private void Start()
        {
            NotifyWaveChanged();
            NotifyFrequencyChanged();
        }

        private void Update()
        {
            if (TemporaryDigitsCount == 0)
            {
                return;
            }

            InputTimer -= Time.deltaTime;

            if (InputTimer <= 0.0f)
            {
                ResetTemporaryInput();
                NotifyWaveChanged();
                NotifyFrequencyChanged();
            }
        }

        public void OnButtonPressed(ERadioButton value)
        {
            if (value >= ERadioButton.ZERO && value <= ERadioButton.NINE)
            {
                InputTimer = InputTimeout;
                InputNumber((int)value);
            }
            else if (value == ERadioButton.PLUS)
            {
                ResetTemporaryInput();
                IncreaseWave();
            }
            else if (value == ERadioButton.MINUS)
            {
                ResetTemporaryInput();
                DecreaseWave();
            }
        }

        private void IncreaseWave()
        {
            WaveNumber = WaveNumber >= MAX_WAVE_NUMBER
                ? MIN_WAVE_NUMBER
                : WaveNumber + 1;

            NotifyWaveChanged();
            NotifyFrequencyChanged();
        }

        private void DecreaseWave()
        {
            WaveNumber = WaveNumber <= MIN_WAVE_NUMBER
                ? MAX_WAVE_NUMBER
                : WaveNumber - 1;

            NotifyWaveChanged();
            NotifyFrequencyChanged();
        }

        private void InputNumber(int value)
        {
            TemporaryWaveNumber = TemporaryWaveNumber * 10 + value;
            ++TemporaryDigitsCount;

            if (TemporaryDigitsCount == 5)
            {
                var newWaveNumber = TemporaryWaveNumber;

                ResetTemporaryInput();
                ApplyNewWaveNumber(newWaveNumber);

                return;
            }

            NotifyWaveChanged();
        }

        private void ApplyNewWaveNumber(int newWaveNumber)
        {
            WaveNumber = Mathf.Clamp(newWaveNumber, MIN_WAVE_NUMBER, MAX_WAVE_NUMBER);
            NotifyWaveChanged();
            NotifyFrequencyChanged();
        }

        private void ResetTemporaryInput()
        {
            TemporaryWaveNumber = 0;
            TemporaryDigitsCount = 0;
            InputTimer = 0.0f;
        }

        private void NotifyWaveChanged()
        {
            WaveChanged?.Invoke(GetDisplayValue());
        }

        private void NotifyFrequencyChanged()
        {
            FrequencyChanged?.Invoke(WaveNumber);
        }

        public string GetDisplayValue()
        {
            if (TemporaryDigitsCount > 0)
            {
                return GetTemporaryWaveString();
            }

            var value = WaveNumber.ToString().PadLeft(5, '0');
            return value.Insert(3, ".");
        }

        private string GetTemporaryWaveString()
        {
            var value = TemporaryWaveNumber
                .ToString()
                .PadLeft(TemporaryDigitsCount, '0')
                .PadRight(5, Constexpr.SKIP_SIMBOL);

            return value.Insert(3, ".");
        }
    }
}
