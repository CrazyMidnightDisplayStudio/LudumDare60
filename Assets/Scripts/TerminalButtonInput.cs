using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TerminalButtonInput : MonoBehaviour
{
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private Transform keyboardRoot;
    [SerializeField] private Button backspaceButton;
    [SerializeField] private Button enterButton;
    [SerializeField] private MonoBehaviour interpreter;

    private readonly Dictionary<Button, UnityAction> _buttonActions = new();

    private IInterpreter _interpreter;

    private void Awake()
    {
        _interpreter = interpreter as IInterpreter;

        if (_interpreter == null)
        {
            Debug.LogError(
                $"{name}: interpreter must implement IInterpreter.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (keyboardRoot != null)
        {
            Button[] buttons = keyboardRoot.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                if (button == backspaceButton || button == enterButton)
                    continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

                if (label == null)
                    continue;

                AddButtonListener(button, () => Append(label.text));
            }
        }

        AddButtonListener(backspaceButton, Backspace);
        AddButtonListener(enterButton, Submit);
    }

    private void OnDisable()
    {
        foreach (var pair in _buttonActions)
        {
            if (pair.Key != null)
            {
                pair.Key.onClick.RemoveListener(pair.Value);
            }
        }

        _buttonActions.Clear();
    }

    private void AddButtonListener(Button button, UnityAction action)
    {
        if (button == null || _buttonActions.ContainsKey(button))
            return;

        _buttonActions.Add(button, action);
        button.onClick.AddListener(action);
    }

    public void Append(string symbol)
    {
        if (inputText == null)
            return;

        inputText.text += symbol;
    }

    public void Backspace()
    {
        if (inputText == null || inputText.text.Length == 0)
            return;

        inputText.text = inputText.text.Remove(inputText.text.Length - 1);
    }

    public void Submit()
    {
        if (inputText == null || string.IsNullOrEmpty(inputText.text) ||
            _interpreter == null || !_interpreter.IsInterpreterOn)
        {
            return;
        }

        _interpreter.IsEnterPressed = true;
    }
}
