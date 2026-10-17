using System;
using System.Collections;
using Turbo.GameTime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Signal
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Shop,
            Round,
            Lose,
            Win
        }
        
        private const int MaxRounds = 10;

        private const int FirstRoundMoney = 10;
        private const int FirstRoundRequiredNetworkStrength = 500;
        private const float RoundDurationSeconds = 12f;
        
        [SerializeField] private Transmitter transmitter;
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private InputActionReference restartAction;
        
        private Coroutine _roundCoroutine;
        private int _money;
        private int _currentRound;
        private int _requiredNetworkStrength;
        private int _roundPoints;
        private float _roundPointsMultiplier;
        private float _roundPointsMultiplierMultiplier;
        private float _playerNetworkStrength;
        private float _roundTimeLeft;
        private GameState _currentState;
        private bool _hasRoundResult;
        private int _shopRerollCount;
        
        public int ShopRerollCost => 1 + _shopRerollCount * (_shopRerollCount + 1) / 2;

        public bool HasRoundResult => _hasRoundResult;

        public static GameManager Instance { get; private set; }

        public int Money => _money;
        public int CurrentRound => _currentRound;
        public int RequiredNetworkStrength => _requiredNetworkStrength;
        public int RoundPoints => _roundPoints;
        public float RoundPointsMultiplier => _roundPointsMultiplier;
        public float RoundPointsMultiplierMultiplier => _roundPointsMultiplierMultiplier;
        public float CurrentMultiplier => _roundPointsMultiplier * _roundPointsMultiplierMultiplier;
        public float PlayerNetworkStrength => _playerNetworkStrength;
        public float RoundTimeLeft => _roundTimeLeft;
        public float RoundDuration => RoundDurationSeconds;
        public GameState CurrentState => _currentState;
        public bool IsRoundActive => _currentState == GameState.Round;

        public event Action OnUiDataChanged;
        public event Action OnGameStarted;
        public event Action OnRoundStarted;
        public event Action OnRoundEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            StartGame();
        }
        
        private void OnRestartPerformed(InputAction.CallbackContext context)
        {
            Restart();
        }

        private void OnEnable()
        {
            if (restartAction != null)
            {
                restartAction.action.performed += OnRestartPerformed;
                restartAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            StopRoundCoroutine();
            
            if (restartAction != null)
            {
                restartAction.action.performed -= OnRestartPerformed;
                restartAction.action.Disable();
            }

            if (transmitter != null)
            {
                transmitter.StopTransmitting();
            }

            SignalSender.ClearAllSignals();
        }
        
        public void RerollShop()
        {
            if (_currentState != GameState.Shop)
            {
                return;
            }

            if (shopManager == null)
            {
                return;
            }

            int rerollCost = ShopRerollCost;

            if (_money < rerollCost)
            {
                return;
            }

            _money -= rerollCost;
            _shopRerollCount++;

            shopManager.ClearStore();
            shopManager.FillStore(_currentRound);

            NotifyUiDataChanged();
        }

        public void StartGame()
        {
            StopRoundCoroutine();
            _hasRoundResult = false;
            
            _shopRerollCount = 0;

            _money = FirstRoundMoney;
            _currentRound = 1;
            _requiredNetworkStrength = FirstRoundRequiredNetworkStrength;
            _currentState = GameState.Shop;
            _roundTimeLeft = 0f;

            ResetRoundValues();
            
            OnGameStarted?.Invoke();

            if (transmitter != null)
            {
                transmitter.StopTransmitting();
            }

            SignalSender.ClearAllSignals();

            if (shopManager != null)
            {
                shopManager.FillStore(_currentRound);
            }

            NotifyUiDataChanged();
        }
        
        public void StartRound()
        {
            if (_currentState != GameState.Shop)
            {
                return;
            }
            
            _hasRoundResult = false;

            StopRoundCoroutine();
            SignalSender.ClearAllSignals();

            if (shopManager != null)
            {
                shopManager.ClearStore();
            }

            ResetRoundValues();
            _roundTimeLeft = RoundDurationSeconds;
            _currentState = GameState.Round;
            
            OnRoundStarted?.Invoke();

            if (transmitter != null)
            {
                transmitter.StartTransmitting();
            }

            NotifyUiDataChanged();
            _roundCoroutine = StartCoroutine(RoundRoutine());
        }

        public void EndRound()
        {
            if (_currentState != GameState.Round)
            {
                return;
            }
            
            OnRoundEnded?.Invoke();

            StopRoundCoroutine();

            if (transmitter != null)
            {
                transmitter.StopTransmitting();
            }

            SignalSender.ClearAllSignals();

            _roundTimeLeft = 0f;
            _playerNetworkStrength = CalculatePlayerNetworkStrength();
            _hasRoundResult = true;

            if (_playerNetworkStrength < _requiredNetworkStrength)
            {
                _currentState = GameState.Lose;
                NotifyUiDataChanged();
                return;
            }
            
            if (_currentRound >= MaxRounds)
            {
                _currentState = GameState.Win;
                NotifyUiDataChanged();
                return;
            }

            _currentRound++;
            _requiredNetworkStrength = CalculateRequiredNetworkStrength(_currentRound);
            _currentState = GameState.Shop;

            if (shopManager != null)
            {
                shopManager.FillStore(_currentRound);
                _shopRerollCount = 0;
            }

            NotifyUiDataChanged();
        }

        public void AddMoney(int value)
        {
            if (value <= 0)
            {
                return;
            }

            _money += value;
            NotifyUiDataChanged();
        }

        public bool TrySpendMoney(int value)
        {
            if (value <= 0)
            {
                return true;
            }

            if (_money < value)
            {
                return false;
            }

            _money -= value;
            NotifyUiDataChanged();
            return true;
        }

        public void AddRoundPoints(int value)
        {
            if (!IsRoundActive || value == 0)
            {
                return;
            }

            _roundPoints += value;

            if (_roundPoints < 0)
            {
                _roundPoints = 0;
            }

            NotifyUiDataChanged();
        }

        public void AddRoundPointsMultiplier(float value)
        {
            if (!IsRoundActive || value == 0f)
            {
                return;
            }

            _roundPointsMultiplier += value;

            if (_roundPointsMultiplier < 0f)
            {
                _roundPointsMultiplier = 0f;
            }

            NotifyUiDataChanged();
        }

        public void AddRoundPointsMultiplierMultiplier(float value)
        {
            if (!IsRoundActive || value == 0f)
            {
                return;
            }

            _roundPointsMultiplierMultiplier += value;

            if (_roundPointsMultiplierMultiplier < 0f)
            {
                _roundPointsMultiplierMultiplier = 0f;
            }

            NotifyUiDataChanged();
        }

        public void MultiplyRoundPointsMultiplier(float value)
        {
            if (!IsRoundActive || value < 0f)
            {
                return;
            }

            _roundPointsMultiplier *= value;
            NotifyUiDataChanged();
        }

        public void MultiplyRoundPointsMultiplierMultiplier(float value)
        {
            if (!IsRoundActive || value < 0f)
            {
                return;
            }

            _roundPointsMultiplierMultiplier *= value;
            NotifyUiDataChanged();
        }

        private IEnumerator RoundRoutine()
        {
            while (_roundTimeLeft > 0f && _currentState == GameState.Round)
            {
                GameTimeManager gameTimeManager = GameTimeManager.Instance;

                if (gameTimeManager == null)
                {
                    yield return null;
                    continue;
                }

                float gameDeltaTime = gameTimeManager.GameDeltaTime;

                if (gameDeltaTime <= 0f)
                {
                    yield return null;
                    continue;
                }

                _roundTimeLeft -= gameDeltaTime;

                if (_roundTimeLeft < 0f)
                {
                    _roundTimeLeft = 0f;
                }

                NotifyUiDataChanged();
                yield return null;
            }

            _roundCoroutine = null;

            if (_currentState == GameState.Round)
            {
                EndRound();
            }
        }

        private void ResetRoundValues()
        {
            _roundPoints = 0;
            _roundPointsMultiplier = 1f;
            _roundPointsMultiplierMultiplier = 1f;
            _playerNetworkStrength = 0f;
        }

        private float CalculatePlayerNetworkStrength()
        {
            return _roundPoints * _roundPointsMultiplier * _roundPointsMultiplierMultiplier;
        }

        private int CalculateRequiredNetworkStrength(int round)
        {
            int value = FirstRoundRequiredNetworkStrength;

            for (int i = 1; i < round; i++)
            {
                int multiplier = i + 1;

                if (value > int.MaxValue / multiplier)
                {
                    return int.MaxValue;
                }

                value *= multiplier;
            }

            return value;
        }

        private void NotifyUiDataChanged()
        {
            OnUiDataChanged?.Invoke();
        }

        private void StopRoundCoroutine()
        {
            if (_roundCoroutine == null)
            {
                return;
            }

            StopCoroutine(_roundCoroutine);
            _roundCoroutine = null;
        }
        
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}