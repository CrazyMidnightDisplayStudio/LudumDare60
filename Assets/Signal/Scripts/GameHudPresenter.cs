using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Signal
{
    public class GameHudPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI requiredStrengthText;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private GameObject endGameMenu;
        [SerializeField] private TextMeshProUGUI endGameMessageText;
        [SerializeField] private float resultPopDuration = 0.2f;
        [SerializeField] private float resultPopScale = 1.2f;
        [SerializeField] private TextMeshProUGUI rerollButtonText;
        
        private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");
        
        private GameManager _gameManager;
        private bool _isSubscribed;
        private Coroutine _resultPopCoroutine;
        private bool _wasShowingRoundResult;

        private void OnEnable()
        {
            TryBind();
            Refresh();
        }

        private void Start()
        {
            TryBind();
            Refresh();
        }

        private void OnDisable()
        {
            Unbind();
            StopResultPop();
            ResetResultTextVisual();
            _wasShowingRoundResult = false;
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

            _gameManager.OnUiDataChanged += Refresh;
            _isSubscribed = true;
        }

        private void Unbind()
        {
            if (!_isSubscribed || _gameManager == null)
            {
                return;
            }

            _gameManager.OnUiDataChanged -= Refresh;
            _isSubscribed = false;
        }

        private void Refresh()
        {
            if (_gameManager == null)
            {
                TryBind();

                if (_gameManager == null)
                {
                    return;
                }
            }
            
            if (rerollButtonText != null)
            {
                rerollButtonText.text = $"Reroll ${_gameManager.ShopRerollCost}";
            }

            if (roundText != null)
            {
                roundText.text = $"Round: {_gameManager.CurrentRound}/10";
            }

            if (moneyText != null)
            {
                moneyText.text = $"Coins: {_gameManager.Money}";
            }

            if (requiredStrengthText != null)
            {
                requiredStrengthText.text = $"Goal: {_gameManager.RequiredNetworkStrength.ToString("N0", UsCulture)}";
            }

            if (pointsText != null)
            {
                pointsText.text = $"{_gameManager.RoundPoints}";
            }

            if (multiplierText != null)
            {
                multiplierText.text = $"{_gameManager.CurrentMultiplier:0.##}";
            }

            if (timerText != null)
            {
                timerText.text = $"Time: {_gameManager.RoundTimeLeft:0.0}";
            }

            bool hasRoundResult = _gameManager.HasRoundResult;

            if (resultText != null)
            {
                if (hasRoundResult)
                {
                    resultText.text = _gameManager.PlayerNetworkStrength.ToString("N0", UsCulture);
                }
                else
                {
                    resultText.text = string.Empty;
                    ResetResultTextVisual();
                }
            }

            if (hasRoundResult && !_wasShowingRoundResult)
            {
                PlayResultPop();
            }

            _wasShowingRoundResult = hasRoundResult;

            bool showEndGameMenu =
                _gameManager.CurrentState == GameManager.GameState.Lose ||
                _gameManager.CurrentState == GameManager.GameState.Win;

            if (endGameMenu != null)
            {
                endGameMenu.SetActive(showEndGameMenu);
            }

            if (endGameMessageText != null)
            {
                if (_gameManager.CurrentState == GameManager.GameState.Lose)
                {
                    endGameMessageText.text = "Your radio network was too weak. Try again.";
                }
                else if (_gameManager.CurrentState == GameManager.GameState.Win)
                {
                    endGameMessageText.text = "You won. Your radio network is very powerful. Thank you for playing!";
                }
                else
                {
                    endGameMessageText.text = string.Empty;
                }
            }
        }

        private void PlayResultPop()
        {
            if (resultText == null)
            {
                return;
            }

            StopResultPop();
            _resultPopCoroutine = StartCoroutine(ResultPopRoutine());
        }

        private IEnumerator ResultPopRoutine()
        {
            if (resultText == null)
            {
                _resultPopCoroutine = null;
                yield break;
            }

            float duration = resultPopDuration;

            if (duration <= 0f)
            {
                ResetResultTextVisual();
                _resultPopCoroutine = null;
                yield break;
            }

            RectTransform rectTransform = resultText.rectTransform;

            Vector3 startScale = Vector3.one * 0.8f;
            Vector3 peakScale = Vector3.one * resultPopScale;
            Vector3 endScale = Vector3.one;

            float halfDuration = duration * 0.5f;
            float time = 0f;

            rectTransform.localScale = startScale;

            while (time < halfDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = halfDuration <= 0f ? 1f : time / halfDuration;
                rectTransform.localScale = Vector3.LerpUnclamped(startScale, peakScale, t);
                yield return null;
            }

            time = 0f;

            while (time < halfDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = halfDuration <= 0f ? 1f : time / halfDuration;
                rectTransform.localScale = Vector3.LerpUnclamped(peakScale, endScale, t);
                yield return null;
            }

            rectTransform.localScale = endScale;
            _resultPopCoroutine = null;
        }

        private void StopResultPop()
        {
            if (_resultPopCoroutine == null)
            {
                return;
            }

            StopCoroutine(_resultPopCoroutine);
            _resultPopCoroutine = null;
        }

        private void ResetResultTextVisual()
        {
            if (resultText == null)
            {
                return;
            }

            resultText.rectTransform.localScale = Vector3.one;
        }
    }
}
