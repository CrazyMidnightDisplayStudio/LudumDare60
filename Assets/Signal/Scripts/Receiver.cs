using System.Collections.Generic;
using UnityEngine;

namespace Signal
{
    public class Receiver : MonoBehaviour, IWaveReceiver
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private bool collectGameStatistics;

        private readonly Dictionary<int, SourceWaveStats> _roundStatsBySource = new();
        private readonly Dictionary<int, SourceWaveStats> _gameStatsBySource = new();

        private Vibrate _vibrate;
        private AudioSource _audioSource;
        private GameManager _gameManager;
        private bool _isSubscribed;

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _vibrate = GetComponent<Vibrate>();
        }

        private void OnEnable()
        {
            TryBind();
        }

        private void Start()
        {
            TryBind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void ReceiveWave(ISignalWave wave)
        {
            RegisterWave(_roundStatsBySource, wave.SourceId, wave.SourceName);

            if (collectGameStatistics)
            {
                RegisterWave(_gameStatsBySource, wave.SourceId, wave.SourceName);
            }

            OnWaveReceivedInternal(wave);

            if (_vibrate != null)
            {
                _vibrate.Play();
            }

            if (_audioSource != null && audioClip != null)
            {
                _audioSource.PlayOneShot(audioClip);
            }
        }

        protected virtual void OnWaveReceivedInternal(ISignalWave wave)
        {
        }

        protected int GetRoundWaveCount(int sourceId)
        {
            if (_roundStatsBySource.TryGetValue(sourceId, out SourceWaveStats stats))
            {
                return stats.WaveCount;
            }

            return 0;
        }

        protected int GetGameWaveCount(int sourceId)
        {
            if (_gameStatsBySource.TryGetValue(sourceId, out SourceWaveStats stats))
            {
                return stats.WaveCount;
            }

            return 0;
        }

        protected string GetRoundSourceName(int sourceId)
        {
            if (_roundStatsBySource.TryGetValue(sourceId, out SourceWaveStats stats))
            {
                return stats.SourceName;
            }

            return string.Empty;
        }

        protected string GetGameSourceName(int sourceId)
        {
            if (_gameStatsBySource.TryGetValue(sourceId, out SourceWaveStats stats))
            {
                return stats.SourceName;
            }

            return string.Empty;
        }

        private void TryBind()
        {
            if (_isSubscribed)
            {
                return;
            }

            _gameManager = GameManager.Instance;

            if (_gameManager == null)
            {
                return;
            }

            _gameManager.OnRoundStarted += ClearRoundStatistics;
            _gameManager.OnGameStarted += ClearGameStatistics;
            _gameManager.OnRoundEnded += HandleRoundEnded;
            _isSubscribed = true;
        }

        private void Unbind()
        {
            if (!_isSubscribed || _gameManager == null)
            {
                return;
            }

            _gameManager.OnRoundStarted -= ClearRoundStatistics;
            _gameManager.OnGameStarted -= ClearGameStatistics;
            _gameManager.OnRoundEnded -= HandleRoundEnded;
            _isSubscribed = false;
        }

        private void ClearRoundStatistics()
        {
            _roundStatsBySource.Clear();
            OnRoundStartedInternal();
        }

        private void ClearGameStatistics()
        {
            _gameStatsBySource.Clear();
        }

        private void RegisterWave(
            Dictionary<int, SourceWaveStats> statsBySource,
            int sourceId,
            string sourceName)
        {
            if (statsBySource.TryGetValue(sourceId, out SourceWaveStats stats))
            {
                stats.AddWave(sourceName);
                return;
            }

            statsBySource[sourceId] = new SourceWaveStats(sourceId, sourceName);
        }

        private void HandleRoundEnded()
        {
            OnRoundEndedInternal();
        }

        protected virtual void OnRoundStartedInternal()
        {
        }

        protected virtual void OnRoundEndedInternal()
        {
        }

        private sealed class SourceWaveStats
        {
            public SourceWaveStats(int sourceId, string sourceName)
            {
                SourceId = sourceId;
                SourceName = sourceName;
                WaveCount = 1;
            }

            public int SourceId { get; }
            public string SourceName { get; private set; }
            public int WaveCount { get; private set; }

            public void AddWave(string sourceName)
            {
                if (!string.IsNullOrEmpty(sourceName))
                {
                    SourceName = sourceName;
                }

                WaveCount++;
            }
        }
    }
}
