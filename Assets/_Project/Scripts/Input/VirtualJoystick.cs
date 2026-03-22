using UnityEngine;
using UnityEngine.EventSystems;

namespace Necrozone.Input
{
    /// <summary>
    /// Joystick virtual para mobile. Adjuntar al RectTransform del fondo del joystick.
    ///
    /// Jerarquía en Canvas:
    ///   JoystickLeft (RectTransform + VirtualJoystick)
    ///     └── Handle (RectTransform — el círculo que se mueve)
    ///
    /// Setup:
    ///   - _background → el propio RectTransform de este GO (o asignar explícitamente)
    ///   - _handle     → el hijo "Handle"
    ///   - _maxRadius  → radio de movimiento en píxeles (recomendado: 60–80)
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float         _maxRadius = 65f;

        public Vector2 Direction { get; private set; }
        public bool    IsActive  { get; private set; }

        private int _fingerId = -1;

        private void Awake()
        {
            if (_background == null)
                _background = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (_fingerId != -1) return; // ya hay un dedo en este joystick
            _fingerId = e.pointerId;
            IsActive  = true;
            UpdateHandle(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _fingerId) return;
            UpdateHandle(e);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _fingerId) return;
            _fingerId = -1;
            IsActive  = false;
            Direction = Vector2.zero;

            if (_handle != null)
                _handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateHandle(PointerEventData e)
        {
            if (_handle == null || _background == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, e.position, e.pressEventCamera, out Vector2 localPos);

            Vector2 clamped = Vector2.ClampMagnitude(localPos, _maxRadius);
            _handle.anchoredPosition = clamped;
            Direction = clamped / _maxRadius;
        }
    }
}
