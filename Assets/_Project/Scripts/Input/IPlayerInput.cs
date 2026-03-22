using UnityEngine;

namespace Necrozone.Input
{
    /// <summary>
    /// Contrato de input para el jugador.
    /// PlayerInputPC y PlayerInputMobile lo implementan.
    /// PlayerController y PlayerShooter consumen esta interfaz — nunca leen hardware directamente.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>Dirección de movimiento normalizada en plano XZ. X=derecha, Y=adelante.</summary>
        Vector2 MoveDirection { get; }

        /// <summary>Dirección de apuntado normalizada en plano XZ. Viene del mouse o joystick derecho.</summary>
        Vector2 AimDirection  { get; }

        /// <summary>True mientras el jugador mantiene el botón/joystick de disparo.</summary>
        bool    FireHeld      { get; }

        /// <summary>True el frame en que se presiona dash.</summary>
        bool    DashPressed   { get; }
    }
}
