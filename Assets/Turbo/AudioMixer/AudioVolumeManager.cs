using UnityEngine;
using UnityEngine.InputSystem;

namespace Turbo.AudioMixer
{
    public class AudioVolumeManager : MonoBehaviour
    {
        public enum VolumeChannel
        {
            Master,
            Sfx,
            Music
        }

        private const float MaxDb = 0f;

        [SerializeField] private UnityEngine.Audio.AudioMixer mixer;
        [SerializeField] private string masterParameter = "Master";
        [SerializeField] private string sfxParameter = "SFX";
        [SerializeField] private string musicParameter = "Music";
        [SerializeField] private float defaultMasterVolume = 1f;
        [SerializeField] private float defaultSfxVolume = 1f;
        [SerializeField] private float defaultMusicVolume = 1f;
        [SerializeField] private float minDb = -80f;
        [SerializeField] private float keyboardStep = 0.1f;
        [SerializeField] private InputActionReference volumeUpAction;
        [SerializeField] private InputActionReference volumeDownAction;

        private float _masterVolume = 1f;
        private float _sfxVolume = 1f;
        private float _musicVolume = 1f;

        public static AudioVolumeManager Instance { get; private set; }

        public float MasterVolume => _masterVolume;
        public float SfxVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (volumeUpAction != null)
            {
                volumeUpAction.action.performed += OnVolumeUpPerformed;
                volumeUpAction.action.Enable();
            }

            if (volumeDownAction != null)
            {
                volumeDownAction.action.performed += OnVolumeDownPerformed;
                volumeDownAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (volumeUpAction != null)
            {
                volumeUpAction.action.performed -= OnVolumeUpPerformed;
                volumeUpAction.action.Disable();
            }

            if (volumeDownAction != null)
            {
                volumeDownAction.action.performed -= OnVolumeDownPerformed;
                volumeDownAction.action.Disable();
            }
        }

        private void Start()
        {
            SetMasterVolume(defaultMasterVolume);
            SetSfxVolume(defaultSfxVolume);
            SetMusicVolume(defaultMusicVolume);
        }

        public void SetMasterVolume(float value)
        {
            _masterVolume = Mathf.Clamp01(value);
            ApplyVolume(masterParameter, _masterVolume);
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            ApplyVolume(sfxParameter, _sfxVolume);
        }

        public void SetMusicVolume(float value)
        {
            _musicVolume = Mathf.Clamp01(value);
            ApplyVolume(musicParameter, _musicVolume);
        }

        public void SetVolume(VolumeChannel channel, float value)
        {
            switch (channel)
            {
                case VolumeChannel.Master:
                    SetMasterVolume(value);
                    break;

                case VolumeChannel.Sfx:
                    SetSfxVolume(value);
                    break;

                case VolumeChannel.Music:
                    SetMusicVolume(value);
                    break;
            }
        }

        public float GetVolume(VolumeChannel channel)
        {
            return channel switch
            {
                VolumeChannel.Master => _masterVolume,
                VolumeChannel.Sfx => _sfxVolume,
                VolumeChannel.Music => _musicVolume,
                _ => 1f
            };
        }

        private void OnVolumeUpPerformed(InputAction.CallbackContext context)
        {
            SetMasterVolume(_masterVolume + keyboardStep);
        }

        private void OnVolumeDownPerformed(InputAction.CallbackContext context)
        {
            SetMasterVolume(_masterVolume - keyboardStep);
        }

        private void ApplyVolume(string parameterName, float linearValue)
        {
            float db = linearValue <= 0f
                ? minDb
                : Mathf.Clamp(Mathf.Log10(linearValue) * 20f, minDb, MaxDb);

            mixer.SetFloat(parameterName, db);
        }

        private void OnValidate()
        {
            if (keyboardStep < 0f)
            {
                keyboardStep = 0f;
            }
        }
    }
}
