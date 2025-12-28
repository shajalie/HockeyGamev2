using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HockeyGame.Input
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        [Header("Settings")]
        [SerializeField] private float _handleRange = 1f;
        [SerializeField] private float _deadZone = 0.1f;

        private Vector2 _input = Vector2.zero;
        private Canvas _canvas;
        private Camera _uiCamera;

        public Vector2 Input => _input;
        public float Horizontal => _input.x;
        public float Vertical => _input.y;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _uiCamera = _canvas.worldCamera;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, _uiCamera, out localPoint))
            {
                Vector2 normalizedInput = localPoint / (_background.sizeDelta / 2f);

                _input = normalizedInput.magnitude > 1f
                    ? normalizedInput.normalized
                    : normalizedInput;

                if (_input.magnitude < _deadZone)
                {
                    _input = Vector2.zero;
                }

                _handle.anchoredPosition = _input * (_background.sizeDelta / 2f) * _handleRange;
            }
        }

        public void SetHandleRange(float range)
        {
            _handleRange = Mathf.Clamp(range, 0f, 1f);
        }
    }
}
