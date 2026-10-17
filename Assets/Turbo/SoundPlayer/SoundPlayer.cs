using System.Collections.Generic;
using UnityEngine;

namespace Turbo.SoundPlayer
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource defaultSource;

        private readonly Dictionary<string, AudioClip> _clipCache = new();

        public static SoundPlayer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Play(string clipPath)
        {
            Play(defaultSource, clipPath);
        }

        public void Play(AudioSource source, string clipPath)
        {
            if (source == null || string.IsNullOrWhiteSpace(clipPath))
                return;

            AudioClip clip = GetClip(clipPath);

            if (clip == null)
                return;

            source.PlayOneShot(clip);
        }

        public void Preload(string clipPath)
        {
            if (string.IsNullOrWhiteSpace(clipPath))
                return;

            GetClip(clipPath);
        }

        public void ClearCache()
        {
            _clipCache.Clear();
        }

        private AudioClip GetClip(string clipPath)
        {
            if (_clipCache.TryGetValue(clipPath, out AudioClip cachedClip))
                return cachedClip;

            AudioClip loadedClip = Resources.Load<AudioClip>(clipPath);

            if (loadedClip == null)
                return null;

            _clipCache.Add(clipPath, loadedClip);
            return loadedClip;
        }
    }
}
