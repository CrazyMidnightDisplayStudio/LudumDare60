using UnityEngine;

namespace Signal
{
    public class NoSignalRoundMoneyReceiver : Receiver
    {
        [SerializeField] private int moneyReward = 5;

        private bool _receivedSignalThisRound;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            _receivedSignalThisRound = true;
        }

        protected override void OnRoundStartedInternal()
        {
            _receivedSignalThisRound = false;
        }

        protected override void OnRoundEndedInternal()
        {
            if (_receivedSignalThisRound)
            {
                return;
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.AddMoney(moneyReward);
        }
    }
}
