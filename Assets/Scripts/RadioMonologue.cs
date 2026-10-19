using System;
using System.Collections.Generic;
using LD60;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioMonologue : MonoBehaviour
{
    [Serializable]
    public sealed class SecretStation
    {
        [SerializeField, Range(Radio.MIN_WAVE_NUMBER, Radio.MAX_WAVE_NUMBER)]
        private int frequency = Radio.MIN_WAVE_NUMBER;
        [SerializeField] private string speakerName = "Unknown";
        [SerializeField] private RadioConversation[] monologues = Array.Empty<RadioConversation>();
        [SerializeField] private bool repeatAfterLast;

        public int Frequency => frequency;
        public string SpeakerName => speakerName;
        public IReadOnlyList<RadioConversation> Monologues => monologues ?? Array.Empty<RadioConversation>();
        public bool RepeatAfterLast => repeatAfterLast;
    }

    [Header("Connection")]
    [SerializeField] private Radio radio;
    [SerializeField] private GameManager gameManager;
    [SerializeField, Range(Radio.MIN_WAVE_NUMBER, Radio.MAX_WAVE_NUMBER)]
    private int contactFrequency = Constexpr.DEFAULT_WAVE;
    [SerializeField, Range(Radio.MIN_WAVE_NUMBER, Radio.MAX_WAVE_NUMBER)]
    private int idleFrequency = Constexpr.DEFAULT_WAVE;
    [SerializeField] private bool repeatContactMonologues = true;
    [SerializeField] private SecretStation[] secretStations = Array.Empty<SecretStation>();

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private string speakerName = "Contact";
    [SerializeField, Min(0f)] private float charactersPerSecond = 35f;

    private readonly Dictionary<int, int> _nextContactMonologues = new();
    private readonly Dictionary<int, int> _nextSecretMonologues = new();
    private IReadOnlyList<string> _lines;
    private SecretStation _activeSecretStation;
    private int _activeStageIndex;
    private int _activeFrequency;
    private int _conversationIndex;
    private int _lastFrequency = -1;
    private int _lastStageIndex;
    private int _lastConversationIndex;
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

        if (!ValidateStations())
        {
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

        if (!gameManager.IsPlaying || radio.IsEnteringFrequency || radio.Frequency != _activeFrequency)
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
            CompleteConversation();
            return;
        }

        ShowLine();
    }

    public void ReplayCurrentMonologue()
    {
        if (!gameManager.IsPlaying)
            return;

        StopMonologue();

        if (_lastFrequency < Radio.MIN_WAVE_NUMBER ||
            (_lastStageIndex >= 0 && _lastStageIndex != gameManager.SolvedPuzzleCount))
        {
            radio.TuneToFrequency(contactFrequency);
            return;
        }

        if (_lastStageIndex >= 0)
        {
            _nextContactMonologues[_lastStageIndex] = _lastConversationIndex;
        }
        else
        {
            _nextSecretMonologues[_lastFrequency] = _lastConversationIndex;
        }

        radio.TuneToFrequency(_lastFrequency);
    }

    private void OnFrequencyChanged(int frequency)
    {
        if (IsPlaying && frequency != _activeFrequency)
        {
            StopMonologue();
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
        if (!isActiveAndEnabled || IsPlaying || !gameManager.IsPlaying || radio.IsEnteringFrequency)
            return;

        if (radio.Frequency == contactFrequency)
        {
            int stageIndex = gameManager.SolvedPuzzleCount;
            _nextContactMonologues.TryGetValue(stageIndex, out int next);
            IReadOnlyList<RadioConversation> conversations = gameManager.GetCurrentRadioMonologues();
            int index = FindConversation(conversations, next);

            if (index < 0 && repeatContactMonologues)
            {
                index = FindConversation(conversations, 0);
            }

            if (index >= 0)
            {
                BeginConversation(conversations[index], index, stageIndex, null);
            }

            return;
        }

        SecretStation station = FindSecretStation(radio.Frequency);

        if (station == null)
            return;

        _nextSecretMonologues.TryGetValue(station.Frequency, out int nextSecret);
        int secretIndex = FindConversation(station.Monologues, nextSecret);

        if (secretIndex < 0 && station.RepeatAfterLast)
        {
            secretIndex = FindConversation(station.Monologues, 0);
        }

        if (secretIndex >= 0)
        {
            BeginConversation(station.Monologues[secretIndex], secretIndex, -1, station);
        }
    }

    private void BeginConversation(RadioConversation conversation, int index, int stageIndex, SecretStation station)
    {
        _lines = conversation.Lines;
        _conversationIndex = index;
        _activeStageIndex = stageIndex;
        _activeSecretStation = station;
        _activeFrequency = radio.Frequency;
        _lastFrequency = _activeFrequency;
        _lastStageIndex = stageIndex;
        _lastConversationIndex = index;
        _lineIndex = 0;
        IsPlaying = true;
        gameManager.SetMonologueActive(true);
        panel.SetActive(true);

        if (speakerText != null)
        {
            speakerText.text = station == null ? speakerName : station.SpeakerName;
        }

        ShowLine();
    }

    private void CompleteConversation()
    {
        if (_activeSecretStation == null)
        {
            _nextContactMonologues[_activeStageIndex] = _conversationIndex + 1;
        }
        else
        {
            _nextSecretMonologues[_activeFrequency] = _conversationIndex + 1;
        }

        StopMonologue();
        TuneToIdleFrequency();
    }

    private void TuneToIdleFrequency()
    {
        int frequency = Mathf.Clamp(idleFrequency, Radio.MIN_WAVE_NUMBER, Radio.MAX_WAVE_NUMBER);
        int channelCount = Radio.MAX_WAVE_NUMBER - Radio.MIN_WAVE_NUMBER + 1;

        for (int i = 0; i < channelCount; i++)
        {
            if (frequency != contactFrequency && FindSecretStation(frequency) == null)
            {
                radio.TuneToFrequency(frequency);
                return;
            }

            frequency = frequency == Radio.MAX_WAVE_NUMBER ? Radio.MIN_WAVE_NUMBER : frequency + 1;
        }

        Debug.LogError($"{name}: no unused frequency is available after the monologue.", this);
    }

    private static int FindConversation(IReadOnlyList<RadioConversation> conversations, int startIndex)
    {
        for (int i = startIndex; i < conversations.Count; i++)
        {
            if (conversations[i] != null && conversations[i].Lines.Count > 0)
                return i;
        }

        return -1;
    }

    private SecretStation FindSecretStation(int frequency)
    {
        if (secretStations == null)
            return null;

        foreach (SecretStation station in secretStations)
        {
            if (station != null && station.Frequency == frequency)
                return station;
        }

        return null;
    }

    private bool ValidateStations()
    {
        var frequencies = new HashSet<int> { contactFrequency };

        if (secretStations == null)
            return true;

        foreach (SecretStation station in secretStations)
        {
            if (station == null || station.Frequency < Radio.MIN_WAVE_NUMBER ||
                station.Frequency > Radio.MAX_WAVE_NUMBER || !frequencies.Add(station.Frequency))
            {
                Debug.LogError($"{name}: secret stations need unique frequencies different from the contact frequency.", this);
                return false;
            }
        }

        return true;
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
        _activeSecretStation = null;

        if (panel != null && panel != gameObject)
        {
            panel.SetActive(false);
        }
    }
}
