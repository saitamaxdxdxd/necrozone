using System;
using UnityEngine;
using Necrozone.Managers;

namespace Necrozone.Player
{
    /// <summary>
    /// Gestiona el HP del jugador. Al llegar a 0 dispara GameManager.GameOver().
    ///
    /// Suscribirse a los eventos estáticos para actualizar HUD:
    ///   PlayerHealth.OnHealthChanged += (current, max) => { ... };
    ///   PlayerHealth.OnPlayerDied    += () => { ... };
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;

        public float MaxHealth     => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool  IsDead        { get; private set; }

        public static event Action<float, float> OnHealthChanged; // current, max
        public static event Action               OnPlayerDied;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            IsDead        = false;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (CurrentHealth <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        public void SetMaxHealth(float newMax, bool refillOnIncrease = false)
        {
            float prev = _maxHealth;
            _maxHealth = newMax;

            if (refillOnIncrease && newMax > prev)
                CurrentHealth += newMax - prev;

            CurrentHealth = Mathf.Min(CurrentHealth, _maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        private void Die()
        {
            IsDead = true;
            OnPlayerDied?.Invoke();
            GameManager.Instance?.GameOver();
        }
    }
}
