using UnityEngine;

namespace Signal
{
    public interface ISignalWave
    {
        int SourceId { get; }
        string SourceName { get; }
        int RepeaterChainCount { get; }
        bool IsAmplified { get; }
        
        void MarkAsAmplified();
        void AddMaxRadius(float value);
        void MultiplySpeed(float multiplier);
        void SetColor(Color value);
    }
}
