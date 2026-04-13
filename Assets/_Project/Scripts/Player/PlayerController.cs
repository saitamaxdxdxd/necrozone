using UnityEngine;
using Necrozone.Input;
using Necrozone.Managers;

namespace Necrozone.Player
{
    /// <summary>
    /// Movimiento top-down del jugador en plano XZ.
    /// Lee dirección de movimiento y apuntado desde IPlayerInput.
    ///
    /// Setup en Unity:
    ///   - RequireComponent(Rigidbody) → ajustar en Inspector:
    ///       · Constraints: Freeze Position Y, Freeze Rotation X/Y/Z
    ///       · Collision Detection: Continuous
    ///       · Gravity: desactivar (Use Gravity = false) o dejar Y frozen
    ///   - _inputSource → arrastrar PlayerInputPC o PlayerInputMobile del mismo GO
    ///   - _rotationSpeed = 0 → rotación instantánea (recomendado para prototype)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _inputSource; // debe implementar IPlayerInput

        [Header("Movimiento")]
        [SerializeField] private float _moveSpeed      = 5f;
        [SerializeField] private float _rotationSpeed  = 0f;  // deg/s · 0 = instantáneo

        private IPlayerInput _input;
        private Rigidbody    _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            // Intentar desde el campo del Inspector primero, luego buscar en el GO
            _input = _inputSource as IPlayerInput;
            if (_input == null)
                _input = GetComponent<IPlayerInput>();

            if (_input == null)
                Debug.LogError("[PlayerController] No se encontró IPlayerInput en el GameObject.", this);

            // Asegurar constraints correctos por código también
            _rb.constraints = RigidbodyConstraints.FreezePositionY
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY
                            | RigidbodyConstraints.FreezeRotationZ;
            _rb.useGravity       = false;
            _rb.interpolation    = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (_input == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            Move(_input.MoveDirection);
            Rotate(_input.AimDirection);
        }

        private void Move(Vector2 dir)
        {
            Vector3 velocity = new Vector3(dir.x, 0f, dir.y) * _moveSpeed;
            _rb.linearVelocity = velocity;
        }

        private void Rotate(Vector2 aimDir)
        {
            if (aimDir.sqrMagnitude < 0.01f) return;

            Quaternion target = Quaternion.LookRotation(new Vector3(aimDir.x, 0f, aimDir.y));

            if (_rotationSpeed <= 0f)
                transform.rotation = target;
            else
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, _rotationSpeed * Time.fixedDeltaTime);
        }

        /// Llamar desde InputBootstrap para inyectar el handler correcto en runtime.
        public void SetInputSource(MonoBehaviour source)
        {
            _inputSource = source;
            _input = source as IPlayerInput;
        }
    }
}
