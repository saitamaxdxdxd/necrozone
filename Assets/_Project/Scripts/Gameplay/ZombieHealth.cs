using System;
using UnityEngine;
using Necrozone.Data;
using Necrozone.Utils;

namespace Necrozone.Gameplay
{
    /// <summary>
    /// HP del zombie. Al morir se devuelve al ObjectPool.
    ///
    /// Setup en Unity:
    ///   - Agregar al prefab del zombie junto a ZombieAI.
    ///   - ZombieSpawner llama Init() al activar desde pool.
    ///   - PlayerShooter llama TakeDamage() al impactar con raycast.
    /// </summary>
    public class ZombieHealth : MonoBehaviour
    {
        public static event Action<ZombieHealth> OnZombieDied;

        private ZombieData _data;
        private float      _currentHealth;
        private bool       _isDead;
        private string     _poolKey;

        private void OnEnable()
        {
            // Reset al sacar del pool
            _isDead = false;
        }

        /// <summary>Llamar desde ZombieSpawner justo después de Get() del pool.</summary>
        public void Init(ZombieData data, string poolKey)
        {
            _data          = data;
            _poolKey       = poolKey;
            _isDead        = false;
            _currentHealth = data.maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            _currentHealth -= amount;

            if (_currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            _isDead = true;
            OnZombieDied?.Invoke(this);

            if (_poolKey != null && ObjectPool.Instance != null)
                ObjectPool.Instance.Return(_poolKey, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
