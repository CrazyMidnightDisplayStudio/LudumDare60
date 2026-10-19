using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalTurbo : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject startLine;
    [SerializeField] private GameObject outputLine;
    
    [Header("UI")]
    [SerializeField] private TMP_Text inputField;
    [SerializeField] private GameObject inputLine;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject terminalContent;

    [Header("Appearance")]
    [SerializeField, Min(1f)] private float fontSize = 12f;

    [Header("Interpreter")]
    [SerializeField] private GameObject interpreterObject;

    private RectTransform _terminalContentRect;
    private IInterpreter _interpreter;
    private float _appliedFontSize;
    private bool _scrollToBottomPending;

    private void Awake()
    {
        _terminalContentRect = terminalContent.GetComponent<RectTransform>();
        ApplyFontSize();

        _interpreter = FindInterpreter(interpreterObject);

        if (_interpreter == null)
        {
            Debug.LogError($"{name}: interpreterObject must contain component implementing IInterpreter", this);
            enabled = false;
        }
    }

    private void OnValidate()
    {
        fontSize = Mathf.Max(1f, fontSize);

        if (interpreterObject == null)
        {
            return;
        }

        if (FindInterpreter(interpreterObject) == null)
        {
            Debug.LogError($"{name}: interpreterObject must contain component implementing IInterpreter", this);
            interpreterObject = null;
        }
    }

    private IInterpreter FindInterpreter(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        var behaviours = target.GetComponents<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IInterpreter interpreter)
            {
                return interpreter;
            }
        }

        return null;
    }
    
    private void Update()
    {
        if (!_interpreter.IsInterpreterOn)
        {
            _interpreter.ReleaseEnter();
            return;
        }

        if (!_interpreter.IsEnterPressed)
            return;

        _interpreter.ReleaseEnter();

        if (string.IsNullOrEmpty(inputField.text))
            return;

        var userInput = inputField.text;
        ClearInputField();
        AddStartLine(userInput);
        AddInterpreterLines(_interpreter.Interpret(userInput));
        inputLine.transform.SetAsLastSibling();
        _scrollToBottomPending = true;
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(_appliedFontSize, fontSize))
        {
            ApplyFontSize();
        }

        if (!_scrollToBottomPending)
            return;

        _scrollToBottomPending = false;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_terminalContentRect);
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ApplyFontSize()
    {
        ApplyFontSize(gameObject);
        _appliedFontSize = fontSize;

        if (terminalContent != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(
                terminalContent.GetComponent<RectTransform>()
            );
        }
    }

    private void ApplyFontSize(GameObject target)
    {
        TMP_Text[] texts = target.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            text.enableAutoSizing = false;
            text.fontSize = fontSize;
        }
    }

    public void Scroll(float value)
    {
        scrollRect.velocity = new Vector2(0, value);
    }

    private void AddStartLine(string userInputText)
    {
        var newStartLine = Instantiate(startLine, terminalContent.transform);
        newStartLine.transform.SetAsLastSibling();
        newStartLine.transform.Find("Input Text (TMP)").GetComponent<TextMeshProUGUI>().text = userInputText;
        ApplyFontSize(newStartLine);
    }

    private void AddInterpreterLines(List<string> outputLines)
    {
        if (outputLines == null)
            return;

        for (int i = 0; i < outputLines.Count; i++)
        {
            var responseLine = Instantiate(outputLine, terminalContent.transform);
            responseLine.transform.SetAsLastSibling();
            responseLine.GetComponentInChildren<TextMeshProUGUI>().text = outputLines[i];
            ApplyFontSize(responseLine);
        }
    }

    public void ClearInputField()
    {
        inputField.text = string.Empty;
    }
}
