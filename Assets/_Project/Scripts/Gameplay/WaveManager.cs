using System;
using System.Collections;
using UnityEngine;
using Necrozone.Data;
using Necrozone.Managers;
using Necrozone.Player;

namespace Necrozone.Gameplay
{
    /// <summary>
    /// Controla el ciclo de oleadas: spawn → esperar a que mueran todos → pausa → siguiente.
    ///
    /// Setup en Unity:
    ///   - Colocar en un GameObject vacío en GameScene (ej. "WaveManager").
    ///   - Asignar _spawner (ZombieSpawner), _playerTransform y _house.
    ///   - _defaultZombieData / _defaultZombiePrefab = Walker para Fase 0.
    ///   - _waves puede quedar vacío en prototype — usará solo Walker escalando cantidad.
    ///
    /// Eventos disponibles para HUD:
    ///   WaveManager.OnWaveStarted    += (wave) => { ... }
    ///   WaveManager.OnWaveCompleted  += (wave) => { ... }
    ///   WaveManager.OnBetweenWaves   += (secsRemaining) => { ... }
    ///   WaveManager.OnKillCountUpdated += (totalKills) => { ... }
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        // ── Referencias ───────────────────────────────────────────────────────

        [Header("Referencias")]
        [SerializeField] private ZombieSpawner _spawner;
        [SerializeField] private Transform     _playerTransform;
        [SerializeField] private HouseHealth   _house;

        [Header("Zombie por defecto (Walker — Fase 0)")]
        [SerializeField] private ZombieData  _defaultZombieData;
        [SerializeField] private GameObject  _defaultZombiePrefab;

        [Header("Oleadas específicas (opcional — dejar vacío para escalado automático)")]
        [SerializeField] private WaveData[] _waves;

        [Header("Timing")]
        [SerializeField] private float _countdownBeforeFirstWave = 3f;
        [SerializeField] private float _betweenWaveDelay         = 12f;
        [SerializeField] private float _spawnInterval            = 0.4f;

        [Header("Escalado automático")]
        [SerializeField] private int _baseZombieCount   = 8;
        [SerializeField] private int _zombiesAddedPerWave = 4;

        // ── Estado público ────────────────────────────────────────────────────

        public int  CurrentWave    { get; private set; }
        public int  TotalKills     { get; private set; }
        public bool IsBetweenWaves { get; private set; }

        // ── Eventos ───────────────────────────────────────────────────────────

        public static event Action<int> OnWaveStarted;        // nº de oleada
        public static event Action<int> OnWaveCompleted;      // nº de oleada
        public static event Action<int> OnBetweenWaves;       // segundos restantes
        public static event Action<int> OnKillCountUpdated;   // total kills

        // ── Estado interno ────────────────────────────────────────────────────

        private int  _zombiesAlive;
        private bool _running;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            ZombieHealth.OnZombieDied += HandleZombieDied;
        }

        private void OnDisable()
        {
            ZombieHealth.OnZombieDied -= HandleZombieDied;
        }

        private void Start()
        {
            GameManager.OnStateChanged += HandleStateChanged;

            if (_spawner != null)
                _spawner.Init(_playerTransform, _house);

            StartCoroutine(RunWaves());
        }

        // ── Loop principal ────────────────────────────────────────────────────

        private IEnumerator RunWaves()
        {
            _running = true;
            yield return new WaitForSeconds(_countdownBeforeFirstWave);

            while (_running)
            {
                CurrentWave++;
                yield return StartCoroutine(SpawnWave(CurrentWave));

                // Esperar a que mueran todos los zombies
                yield return new WaitUntil(() => _zombiesAlive <= 0);

                OnWaveCompleted?.Invoke(CurrentWave);
                IsBetweenWaves = true;

                // Countdown entre oleadas
                float remaining = _betweenWaveDelay;
                while (remaining > 0f && _running)
                {
                    OnBetweenWaves?.Invoke(Mathf.CeilToInt(remaining));
                    yield return new WaitForSeconds(1f);
                    remaining -= 1f;
                }

                IsBetweenWaves = false;
            }
        }

        private IEnumerator SpawnWave(int wave)
        {
            int count = GetZombieCount(wave);
            _zombiesAlive = 0;

            OnWaveStarted?.Invoke(wave);

            for (int i = 0; i < count; i++)
            {
                if (!_running) yield break;

                SpawnOne(wave);
                _zombiesAlive++;

                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        // ── Spawn ─────────────────────────────────────────────────────────────

        private void SpawnOne(int wave)
        {
            int index = wave - 1;

            // Usar WaveData específica si existe para esta oleada
            if (_waves != null && index < _waves.Length && _waves[index] != null)
            {
                var waveData = _waves[index];
                var entry    = waveData.entries[UnityEngine.Random.Range(0, waveData.entries.Length)];
                _spawner.SpawnZombie(entry.zombieData, entry.prefab);
            }
            else
            {
                // Escalado automático con Walker
                _spawner.SpawnZombie(_defaultZombieData, _defaultZombiePrefab);
            }
        }

        private int GetZombieCount(int wave)
        {
            // Si hay WaveData para esta oleada, sumar el count de sus entries
            int index = wave - 1;
            if (_waves != null && index < _waves.Length && _waves[index] != null)
            {
                int total = 0;
                foreach (var e in _waves[index].entries) total += e.count;
                return total > 0 ? total : _baseZombieCount + index * _zombiesAddedPerWave;
            }

            return _baseZombieCount + (wave - 1) * _zombiesAddedPerWave;
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void HandleZombieDied(ZombieHealth _)
        {
            _zombiesAlive = Mathf.Max(0, _zombiesAlive - 1);
            TotalKills++;
            OnKillCountUpdated?.Invoke(TotalKills);
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                _running = false;
                StopAllCoroutines();
            }
        }

        private void OnDestroy()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
        }
    }
}
