using UnityEngine;

namespace Signal
{
    public class FirstThreeWaveSequenceReceiver : Receiver
    {
        [SerializeField] private int firstWavePointsReward = 50;
        [SerializeField] private float secondWaveMultiplierReward = 2f;
        [SerializeField] private int thirdWaveMoneyReward = 2;

        private int _receivedSignalCountThisRound;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            _receivedSignalCountThisRound++;

            if (_receivedSignalCountThisRound == 1)
            {
                GameManager.Instance.AddRoundPoints(firstWavePointsReward);
                return;
            }

            if (_receivedSignalCountThisRound == 2)
            {
                GameManager.Instance.AddRoundPointsMultiplier(secondWaveMultiplierReward);
                return;
            }

            if (_receivedSignalCountThisRound == 3)
            {
                GameManager.Instance.AddMoney(thirdWaveMoneyReward);
            }
        }

        protected override void OnRoundStartedInternal()
        {
            _receivedSignalCountThisRound = 0;
        }
    }
}
