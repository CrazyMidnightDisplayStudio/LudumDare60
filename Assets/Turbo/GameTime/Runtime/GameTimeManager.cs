using System;
using System.Collections.Generic;
using UnityEngine;

namespace Turbo.GameTime
{
    public class GameTimeManager : MonoBehaviour
    {
        public enum TimeState
        {
            Normal,
            Fast,
            Paused
        }

        [SerializeField] private TimeState initialState = TimeState.Normal;

        private readonly Dictionary<TimeState, float> _stateSpeeds = new()
        {
            { TimeState.Normal, 1f },
            { TimeState.Fast, 2f },
            { TimeState.Paused, 0f }
        };

        private readonly List<TimeState> _cycleOrder = new()
        {
            TimeState.Normal,
            TimeState.Fast,
            TimeState.Paused
        };

        private int _cycleIndex;
        private float _gameTime;
        private float _gameDeltaTime;
        private float _currentSpeed;
        private TimeState _currentState;

        public static GameTimeManager Instance { get; private set; }

        public float GameTime => _gameTime;
        public float GameDeltaTime => _gameDeltaTime;
        public float Speed => _currentSpeed;
        public TimeState CurrentState => _currentState;
        public bool IsPaused => _currentState == TimeState.Paused;

        public event Action<TimeState> OnTimeStateChanged;

        private void Awake()
        {
            Instance = this;
            ApplyStateInternal(initialState);
        }

        private void Update()
        {
            _gameDeltaTime = Time.deltaTime * _currentSpeed;
            _gameTime += _gameDeltaTime;
        }

        public void SetNormal()
        {
            SetState(TimeState.Normal);
        }

        public void SetFast()
        {
            SetState(TimeState.Fast);
        }

        public void SetPaused()
        {
            SetState(TimeState.Paused);
        }

        public void NextState()
        {
            _cycleIndex = (_cycleIndex + 1) % _cycleOrder.Count;
            SetState(_cycleOrder[_cycleIndex]);
        }

        public void SetState(TimeState state)
        {
            if (_currentState == state)
                return;

            ApplyStateInternal(state);
            OnTimeStateChanged?.Invoke(_currentState);
        }

        public void Restore(float gameTime, TimeState state)
        {
            _gameTime = gameTime;
            ApplyStateInternal(state);
        }

        private void ApplyStateInternal(TimeState state)
        {
            _currentState = state;
            _currentSpeed = _stateSpeeds[state];
            _cycleIndex = _cycleOrder.IndexOf(state);
        }
    }
}