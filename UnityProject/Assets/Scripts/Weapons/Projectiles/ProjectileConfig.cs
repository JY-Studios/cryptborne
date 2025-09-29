using UnityEngine;

namespace Weapons.Projectiles
{
    public enum SpreadPattern
    {
        Random,      // Zufällige Streuung (Machinegun)
        Cone,        // Gleichmäßiger Kegel (Shotgun)
        Horizontal,  // Horizontale Linie
        Vertical,    // Vertikale Linie
        Circle       // Kreisförmig (experimentell)
    }

    [System.Serializable]
    public class ProjectileConfig
    {
        [Header("Basic Settings")]
        public GameObject prefab;
        public int count = 1;           // wie viele gleichzeitig
        public float spread = 0f;       // Streuung für diesen Typ
        public float range = 10f;
        public float speed = 20f;
        
        [Header("Spread Pattern")]
        public SpreadPattern pattern = SpreadPattern.Random;
    }
}