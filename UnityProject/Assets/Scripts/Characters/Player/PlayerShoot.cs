using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Core.Events;

namespace Characters.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [Header("Shooting")]
        public Transform firePoint;
        public float bulletSpeed = 20f;
        public float shootInterval = 2f; // Schießintervall in Sekunden
        
        [Header("Targeting")]
        public float detectionRange = 8f; // Maximale Reichweite für Feinderkennung
        
        [Header("Performance")]
        [SerializeField] private int rangeCheckInterval = 10; // Alle wieviele Frames Range checken
        
        // Private Felder
        private float nextShootTime = 0f;
        private int frameCounter = 0;
        
        // Enemy Tracking (VIEL performanter als FindGameObjectsWithTag!)
        private List<GameObject> allEnemies = new List<GameObject>();
        private List<GameObject> enemiesInRange = new List<GameObject>();
        
        void OnEnable()
        {
            // Events abonnieren
            GameEvents.OnEnemySpawned += OnEnemySpawned;
            GameEvents.OnEnemyDied += OnEnemyDied;
        }
        
        void OnDisable()
        {
            // Events abmelden (wichtig um Memory Leaks zu vermeiden!)
            GameEvents.OnEnemySpawned -= OnEnemySpawned;
            GameEvents.OnEnemyDied -= OnEnemyDied;
        }
        
        void Start()
        {
            // Falls schon Enemies existieren (z.B. beim Scene Reload)
            RefreshEnemyList();
        }
        
        void Update()
        {
            // Automatisches Schießen
            if (Time.time >= nextShootTime)
            {
                FindAndShootNearestEnemy();
                nextShootTime = Time.time + shootInterval;
            }
            
            // Range Check nur alle X Frames (Performance!)
            frameCounter++;
            if (frameCounter >= rangeCheckInterval)
            {
                frameCounter = 0;
                UpdateEnemiesInRange();
            }
        }
        
        // ==================== EVENT HANDLERS ====================
        
        void OnEnemySpawned(GameObject enemy)
        {
            if (enemy == null) return;
            
            // Zur Gesamtliste hinzufügen
            if (!allEnemies.Contains(enemy))
            {
                allEnemies.Add(enemy);
                
                // Sofort prüfen ob in Reichweite
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= detectionRange)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
        
        void OnEnemyDied(GameObject enemy)
        {
            // Aus beiden Listen entfernen
            allEnemies.Remove(enemy);
            enemiesInRange.Remove(enemy);
        }
        
        // ==================== ENEMY MANAGEMENT ====================
        
        void UpdateEnemiesInRange()
        {
            // Alte Liste clearen
            enemiesInRange.Clear();
            
            // Null-Einträge entfernen (Safety)
            allEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);
            
            // Alle Enemies durchgehen und Range prüfen
            foreach (var enemy in allEnemies)
            {
                if (enemy == null) continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= detectionRange)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
        
        GameObject FindNearestEnemy()
        {
            // Safety: Null-Einträge entfernen
            enemiesInRange.RemoveAll(e => e == null || !e.activeInHierarchy);
            
            if (enemiesInRange.Count == 0)
                return null;
            
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var enemy in enemiesInRange)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
            
            return nearestEnemy;
        }
        
        // ==================== SHOOTING ====================
        
        void FindAndShootNearestEnemy()
        {
            GameObject nearestEnemy = FindNearestEnemy();
            
            if (nearestEnemy != null)
            {
                // Richtung zum Feind berechnen
                Vector3 directionToEnemy = (nearestEnemy.transform.position - transform.position).normalized;
                directionToEnemy.y = 0; // Nur horizontale Rotation
                
                // Optional: Charakter zum Feind drehen
                transform.rotation = Quaternion.LookRotation(directionToEnemy);
                
                // Schießen
                Shoot(directionToEnemy);
                
                Debug.Log($"Shooting at enemy {nearestEnemy.name} - Distance: {Vector3.Distance(transform.position, nearestEnemy.transform.position):F1}m");
            }
        }
        
        void Shoot(Vector3 direction)
        {
            Vector3 spawnPos = firePoint ? firePoint.position : transform.position + direction;
            
            // Bullet vom PoolManager holen
            GameObject bullet = PoolManager.Instance.Spawn("Bullet", spawnPos, Quaternion.LookRotation(direction));
            
            if (bullet == null)
            {
                Debug.LogWarning("Could not spawn bullet from pool!");
                return;
            }
            
            // Geschwindigkeit setzen
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }
        }
        
        // ==================== HELPER METHODS ====================
        
        /// <summary>
        /// Aktualisiert die Enemy-Liste (z.B. beim Start oder nach Scene-Reload)
        /// </summary>
        void RefreshEnemyList()
        {
            allEnemies.Clear();
            enemiesInRange.Clear();
            
            // Falls doch noch alte Enemies da sind
            GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            allEnemies.AddRange(existingEnemies);
            
            UpdateEnemiesInRange();
            
            if (existingEnemies.Length > 0)
            {
                Debug.Log($"PlayerShoot: Found {existingEnemies.Length} existing enemies on start");
            }
        }
        
        // ==================== DEBUG ====================
        
        void OnDrawGizmosSelected()
        {
            // Reichweite visualisieren
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Enemies in Range visualisieren
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                foreach (var enemy in enemiesInRange)
                {
                    if (enemy != null)
                    {
                        Gizmos.DrawLine(transform.position, enemy.transform.position);
                        Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
                    }
                }
                
                // Nächsten Enemy highlighten
                GameObject nearest = FindNearestEnemy();
                if (nearest != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(nearest.transform.position, 0.75f);
                }
            }
        }
        
        // ==================== PUBLIC GETTERS (für Debug/UI) ====================
        
        public int GetTotalEnemyCount() => allEnemies.Count;
        public int GetEnemiesInRangeCount() => enemiesInRange.Count;
        public float GetTimeToNextShot() => Mathf.Max(0, nextShootTime - Time.time);
    }
}