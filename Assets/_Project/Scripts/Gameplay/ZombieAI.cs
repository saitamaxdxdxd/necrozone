using UnityEngine;
using UnityEngine.AI;
using Necrozone.Data;
using Necrozone.Player;

namespace Necrozone.Gameplay
{
    /// <summary>
    /// IA del zombie: persigue al jugador o a la casa (el que esté más cerca) y ataca.
    ///
    /// REQUISITO: Debes hornear (Bake) el NavMesh en la escena:
    ///   Window → AI → Navigation → Bake
    ///   Marcar el suelo como "Navigation Static" antes de hornear.
    ///
    /// Setup en Unity:
    ///   - RequireComponent(NavMeshAgent) — configurar speed/stoppingDistance vía ZombieData.
    ///   - ZombieSpawner llama Init() al activar desde pool.
    ///   - Layer del prefab zombie: "Zombie" (para que PlayerShooter lo detecte).
    ///   - Collider en el prefab (puede ser CapsuleCollider).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(ZombieHealth))]
    public class ZombieAI : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private ZombieData   _data;
        private Transform    _playerTransform;
        private HouseHealth  _house;

        private Transform _currentTarget;
        private float     _nextTargetUpdate;
        private float     _nextAttackTime;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _nextTargetUpdate = 0f;
            _nextAttackTime   = 0f;
            _currentTarget    = null;
        }

        /// <summary>Llamar desde ZombieSpawner justo después de Get() del pool.</summary>
        public void Init(ZombieData data, Transform player, HouseHealth house)
        {
            _data            = data;
            _playerTransform = player;
            _house           = house;
            ApplyStats();
        }

        private void ApplyStats()
        {
            _agent.speed            = _data.moveSpeed;
            _agent.stoppingDistance = _data.attackRange * 0.75f;
            _agent.angularSpeed     = 360f;
            _agent.acceleration     = 12f;
        }

        private void Update()
        {
            if (_data == null || !_agent.isOnNavMesh) return;

            // Actualizar target cada 0.35s (no cada frame — optimización mobile)
            if (Time.time >= _nextTargetUpdate)
            {
                _currentTarget    = GetClosestTarget();
                _nextTargetUpdate = Time.time + 0.35f;
            }

            if (_currentTarget != null)
                _agent.SetDestination(_currentTarget.position);

            TryAttack();
        }

        // ── Target ────────────────────────────────────────────────────────────

        private Transform GetClosestTarget()
        {
            bool hasPlayer = _playerTransform != null;
            bool hasHouse  = _house != null && !_house.IsDestroyed;

            if (!hasPlayer) return hasHouse ? _house.transform : null;
            if (!hasHouse)  return _playerTransform;

            float dPlayer = Vector3.SqrMagnitude(transform.position - _playerTransform.position);
            float dHouse  = Vector3.SqrMagnitude(transform.position - _house.transform.position);

            return dPlayer < dHouse ? _playerTransform : _house.transform;
        }

        // ── Ataque ────────────────────────────────────────────────────────────

        private void TryAttack()
        {
            if (_currentTarget == null || Time.time < _nextAttackTime) return;

            float dist = Vector3.Distance(transform.position, _currentTarget.position);
            if (dist > _data.attackRange) return;

            _nextAttackTime = Time.time + 1f / _data.attackRate;

            // Detectar a quién atacar
            var ph = _currentTarget.GetComponent<PlayerHealth>();
            if (ph != null) { ph.TakeDamage(_data.attackDamage); return; }

            var hh = _currentTarget.GetComponent<HouseHealth>();
            if (hh != null) hh.TakeDamage(_data.attackDamage);
        }
    }
}
