using UnityEngine;

namespace Signal
{
    public class EveryNthWaveMoneyReceiver : Receiver
    {
        [SerializeField] private int requiredWaveCount = 2;
        [SerializeField] private int moneyReward = 1;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (wave == null)
            {
                return;
            }

            if (requiredWaveCount <= 0)
            {
                return;
            }

            if (moneyReward <= 0)
            {
                return;
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            int roundWaveCount = GetRoundWaveCount(wave.SourceId);

            if (roundWaveCount % requiredWaveCount != 0)
            {
                return;
            }

            GameManager.Instance.AddMoney(moneyReward);
        }
    }
}
