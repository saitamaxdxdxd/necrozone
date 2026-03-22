using UnityEngine;
using Necrozone.Data;
using Necrozone.Player;
using Necrozone.Utils;

namespace Necrozone.Gameplay
{
    /// <summary>
    /// Spawna zombies en los bordes del mapa usando el ObjectPool.
    /// WaveManager lo llama para cada zombie de la oleada.
    ///
    /// Setup en Unity:
    ///   - Colocar en un GameObject vacío en GameScene.
    ///   - _mapHalfSize debe coincidir con el tamaño real del mapa.
    ///   - WaveManager asigna referencias vía Init() en Start().
    /// </summary>
    public class ZombieSpawner : MonoBehaviour
    {
        [Tooltip("Mitad del ancho del área de juego en unidades.")]
        [SerializeField] private float _mapHalfSize = 20f;

        [Tooltip("Cuánto más allá del borde del mapa spawnean los zombies.")]
        [SerializeField] private float _spawnMargin = 1.5f;

        private Transform   _playerTransform;
        private HouseHealth _house;

        public void Init(Transform player, HouseHealth house)
        {
            _playerTransform = player;
            _house           = house;
        }

        /// <summary>
        /// Spawna un zombie del tipo dado. Clave de pool = "Zombie_" + ZombieType.
        /// Ej: "Zombie_Walker", "Zombie_Runner".
        /// </summary>
        public void SpawnZombie(ZombieData data, GameObject fallbackPrefab)
        {
            string poolKey = "Zombie_" + data.type.ToString();
            Vector3 spawnPos = GetEdgePosition();

            GameObject obj = ObjectPool.Instance != null
                ? ObjectPool.Instance.Get(poolKey)
                : Instantiate(fallbackPrefab);

            if (obj == null) return;

            obj.transform.position = spawnPos;
            obj.transform.rotation = Quaternion.identity;

            var ai = obj.GetComponent<ZombieAI>();
            if (ai != null) ai.Init(data, _playerTransform, _house);

            var health = obj.GetComponent<ZombieHealth>();
            if (health != null) health.Init(data, poolKey);
        }

        // ── Posición de spawn ────────────────────────────────────────────────

        private Vector3 GetEdgePosition()
        {
            float edge = _mapHalfSize + _spawnMargin;
            int   side = Random.Range(0, 4);

            float randAxis = Random.Range(-_mapHalfSize, _mapHalfSize);

            return side switch
            {
                0 => new Vector3(randAxis,  0f,  edge),  // Norte
                1 => new Vector3(randAxis,  0f, -edge),  // Sur
                2 => new Vector3( edge,     0f, randAxis), // Este
                _ => new Vector3(-edge,     0f, randAxis), // Oeste
            };
        }
    }
}
