using UnityEngine;

namespace Necrozone.Input
{
    /// <summary>
    /// Input mobile: joystick virtual izquierdo para mover, derecho para apuntar.
    /// El disparo es automático cuando el joystick derecho supera el umbral.
    ///
    /// Setup en Unity:
    ///   - Agregar al GameObject del jugador (o a un GO de UI dedicado).
    ///   - Asignar los dos VirtualJoystick desde el Canvas.
    ///   - En PlayerController, arrastrar este componente al campo _inputSource.
    ///
    /// Nota: Este componente se activa solo en mobile. En PC usar PlayerInputPC.
    /// Puedes activar/desactivar el GO del joystick según plataforma en un GameBootstrap.
    /// </summary>
    public class PlayerInputMobile : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private VirtualJoystick _moveJoystick;
        [SerializeField] private VirtualJoystick _aimJoystick;

        [Tooltip("Magnitud mínima del joystick derecho para disparar automáticamente.")]
        [SerializeField] private float _fireThreshold = 0.2f;

        public Vector2 MoveDirection => _moveJoystick != null ? _moveJoystick.Direction : Vector2.zero;
        // Sin joystick derecho: el player rota hacia donde se mueve (un solo joystick)
        public Vector2 AimDirection  => _aimJoystick != null ? _aimJoystick.Direction : MoveDirection;
        public bool    FireHeld      => _aimJoystick != null
                                        ? _aimJoystick.Direction.magnitude > _fireThreshold
                                        : MoveDirection.magnitude > _fireThreshold;
        public bool    DashPressed   => false; // Fase futura: botón dedicado en pantalla
    }
}
