using UnityEngine;

namespace Signal
{
    public class EarlyRoundThreeWaveSequenceReceiver : Receiver
    {
        [SerializeField] private int maxRound = 5;
        [SerializeField] private int firstWavePointsReward = 50;
        [SerializeField] private float secondWaveMultiplierReward = 2f;
        [SerializeField] private int thirdWaveMoneyReward = 2;

        private int _receivedSignalCountThisRound;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                return;
            }

            if (gameManager.CurrentRound > maxRound)
            {
                return;
            }

            _receivedSignalCountThisRound++;

            if (_receivedSignalCountThisRound == 1)
            {
                gameManager.AddRoundPoints(firstWavePointsReward);
                return;
            }

            if (_receivedSignalCountThisRound == 2)
            {
                gameManager.AddRoundPointsMultiplier(secondWaveMultiplierReward);
                return;
            }

            if (_receivedSignalCountThisRound == 3)
            {
                gameManager.AddMoney(thirdWaveMoneyReward);
            }
        }

        protected override void OnRoundStartedInternal()
        {
            _receivedSignalCountThisRound = 0;
        }

        protected override void OnRoundEndedInternal()
        {
            _receivedSignalCountThisRound = 0;
        }
    }
}
