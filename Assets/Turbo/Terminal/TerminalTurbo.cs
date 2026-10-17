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

    private RectTransform _terminalContentRect;
    
    [SerializeField] private GameObject interpreterObject;

    private IInterpreter _interpreter;

    private void Awake()
    {
        _interpreter = FindInterpreter(interpreterObject);

        if (_interpreter == null)
        {
            Debug.LogError($"{name}: interpreterObject must contain component implementing IInterpreter", this);
            enabled = false;
        }
    }

    private void OnValidate()
    {
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
    
    private void Start()
    {
        _terminalContentRect = terminalContent.GetComponent<RectTransform>();
    }

    private void OnGUI()
    {
        if (!_interpreter.IsInterpreterOn || inputField.text == "" && _interpreter.IsEnterPressed)
        {
            _interpreter.ReleaseEnter();
            return;
        }
       
        if (inputField.text != "" && _interpreter.IsEnterPressed)
        {
            _interpreter.ReleaseEnter();
            var userInput = inputField.text;
            ClearInputField();
            AddStartLine(userInput);
            var lineCount = AddInterpreterLines(_interpreter.Interpret(userInput));
            ScrollToBottom(lineCount);
            inputLine.transform.SetAsLastSibling();
        }
    }

    private void ScrollToBottom(int lineCount)
    {
        if (lineCount > 4)
        {
            scrollRect.velocity = new Vector2(0, 1050f);
        }
        else
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void Scroll(float value)
    {
        scrollRect.velocity = new Vector2(0, value);
    }

    private void AddStartLine(string userInputText)
    {
       var terminalContentSize = _terminalContentRect.sizeDelta;
       _terminalContentRect.sizeDelta = new Vector2(terminalContentSize.x, terminalContentSize.y + 35f);
       
       var newStartLine = Instantiate(startLine, terminalContent.transform);
       
       newStartLine.transform.SetSiblingIndex(terminalContent.transform.childCount - 1); // Set as last child
       
       newStartLine.transform.Find("Input Text (TMP)").GetComponent<TextMeshProUGUI>().text = userInputText;
    }

    private int AddInterpreterLines(List<string> outputLines)
    {
        for (int i = 0; i < outputLines.Count; i++)
        {
            var responseLine = Instantiate(outputLine, terminalContent.transform);
            responseLine.transform.SetAsLastSibling();
            var terminalContentSize = _terminalContentRect.sizeDelta;
            _terminalContentRect.sizeDelta = new Vector2(terminalContentSize.x, terminalContentSize.y + 35f);
            responseLine.GetComponentInChildren<TextMeshProUGUI>().text = outputLines[i];
        }
        
        return outputLines.Count;
    }

    public void ClearInputField()
    {
        inputField.text = string.Empty;
    }
}
