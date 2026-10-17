using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Turbo.ToolTip
{
    public sealed class TooltipScreenSpaceUI : MonoBehaviour
    {
        public static TooltipScreenSpaceUI Instance { get; private set; }

        [SerializeField] private RectTransform canvasRectTransform;

        private RectTransform _backgroundRectTransform;
        private TextMeshProUGUI _textMeshPro;
        private RectTransform _rectTransform;

        private Func<string> _getTooltipTextFunc;

        private void Awake()
        {
            Instance = this;

            _rectTransform = GetComponent<RectTransform>();

            Transform background = transform.Find("Background");
            Transform text = transform.Find("Text");

            _backgroundRectTransform = background.GetComponent<RectTransform>();
            _textMeshPro = text.GetComponent<TextMeshProUGUI>();

            Image backgroundImage = background.GetComponent<Image>();
            if (backgroundImage != null)
                backgroundImage.raycastTarget = false;

            _textMeshPro.raycastTarget = false;

            HideTooltip();
        }

        private void Update()
        {
            if (DragNDrop.DragNDropManager.IsDragging)
            {
                HideTooltip();
                return;
            }
            
            if (!gameObject.activeSelf || _getTooltipTextFunc == null)
                return;

            SetText(_getTooltipTextFunc());

            Vector2 anchoredPosition = GetPointerPosition() / canvasRectTransform.localScale.x;

            if (anchoredPosition.x + _backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
                anchoredPosition.x = canvasRectTransform.rect.width - _backgroundRectTransform.rect.width;

            if (anchoredPosition.y + _backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
                anchoredPosition.y = canvasRectTransform.rect.height - _backgroundRectTransform.rect.height;

            _rectTransform.anchoredPosition = anchoredPosition;
        }

        private void SetText(string tooltipText)
        {
            _textMeshPro.SetText(tooltipText);
            _textMeshPro.ForceMeshUpdate();

            Vector2 textSize = _textMeshPro.GetRenderedValues(false);
            Vector2 paddingSize = new(8f, 8f);

            _backgroundRectTransform.sizeDelta = textSize + paddingSize;
        }

        private Vector2 GetPointerPosition()
        {
            if (Pointer.current != null)
                return Pointer.current.position.ReadValue();

            return Vector2.zero;
        }

        private void ShowTooltip(string tooltipText)
        {
            ShowTooltip(() => tooltipText);
        }

        private void ShowTooltip(Func<string> getTooltipTextFunc)
        {
            _getTooltipTextFunc = getTooltipTextFunc;
            gameObject.SetActive(true);
            SetText(getTooltipTextFunc());
        }

        private void HideTooltip()
        {
            _getTooltipTextFunc = null;
            gameObject.SetActive(false);
        }

        public static void ShowTooltip_Static(string tooltipText)
        {
            if (Instance == null)
                return;

            Instance.ShowTooltip(tooltipText);
        }

        public static void ShowTooltip_Static(Func<string> getTooltipTextFunc)
        {
            if (Instance == null)
                return;

            Instance.ShowTooltip(getTooltipTextFunc);
        }

        public static void HideTooltip_Static()
        {
            if (Instance == null)
                return;

            Instance.HideTooltip();
        }
    }
}

