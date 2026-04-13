using UnityEngine;

namespace Necrozone.Core
{
    /// <summary>
    /// Cámara ortográfica top-down que sigue al jugador con suavizado.
    ///
    /// Setup en Unity:
    ///   1. Seleccionar la cámara principal → Camera component → Projection: Orthographic
    ///   2. Rotar la cámara: X = 60, Y = 0, Z = 0  (ajustar a gusto para el ángulo 3/4)
    ///   3. Asignar este script a la cámara
    ///   4. Arrastrar el Transform del jugador a _target
    ///   5. Ajustar _offset Y para la altura, Z para el retroceso
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float     _smoothSpeed = 6f;
        [SerializeField] private Vector3   _offset      = new Vector3(0f, 8f, -8f);

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired  = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
        }

        // Llamar para asignar el target en runtime (ej. al spawnear al jugador)
        public void SetTarget(Transform target) => _target = target;
    }
}
