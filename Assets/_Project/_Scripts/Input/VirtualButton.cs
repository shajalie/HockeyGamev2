using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HockeyGame.Input
{
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual Feedback")]
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        public event Action OnButtonDown;
        public event Action OnButtonUp;

        public bool IsPressed { get; private set; }

        private void Awake()
        {
            if (_buttonImage == null)
            {
                _buttonImage = GetComponent<Image>();
            }

            if (_buttonImage != null)
            {
                _buttonImage.color = _normalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            OnButtonDown?.Invoke();

            if (_buttonImage != null)
            {
                _buttonImage.color = _pressedColor;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            OnButtonUp?.Invoke();

            if (_buttonImage != null)
            {
                _buttonImage.color = _normalColor;
            }
        }
    }
}
