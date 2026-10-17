using UnityEngine;

namespace Signal
{
    public class BaseReceiver : Receiver
    {
        [SerializeField] private int pointsPerWave = 50;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.AddRoundPoints(pointsPerWave);
        }
    }
}
