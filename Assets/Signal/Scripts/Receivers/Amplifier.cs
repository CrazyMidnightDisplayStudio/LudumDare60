using UnityEngine;

namespace Signal
{
    public class Amplifier : Receiver
    {
        [SerializeField] private float addedRadius = 1f;
        [SerializeField] private bool changeSpeed;
        [SerializeField] private float speedMultiplier = 2f;
        [SerializeField] private bool changeColor;
        [SerializeField] private Color newColor = Color.white;

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            if (wave == null)
            {
                return;
            }
            
            wave.MarkAsAmplified();
            
            // Здесь можно писать условия Amplifier как для обычного Receiver.

            wave.AddMaxRadius(addedRadius);

            if (changeSpeed)
            {
                wave.MultiplySpeed(speedMultiplier);
            }

            if (changeColor)
            {
                wave.SetColor(newColor);
            }
        }

        private void OnValidate()
        {
            if (addedRadius < 0f)
            {
                addedRadius = 0f;
            }

            if (speedMultiplier < 1f)
            {
                speedMultiplier = 1f;
            }
        }
    }
}