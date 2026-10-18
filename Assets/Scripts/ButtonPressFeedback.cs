using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonPressFeedback : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Текст")]
    [SerializeField] private TMP_Text buttonText;
    [SerializeField, Min(0f)] private float textOffset = 4f;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pressSound;

    [Header("Клавиатура")]
    [SerializeField] private Key keyboardKey = Key.None;

    private Button _button;
    private RectTransform _textTransform;
    private Vector2 _initialTextPosition;
    private PointerEventData _keyboardPointerData;

    private bool _pointerPressed;
    private bool _keyboardPressed;
    private bool _isPressed;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (buttonText != null)
        {
            _textTransform = buttonText.rectTransform;
            _initialTextPosition = _textTransform.anchoredPosition;
        }
    }

    private void Update()
    {
        if (!_button.IsActive() || !_button.IsInteractable())
        {
            ResetPress();
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || keyboardKey == Key.None)
        {
            ReleaseKeyboardPress(false);
            return;
        }

        var key = keyboard[keyboardKey];

        if (key.wasPressedThisFrame)
        {
            BeginKeyboardPress();
        }

        if (key.wasReleasedThisFrame)
        {
            ReleaseKeyboardPress(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left ||
            !_button.IsActive() ||
            !_button.IsInteractable())
        {
            return;
        }

        _pointerPressed = true;
        UpdatePressState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _pointerPressed = false;
        UpdatePressState();
    }

    public void PlayPressFeedback()
    {
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        if (_textTransform != null)
        {
            _textTransform.anchoredPosition =
                _initialTextPosition + Vector2.down * textOffset;
        }
    }

    private void BeginKeyboardPress()
    {
        if (_keyboardPressed)
            return;

        _keyboardPointerData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left
        };

        _keyboardPressed = true;

        // Включает штатное состояние Pressed и Sprite Swap.
        _button.OnPointerDown(_keyboardPointerData);

        UpdatePressState();
    }

    private void ReleaseKeyboardPress(bool invokeClick)
    {
        if (!_keyboardPressed)
            return;

        _keyboardPressed = false;

        PointerEventData pointerData = _keyboardPointerData;
        _keyboardPointerData = null;

        _button.OnPointerUp(pointerData);

        UpdatePressState();

        if (invokeClick && _button.IsActive() && _button.IsInteractable())
        {
            _button.OnPointerClick(pointerData);
        }
    }

    private void UpdatePressState()
    {
        bool pressed = _pointerPressed || _keyboardPressed;

        if (_isPressed == pressed)
            return;

        _isPressed = pressed;

        if (_isPressed)
        {
            PlayPressFeedback();
        }
        else
        {
            RestoreTextPosition();
        }
    }

    private void RestoreTextPosition()
    {
        if (_textTransform != null)
        {
            _textTransform.anchoredPosition = _initialTextPosition;
        }
    }

    private void ResetPress()
    {
        _pointerPressed = false;

        ReleaseKeyboardPress(false);

        _isPressed = false;
        RestoreTextPosition();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ResetPress();
    }

    private void OnDisable()
    {
        ResetPress();
    }
}
