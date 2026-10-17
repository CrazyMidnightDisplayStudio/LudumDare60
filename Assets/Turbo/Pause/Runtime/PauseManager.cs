using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Turbo.Pause
{
    [Serializable]
    public sealed class PauseStateChangedUnityEvent : UnityEvent<bool>
    {
    }

    public sealed class PauseManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GameObject pauseMenuRoot;
        [SerializeField] private Selectable firstSelected;
        [SerializeField] private PauseStateChangedUnityEvent pauseStateChanged;
        [SerializeField] private UnityEvent paused;
        [SerializeField] private UnityEvent resumed;

        private InputActionMap _gameplayMap;
        private InputActionMap _menuUiMap;
        private InputAction _pauseAction;
        private bool _isPaused;

        public static PauseManager Instance { get; private set; }
        public static bool IsPaused => Instance != null && Instance._isPaused;

        public PauseStateChangedUnityEvent PauseStateChanged => pauseStateChanged;
        public UnityEvent Paused => paused;
        public UnityEvent Resumed => resumed;

        public event Action<bool> PauseStateChangedAction;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _gameplayMap = playerInput.actions.FindActionMap("Gameplay", true);
            _menuUiMap = playerInput.actions.FindActionMap("MenuUI", true);
            _pauseAction = _gameplayMap.FindAction("Pause", true);

            Time.timeScale = 1f;
            AudioListener.pause = false;

            pauseMenuRoot.SetActive(false);
            _menuUiMap.Disable();
            _gameplayMap.Enable();
            _isPaused = false;
        }

        private void OnEnable()
        {
            _pauseAction.performed += OnPausePerformed;
        }

        private void OnDisable()
        {
            _pauseAction.performed -= OnPausePerformed;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            if (_isPaused)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }

            Instance = null;
        }

        public static void SetPaused(bool value)
        {
            if (Instance == null)
                return;

            if (value)
                Instance.Pause();
            else
                Instance.Resume();
        }

        public static void TogglePause()
        {
            if (Instance == null)
                return;

            if (Instance._isPaused)
                Instance.Resume();
            else
                Instance.Pause();
        }

        public void Pause()
        {
            if (_isPaused)
                return;

            _isPaused = true;

            _gameplayMap.Disable();
            _menuUiMap.Enable();

            Time.timeScale = 0f;
            AudioListener.pause = true;

            pauseMenuRoot.SetActive(true);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);

                if (firstSelected != null)
                    firstSelected.Select();
            }

            PauseStateChangedAction?.Invoke(true);
            pauseStateChanged.Invoke(true);
            paused.Invoke();
        }

        public void Resume()
        {
            if (!_isPaused)
                return;

            _isPaused = false;

            pauseMenuRoot.SetActive(false);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            _menuUiMap.Disable();
            _gameplayMap.Enable();

            AudioListener.pause = false;
            Time.timeScale = 1f;

            PauseStateChangedAction?.Invoke(false);
            pauseStateChanged.Invoke(false);
            resumed.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (_isPaused)
                return;

            Pause();
        }
    }
}