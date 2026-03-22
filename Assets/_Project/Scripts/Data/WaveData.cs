using System;
using UnityEngine;

namespace Necrozone.Data
{
    [Serializable]
    public class ZombieSpawnEntry
    {
        public ZombieData  zombieData;
        public GameObject  prefab;
        public int         count;
    }

    /// <summary>
    /// Configuración de una oleada específica.
    /// Si no se asigna WaveData para una oleada, WaveManager usa el zombie por defecto (Walker).
    /// Crear assets en: ScriptableObjects/Waves/
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "Necrozone/WaveData")]
    public class WaveData : ScriptableObject
    {
        public ZombieSpawnEntry[] entries;
        public float spawnInterval = 0.3f; // segundos entre cada spawn individual
    }
}
