using System;
using System.Collections.Generic;

namespace Necrozone.Data
{
    /// <summary>
    /// Modelo de todos los datos persistentes del juego.
    /// Agregar campos aquí cuando se necesite guardar algo nuevo.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // ── Configuración ─────────────────────────────────────────────────────
        public int   language       = -1;   // -1 = detectar dispositivo · 0 = English · 1 = Spanish
        public float musicVolume    = 1f;
        public float sfxVolume      = 1f;

        // ── Legacy (LevelSelectController — no usado en el nuevo diseño) ──────
        public int unlockedLevels = 1;

        // ── Progresión de juego ───────────────────────────────────────────────
        public int bestWave       = 0;
        public int totalKills     = 0;
        public int coins          = 0;
        public int coinsEarnedTotal = 0;  // histórico para achievements

        // ── Personajes ────────────────────────────────────────────────────────
        public int        selectedCharacter   = 0;
        public List<int>  unlockedCharacters  = new List<int> { 0 }; // 0 = Marcus (gratis)

        // ── Monetización ──────────────────────────────────────────────────────
        public bool adsRemoved = false;

        // ── Retención ─────────────────────────────────────────────────────────
        public int    dailyLoginStreak = 0;
        public string lastLoginDate    = "";
    }
}
