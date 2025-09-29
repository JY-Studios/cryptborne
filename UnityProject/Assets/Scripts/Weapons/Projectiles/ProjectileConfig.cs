using UnityEngine;

namespace Weapons.Projectiles
{
    public enum SpreadPattern
    {
        None,        // Kein Spread - direkter Schuss (Sniper)
        Random,      // Zufällige Streuung (Machinegun)
        Cone,        // Gleichmäßiger Kegel (Shotgun)
        Horizontal,  // Horizontale Linie
        Vertical,    // Vertikale Linie
        Radial,      // 360° gleichmäßig verteilt nach außen (AoE Magic)
        Orbit,       // Kreist um den Spieler herum (Defensive Magic)
        Spiral       // Spiralförmig rotierend (Wirbel-Effekt)
    }

    [System.Serializable]
    public class ProjectileConfig
    {
        [Header("Basic Settings")]
        public GameObject prefab;
        public int count = 1;           // wie viele gleichzeitig
        public float spread = 0f;       // Streuung/Radius für diesen Typ
        public float range = 10f;
        public float speed = 20f;
        
        [Header("Spread Pattern")]
        public SpreadPattern pattern = SpreadPattern.None;
        
        [Header("Orbit Settings (nur für Orbit Pattern)")]
        [Tooltip("Radius um den Spieler für Orbit Pattern")]
        public float orbitRadius = 2f;
        [Tooltip("Rotationsgeschwindigkeit für Orbit (Grad/Sekunde)")]
        public float orbitSpeed = 180f;
        [Tooltip("Startwinkel-Offset für Orbit")]
        public float orbitStartAngle = 0f;
    }
}