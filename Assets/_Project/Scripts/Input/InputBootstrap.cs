using UnityEngine;

namespace Necrozone.Input
{
    /// <summary>
    /// Activa el input correcto según plataforma en tiempo de ejecución.
    ///
    /// Setup en Unity (todos los componentes pueden estar en el mismo GO del jugador):
    ///   1. Agregar InputBootstrap, PlayerInputPC y PlayerInputMobile al GO del jugador.
    ///   2. _joystickCanvasGO → el GameObject del Canvas/panel del joystick (UI, separado del player).
    ///   3. En PlayerController._inputSource dejar vacío — InputBootstrap lo asigna en Awake.
    ///   4. Dejar _forceMobileInEditor = false para probar con WASD+mouse en editor.
    ///      Activar para probar el joystick con el mouse en el editor.
    ///
    /// En el Editor usa PC (WASD + mouse). En device real (iOS/Android) usa Mobile (joystick).
    /// </summary>
    public class InputBootstrap : MonoBehaviour
    {
        [Tooltip("GO del Canvas o panel que contiene los joysticks. Se oculta en PC.")]
        [SerializeField] private GameObject _joystickCanvasGO;

        [Tooltip("Activa para probar el joystick virtual en el Editor.")]
        [SerializeField] private bool _forceMobileInEditor = false;

        private void Awake()
        {
            bool useMobile = IsMobilePlatform();

            var pc     = GetComponent<PlayerInputPC>();
            var mobile = GetComponent<PlayerInputMobile>();

            // Habilitar solo el componente correcto
            if (pc != null)     pc.enabled     = !useMobile;
            if (mobile != null) mobile.enabled = useMobile;

            // Mostrar/ocultar canvas de joysticks
            if (_joystickCanvasGO != null)
                _joystickCanvasGO.SetActive(useMobile);

            // Inyectar en PlayerController
            var controller = GetComponent<Necrozone.Player.PlayerController>();
            if (controller == null)
                controller = FindFirstObjectByType<Necrozone.Player.PlayerController>();

            if (controller != null)
            {
                MonoBehaviour handler = useMobile ? (MonoBehaviour)mobile : (MonoBehaviour)pc;
                if (handler != null)
                    controller.SetInputSource(handler);
            }
        }

        private bool IsMobilePlatform()
        {
#if UNITY_EDITOR
            return _forceMobileInEditor;
#elif UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return false;
#endif
        }
    }
}
