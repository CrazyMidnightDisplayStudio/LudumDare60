using System;
using System.Collections.Generic;
using Turbo.Fade;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GameManager : MonoBehaviour, IInterpreter
{
    [Serializable]
    public sealed class PuzzleStage
    {
        [SerializeField] private string password;
        [SerializeField] private GameObject led;
        [SerializeField, TextArea(2, 6)] private string[] radioMonologue = Array.Empty<string>();

        public string Password => password;
        public GameObject Led => led;
        public IReadOnlyList<string> RadioMonologue => radioMonologue;
    }

    private const int PuzzleCount = 5;

    [Header("Puzzles, in order")]
    [SerializeField] private PuzzleStage[] puzzles = { new(), new(), new(), new(), new() };
    [SerializeField] private bool ignorePasswordCase = true;

    [Header("Terminal responses")]
    [SerializeField] private string correctPasswordResponse = "Пароль принят.";
    [SerializeField] private string wrongPasswordResponse = "Неверный пароль.";
    [SerializeField, TextArea(2, 6)] private string finalText = "Все пароли приняты. Задание выполнено.";

    [Header("Success signal")]
    [SerializeField] private AudioSource signalSource;
    [SerializeField] private AudioClip correctPasswordSound;

    [Header("Ending")]
    [SerializeField] private FadeManager fadeManager;
    [SerializeField, Min(0f)] private float finalInputDelay = 1f;
    [SerializeField] private bool finishOnMouseMovement = true;

    private int _solvedPuzzleCount;
    private bool _configurationValid;
    private bool _interpreterOn = true;
    private bool _monologueActive;
    private bool _waitForInputRelease;
    private int _inputResumeFrame = -1;
    private int _endingStartFrame;
    private float _endingInputReadyTime;
    private bool _endingInputArmed;
    private bool _endingFadeStarted;

    public int SolvedPuzzleCount => _solvedPuzzleCount;
    public bool IsGameCompleted => _solvedPuzzleCount == PuzzleCount;
    public bool IsPlaying => isActiveAndEnabled && _configurationValid && !IsGameCompleted;
    public bool IsEnterPressed { get; set; }

    public bool IsInterpreterOn
    {
        get => isActiveAndEnabled && IsPlaying && _interpreterOn && !_monologueActive &&
               !_waitForInputRelease && Time.frameCount > _inputResumeFrame;
        set => _interpreterOn = value;
    }

    public event Action<int> PuzzleSolved;
    public event Action GameCompleted;

    private void Awake()
    {
        _configurationValid = ValidatePuzzles();

        if (puzzles == null)
            return;

        foreach (PuzzleStage puzzle in puzzles)
        {
            if (puzzle?.Led != null)
            {
                puzzle.Led.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!Application.isFocused)
            return;

        if (_waitForInputRelease && !HasHeldInput())
        {
            _waitForInputRelease = false;
            _inputResumeFrame = Time.frameCount + 1;
        }

        if (!IsGameCompleted || _endingFadeStarted ||
            Time.frameCount <= _endingStartFrame || Time.unscaledTime < _endingInputReadyTime)
        {
            return;
        }

        if (!_endingInputArmed)
        {
            _endingInputArmed = !HasHeldInput();
            return;
        }

        if (HasNewInput())
        {
            FinishGame();
        }
    }

    public List<string> Interpret(string input)
    {
        var output = new List<string>();

        if (!IsInterpreterOn || string.IsNullOrEmpty(input))
            return output;

        PuzzleStage puzzle = puzzles[_solvedPuzzleCount];
        StringComparison comparison = ignorePasswordCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!string.Equals(input.Trim(), puzzle.Password.Trim(), comparison))
        {
            output.Add(wrongPasswordResponse);
            return output;
        }

        if (puzzle.Led != null)
        {
            puzzle.Led.SetActive(true);
        }

        if (signalSource != null && correctPasswordSound != null)
        {
            signalSource.PlayOneShot(correctPasswordSound);
        }

        _solvedPuzzleCount++;
        output.Add(correctPasswordResponse);

        if (IsGameCompleted)
        {
            ReleaseEnter();
            _endingStartFrame = Time.frameCount;
            _endingInputReadyTime = Time.unscaledTime + finalInputDelay;
            output.Add(finalText);
        }

        PuzzleSolved?.Invoke(_solvedPuzzleCount);

        if (IsGameCompleted)
        {
            GameCompleted?.Invoke();
        }

        return output;
    }

    public void ReleaseEnter()
    {
        IsEnterPressed = false;
    }

    public IReadOnlyList<string> GetCurrentRadioMonologue()
    {
        if (!IsPlaying)
            return Array.Empty<string>();

        return puzzles[_solvedPuzzleCount].RadioMonologue ?? Array.Empty<string>();
    }

    public void SetMonologueActive(bool active)
    {
        if (_monologueActive == active)
            return;

        _monologueActive = active;
        ReleaseEnter();

        if (!active)
        {
            // Не передаём закрывающий диалог клик или Space в терминал.
            _waitForInputRelease = true;
        }
    }

    private bool ValidatePuzzles()
    {
        if (puzzles == null || puzzles.Length != PuzzleCount)
        {
            Debug.LogError($"{name}: configure exactly {PuzzleCount} puzzles.", this);
            return false;
        }

        var passwords = new HashSet<string>(ignorePasswordCase
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);

        for (int i = 0; i < puzzles.Length; i++)
        {
            string password = puzzles[i]?.Password;

            if (string.IsNullOrWhiteSpace(password) || !passwords.Add(password.Trim()))
            {
                Debug.LogError($"{name}: puzzle {i + 1} needs a nonempty, unique password.", this);
                return false;
            }
        }

        return true;
    }

    private void FinishGame()
    {
        if (fadeManager == null)
        {
            fadeManager = FadeManager.Instance;
        }

        if (fadeManager == null)
        {
            Debug.LogError($"{name}: assign a FadeManager to finish the game.", this);
            _endingInputArmed = false;
            return;
        }

        if (fadeManager.IsFading)
            return;

        _endingFadeStarted = true;
        fadeManager.FadeOut();
    }

    private static bool HasHeldInput()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            foreach (InputControl control in device.allControls)
            {
                if (control is ButtonControl button && button.isPressed)
                    return true;
            }
        }

        return HasGamepadStickInput();
    }

    private bool HasNewInput()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            foreach (InputControl control in device.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                    return true;
            }
        }

        Mouse mouse = Mouse.current;

        return (mouse != null && (mouse.scroll.ReadValue().sqrMagnitude > 0f ||
                (finishOnMouseMovement && mouse.delta.ReadValue().sqrMagnitude > 0f))) ||
               HasGamepadStickInput();
    }

    private static bool HasGamepadStickInput()
    {
        Gamepad gamepad = Gamepad.current;
        return gamepad != null && (gamepad.leftStick.ReadValue().sqrMagnitude > 0.1f ||
                                   gamepad.rightStick.ReadValue().sqrMagnitude > 0.1f);
    }
}
