using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Turbo.Fade
{
    public class FadeManager : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private float duration = 0.3f;

        private Coroutine _fadeRoutine;

        public static FadeManager Instance { get; private set; }

        public bool IsFading { get; private set; }
        public bool IsVisible => fadeImage.gameObject.activeSelf;

        private void Awake()
        {
            Instance = this;
        }

        public void FadeIn()
        {
            if (!fadeImage.gameObject.activeSelf || IsFading)
                return;

            _fadeRoutine = StartCoroutine(FadeInRoutine());
        }

        public void FadeOut()
        {
            if (fadeImage.gameObject.activeSelf || IsFading)
                return;

            _fadeRoutine = StartCoroutine(FadeOutRoutine());
        }

        public void FadeOutIn()
        {
            if (IsFading)
                return;

            _fadeRoutine = StartCoroutine(FadeOutInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            IsFading = true;

            float elapsed = 0f;
            float startAlpha = fadeImage.color.a;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(startAlpha, 0f, t));
                yield return null;
            }

            SetAlpha(0f);
            fadeImage.gameObject.SetActive(false);
            IsFading = false;
            _fadeRoutine = null;
        }

        private IEnumerator FadeOutRoutine()
        {
            IsFading = true;
            fadeImage.gameObject.SetActive(true);

            float elapsed = 0f;
            float startAlpha = fadeImage.color.a;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(startAlpha, 1f, t));
                yield return null;
            }

            SetAlpha(1f);
            IsFading = false;
            _fadeRoutine = null;
        }

        private IEnumerator FadeOutInRoutine()
        {
            IsFading = true;
            fadeImage.gameObject.SetActive(true);

            float elapsed = 0f;
            float startAlpha = fadeImage.color.a;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(startAlpha, 1f, t));
                yield return null;
            }

            SetAlpha(1f);

            elapsed = 0f;
            startAlpha = fadeImage.color.a;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(startAlpha, 0f, t));
                yield return null;
            }

            SetAlpha(0f);
            fadeImage.gameObject.SetActive(false);
            IsFading = false;
            _fadeRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
