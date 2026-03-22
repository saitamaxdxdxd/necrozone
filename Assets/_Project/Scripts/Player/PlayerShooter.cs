using System;
using System.Collections;
using UnityEngine;
using Necrozone.Data;
using Necrozone.Gameplay;
using Necrozone.Input;
using Necrozone.Managers;

namespace Necrozone.Player
{
    /// <summary>
    /// Maneja el disparo del jugador.
    /// Fase 0: modo raycast (sin proyectil). Fase 1+: instanciar prefab de proyectil.
    ///
    /// Setup en Unity:
    ///   - _inputSource → mismo componente que PlayerController usa (PlayerInputPC/Mobile)
    ///   - _weaponData  → ScriptableObject del arma inicial (Pistola)
    ///   - _firePoint   → Transform hijo del jugador, posicionado en la "boca" del arma
    ///   - _zombieLayer → Layer mask del layer "Zombie"
    ///   - Crear layer "Zombie" en Unity y asignarlo a todos los prefabs de zombie
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _inputSource; // IPlayerInput
        [SerializeField] private WeaponData    _weaponData;
        [SerializeField] private Transform     _firePoint;
        [SerializeField] private LayerMask     _zombieLayer;

        public int  CurrentAmmo  => _currentAmmo;
        public int  MagazineSize => _weaponData != null ? _weaponData.magazineSize : 0;
        public bool IsReloading  => _isReloading;

        public static event Action<int, int> OnAmmoChanged;    // current, max
        public static event Action           OnReloadStarted;
        public static event Action           OnReloadFinished;

        private IPlayerInput _input;
        private float        _nextFireTime;
        private int          _currentAmmo;
        private bool         _isReloading;

        private void Awake()
        {
            _input = _inputSource as IPlayerInput;
            if (_input == null)
                _input = GetComponent<IPlayerInput>();

            if (_input == null)
                Debug.LogError("[PlayerShooter] No se encontró IPlayerInput en el GameObject.", this);

            if (_weaponData != null)
                _currentAmmo = _weaponData.magazineSize;
        }

        private void Update()
        {
            if (_input == null || _weaponData == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_isReloading) return;

            if (_input.FireHeld && Time.time >= _nextFireTime)
            {
                if (_currentAmmo > 0)
                    Fire();
                else
                    StartCoroutine(ReloadRoutine());
            }
        }

        // ── Disparo ──────────────────────────────────────────────────────────

        private void Fire()
        {
            _nextFireTime = Time.time + 1f / _weaponData.fireRate;
            _currentAmmo--;
            OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.magazineSize);

            Vector3 origin = _firePoint != null
                ? _firePoint.position
                : transform.position + Vector3.up * 0.1f;

            Vector3 dir = transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, _weaponData.range, _zombieLayer))
            {
                var zh = hit.collider.GetComponent<ZombieHealth>();
                if (zh != null) zh.TakeDamage(_weaponData.damage);
            }

            // Feedback visual placeholder — reemplazar con partículas en Fase 2
            Debug.DrawRay(origin, dir * _weaponData.range, Color.red, 0.05f);

            AudioManager.Instance?.PlaySFX(_weaponData.sfxShoot);

            // Auto-recarga al quedar sin balas
            if (_currentAmmo <= 0)
                StartCoroutine(ReloadRoutine());
        }

        // ── Recarga ───────────────────────────────────────────────────────────

        private IEnumerator ReloadRoutine()
        {
            if (_isReloading || _currentAmmo == _weaponData.magazineSize) yield break;

            _isReloading = true;
            OnReloadStarted?.Invoke();

            AudioManager.Instance?.PlaySFX(_weaponData.sfxReload);

            yield return new WaitForSeconds(_weaponData.reloadTime);

            _currentAmmo = _weaponData.magazineSize;
            _isReloading = false;

            OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.magazineSize);
            OnReloadFinished?.Invoke();
        }

        /// <summary>Llamar desde UI (botón) o input para recarga manual.</summary>
        public void ManualReload()
        {
            if (!_isReloading && _currentAmmo < _weaponData.magazineSize)
                StartCoroutine(ReloadRoutine());
        }

        /// <summary>Cambia el arma activa (cartas de mejora, pickups).</summary>
        public void EquipWeapon(WeaponData newWeapon)
        {
            _weaponData  = newWeapon;
            _currentAmmo = newWeapon.magazineSize;
            _isReloading = false;
            OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.magazineSize);
        }
    }
}
