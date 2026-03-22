using UnityEngine;

namespace Necrozone.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Necrozone/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName  = "Pistol";
        public float  damage      = 25f;
        public float  fireRate    = 3f;      // disparos por segundo
        public int    magazineSize = 12;
        public float  reloadTime  = 1.5f;
        public float  range       = 20f;

        // null = modo raycast (Fase 0). Asignar prefab para modo proyectil (Fase 1+)
        public GameObject projectilePrefab;

        public SoundData sfxShoot;
        public SoundData sfxReload;
        public SoundData sfxEmpty;
    }
}
