using UnityEngine;

namespace Signal
{
    [RequireComponent(typeof(SignalSender))]
    public class Repeater : Receiver
    {
        private int _lockedSourceId = -1;
        private SignalSender _signalSender;

        protected override void Awake()
        {
            base.Awake();
            _signalSender = GetComponent<SignalSender>();
        }

        protected override void OnWaveReceivedInternal(ISignalWave wave)
        {
            // Здесь можно писать логику Repeater как у обычного Receiver.

            if (!IsLockedSourceWave(wave))
            {
                return;
            }

            if (_signalSender == null)
            {
                return;
            }

            _signalSender.SendSignal(wave);
        }
        
        protected override void OnRoundStartedInternal()
        {
            _lockedSourceId = -1;
        }
        
        protected bool IsLockedSourceWave(ISignalWave wave)
        {
            if (wave == null)
            {
                return false;
            }

            if (_lockedSourceId == -1)
            {
                _lockedSourceId = wave.SourceId;
            }

            return wave.SourceId == _lockedSourceId;
        }
    }
}
