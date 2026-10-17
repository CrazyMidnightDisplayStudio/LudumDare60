using UnityEngine;

namespace Signal
{
    public class ChanceMultiplierMultiplierReceiver : Receiver
    {
        [SerializeField] private float chancePercent = 25f;
        [SerializeField] private float multiplierMultiplierValue = 1.5f;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (chancePercent <= 0f)
            {
                return;
            }

            if (Random.Range(0f, 100f) > chancePercent)
            {
                return;
            }

            GameManager.Instance.AddRoundPointsMultiplierMultiplier(multiplierMultiplierValue);
        }

        private void OnValidate()
        {
            if (chancePercent < 0f)
            {
                chancePercent = 0f;
            }

            if (chancePercent > 100f)
            {
                chancePercent = 100f;
            }
        }
    }
}
