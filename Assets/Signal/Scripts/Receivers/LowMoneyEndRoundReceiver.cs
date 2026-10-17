using UnityEngine;

namespace Signal
{
    public class LowMoneyEndRoundReceiver : Receiver
    {
        [SerializeField] private int requiredSignalCount = 4;
        [SerializeField] private int moneyPerSignalThreshold = 1;
        [SerializeField] private int lowMoneyThreshold = 5;
        [SerializeField] private int endRoundBonusMoney = 5;

        private int _receivedSignalCountThisRound;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (requiredSignalCount <= 0)
            {
                return;
            }

            if (moneyPerSignalThreshold <= 0)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                return;
            }

            _receivedSignalCountThisRound++;

            if (_receivedSignalCountThisRound % requiredSignalCount != 0)
            {
                return;
            }

            gameManager.AddMoney(moneyPerSignalThreshold);
        }

        protected override void OnRoundStartedInternal()
        {
            _receivedSignalCountThisRound = 0;
        }

        protected override void OnRoundEndedInternal()
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                return;
            }

            if (endRoundBonusMoney <= 0)
            {
                return;
            }

            if (gameManager.Money >= lowMoneyThreshold)
            {
                return;
            }

            gameManager.AddMoney(endRoundBonusMoney);
        }
    }
}
