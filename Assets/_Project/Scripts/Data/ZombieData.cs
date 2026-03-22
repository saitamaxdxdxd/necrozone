using UnityEngine;

namespace Necrozone.Data
{
    public enum ZombieType { Walker, Runner, Bloater, Screamer, Brute }

    [CreateAssetMenu(fileName = "ZombieData", menuName = "Necrozone/ZombieData")]
    public class ZombieData : ScriptableObject
    {
        [Header("Identidad")]
        public ZombieType type        = ZombieType.Walker;

        [Header("Stats")]
        public float maxHealth        = 50f;
        public float moveSpeed        = 1.5f;
        public float attackDamage     = 10f;
        public float attackRate       = 1f;    // ataques por segundo
        public float attackRange      = 1.2f;  // distancia para activar ataque

        [Header("Drops")]
        public float ammoDropChance   = 0.9f;
        public float healthDropChance = 0.1f;
    }
}
