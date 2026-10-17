using Turbo.GameTime;
using UnityEngine;

namespace Signal
{
    [RequireComponent(typeof(Transmitter))]
    public class TransmitterFillAnimator : MonoBehaviour
    {
        private static readonly int FillRate = Shader.PropertyToID("_FillRate");

        [SerializeField] private SpriteRenderer targetRenderer;

        private Material _material;
        private Transmitter _transmitter;
        private float _timer;
        private float _minFillRate;
        private float _maxFillRate;

        private void Awake()
        {
            _transmitter = GetComponent<Transmitter>();
            _material = targetRenderer.material;

            Bounds bounds = targetRenderer.sprite.bounds;
            _minFillRate = bounds.min.y;
            _maxFillRate = bounds.max.y;

            _material.SetFloat(FillRate, _minFillRate);
        }

        private void OnEnable()
        {
            ResetFill();
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsRoundActive)
            {
                ResetFill();
                return;
            }

            if (GameTimeManager.Instance == null)
            {
                return;
            }

            float interval = _transmitter.IntervalSeconds;

            if (interval <= 0f)
            {
                _material.SetFloat(FillRate, _maxFillRate);
                return;
            }

            float gameDeltaTime = GameTimeManager.Instance.GameDeltaTime;

            if (gameDeltaTime <= 0f)
            {
                return;
            }

            _timer += gameDeltaTime;

            while (_timer >= interval)
            {
                _timer -= interval;
            }

            float t = _timer / interval;
            float fillRate = Mathf.Lerp(_minFillRate, _maxFillRate, t);

            _material.SetFloat(FillRate, fillRate);
        }

        private void ResetFill()
        {
            _timer = 0f;

            if (_material != null)
            {
                _material.SetFloat(FillRate, _minFillRate);
            }
        }
    }
}
