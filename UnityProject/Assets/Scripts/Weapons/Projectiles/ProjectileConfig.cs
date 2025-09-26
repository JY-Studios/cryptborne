using UnityEngine;

namespace Weapons.Projectiles
{
    [System.Serializable]
    public class ProjectileConfig
    {
        public GameObject projectilePrefab;
        public int count = 1;           // wie viele gleichzeitig
        public float spread = 0f;       // Streuung für diesen Typ
        public float extraDamage = 0f;  // Bonus-Schaden für diesen Typ
    }

}