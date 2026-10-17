using Turbo.Pause;
using UnityEngine;

namespace Turbo.Shaders
{
    public class SpriteFillAnimator : MonoBehaviour
    {
        private static readonly int FillRate = Shader.PropertyToID("_FillRate");
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float duration = 1f;

        private Material _material;
        private float _timer;
        private float _minFillRate;
        private float _maxFillRate;

        private void Awake()
        {
            _material = targetRenderer.material;

            Bounds bounds = targetRenderer.sprite.bounds;
            _minFillRate = bounds.min.y;
            _maxFillRate = bounds.max.y;

            _material.SetFloat(FillRate, _minFillRate);
        }

        private void OnEnable()
        {
            _timer = 0f;

            if (_material != null)
            {
                _material.SetFloat(FillRate, _minFillRate);
            }
        }

        private void Update()
        {
            if (PauseManager.IsPaused) return;
            if (duration <= 0f) return;

            _timer += Time.deltaTime;

            float t = Mathf.Repeat(_timer / duration, 1f);
            float fillRate = Mathf.Lerp(_minFillRate, _maxFillRate, t);

            _material.SetFloat(FillRate, fillRate);
        }
    }
}

