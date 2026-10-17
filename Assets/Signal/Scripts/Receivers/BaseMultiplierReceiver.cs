using UnityEngine;

namespace Signal
{
    public class BaseMultiplierReceiver : Receiver
    {
        [SerializeField] private float multiplierValue = 1f;
        [SerializeField] private int requiredSignalCount = 1;

        private int _receivedSignalCountThisRound;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (GameManager.Instance == null)
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

            GameManager.Instance.AddRoundPointsMultiplier(multiplierValue);
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