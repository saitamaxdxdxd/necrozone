using System.Collections.Generic;
using UnityEngine;

namespace Necrozone.Utils
{
    [System.Serializable]
    public class PoolEntry
    {
        public string     key;
        public GameObject prefab;
        public int        initialSize = 10;
    }

    /// <summary>
    /// Pool genérico de objetos. Colocar en un GameObject de GameScene.
    /// Configurar las entradas en Inspector antes de entrar en gameplay.
    ///
    /// Uso:
    ///   ObjectPool.Instance.Get("Zombie_Walker")     → activa desde pool
    ///   ObjectPool.Instance.Return("Zombie_Walker", go) → desactiva y devuelve
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [SerializeField] private PoolEntry[] _pools;

        private Dictionary<string, Queue<GameObject>> _poolDict  = new();
        private Dictionary<string, GameObject>        _prefabDict = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var entry in _pools)
            {
                var queue = new Queue<GameObject>();
                _prefabDict[entry.key] = entry.prefab;

                for (int i = 0; i < entry.initialSize; i++)
                {
                    var obj = Instantiate(entry.prefab, transform);
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }

                _poolDict[entry.key] = queue;
            }
        }

        public GameObject Get(string key)
        {
            if (!_poolDict.ContainsKey(key))
            {
                Debug.LogError($"[ObjectPool] Key '{key}' no encontrada. Verifica Inspector.");
                return null;
            }

            if (_poolDict[key].Count > 0)
            {
                var obj = _poolDict[key].Dequeue();
                obj.SetActive(true);
                return obj;
            }

            // Pool agotado: crecer dinámicamente
            Debug.LogWarning($"[ObjectPool] Pool '{key}' agotado. Creciendo...");
            var newObj = Instantiate(_prefabDict[key], transform);
            newObj.SetActive(true);
            return newObj;
        }

        public void Return(string key, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (!_poolDict.ContainsKey(key))
                _poolDict[key] = new Queue<GameObject>();

            _poolDict[key].Enqueue(obj);
        }
    }
}
