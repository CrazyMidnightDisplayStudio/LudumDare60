using UnityEngine;

namespace Signal
{
    public class AmplifiedSignalBonusReceiver : Receiver
    {
        [SerializeField] private int requiredSignalCount = 1;
        [SerializeField] private int moneyPerSignal = 1;
        [SerializeField] private int extraMoneyIfAmplified = 1;
        [SerializeField] private int extraPointsIfAmplified = 25;
        [SerializeField] private float extraMultiplierIfAmplified = 1f;

        private int _receivedSignalCountThisRound;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                return;
            }

            if (requiredSignalCount <= 0)
            {
                return;
            }

            _receivedSignalCountThisRound++;

            if (_receivedSignalCountThisRound % requiredSignalCount != 0)
            {
                return;
            }

            if (moneyPerSignal > 0)
            {
                gameManager.AddMoney(moneyPerSignal);
            }

            if (wave == null || !wave.IsAmplified)
            {
                return;
            }

            if (extraMoneyIfAmplified > 0)
            {
                gameManager.AddMoney(extraMoneyIfAmplified);
            }

            if (extraPointsIfAmplified != 0)
            {
                gameManager.AddRoundPoints(extraPointsIfAmplified);
            }

            if (extraMultiplierIfAmplified != 0f)
            {
                gameManager.AddRoundPointsMultiplier(extraMultiplierIfAmplified);
            }
        }

        protected override void OnRoundStartedInternal()
        {
            _receivedSignalCountThisRound = 0;
        }

        private void OnValidate()
        {
            if (requiredSignalCount < 1)
            {
                requiredSignalCount = 1;
            }
        }
    }
}