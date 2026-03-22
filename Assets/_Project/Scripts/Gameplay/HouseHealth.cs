using System;
using UnityEngine;
using Necrozone.Managers;

namespace Necrozone.Gameplay
{
    /// <summary>
    /// HP de la casa que el jugador debe proteger.
    /// Si llega a 0 → GameManager.GameOver().
    ///
    /// Setup en Unity:
    ///   - Agregar al GameObject de la casa.
    ///   - Los zombies llaman a TakeDamage() desde ZombieAI al atacar la casa.
    ///   - Suscribirse a los eventos para actualizar HUD.
    /// </summary>
    public class HouseHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 300f;

        public float MaxHealth     => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool  IsDestroyed   { get; private set; }

        public static event Action<float, float> OnHouseHealthChanged; // current, max
        public static event Action               OnHouseDestroyed;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            IsDestroyed   = false;
        }

        public void TakeDamage(float amount)
        {
            if (IsDestroyed) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHouseHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (CurrentHealth <= 0f)
                DestroyHouse();
        }

        public void Repair(float amount)
        {
            if (IsDestroyed) return;

            CurrentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);
            OnHouseHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        private void DestroyHouse()
        {
            IsDestroyed = true;
            OnHouseDestroyed?.Invoke();
            GameManager.Instance?.GameOver();
        }
    }
}
