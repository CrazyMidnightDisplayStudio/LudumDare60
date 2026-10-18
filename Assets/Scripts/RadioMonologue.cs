using System.Collections.Generic;
using LD60;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioMonologue : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private Radio radio;
    [SerializeField] private GameManager gameManager;
    [SerializeField, Range(Radio.MIN_WAVE_NUMBER, Radio.MAX_WAVE_NUMBER)]
    private int contactFrequency = Constexpr.DEFAULT_WAVE;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private string speakerName = "Связной";
    [SerializeField, Min(0f)] private float charactersPerSecond = 35f;

    private readonly HashSet<int> _completedStages = new();
    private IReadOnlyList<string> _lines;
    private int _stageIndex;
    private int _lineIndex;
    private int _characterCount;
    private float _visibleCharacters;
    private int _lastAdvanceFrame;
    private bool _isTyping;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (radio == null || gameManager == null || panel == null || lineText == null || panel == gameObject)
        {
            Debug.LogError($"{name}: assign Radio, GameManager, text and a separate monologue panel.", this);
            enabled = false;
            return;
        }

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        radio.FrequencyChanged += OnFrequencyChanged;
        gameManager.PuzzleSolved += OnPuzzleSolved;
        TryStartMonologue();
    }

    private void Start()
    {
        TryStartMonologue();
    }

    private void OnDisable()
    {
        if (radio != null)
            radio.FrequencyChanged -= OnFrequencyChanged;

        if (gameManager != null)
            gameManager.PuzzleSolved -= OnPuzzleSolved;

        StopMonologue();
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        if (!gameManager.IsPlaying || radio.IsEnteringFrequency || radio.Frequency != contactFrequency)
        {
            StopMonologue();
            return;
        }

        if (_isTyping)
        {
            _visibleCharacters += charactersPerSecond * Time.unscaledDeltaTime;
            lineText.maxVisibleCharacters = Mathf.Min((int)_visibleCharacters, _characterCount);

            if (_visibleCharacters >= _characterCount)
            {
                _isTyping = false;
            }
        }

        if (!Application.isFocused || Time.frameCount == _lastAdvanceFrame)
            return;

        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool pressedSpace = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (clicked || pressedSpace)
        {
            Advance();
        }
    }

    public void Advance()
    {
        if (!IsPlaying || Time.frameCount == _lastAdvanceFrame)
            return;

        _lastAdvanceFrame = Time.frameCount;

        if (_isTyping)
        {
            lineText.maxVisibleCharacters = int.MaxValue;
            _isTyping = false;
            return;
        }

        _lineIndex++;

        if (_lineIndex >= _lines.Count)
        {
            _completedStages.Add(_stageIndex);
            StopMonologue();
            return;
        }

        ShowLine();
    }

    public void ReplayCurrentMonologue()
    {
        StopMonologue();
        _completedStages.Remove(gameManager.SolvedPuzzleCount);
        TryStartMonologue();
    }

    private void OnFrequencyChanged(int frequency)
    {
        if (frequency != contactFrequency)
        {
            _completedStages.Remove(gameManager.SolvedPuzzleCount);
            StopMonologue();
            return;
        }

        TryStartMonologue();
    }

    private void OnPuzzleSolved(int solvedPuzzleCount)
    {
        StopMonologue();
        TryStartMonologue();
    }

    private void TryStartMonologue()
    {
        if (!isActiveAndEnabled || IsPlaying || !gameManager.IsPlaying ||
            radio.IsEnteringFrequency || radio.Frequency != contactFrequency ||
            _completedStages.Contains(gameManager.SolvedPuzzleCount))
        {
            return;
        }

        _lines = gameManager.GetCurrentRadioMonologue();

        if (_lines.Count == 0)
            return;

        _stageIndex = gameManager.SolvedPuzzleCount;
        _lineIndex = 0;
        IsPlaying = true;
        gameManager.SetMonologueActive(true);
        panel.SetActive(true);

        if (speakerText != null)
        {
            speakerText.text = speakerName;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        lineText.text = _lines[_lineIndex] ?? string.Empty;
        lineText.maxVisibleCharacters = int.MaxValue;
        lineText.ForceMeshUpdate();
        _characterCount = lineText.textInfo.characterCount;
        _visibleCharacters = 0f;
        _isTyping = charactersPerSecond > 0f && _characterCount > 0;
        lineText.maxVisibleCharacters = _isTyping ? 0 : int.MaxValue;
        _lastAdvanceFrame = Time.frameCount;
    }

    private void StopMonologue()
    {
        if (IsPlaying && gameManager != null)
        {
            gameManager.SetMonologueActive(false);
        }

        IsPlaying = false;
        _isTyping = false;
        _lines = null;

        if (panel != null && panel != gameObject)
        {
            panel.SetActive(false);
        }
    }
}
