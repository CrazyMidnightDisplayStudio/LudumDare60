using Turbo.GameTime;
using UnityEngine;

namespace Signal
{
    [RequireComponent(typeof(SignalSender))]
    public class Transmitter : MonoBehaviour
    {
        [SerializeField] private float intervalSeconds = 2f;
        
        private Vibrate _vibrate;
        private SignalSender _signalSender;
        private float _timer;
        private bool _isTransmitting;
        
        public float IntervalSeconds => intervalSeconds;

        private void Awake()
        {
            _signalSender = GetComponent<SignalSender>();
            _vibrate = GetComponent<Vibrate>();
        }

        private void Update()
        {
            if (!_isTransmitting)
            {
                return;
            }

            GameTimeManager gameTimeManager = GameTimeManager.Instance;

            if (gameTimeManager == null)
            {
                return;
            }

            float gameDeltaTime = gameTimeManager.GameDeltaTime;

            if (gameDeltaTime <= 0f)
            {
                return;
            }

            _timer += gameDeltaTime;

            while (_timer >= intervalSeconds)
            {
                _timer -= intervalSeconds;
                _vibrate.Play();
                _signalSender.SendSignal();
            }
        }

        private void OnValidate()
        {
            if (intervalSeconds < 0.01f)
            {
                intervalSeconds = 0.01f;
            }
        }

        public void StartTransmitting()
        {
            _timer = 0f;
            _isTransmitting = true;
            _signalSender.SendSignal();
        }

        public void StopTransmitting()
        {
            _timer = 0f;
            _isTransmitting = false;
        }
    }
}
