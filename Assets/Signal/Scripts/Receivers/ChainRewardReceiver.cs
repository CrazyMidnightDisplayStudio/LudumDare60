using UnityEngine;

namespace Signal
{
    public class ChainRewardReceiver : Receiver
    {
        [SerializeField] private float multiplierPerRepeater = 1f;
        [SerializeField] private int pointsPerRepeater = 30;
        [SerializeField] private int requiredWaveCount = 2;
        [SerializeField] private int moneyReward = 1;

        private int _receivedWaveCount;
        private int _lockedSourceId = -1;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (wave == null)
            {
                return;
            }
            
            if (_lockedSourceId == -1)
            {
                _lockedSourceId = wave.SourceId;
            }

            if (wave.SourceId != _lockedSourceId)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                return;
            }

            int repeaterChainCount = wave.RepeaterChainCount;

            if (repeaterChainCount > 0)
            {
                float multiplierReward = multiplierPerRepeater * repeaterChainCount;
                int pointsReward = pointsPerRepeater * repeaterChainCount;

                if (multiplierReward != 0f)
                {
                    gameManager.AddRoundPointsMultiplier(multiplierReward);
                }

                if (pointsReward != 0)
                {
                    gameManager.AddRoundPoints(pointsReward);
                }
            }

            _receivedWaveCount++;

            if (requiredWaveCount <= 0)
            {
                return;
            }

            if (moneyReward <= 0)
            {
                return;
            }

            if (_receivedWaveCount % requiredWaveCount != 0)
            {
                return;
            }

            gameManager.AddMoney(moneyReward);
        }

        protected override void OnRoundStartedInternal()
        {
            _receivedWaveCount = 0;
            _lockedSourceId = -1;
        }
    }
}
