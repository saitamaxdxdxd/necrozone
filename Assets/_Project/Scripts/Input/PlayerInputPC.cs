using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrozone.Input
{
    /// <summary>
    /// Input de PC: WASD para mover, Mouse para apuntar, Clic izquierdo para disparar.
    ///
    /// Setup en Unity:
    ///   - Agregar este componente al GameObject del jugador (mismo GO que PlayerController).
    ///   - Asignar la cámara principal en _camera.
    ///   - En PlayerController, arrastrar este componente al campo _inputSource.
    /// </summary>
    public class PlayerInputPC : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private Camera _camera;

        private Vector2 _moveDir;
        private Vector2 _aimDir;

        public Vector2 MoveDirection => _moveDir;
        public Vector2 AimDirection  => _aimDir;
        public bool    FireHeld      => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public bool    DashPressed   => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            ReadMove();
            ReadAim();
        }

        private void ReadMove()
        {
            var kb = Keyboard.current;
            if (kb == null) { _moveDir = Vector2.zero; return; }

            float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            _moveDir = new Vector2(x, y).normalized;
        }

        private void ReadAim()
        {
            if (_camera == null || Mouse.current == null) { _aimDir = Vector2.zero; return; }

            // Proyectar rayo de cámara → plano XZ a la altura del jugador
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Mathf.Approximately(ray.direction.y, 0f)) { _aimDir = Vector2.zero; return; }

            float    t          = (transform.position.y - ray.origin.y) / ray.direction.y;
            Vector3  worldPoint = ray.origin + ray.direction * t;
            Vector3  dir        = worldPoint - transform.position;

            _aimDir = new Vector2(dir.x, dir.z).normalized;
        }
    }
}
