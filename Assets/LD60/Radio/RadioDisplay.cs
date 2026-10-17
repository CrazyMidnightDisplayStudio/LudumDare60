using System.Collections;
using TMPro;
using UnityEngine;

namespace LD60
{
    public class RadioDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float blinkInterval = 0.5f;

        private string currentValue = "000.00";
        private Coroutine blinkCoroutine;
        private char blinkSimbol = '*';

        private void OnEnable()
        {
            Radio.WaveChanged += OnWaveChanged;
        }

        private void OnDisable()
        {
            Radio.WaveChanged -= OnWaveChanged;

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
        }

        private void OnWaveChanged(string value)
        {
            currentValue = value;

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            if (currentValue.Contains(Constexpr.SKIP_SIMBOL))
            {
                blinkCoroutine = StartCoroutine(BlinkCursor());
            }
            else
            {
                text.text = currentValue;
            }
        }

        private IEnumerator BlinkCursor()
        {
            bool visible = true;

            while (true)
            {
                text.text = visible
                    ? currentValue
                    : currentValue.Replace(Constexpr.SKIP_SIMBOL, blinkSimbol);

                visible = !visible;

                yield return new WaitForSeconds(blinkInterval);
            }
        }
    }
}
