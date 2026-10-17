using UnityEngine;

namespace Signal
{
    public class GrowingChanceX4MultiplierReceiver : Receiver
    {
        [SerializeField] private float baseChancePercent = 1f;
        [SerializeField] private float chancePercentPerReceivedSignal = 0.5f;
        [SerializeField] private float multiplierValue = 4f;

        private int _receivedSignalCount;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            float chancePercent =
                baseChancePercent +
                chancePercentPerReceivedSignal * _receivedSignalCount;

            if (chancePercent > 100f)
            {
                chancePercent = 100f;
            }

            if (Random.Range(0f, 100f) < chancePercent)
            {
                GameManager.Instance.MultiplyRoundPointsMultiplierMultiplier(multiplierValue);
            }

            _receivedSignalCount++;
        }

        private void OnValidate()
        {
            if (baseChancePercent < 0f)
            {
                baseChancePercent = 0f;
            }

            if (chancePercentPerReceivedSignal < 0f)
            {
                chancePercentPerReceivedSignal = 0f;
            }

            if (multiplierValue < 0f)
            {
                multiplierValue = 0f;
            }
        }
    }
}