using UnityEngine;

namespace LD60
{
    [RequireComponent(typeof(AudioSource))]
    public class RadioTonePlayer : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            RadioInput.ButtonPressed += OnButtonPressed;
        }

        private void OnDisable()
        {
            RadioInput.ButtonPressed -= OnButtonPressed;
        }

        private void OnButtonPressed(ERadioButton button)
        {
            var (low, high) = GetFrequencies(button);

            var clip = GenerateTone(low, high, 0.15f);
            audioSource.PlayOneShot(clip);
        }

        private static (float, float) GetFrequencies(ERadioButton button)
        {
            return button switch
            {
                ERadioButton.ONE   => (697, 1209),
                ERadioButton.TWO   => (697, 1336),
                ERadioButton.THREE => (697, 1477),

                ERadioButton.FOUR  => (770, 1209),
                ERadioButton.FIVE  => (770, 1336),
                ERadioButton.SIX   => (770, 1477),

                ERadioButton.SEVEN => (852, 1209),
                ERadioButton.EIGHT => (852, 1336),
                ERadioButton.NINE  => (852, 1477),

                ERadioButton.ZERO  => (941, 1336),

                // Используем DTMF * и # для наших + и -
                ERadioButton.PLUS  => (941, 1209),
                ERadioButton.MINUS => (941, 1477),

                _ => (0, 0)
            };
        }

        private static AudioClip GenerateTone(
            float frequency1,
            float frequency2,
            float duration)
        {
            const int sampleRate = 44100;

            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; ++i)
            {
                float time = (float)i / sampleRate;

                samples[i] =
                    (Mathf.Sin(2 * Mathf.PI * frequency1 * time) +
                     Mathf.Sin(2 * Mathf.PI * frequency2 * time))
                    * 0.25f;
            }

            var clip = AudioClip.Create(
                "RadioTone",
                sampleCount,
                1,
                sampleRate,
                false);

            clip.SetData(samples, 0);

            return clip;
        }
    }
}
