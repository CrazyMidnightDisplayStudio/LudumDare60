using System;
using UnityEngine;

namespace Signal
{
    public class RepeaterPointsFromRepeater : Repeater
    {
        [SerializeField] private int pointsReward = 25;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (!IsLockedSourceWave(wave))
            {
                return;
            }

            if (IsSignalFromRepeater(wave) && GameManager.Instance != null)
            {
                GameManager.Instance.AddRoundPoints(pointsReward);
            }

            base.OnWaveReceivedInternal(wave);
        }

        private bool IsSignalFromRepeater(ISignalWave wave)
        {
            if (wave == null)
            {
                return false;
            }

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
    }
}
