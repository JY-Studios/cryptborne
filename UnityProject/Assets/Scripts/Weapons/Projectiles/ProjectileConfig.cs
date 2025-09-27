using UnityEngine;

namespace Weapons.Projectiles
{
    [System.Serializable]
    public class ProjectileConfig
    {
        public GameObject prefab;
        public int count = 1;           // wie viele gleichzeitig
        public float spread = 0f;       // Streuung für diesen Typ
        public float range = 0f;
        public float speed = 0f;
    }

}