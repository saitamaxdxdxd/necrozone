using UnityEngine;

namespace Necrozone.Data
{
    /// <summary>
    /// ScriptableObject que representa un sonido individual.
    /// Crear con clic derecho → Necrozone → Sound Data.
    /// Guardar en Assets/_Project/ScriptableObjects/Audio/
    /// </summary>
    [CreateAssetMenu(fileName = "SoundData", menuName = "Necrozone/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public AudioClip clip;

        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        public bool loop = false;
    }
}
