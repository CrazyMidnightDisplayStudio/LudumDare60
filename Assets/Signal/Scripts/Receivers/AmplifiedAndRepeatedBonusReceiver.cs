using System;
using UnityEngine;

namespace Signal
{
    public class AmplifiedAndRepeatedBonusReceiver : Receiver
    {
        [SerializeField] private int moneyReward = 2;
        [SerializeField] private int pointsReward = 50;
        [SerializeField] private float multiplierReward = 2f;
        [SerializeField] private int amplifiedRewardMultiplier = 2;
        [SerializeField] private int repeatedRewardMultiplier = 2;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager == null || wave == null)
            {
                return;
            }

            int rewardMultiplier = 1;

            if (wave.IsAmplified)
            {
                rewardMultiplier *= amplifiedRewardMultiplier;
            }

            if (IsRepeatedSignal(wave))
            {
                rewardMultiplier *= repeatedRewardMultiplier;
            }

            int totalMoneyReward = moneyReward * rewardMultiplier;
            int totalPointsReward = pointsReward * rewardMultiplier;
            float totalMultiplierReward = multiplierReward * rewardMultiplier;

            if (totalMoneyReward > 0)
            {
                gameManager.AddMoney(totalMoneyReward);
            }

            if (totalPointsReward != 0)
            {
                gameManager.AddRoundPoints(totalPointsReward);
            }

            if (totalMultiplierReward != 0f)
            {
                gameManager.AddRoundPointsMultiplier(totalMultiplierReward);
            }
        }

        private bool IsRepeatedSignal(ISignalWave wave)
        {
            if (wave.RepeaterChainCount <= 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(wave.SourceName))
            {
                return false;
            }

            return wave.SourceName.IndexOf("Repeater", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnValidate()
        {
            if (amplifiedRewardMultiplier < 1)
            {
                amplifiedRewardMultiplier = 1;
            }

            if (repeatedRewardMultiplier < 1)
            {
                repeatedRewardMultiplier = 1;
            }
        }
    }
}
