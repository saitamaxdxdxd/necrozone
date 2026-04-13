using UnityEngine;
using UnityEngine.EventSystems;

namespace Necrozone.Input
{
    /// <summary>
    /// Joystick flotante: aparece donde tocas y desaparece al soltar.
    ///
    /// Jerarquía en Canvas:
    ///   JoystickZone  (RectTransform que cubre la zona táctil, ej. mitad izquierda)
    ///     └── JoystickVisual  (RectTransform + CanvasGroup — el fondo visible)
    ///           └── Handle    (RectTransform — el círculo interior)
    ///
    /// Setup:
    ///   - Este script va en JoystickZone (zona de detección, invisible).
    ///   - _visual   → JoystickVisual  (el fondo del joystick)
    ///   - _handle   → Handle          (el nub interior)
    ///   - _maxRadius → radio en px del handle (recomendado 60-80)
    ///
    /// Al iniciar, JoystickVisual debe estar desactivado (SetActive false) o con alpha 0.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _visual;    // el fondo del joystick
        [SerializeField] private RectTransform _handle;    // el circulo interior
        [SerializeField] private float         _maxRadius = 70f;

        public Vector2 Direction { get; private set; }
        public bool    IsActive  { get; private set; }

        private int            _fingerId   = -1;
        private RectTransform  _zoneRT;
        private Canvas         _canvas;

        private void Awake()
        {
            _zoneRT  = GetComponent<RectTransform>();
            _canvas  = GetComponentInParent<Canvas>();

            // Asegurarse de que empiece oculto
            if (_visual != null)
                _visual.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (_fingerId != -1) return;
            _fingerId = e.pointerId;
            IsActive  = true;

            // Mover el visual al punto de toque
            if (_visual != null)
            {
                _visual.gameObject.SetActive(true);
                _visual.position = e.position;  // posición en screen space
            }

            if (_handle != null)
                _handle.anchoredPosition = Vector2.zero;

            Direction = Vector2.zero;
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _fingerId || _visual == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _visual, e.position, e.pressEventCamera, out Vector2 localPos);

            Vector2 clamped          = Vector2.ClampMagnitude(localPos, _maxRadius);
            _handle.anchoredPosition = clamped;
            Direction                = clamped / _maxRadius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _fingerId) return;
            _fingerId = -1;
            IsActive  = false;
            Direction = Vector2.zero;

            if (_visual != null)
                _visual.gameObject.SetActive(false);

            if (_handle != null)
                _handle.anchoredPosition = Vector2.zero;
        }
    }
}
